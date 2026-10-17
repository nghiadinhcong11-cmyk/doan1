using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace MockAiEmbeddingSpike;

public sealed record CalibrationCase(string Id, string Text, string ExpectedIntent, string Category);

public static class CalibrationRunner
{
    public static async Task<int> RunAsync(string[] args)
    {
        var artifactRoot = GetOption(args, "--artifact-root") ?? "artifacts";
        var modelPath = GetOption(args, "--model") ?? Path.Combine(artifactRoot, "onnx", "model_O4.onnx");
        var tokenizerPath = GetOption(args, "--tokenizer") ?? Path.Combine(artifactRoot, "onnx", "tokenizer.json");
        var calibrationPath = GetOption(args, "--calibration") ?? "calibration.json";
        var outputPath = GetOption(args, "--calibration-output") ?? Path.Combine(artifactRoot, "calibration-results.json");
        var minimumScore = ParseDouble(args, "--minimum-score", 0.85);
        var minimumMargin = ParseDouble(args, "--minimum-margin", 0.02);

        if (!File.Exists(calibrationPath))
        {
            Console.Error.WriteLine($"Calibration data was not found: {calibrationPath}");
            return 2;
        }

        var cases = JsonSerializer.Deserialize<List<CalibrationCase>>(await File.ReadAllTextAsync(calibrationPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Calibration data is empty or invalid.");
        if (cases.Any(item => string.IsNullOrWhiteSpace(item.Text) || string.IsNullOrWhiteSpace(item.ExpectedIntent)))
            throw new InvalidDataException("Calibration data contains a case with missing text or expected intent.");
        var options = new SentenceEmbeddingOptions(modelPath, tokenizerPath, MaxConcurrency: 1, QueueCapacity: 2);
        Console.WriteLine($"CALIBRATION_CASES={cases.Count}");
        Console.WriteLine($"EXPERIMENTAL_THRESHOLD={minimumScore:F3}|EXPERIMENTAL_MARGIN={minimumMargin:F3}|NOT_PRODUCTION_POLICY=true");

        var rssBefore = Process.GetCurrentProcess().WorkingSet64;
        var initWatch = Stopwatch.StartNew();
        using var adapter = new SentenceEmbeddingAdapter(options);
        initWatch.Stop();
        var catalogTexts = IntentCatalog.Examples.Select(x => x.Text).ToArray();
        var catalogVectors = await adapter.EmbedBatchAsync(catalogTexts);
        var vectors = IntentCatalog.Examples.Select((example, index) => (example.Id, Vector: catalogVectors[index]))
            .ToDictionary(x => x.Id, x => x.Vector);
        Console.WriteLine($"CATALOG_SELF_COSINE={catalogVectors[0].Zip(catalogVectors[0], (a, b) => a * b).Sum():F6}");
        var scorer = new IntentScorer(IntentCatalog.Examples, vectors);

        var inputWatch = Stopwatch.StartNew();
        var inputVectors = await adapter.EmbedBatchAsync(cases.Select(x => x.Text).ToArray());
        inputWatch.Stop();
        var results = cases.Select((item, index) =>
        {
            var recognition = scorer.Score(inputVectors[index], minimumScore, minimumMargin);
            var predicted = recognition.Top?.IntentId ?? "NONE";
            var correct = predicted == item.ExpectedIntent;
            Console.WriteLine($"CAL|{item.Id}|expected={item.ExpectedIntent}|predicted={predicted}|score={recognition.Top?.Score:F6}|margin={recognition.Margin:F6}|second={recognition.Second?.IntentId}|status={recognition.Status}|correct={correct}");
            return new
            {
                item.Id, item.Text, item.ExpectedIntent, item.Category,
                predictedIntent = predicted,
                topScore = recognition.Top?.Score ?? 0,
                margin = recognition.Margin,
                secondIntent = recognition.Second?.IntentId,
                status = recognition.Status.ToString(),
                correct
            };
        }).ToArray();

        // Multiple callers are deliberately submitted to verify bounded admission. The adapter's
        // limit is one because tokenizer thread safety is not assumed; capacity is 1 running + 2 waiting.
        var concurrentWatch = Stopwatch.StartNew();
        var loadTasks = Enumerable.Range(0, 12).Select(_ => Task.Run(async () =>
        {
            try
            {
                await adapter.EmbedAsync(string.Join(' ', Enumerable.Repeat("đo tài nguyên", 120)));
                return "completed";
            }
            catch (EmbeddingBusyException) { return "busy"; }
            catch (OperationCanceledException) { return "cancelled"; }
        })).ToArray();
        var loadOutcomes = await Task.WhenAll(loadTasks);
        concurrentWatch.Stop();
        using var alreadyCancelled = new CancellationTokenSource();
        alreadyCancelled.Cancel();
        var cancellationObserved = false;
        try { await adapter.EmbedAsync("cancel trước khi chạy", alreadyCancelled.Token); }
        catch (OperationCanceledException) { cancellationObserved = true; }
        var rssAfter = Process.GetCurrentProcess().WorkingSet64;

        var correctCount = results.Count(x => x.correct);
        Console.WriteLine($"INIT_MS={initWatch.Elapsed.TotalMilliseconds:F3}");
        Console.WriteLine($"CALIBRATION_BATCH_MS={inputWatch.Elapsed.TotalMilliseconds:F3}");
        Console.WriteLine($"CONCURRENT_TASKS={loadTasks.Length}|MAX_CONCURRENCY={adapter.MaxConcurrency}|QUEUE_CAPACITY={adapter.QueueCapacity}|COMPLETED={loadOutcomes.Count(x => x == "completed")}|BUSY={loadOutcomes.Count(x => x == "busy")}|ELAPSED_MS={concurrentWatch.Elapsed.TotalMilliseconds:F3}");
        Console.WriteLine($"CANCELLATION_BEFORE_INFERENCE_OBSERVED={cancellationObserved}");
        Console.WriteLine($"PROCESS_RSS_BEFORE={rssBefore}|PROCESS_RSS_AFTER={rssAfter}|DELTA={rssAfter - rssBefore}");
        Console.WriteLine($"CALIBRATION_EXACT_INTENT_MATCH={correctCount}/{results.Length}");
        Console.WriteLine("NOTE=Calibration is component evidence only; no permission, parser, tool or chat flow was called.");

        var report = new
        {
            source = Path.GetFileName(calibrationPath),
            catalogCount = IntentCatalog.Examples.Count,
            experimentalThreshold = minimumScore,
            experimentalMargin = minimumMargin,
            maxConcurrency = adapter.MaxConcurrency,
            initializationMs = initWatch.Elapsed.TotalMilliseconds,
            calibrationBatchMs = inputWatch.Elapsed.TotalMilliseconds,
            concurrentTasks = loadTasks.Length,
            queueCapacity = adapter.QueueCapacity,
            completedTasks = loadOutcomes.Count(x => x == "completed"),
            busyTasks = loadOutcomes.Count(x => x == "busy"),
            cancellationBeforeInferenceObserved = cancellationObserved,
            concurrentElapsedMs = concurrentWatch.Elapsed.TotalMilliseconds,
            rssBefore,
            rssAfter,
            results
        };
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"CALIBRATION_JSON={outputPath}");
        return 0;
    }

    private static string? GetOption(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static double ParseDouble(string[] args, string name, double fallback) =>
        double.TryParse(GetOption(args, name), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : fallback;
}
