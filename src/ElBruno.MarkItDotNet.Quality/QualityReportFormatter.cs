// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using System.Text;

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Provides static methods to format a <see cref="QualityReport"/> as plain text or Markdown.
/// </summary>
public static class QualityReportFormatter
{
    /// <summary>
    /// Formats the quality report as plain text suitable for console output.
    /// </summary>
    /// <param name="report">The quality report to format.</param>
    /// <returns>A plain text representation of the report.</returns>
    public static string FormatAsText(QualityReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Quality Report — Overall Score: {report.OverallScore:F2} ({report.SuggestedAction})");
        sb.AppendLine(new string('-', 60));

        sb.AppendLine("Metrics:");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Text Density:             {report.Metrics.TextDensity:P0}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Average Block Length:      {report.Metrics.AverageBlockLength:F1}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Empty Block Ratio:         {report.Metrics.EmptyBlockRatio:P0}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Duplicate Line Ratio:      {report.Metrics.DuplicateLineRatio:P0}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Table Warning Count:       {report.Metrics.TableWarningCount}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  OCR Suspicion Score:       {report.Metrics.OcrSuspicionScore:F2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Reading Order Score:       {report.Metrics.ReadingOrderScore:F2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Heading Consistency Score: {report.Metrics.HeadingConsistencyScore:F2}");

        if (report.Issues.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Issues ({report.Issues.Count}):");
            foreach (var issue in report.Issues)
            {
                string location = FormatIssueLocation(issue);
                sb.AppendLine(CultureInfo.InvariantCulture, $"  [{issue.Severity}] {issue.Code}: {issue.Message}{location}");
            }
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("No issues found.");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats the quality report as Markdown suitable for documentation or reports.
    /// </summary>
    /// <param name="report">The quality report to format.</param>
    /// <returns>A Markdown representation of the report.</returns>
    public static string FormatAsMarkdown(QualityReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"# Quality Report");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"**Overall Score:** {report.OverallScore:F2} | **Suggested Action:** {report.SuggestedAction}");
        sb.AppendLine();

        sb.AppendLine("## Metrics");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine(CultureInfo.InvariantCulture, $"| Text Density | {report.Metrics.TextDensity:P0} |");
        sb.AppendLine(CultureInfo.InvariantCulture, $"| Average Block Length | {report.Metrics.AverageBlockLength:F1} |");
        sb.AppendLine(CultureInfo.InvariantCulture, $"| Empty Block Ratio | {report.Metrics.EmptyBlockRatio:P0} |");
        sb.AppendLine(CultureInfo.InvariantCulture, $"| Duplicate Line Ratio | {report.Metrics.DuplicateLineRatio:P0} |");
        sb.AppendLine(CultureInfo.InvariantCulture, $"| Table Warning Count | {report.Metrics.TableWarningCount} |");
        sb.AppendLine(CultureInfo.InvariantCulture, $"| OCR Suspicion Score | {report.Metrics.OcrSuspicionScore:F2} |");
        sb.AppendLine(CultureInfo.InvariantCulture, $"| Reading Order Score | {report.Metrics.ReadingOrderScore:F2} |");
        sb.AppendLine(CultureInfo.InvariantCulture, $"| Heading Consistency Score | {report.Metrics.HeadingConsistencyScore:F2} |");

        if (report.Issues.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"## Issues ({report.Issues.Count})");
            sb.AppendLine();
            sb.AppendLine("| Severity | Code | Message | Location |");
            sb.AppendLine("|----------|------|---------|----------|");
            foreach (var issue in report.Issues)
            {
                string location = FormatIssueLocationMarkdown(issue);
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {issue.Severity} | `{issue.Code}` | {issue.Message} | {location} |");
            }
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("## Issues");
            sb.AppendLine();
            sb.AppendLine("No issues found. ✅");
        }

        return sb.ToString();
    }

    private static string FormatIssueLocation(QualityIssue issue)
    {
        if (issue.BlockId is not null && issue.SectionId is not null)
        {
            return $" (section: {issue.SectionId}, block: {issue.BlockId})";
        }

        if (issue.BlockId is not null)
        {
            return $" (block: {issue.BlockId})";
        }

        if (issue.SectionId is not null)
        {
            return $" (section: {issue.SectionId})";
        }

        return string.Empty;
    }

    private static string FormatIssueLocationMarkdown(QualityIssue issue)
    {
        if (issue.BlockId is not null && issue.SectionId is not null)
        {
            return $"Section: `{issue.SectionId}`, Block: `{issue.BlockId}`";
        }

        if (issue.BlockId is not null)
        {
            return $"Block: `{issue.BlockId}`";
        }

        if (issue.SectionId is not null)
        {
            return $"Section: `{issue.SectionId}`";
        }

        return "—";
    }
}
