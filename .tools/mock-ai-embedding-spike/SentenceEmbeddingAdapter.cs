using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;

namespace MockAiEmbeddingSpike;

public sealed record EncodedInput(
    string Text,
    long[] Ids,
    long[] AttentionMask,
    string[] Tokens,
    int OriginalTokenCount,
    bool WasTruncated);

public sealed class EmbeddingBusyException : Exception
{
    public EmbeddingBusyException(int queueCapacity)
        : base($"Embedding adapter is busy; its bounded capacity ({queueCapacity}) is full.") { }
}

/// <summary>
/// Offline-only adapter extracted from the verified spike pipeline. It never downloads artifacts.
/// Tokenization and inference are serialized by default because the tokenizer wrapper's concurrent
/// safety is not assumed; callers still get bounded async admission and cancellation.
/// </summary>
public sealed class SentenceEmbeddingAdapter : IDisposable
{
    private readonly SentenceEmbeddingOptions _options;
    private readonly Tokenizer _tokenizer;
    private readonly InferenceSession _session;
    private readonly SemaphoreSlim _capacity;
    private readonly SemaphoreSlim _inferenceSlots;
    private readonly object _lifecycleGate = new();
    private int _activeOperations;
    private bool _disposed;
    private bool _resourcesDisposed;

    public SentenceEmbeddingAdapter(SentenceEmbeddingOptions options)
    {
        _options = Validate(options);
        _tokenizer = LoadTokenizer(_options.TokenizerPath);
        try
        {
            _session = new InferenceSession(_options.ModelPath);
            ValidateModel(_session);
        }
        catch
        {
            _session?.Dispose();
            _tokenizer.Dispose();
            throw;
        }

        _capacity = new SemaphoreSlim(_options.MaxConcurrency + _options.QueueCapacity, _options.MaxConcurrency + _options.QueueCapacity);
        _inferenceSlots = new SemaphoreSlim(_options.MaxConcurrency, _options.MaxConcurrency);
    }

    public int MaxConcurrency => _options.MaxConcurrency;
    public int QueueCapacity => _options.QueueCapacity;
    public int Dimension => 384;

    internal bool IsInferenceRunning
    {
        get
        {
            lock (_lifecycleGate)
                return !_resourcesDisposed && _inferenceSlots.CurrentCount == 0;
        }
    }

    private EncodedInput Encode(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text must not be empty.", nameof(text));

        var encoding = _tokenizer.Encode(
            _options.Prefix + text,
            addSpecialTokens: true,
            includeTypeIds: true,
            includeTokens: true,
            includeAttentionMask: true).First();

        var ids = encoding.Ids.Select(x => (long)x).ToArray();
        var mask = encoding.AttentionMask.Select(x => (long)x).ToArray();
        var tokens = encoding.Tokens.ToArray();
        var originalCount = ids.Length;
        var truncated = originalCount > _options.MaxTokens;
        if (truncated)
        {
            // Preserve the final special token while retaining the first MaxTokens - 1 tokens.
            ids = ids.Take(_options.MaxTokens - 1).Append(ids[^1]).ToArray();
            mask = Enumerable.Repeat(1L, _options.MaxTokens).ToArray();
            tokens = tokens.Take(_options.MaxTokens - 1).Append(tokens[^1]).ToArray();
        }

        return new EncodedInput(text, ids, mask, tokens, originalCount, truncated);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var result = await EmbedBatchAsync(new[] { text }, cancellationToken);
        return result[0];
    }

    public async Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        BeginOperation();
        if (texts.Count == 0)
        {
            EndOperation();
            cancellationToken.ThrowIfCancellationRequested();
            return Array.Empty<float[]>();
        }

        if (!_capacity.Wait(0))
        {
            EndOperation();
            throw new EmbeddingBusyException(_options.QueueCapacity);
        }

