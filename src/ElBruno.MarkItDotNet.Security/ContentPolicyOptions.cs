namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Configuration for <see cref="ContentPolicyEngine"/>.
/// </summary>
public sealed class ContentPolicyOptions
{
    /// <summary>
    /// Keywords that cause an immediate violation when found in content (case-insensitive).
    /// </summary>
    public IList<string> DenyKeywords { get; set; } = [];

    /// <summary>
    /// Keywords that are always allowed; content that contains ONLY these keywords
    /// and no deny keywords will pass (informational — does not override deny rules).
    /// </summary>
    public IList<string> AllowKeywords { get; set; } = [];

    /// <summary>
    /// Domain names (e.g. <c>example.com</c>) that are blocked when referenced in links or URLs.
    /// Matching is case-insensitive substring match.
    /// </summary>
    public IList<string> BlockedDomains { get; set; } = [];

    /// <summary>
    /// When <c>true</c>, the check stops after the first violation (default: false — collect all).
    /// </summary>
    public bool ShortCircuit { get; set; } = false;
}
