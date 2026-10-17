using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;

if (args.Contains("--adapter-checks", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.AdapterLifecycleChecks.RunAsync();

if (args.Contains("--decision-review", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.DecisionReviewRunner.RunAsync(args);

if (args.Contains("--two-intent-experiment", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.TwoIntentExperimentRunner.RunAsync(args);

if (args.Contains("--ood-abstention-experiment", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.OodAbstentionExperimentRunner.RunAsync(args);

if (args.Contains("--v3-experiment", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.V3ExperimentRunner.RunAsync(args);

if (args.Contains("--v31-verification", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.V3ExperimentRunner.RunAsync(args);

if (args.Contains("--v33-verification", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.V3ExperimentRunner.RunAsync(args);

if (args.Contains("--v31-policy-checks", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.PolicyUnitChecks.RunAsync();

if (args.Contains("--calibrate", StringComparer.OrdinalIgnoreCase))
    return await MockAiEmbeddingSpike.CalibrationRunner.RunAsync(args);

var artifactRoot = GetOption(args, "--artifact-root") ?? "artifacts";
var modelPath = GetOption(args, "--model") ?? Path.Combine(artifactRoot, "onnx", "model_O4.onnx");
var tokenizerPath = GetOption(args, "--tokenizer") ?? Path.Combine(artifactRoot, "onnx", "tokenizer.json");
var outputPath = GetOption(args, "--output") ?? Path.Combine(artifactRoot, "dotnet-results.json");

if (!File.Exists(modelPath) || !File.Exists(tokenizerPath))
{
    Console.Error.WriteLine($"Missing artifact. model={modelPath}, tokenizer={tokenizerPath}");
    return 2;
}

var examples = new[]
{
    "Hôm nay doanh thu được bao nhiêu?",
    "Tiền bán hàng trong ngày hiện tại là bao nhiêu?",
    "So sánh doanh thu hôm nay với hôm qua.",
    "Món nào đang bán chạy nhất tuần này?",
    "Tóm tắt tình hình kinh doanh tháng này.",
    "Tôi muốn xem thực đơn hôm nay.",
    "Còn hôm qua thì sao?",
    "Kiểm tra tồn kho nguyên liệu.",
    "Tôi không muốn xem doanh thu hôm nay."
};

Console.WriteLine($"MODEL={Path.GetFileName(modelPath)}");
Console.WriteLine($"TOKENIZER={Path.GetFileName(tokenizerPath)}");
Console.WriteLine($"PROCESS_RSS_BEFORE={Environment.ProcessId}:{Process.GetCurrentProcess().WorkingSet64}");

var initWatch = Stopwatch.StartNew();
using var tokenizer = Tokenizer.FromFile(tokenizerPath);
using var session = new InferenceSession(modelPath);
initWatch.Stop();

Console.WriteLine($"SESSION_INIT_MS={initWatch.Elapsed.TotalMilliseconds:F3}");
Console.WriteLine($"INPUTS={string.Join(",", session.InputMetadata.Select(x => $"{x.Key}:{x.Value.ElementType}:{string.Join("x", x.Value.Dimensions)}"))}");
Console.WriteLine($"OUTPUTS={string.Join(",", session.OutputMetadata.Select(x => $"{x.Key}:{x.Value.ElementType}:{string.Join("x", x.Value.Dimensions)}"))}");

var encoded = examples.Select(text => Encode(tokenizer, text)).ToArray();
var longInput = Encode(tokenizer, string.Join(' ', Enumerable.Repeat("doanh thu", 300)));
Console.WriteLine($"TRUNCATION_TEST|original={longInput.OriginalTokenCount}|used={longInput.Ids.Length}|truncated={longInput.WasTruncated}|lastToken={longInput.Tokens[^1]}");
foreach (var item in encoded)
{
    Console.WriteLine($"TOKENS|{item.Text}|count={item.Ids.Length}|ids={string.Join(",", item.Ids.Take(12))}|mask={string.Join(",", item.AttentionMask.Take(12))}|tokens={string.Join(" ", item.Tokens.Take(12))}");
}

var firstEmbedding = EmbedBatch(session, encoded);
var repeatEmbedding = EmbedBatch(session, encoded);
var maxRepeatDelta = firstEmbedding.Zip(repeatEmbedding, (a, b) => MaxAbsDelta(a, b)).Max();
Console.WriteLine($"VECTOR_DIMENSION={firstEmbedding[0].Length}");
Console.WriteLine($"VECTOR_NORM_FIRST={Norm(firstEmbedding[0]):F9}");
Console.WriteLine($"REPEAT_MAX_ABS_DELTA={maxRepeatDelta:E6}");

var warmup = EmbedBatch(session, encoded[..1]);
_ = warmup;
var singleTimes = new List<double>();
for (var i = 0; i < 20; i++)
{
    var watch = Stopwatch.StartNew();
    _ = EmbedBatch(session, encoded[..1]);
    watch.Stop();
    singleTimes.Add(watch.Elapsed.TotalMilliseconds);
}

var batchTimes = new List<double>();
for (var i = 0; i < 10; i++)
{
    var watch = Stopwatch.StartNew();
    _ = EmbedBatch(session, encoded);
    watch.Stop();
    batchTimes.Add(watch.Elapsed.TotalMilliseconds);
}

Console.WriteLine($"SINGLE_WARM_LATENCY_MS={Stats(singleTimes)}");
Console.WriteLine($"BATCH_{encoded.Length}_WARM_LATENCY_MS={Stats(batchTimes)}");
Console.WriteLine($"PROCESS_RSS_AFTER={Environment.ProcessId}:{Process.GetCurrentProcess().WorkingSet64}");

var similarityRows = new[]
{
    new { A = 0, B = 1, Label = "same-revenue-paraphrase" },
    new { A = 0, B = 2, Label = "revenue-vs-comparison" },
    new { A = 0, B = 3, Label = "revenue-vs-ranking" },
    new { A = 0, B = 7, Label = "revenue-vs-out-of-scope" },
    new { A = 0, B = 8, Label = "revenue-vs-negative" },
    new { A = 5, B = 6, Label = "menu-vs-continuation" }
};
foreach (var row in similarityRows)
{
    Console.WriteLine($"SIM|{row.Label}|{Cosine(firstEmbedding[row.A], firstEmbedding[row.B]):F9}");
}

var result = new
{
    model = Path.GetFileName(modelPath),
    tokenizer = Path.GetFileName(tokenizerPath),
    prefix = "query: ",
    pooling = "attention-mask-aware mean pooling over last_hidden_state, then L2 normalization",
    maxPositionEmbeddings = 512,
    dimension = firstEmbedding[0].Length,
    inputs = encoded.Select(x => new { text = x.Text, tokenCount = x.Ids.Length, ids = x.Ids, attentionMask = x.AttentionMask, tokens = x.Tokens }).ToArray(),
    vectors = firstEmbedding,
    maxRepeatAbsDelta = maxRepeatDelta,
    singleWarmLatencyMs = Stats(singleTimes),
    batchWarmLatencyMs = Stats(batchTimes),
    rssBefore = Process.GetCurrentProcess().WorkingSet64,
    rssAfter = Process.GetCurrentProcess().WorkingSet64
};
await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"RESULT_JSON={outputPath}");
return 0;

static Encoded Encode(Tokenizer tokenizer, string text)
{
    const int maxTokens = 512;
    var input = "query: " + text;
    var encoding = tokenizer.Encode(input, true, includeTypeIds: true, includeTokens: true, includeAttentionMask: true).First();
    var ids = encoding.Ids.Select(x => (long)x).ToArray();
    var mask = encoding.AttentionMask.Select(x => (long)x).ToArray();
    var tokens = encoding.Tokens.ToArray();
    if (ids.Length > maxTokens)
    {
        ids = ids.Take(maxTokens - 1).Append(ids[^1]).ToArray();
        mask = Enumerable.Repeat(1L, maxTokens).ToArray();
        tokens = tokens.Take(maxTokens - 1).Append(tokens[^1]).ToArray();
    }

    return new Encoded(text, ids, mask, tokens, encoding.Ids.Count, encoding.Ids.Count > maxTokens);
}

static float[][] EmbedBatch(InferenceSession session, IReadOnlyList<Encoded> batch)
{
    var maxLength = batch.Max(x => x.Ids.Length);
    var inputIds = new DenseTensor<long>(new int[] { batch.Count, maxLength });
    var typeIds = new DenseTensor<long>(new int[] { batch.Count, maxLength });
    var attentionMask = new DenseTensor<long>(new int[] { batch.Count, maxLength });

    for (var row = 0; row < batch.Count; row++)
    {
        for (var col = 0; col < batch[row].Ids.Length; col++)
        {
            inputIds[row, col] = batch[row].Ids[col];
            typeIds[row, col] = 0;
            attentionMask[row, col] = batch[row].AttentionMask[col];
        }
    }

    using var results = session.Run(new[]
    {
        NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
        NamedOnnxValue.CreateFromTensor("token_type_ids", typeIds),
        NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
    });

    var output = results.First().AsTensor<float>();
    var dimensions = output.Dimensions.ToArray();
    if (dimensions.Length != 3 || dimensions[0] != batch.Count || dimensions[2] != 384)
    {
        throw new InvalidOperationException($"Expected [batch, sequence, 384] output; got [{string.Join(",", dimensions)}].");
    }

    var values = output.ToArray();
    var embeddings = new float[batch.Count][];
    for (var row = 0; row < batch.Count; row++)
    {
        var pooled = new float[dimensions[2]];
        var count = 0f;
        for (var col = 0; col < batch[row].Ids.Length; col++)
        {
            if (batch[row].AttentionMask[col] == 0) continue;
            count++;
            var offset = (row * dimensions[1] + col) * dimensions[2];
            for (var dim = 0; dim < dimensions[2]; dim++) pooled[dim] += values[offset + dim];
        }

        for (var dim = 0; dim < pooled.Length; dim++) pooled[dim] /= count;
        var norm = Norm(pooled);
        for (var dim = 0; dim < pooled.Length; dim++) pooled[dim] /= (float)norm;
        embeddings[row] = pooled;
    }

    return embeddings;
}

static double Norm(IReadOnlyList<float> vector) => Math.Sqrt(vector.Sum(value => value * value));

static double Cosine(IReadOnlyList<float> a, IReadOnlyList<float> b) => a.Zip(b, (x, y) => x * y).Sum();

static double MaxAbsDelta(IReadOnlyList<float> a, IReadOnlyList<float> b) => a.Zip(b, (x, y) => Math.Abs(x - y)).Max();

static string Stats(IReadOnlyList<double> values) =>
    $"n={values.Count},min={values.Min():F3},median={Median(values):F3},p95={Percentile(values, .95):F3},max={values.Max():F3}";

static double Median(IReadOnlyList<double> values) => Percentile(values, .5);

static double Percentile(IReadOnlyList<double> values, double percentile)
{
    var ordered = values.OrderBy(x => x).ToArray();
    var index = (ordered.Length - 1) * percentile;
    var lower = (int)Math.Floor(index);
    var upper = (int)Math.Ceiling(index);
    return lower == upper ? ordered[lower] : ordered[lower] + (ordered[upper] - ordered[lower]) * (index - lower);
}

static string? GetOption(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

sealed record Encoded(string Text, long[] Ids, long[] AttentionMask, string[] Tokens, int OriginalTokenCount, bool WasTruncated);
