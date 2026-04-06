// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Quality.Tests;

public class QualityReportFormatterTests
{
    [Fact]
    public void FormatAsText_WithIssues_ContainsExpectedSections()
    {
        var report = CreateSampleReport();

        var text = QualityReportFormatter.FormatAsText(report);

        text.Should().Contain("Quality Report");
        text.Should().Contain("Overall Score:");
        text.Should().Contain("Metrics:");
        text.Should().Contain("Issues (1):");
        text.Should().Contain("EMPTY_BLOCK");
    }

    [Fact]
    public void FormatAsText_NoIssues_SaysNoIssuesFound()
    {
        var report = new QualityReport(
            0.95,
            [],
            new QualityMetrics(0.8, 50.0, 0.0, 0.0, 0, 0.0, 1.0, 1.0),
            QualityAction.None);

        var text = QualityReportFormatter.FormatAsText(report);

        text.Should().Contain("No issues found.");
    }

    [Fact]
    public void FormatAsMarkdown_WithIssues_ContainsTableHeaders()
    {
        var report = CreateSampleReport();

        var markdown = QualityReportFormatter.FormatAsMarkdown(report);

        markdown.Should().Contain("# Quality Report");
        markdown.Should().Contain("## Metrics");
        markdown.Should().Contain("| Metric | Value |");
        markdown.Should().Contain("## Issues (1)");
        markdown.Should().Contain("| Severity | Code | Message | Location |");
        markdown.Should().Contain("`EMPTY_BLOCK`");
    }

    [Fact]
    public void FormatAsMarkdown_NoIssues_ShowsCheckMark()
    {
        var report = new QualityReport(
            0.95,
            [],
            new QualityMetrics(0.8, 50.0, 0.0, 0.0, 0, 0.0, 1.0, 1.0),
            QualityAction.None);

        var markdown = QualityReportFormatter.FormatAsMarkdown(report);

        markdown.Should().Contain("No issues found. ✅");
    }

    [Fact]
    public void FormatAsText_WithBlockId_IncludesLocation()
    {
        var issue = new QualityIssue("TEST", QualitySeverity.Warning, "Test message", BlockId: "b1");
        var report = new QualityReport(
            0.5,
            [issue],
            new QualityMetrics(0.5, 30.0, 0.1, 0.0, 0, 0.0, 1.0, 1.0),
            QualityAction.Review);

        var text = QualityReportFormatter.FormatAsText(report);

        text.Should().Contain("(block: b1)");
    }

    [Fact]
    public void FormatAsText_NullReport_ThrowsArgumentNullException()
    {
        var act = () => QualityReportFormatter.FormatAsText(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FormatAsMarkdown_NullReport_ThrowsArgumentNullException()
    {
        var act = () => QualityReportFormatter.FormatAsMarkdown(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    private static QualityReport CreateSampleReport()
    {
        var issues = new List<QualityIssue>
        {
            new("EMPTY_BLOCK", QualitySeverity.Info, "Empty paragraph block.", BlockId: "p5"),
        };
        var metrics = new QualityMetrics(0.6, 42.5, 0.1, 0.0, 0, 0.0, 0.9, 0.85);
        return new QualityReport(0.78, issues, metrics, QualityAction.None);
    }
}
