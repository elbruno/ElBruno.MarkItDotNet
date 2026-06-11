namespace MarkItDotNet.FoundryHostedAgent;

public sealed record InvocationEnvelope(InvocationInput Input);

public sealed record InvocationInput(
    string? FileName,
    string Extension,
    string ContentBase64);

public sealed record InvocationResponse(InvocationOutput Output);

public sealed record InvocationOutput(
    string FileName,
    string Extension,
    string Markdown);
