namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Executes a sequence of <see cref="ISecurityPolicy"/> instances, optionally
/// short-circuiting on the first failure and merging redacted content through the chain.
/// </summary>
public sealed class SecurityPolicyChain : ISecurityPolicy
{
    private readonly IReadOnlyList<ISecurityPolicy> _policies;
    private readonly bool _shortCircuit;

    /// <inheritdoc/>
    public string PolicyName => "SecurityPolicyChain";

    /// <summary>
    /// Creates a policy chain that runs all policies and collects every violation.
    /// </summary>
    /// <param name="policies">Ordered list of policies to evaluate.</param>
    public SecurityPolicyChain(params ISecurityPolicy[] policies)
        : this(shortCircuit: false, policies) { }

    /// <summary>
    /// Creates a policy chain with configurable short-circuit behaviour.
    /// </summary>
    /// <param name="shortCircuit">
    /// When <c>true</c> the chain stops after the first policy that fails.
    /// </param>
    /// <param name="policies">Ordered list of policies to evaluate.</param>
    public SecurityPolicyChain(bool shortCircuit, params ISecurityPolicy[] policies)
    {
        ArgumentNullException.ThrowIfNull(policies);
        _shortCircuit = shortCircuit;
        _policies = policies.ToList();
    }

    /// <summary>
    /// Creates a policy chain from an enumerable of policies.
    /// </summary>
    public SecurityPolicyChain(IEnumerable<ISecurityPolicy> policies, bool shortCircuit = false)
    {
        ArgumentNullException.ThrowIfNull(policies);
        _shortCircuit = shortCircuit;
        _policies = policies.ToList();
    }

    /// <inheritdoc/>
    public async Task<PolicyResult> EvaluateAsync(string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var allViolations = new List<PolicyViolation>();
        var currentContent = content;
        var anyRedaction = false;

        foreach (var policy in _policies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await policy.EvaluateAsync(currentContent, cancellationToken);

            allViolations.AddRange(result.Violations);

            // Thread redacted content through: each subsequent policy sees the already-redacted text
            if (result.RedactedContent is not null)
            {
                currentContent = result.RedactedContent;
                anyRedaction = true;
            }

            if (_shortCircuit && !result.Passed)
                break;
        }

        return allViolations.Count == 0
            ? PolicyResult.Pass(anyRedaction ? currentContent : null)
            : PolicyResult.Fail(
                allViolations,
                redactedContent: anyRedaction ? currentContent : null,
                metadata: new Dictionary<string, object>
                {
                    ["policyCount"] = _policies.Count,
                    ["shortCircuit"] = _shortCircuit
                });
    }

    /// <summary>Gets the number of policies in the chain.</summary>
    public int Count => _policies.Count;
}