        try
        {
            await _inferenceSlots.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var encoded = texts.Select(Encode).ToArray();
                cancellationToken.ThrowIfCancellationRequested();
                var result = Infer(encoded);
                // ONNX Runtime inference is synchronous here. Cancellation cannot interrupt native
                // execution, so this check prevents returning a successful result to a cancelled caller.
                cancellationToken.ThrowIfCancellationRequested();
                return result;
            }
            finally
            {
                _inferenceSlots.Release();
            }
        }
        finally
        {
            _capacity.Release();
            EndOperation();
        }
    }

    public void Dispose()
    {
        lock (_lifecycleGate)
        {
            if (_disposed) return;
            _disposed = true;
            if (_activeOperations == 0) DisposeResourcesLocked();
        }
    }

    private float[][] Infer(IReadOnlyList<EncodedInput> batch)
    {
        var maxLength = batch.Max(x => x.Ids.Length);
        var inputIds = new DenseTensor<long>(new[] { batch.Count, maxLength });
        var typeIds = new DenseTensor<long>(new[] { batch.Count, maxLength });
        var attentionMask = new DenseTensor<long>(new[] { batch.Count, maxLength });
        for (var row = 0; row < batch.Count; row++)
        {
            for (var col = 0; col < batch[row].Ids.Length; col++)
            {
                inputIds[row, col] = batch[row].Ids[col];
                attentionMask[row, col] = batch[row].AttentionMask[col];
            }
        }

        using var results = _session.Run(new[]
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("token_type_ids", typeIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
        });
        var output = results.First().AsTensor<float>();
        var dimensions = output.Dimensions.ToArray();
        if (dimensions.Length != 3 || dimensions[0] != batch.Count || dimensions[2] != Dimension)
            throw new InvalidDataException($"Model output must be [batch, sequence, {Dimension}], got [{string.Join(",", dimensions)}].");

        var values = output.ToArray();
        var embeddings = new float[batch.Count][];
        for (var row = 0; row < batch.Count; row++)
        {
            var pooled = new float[Dimension];
            var count = 0f;
            for (var col = 0; col < batch[row].Ids.Length; col++)
            {
                if (batch[row].AttentionMask[col] == 0) continue;
                count++;
                var offset = (row * dimensions[1] + col) * Dimension;
                for (var dimension = 0; dimension < Dimension; dimension++) pooled[dimension] += values[offset + dimension];
            }

            if (count == 0) throw new InvalidDataException("Attention mask contains no active tokens.");
            for (var dimension = 0; dimension < Dimension; dimension++) pooled[dimension] /= count;
            var norm = Math.Sqrt(pooled.Sum(value => value * value));
            if (norm == 0) throw new InvalidDataException("Model returned a zero-norm sentence vector.");
            for (var dimension = 0; dimension < Dimension; dimension++) pooled[dimension] /= (float)norm;
            embeddings[row] = pooled;
        }

        return embeddings;
    }

    private static SentenceEmbeddingOptions Validate(SentenceEmbeddingOptions options)
    {
        if (!File.Exists(options.ModelPath)) throw new FileNotFoundException("Embedding model was not found; provide a prepared local artifact.", options.ModelPath);
        if (!File.Exists(options.TokenizerPath)) throw new FileNotFoundException("Tokenizer artifact was not found; provide a prepared local artifact.", options.TokenizerPath);
        if (string.IsNullOrWhiteSpace(options.Prefix)) throw new ArgumentException("Prefix must not be empty.", nameof(options));
        if (options.MaxTokens < 2 || options.MaxTokens > 512) throw new ArgumentOutOfRangeException(nameof(options), "MaxTokens must be between 2 and the model limit 512.");
        if (options.MaxConcurrency != 1) throw new ArgumentOutOfRangeException(nameof(options), "This adapter serializes tokenizer/session use; MaxConcurrency must be 1 until tokenizer concurrency is verified.");
        if (options.QueueCapacity < 0 || options.QueueCapacity > 32) throw new ArgumentOutOfRangeException(nameof(options), "QueueCapacity must be between 0 and 32 for this spike.");
        return options;
    }

    private static Tokenizer LoadTokenizer(string path)
    {
        try { return Tokenizer.FromFile(path); }
        catch (Exception ex) { throw new InvalidDataException($"Tokenizer artifact could not be loaded: {path}", ex); }
    }

    private static void ValidateModel(InferenceSession session)
    {
        var required = new[] { "input_ids", "attention_mask", "token_type_ids" };
        foreach (var name in required)
        {
            if (!session.InputMetadata.TryGetValue(name, out var metadata) || metadata.ElementType != typeof(long))
                throw new InvalidDataException($"Model is missing int64 input '{name}'.");
        }
        if (!session.OutputMetadata.TryGetValue("last_hidden_state", out var output) || output.ElementType != typeof(float))
            throw new InvalidDataException("Model is missing float output 'last_hidden_state'.");
        if (output.Dimensions.Length != 3 || output.Dimensions[2] != 384)
            throw new InvalidDataException("Model last_hidden_state must have final dimension 384.");
    }

    private void BeginOperation()
    {
        lock (_lifecycleGate)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SentenceEmbeddingAdapter));
            _activeOperations++;
        }
    }

    private void EndOperation()
    {
        lock (_lifecycleGate)
        {
            _activeOperations--;
            if (_disposed && _activeOperations == 0) DisposeResourcesLocked();
        }
    }

    private void DisposeResourcesLocked()
    {
        if (_resourcesDisposed) return;
        _resourcesDisposed = true;
        _inferenceSlots.Dispose();
        _capacity.Dispose();
        _session.Dispose();
        _tokenizer.Dispose();
    }
}
