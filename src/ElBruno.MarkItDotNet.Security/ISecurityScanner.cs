namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Scans Markdown content for security-relevant patterns.
/// </summary>
public interface ISecurityScanner
{
    /// <summary>
    /// Scans the provided Markdown content and returns a structured report.
    /// </summary>
    /// <param name="markdown">Markdown content to scan.</param>
    /// <returns>A <see cref="SecurityScanResult"/> containing issues and an overall score.</returns>
    SecurityScanResult Scan(string markdown);
}
