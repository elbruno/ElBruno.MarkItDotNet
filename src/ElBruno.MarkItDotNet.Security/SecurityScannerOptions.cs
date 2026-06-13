namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Options that control security scanning behavior.
/// </summary>
public sealed class SecurityScannerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether JavaScript link detection is enabled.
    /// </summary>
    public bool DetectJavaScriptLinks { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether secret-like token detection is enabled.
    /// </summary>
    public bool DetectSecretLikeTokens { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether control character detection is enabled.
    /// </summary>
    public bool DetectControlCharacters { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of issues to emit for a single scan.
    /// </summary>
    public int MaxIssues { get; set; } = 50;
}
