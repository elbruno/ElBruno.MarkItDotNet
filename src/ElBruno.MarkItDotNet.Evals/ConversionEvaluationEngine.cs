using System.Text.RegularExpressions;
using ElBruno.MarkItDotNet;

namespace ElBruno.MarkItDotNet.Evals;

/// <summary>
/// Basic heuristic evaluation engine for conversion output.
/// </summary>
public sealed partial class ConversionEvaluationEngine : IEvaluationEngine
{
    private readonly EvaluationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionEvaluationEngine"/> class.
    /// </summary>
    /// <param name="options">Optional options.</param>
    public ConversionEvaluationEngine(EvaluationOptions? options = null)
    {
        _options = options ?? new EvaluationOptions();
    }

    /// <inheritdoc />
    public EvaluationReport Evaluate(ConversionResult result, string? sourceText = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        var issues = new List<EvaluationIssue>();
        var metrics = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        if (!result.Success)
        {
            issues.Add(new EvaluationIssue("CONVERSION_FAILED", EvaluationIssueSeverity.Error, result.ErrorMessage ?? "Conversion failed."));
            metrics["success"] = 0;
            metrics["contentLength"] = 0;
            metrics["headingDensity"] = 0;
            metrics["retentionRatio"] = 0;
            return new EvaluationReport(0.0, issues, metrics);
        }

        var markdown = result.Markdown ?? string.Empty;
        var contentLength = markdown.Trim().Length;
        var headingCount = HeadingRegex().Matches(markdown).Count;
        var lineCount = Math.Max(1, markdown.Split('\n').Length);

        var headingDensity = Math.Clamp((double)headingCount / lineCount * 8.0, 0.0, 1.0);
        var lengthScore = contentLength switch
        {
            0 => 0.0,
            < 60 => 0.35,
            < 200 => 0.65,
            _ => 1.0,
        };

        var retentionRatio = 1.0;
        if (!string.IsNullOrWhiteSpace(sourceText))
        {
            retentionRatio = ComputeTokenRetentionRatio(sourceText, markdown);
            if (retentionRatio < 0.35)
            {
                issues.Add(new EvaluationIssue(
                    "LOW_RETENTION",
                    EvaluationIssueSeverity.Warning,
                    $"Estimated token retention is low ({retentionRatio:P0})."));
            }
        }

        if (contentLength == 0)
        {
            issues.Add(new EvaluationIssue("EMPTY_MARKDOWN", EvaluationIssueSeverity.Error, "Converted Markdown is empty."));
        }

        if (headingCount == 0)
        {
            issues.Add(new EvaluationIssue("NO_HEADINGS", EvaluationIssueSeverity.Info, "No Markdown headings were detected."));
        }

        var score = (0.45 * lengthScore) + (0.20 * headingDensity) + (0.35 * retentionRatio);
        score = Math.Clamp(score, 0.0, 1.0);

        metrics["success"] = 1;
        metrics["contentLength"] = contentLength;
        metrics["headingDensity"] = headingDensity;
        metrics["retentionRatio"] = retentionRatio;
        metrics["passThreshold"] = _options.PassThreshold;

        return new EvaluationReport(score, issues, metrics);
    }

    private static double ComputeTokenRetentionRatio(string sourceText, string markdown)
    {
        static HashSet<string> Tokens(string text) =>
            [.. TokenRegex().Matches(text).Select(m => m.Value.ToLowerInvariant())];

        var sourceTokens = Tokens(sourceText);
        if (sourceTokens.Count == 0)
        {
            return 1.0;
        }

        var markdownTokens = Tokens(markdown);
        var intersection = sourceTokens.Intersect(markdownTokens).Count();
        return Math.Clamp((double)intersection / sourceTokens.Count, 0.0, 1.0);
    }

    [GeneratedRegex(@"^\s{0,3}#{1,6}\s+", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"[A-Za-z0-9][A-Za-z0-9_-]{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();
}
