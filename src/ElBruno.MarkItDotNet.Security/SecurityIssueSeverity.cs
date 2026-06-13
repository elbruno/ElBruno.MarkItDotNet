namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Indicates the severity level of a security issue found in content.
/// </summary>
public enum SecurityIssueSeverity
{
    /// <summary>
    /// Informational issue with low impact.
    /// </summary>
    Info,

    /// <summary>
    /// Warning issue that may require review.
    /// </summary>
    Warning,

    /// <summary>
    /// Error-level issue that should block ingestion or publishing.
    /// </summary>
    Error,
}
