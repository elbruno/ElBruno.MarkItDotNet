namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Configuration for <see cref="GuardrailsPolicy"/>.
/// </summary>
public sealed class GuardrailsPolicyOptions
{
    /// <summary>Maximum allowed content length in characters (default: 10,000,000).</summary>
    public int MaxContentLength { get; set; } = 10_000_000;

    /// <summary>
    /// Maximum allowed source file size in bytes (default: 100 MB).
    /// Used when <see cref="GuardrailsPolicy.EvaluateAsync"/> is called with metadata.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100_000_000;

    /// <summary>Maximum allowed number of lines in the content (default: 500,000).</summary>
    public int MaxLineCount { get; set; } = 500_000;
}
