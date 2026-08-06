namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Evaluates content against a named security policy and returns a structured result.
/// Policies are composable and can be chained together for multi-stage evaluation.
/// </summary>
public interface ISecurityPolicy
{
    /// <summary>Gets the unique name of this policy.</summary>
    string PolicyName { get; }

    /// <summary>
    /// Evaluates the provided content against this policy.
    /// </summary>
    /// <param name="content">The text content to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PolicyResult"/> describing violations and optionally redacted content.</returns>
    Task<PolicyResult> EvaluateAsync(string content, CancellationToken cancellationToken = default);
}
