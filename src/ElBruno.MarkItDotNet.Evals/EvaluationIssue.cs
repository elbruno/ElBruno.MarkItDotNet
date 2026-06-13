namespace ElBruno.MarkItDotNet.Evals;

/// <summary>
/// Represents a single finding from conversion evaluation.
/// </summary>
/// <param name="Code">Stable issue code.</param>
/// <param name="Severity">Issue severity.</param>
/// <param name="Message">Issue description.</param>
public sealed record EvaluationIssue(string Code, EvaluationIssueSeverity Severity, string Message);
