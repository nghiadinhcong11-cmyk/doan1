using System.Diagnostics;
using System.Text.Json;

namespace MockAiEmbeddingSpike;

/// <summary>Small local lifecycle checks with real embedding calls; no chat/API/business execution.</summary>
public static class AdapterLifecycleChecks
{
    public static async Task<int> RunAsync()
    {
        var observations = new Dictionary<string, bool>();
        var options = new SentenceEmbeddingOptions("artifacts/onnx/model_O4.onnx", "artifacts/onnx/tokenizer.json", QueueCapacity: 2);
        using var adapter = new SentenceEmbeddingAdapter(options);
        var input = string.Join(' ', Enumerable.Repeat("kiểm tra vòng đời tài nguyên", 160));
        var batch = Enumerable.Repeat(input, 24).ToArray();
        async Task WaitForNativeWork()
        {
            var timeout = Stopwatch.StartNew();
            while (!adapter.IsInferenceRunning)
            {
                if (timeout.Elapsed > TimeSpan.FromSeconds(10)) throw new TimeoutException("Inference did not start within check timeout.");
                await Task.Delay(1);
            }
        }

        var running = Task.Run(() => adapter.EmbedBatchAsync(batch));
        await WaitForNativeWork();
        using var waitingCancellation = new CancellationTokenSource();
        var waiting = adapter.EmbedAsync("câu đang chờ", waitingCancellation.Token);
        var waitingSecond = adapter.EmbedAsync("câu chờ thứ hai");
        try { await adapter.EmbedAsync("câu vượt giới hạn"); observations["busyWhenFull"] = false; }
        catch (EmbeddingBusyException) { observations["busyWhenFull"] = true; }
        waitingCancellation.Cancel();
        try { await waiting; observations["cancelWaiting"] = false; }
        catch (OperationCanceledException) { observations["cancelWaiting"] = true; }
        await running;
        await waitingSecond;
        var recovery = await adapter.EmbedAsync("câu sau hủy");
        observations["slotRecoveredAfterCancellation"] = recovery.All(float.IsFinite) && Math.Abs(recovery.Sum(x => x * x) - 1) < .001;

        using var nativeCancellation = new CancellationTokenSource();
        var native = Task.Run(() => adapter.EmbedBatchAsync(batch, nativeCancellation.Token));
        await WaitForNativeWork();
        nativeCancellation.Cancel();
        try { await native; observations["cancelResultAfterNativeWork"] = false; }
        catch (OperationCanceledException) { observations["cancelResultAfterNativeWork"] = true; }
        observations["slotRecoveredAfterNativeWork"] = (await adapter.EmbedAsync("câu sau native")).All(float.IsFinite);

        var duringDispose = Task.Run(() => adapter.EmbedBatchAsync(batch));
        await WaitForNativeWork();
        adapter.Dispose();
        observations["activeRequestSurvivesDispose"] = (await duringDispose).Length == batch.Length;
        try { await adapter.EmbedBatchAsync(Array.Empty<string>()); observations["rejectAfterDisposeIncludingEmpty"] = false; }
        catch (ObjectDisposedException) { observations["rejectAfterDisposeIncludingEmpty"] = true; }
        Console.WriteLine(JsonSerializer.Serialize(observations));
        await File.WriteAllTextAsync("artifacts/adapter-lifecycle-checks.json", JsonSerializer.Serialize(observations, new JsonSerializerOptions { WriteIndented = true }));
        return observations.Values.All(value => value) ? 0 : 1;
    }
}
