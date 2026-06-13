namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Represents a single security issue identified by the scanner.
/// </summary>
/// <param name="Code">Stable issue code.</param>
/// <param name="Severity">Issue severity.</param>
/// <param name="Message">Human-readable description.</param>
/// <param name="Offset">Optional character offset where the issue was detected.</param>
public sealed record SecurityIssue(string Code, SecurityIssueSeverity Severity, string Message, int? Offset = null);
