namespace MarkItDotNet.FoundryHostedAgent.WebUi;

public sealed record AgentConversionResult(
    bool Success,
    string? Markdown,
    string? ErrorMessage,
    string? FileName,
    string? Extension,
    int? StatusCode = null);
