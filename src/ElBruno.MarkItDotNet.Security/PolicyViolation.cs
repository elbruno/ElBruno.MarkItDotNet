namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// A single rule violation discovered during policy evaluation.
/// </summary>
/// <param name="RuleName">Machine-readable rule identifier (e.g. <c>PII_EMAIL</c>).</param>
/// <param name="Message">Human-readable description of the violation.</param>
/// <param name="LineNumber">1-based line number where the violation was detected, or 0 if not applicable.</param>
/// <param name="SuggestedAction">Optional remediation hint.</param>
public sealed record PolicyViolation(
    string RuleName,
    string Message,
    int LineNumber = 0,
    string? SuggestedAction = null);
