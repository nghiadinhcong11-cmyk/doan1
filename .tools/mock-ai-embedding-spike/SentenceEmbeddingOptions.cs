namespace MockAiEmbeddingSpike;

public sealed record SentenceEmbeddingOptions(
    string ModelPath,
    string TokenizerPath,
    string Prefix = "query: ",
    int MaxTokens = 512,
    int MaxConcurrency = 1,
    int QueueCapacity = 4);
