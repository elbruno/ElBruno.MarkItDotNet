namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Represents the result of scanning Markdown content for security issues.
/// </summary>
/// <param name="Score">Overall security score in range [0, 1], where 1 is best.</param>
/// <param name="Issues">Detected issues.</param>
public sealed record SecurityScanResult(double Score, IReadOnlyList<SecurityIssue> Issues)
{
    /// <summary>
    /// Gets a value indicating whether the scan is considered safe for processing.
    /// </summary>
    public bool IsSafe => Issues.All(i => i.Severity is not SecurityIssueSeverity.Error);
}
