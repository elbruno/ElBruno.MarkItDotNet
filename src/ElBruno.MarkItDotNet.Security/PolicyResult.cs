namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// The outcome of evaluating content against a single <see cref="ISecurityPolicy"/>
/// or a composed policy chain.
/// </summary>
/// <param name="Passed">True when no violations were found.</param>
/// <param name="Violations">All violations discovered during evaluation.</param>
/// <param name="RedactedContent">
/// The content after redaction has been applied, or <c>null</c> if no redaction occurred.
/// </param>
/// <param name="Metadata">Optional structured metadata emitted by the policy.</param>
public sealed record PolicyResult(
    bool Passed,
    IReadOnlyList<PolicyViolation> Violations,
    string? RedactedContent = null,
    IReadOnlyDictionary<string, object>? Metadata = null)
{
    /// <summary>Creates a passing result with no violations.</summary>
    public static PolicyResult Pass(string? redactedContent = null) =>
        new(true, [], redactedContent);

    /// <summary>Creates a failing result from one or more violations.</summary>
    public static PolicyResult Fail(
        IReadOnlyList<PolicyViolation> violations,
        string? redactedContent = null,
        IReadOnlyDictionary<string, object>? metadata = null) =>
        new(false, violations, redactedContent, metadata);
}
