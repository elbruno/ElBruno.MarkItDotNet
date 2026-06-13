using System.Text.RegularExpressions;

namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Default implementation of <see cref="ISecurityScanner"/> for Markdown text.
/// </summary>
public sealed partial class MarkdownSecurityScanner : ISecurityScanner
{
    private readonly SecurityScannerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownSecurityScanner"/> class.
    /// </summary>
    /// <param name="options">Optional scanner options.</param>
    public MarkdownSecurityScanner(SecurityScannerOptions? options = null)
    {
        _options = options ?? new SecurityScannerOptions();
    }

    /// <inheritdoc />
    public SecurityScanResult Scan(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var issues = new List<SecurityIssue>();

        if (_options.DetectJavaScriptLinks)
        {
            foreach (Match match in JavaScriptUrlRegex().Matches(markdown))
            {
                AddIssue(issues, new SecurityIssue(
                    "JAVASCRIPT_LINK",
                    SecurityIssueSeverity.Error,
                    "JavaScript URL detected in Markdown link.",
                    match.Index));
            }
        }

        if (_options.DetectSecretLikeTokens)
        {
            foreach (Match match in SecretTokenRegex().Matches(markdown))
            {
                AddIssue(issues, new SecurityIssue(
                    "SECRET_LIKE_TOKEN",
                    SecurityIssueSeverity.Warning,
                    "Potential secret-like token detected. Review before sharing.",
                    match.Index));
            }
        }

        if (_options.DetectControlCharacters)
        {
            for (var i = 0; i < markdown.Length; i++)
            {
                var c = markdown[i];
                if (char.IsControl(c) && c is not ('\n' or '\r' or '\t'))
                {
                    AddIssue(issues, new SecurityIssue(
                        "CONTROL_CHARACTER",
                        SecurityIssueSeverity.Warning,
                        $"Unexpected control character U+{((int)c):X4} detected.",
                        i));

                    if (issues.Count >= _options.MaxIssues)
                    {
                        break;
                    }
                }
            }
        }

        double score = ComputeScore(issues);
        return new SecurityScanResult(score, issues);
    }

    private static double ComputeScore(IReadOnlyCollection<SecurityIssue> issues)
    {
        if (issues.Count == 0)
        {
            return 1.0;
        }

        var penalty = 0.0;
        foreach (var issue in issues)
        {
            penalty += issue.Severity switch
            {
                SecurityIssueSeverity.Info => 0.02,
                SecurityIssueSeverity.Warning => 0.10,
                SecurityIssueSeverity.Error => 0.35,
                _ => 0.0,
            };
        }

        return Math.Clamp(1.0 - penalty, 0.0, 1.0);
    }

    private void AddIssue(List<SecurityIssue> issues, SecurityIssue issue)
    {
        if (issues.Count < _options.MaxIssues)
        {
            issues.Add(issue);
        }
    }

    [GeneratedRegex(@"\]\(\s*javascript\s*:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex JavaScriptUrlRegex();

    [GeneratedRegex(@"\b(?:sk|api|token|key)_[A-Za-z0-9_-]{16,}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SecretTokenRegex();
}
