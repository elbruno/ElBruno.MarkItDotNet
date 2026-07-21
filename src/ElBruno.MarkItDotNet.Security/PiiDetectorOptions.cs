namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Configuration for <see cref="PiiDetector"/>.
/// </summary>
public sealed class PiiDetectorOptions
{
    /// <summary>Enable detection of email addresses (default: true).</summary>
    public bool DetectEmails { get; set; } = true;

    /// <summary>Enable detection of phone numbers (default: true).</summary>
    public bool DetectPhones { get; set; } = true;

    /// <summary>Enable detection of US Social Security Numbers (default: true).</summary>
    public bool DetectSsn { get; set; } = true;

    /// <summary>Enable detection of credit card numbers (default: true).</summary>
    public bool DetectCreditCards { get; set; } = true;

    /// <summary>Enable detection of API keys and secret tokens (default: true).</summary>
    public bool DetectApiKeys { get; set; } = true;

    /// <summary>
    /// Additional custom regex patterns to flag as PII violations.
    /// Each entry is matched case-insensitively.
    /// </summary>
    public IList<string> CustomPatterns { get; set; } = [];

    /// <summary>
    /// When true, detected PII is replaced with <see cref="RedactionMask"/>
    /// in <see cref="PolicyResult.RedactedContent"/> (default: true).
    /// </summary>
    public bool EnableRedaction { get; set; } = true;

    /// <summary>Replacement text used when <see cref="EnableRedaction"/> is true (default: [REDACTED]).</summary>
    public string RedactionMask { get; set; } = "[REDACTED]";
}
