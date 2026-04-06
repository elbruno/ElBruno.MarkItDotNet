// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Quality.Tests;

public class DocumentQualityAnalyzerTests
{
    private readonly DocumentQualityAnalyzer _analyzer = new();

    [Fact]
    public void Analyze_CleanDocument_ReturnsHighScore()
    {
        var doc = TestDocumentBuilder.CreateCleanDocument();

        var report = _analyzer.Analyze(doc);

        report.OverallScore.Should().BeGreaterThanOrEqualTo(0.75);
        report.SuggestedAction.Should().Be(QualityAction.None);
        report.Issues.Should().NotContain(i => i.Severity == QualitySeverity.Error);
    }

    [Fact]
    public void Analyze_GarbledOcrDocument_DetectsOcrSuspicion()
    {
        var doc = TestDocumentBuilder.CreateGarbledOcrDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.OcrSuspicionScore.Should().BeGreaterThan(0.0);
        report.Issues.Should().Contain(i => i.Code == "OCR_GARBLED");
    }

    [Fact]
    public void Analyze_DuplicateContentDocument_DetectsDuplicates()
    {
        var doc = TestDocumentBuilder.CreateDuplicateContentDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.DuplicateLineRatio.Should().BeGreaterThan(0.0);
        report.Issues.Should().Contain(i => i.Code == "DUPLICATE_LINE");
    }

    [Fact]
    public void Analyze_EmptyBlocksDocument_DetectsEmptyBlocks()
    {
        var doc = TestDocumentBuilder.CreateEmptyBlocksDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.EmptyBlockRatio.Should().BeGreaterThan(0.0);
        report.Issues.Should().Contain(i => i.Code == "EMPTY_BLOCK");
    }

    [Fact]
    public void Analyze_BrokenHeadingOrder_DetectsSkippedLevels()
    {
        var doc = TestDocumentBuilder.CreateBrokenHeadingOrderDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.ReadingOrderScore.Should().BeLessThan(1.0);
        report.Issues.Should().Contain(i => i.Code == "HEADING_ORDER_SKIP");
    }

    [Fact]
    public void Analyze_InconsistentHeadings_DetectsInconsistency()
    {
        var doc = TestDocumentBuilder.CreateInconsistentHeadingsDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.HeadingConsistencyScore.Should().BeLessThan(1.0);
        report.Issues.Should().Contain(i => i.Code == "HEADING_INCONSISTENT_CAPS");
    }

    [Fact]
    public void Analyze_ProblematicTables_DetectsTableIssues()
    {
        var doc = TestDocumentBuilder.CreateProblematicTablesDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.TableWarningCount.Should().BeGreaterThan(0);
        report.Issues.Should().Contain(i =>
            i.Code == "TABLE_ALL_EMPTY" || i.Code == "TABLE_SINGLE_COLUMN" || i.Code == "TABLE_ROW_MISMATCH");
    }

    [Fact]
    public void Analyze_EmptyDocument_HasZeroTextDensityAndNoBlocksIssue()
    {
        var doc = TestDocumentBuilder.CreateEmptyDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.TextDensity.Should().Be(0.0);
        report.Metrics.AverageBlockLength.Should().Be(0.0);
        report.Issues.Should().Contain(i => i.Code == "NO_BLOCKS");
    }

    [Fact]
    public void Analyze_NullDocument_ThrowsArgumentNullException()
    {
        var act = () => _analyzer.Analyze(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Analyze_OverallScoreIsClampedBetweenZeroAndOne()
    {
        var doc = TestDocumentBuilder.CreateCleanDocument();

        var report = _analyzer.Analyze(doc);

        report.OverallScore.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void Analyze_GarbledDocument_ScoresLowerThanCleanDocument()
    {
        var garbled = TestDocumentBuilder.CreateGarbledOcrDocument();
        var clean = TestDocumentBuilder.CreateCleanDocument();

        var garbledReport = _analyzer.Analyze(garbled);
        var cleanReport = _analyzer.Analyze(clean);

        garbledReport.OverallScore.Should().BeLessThan(cleanReport.OverallScore);
    }
}
