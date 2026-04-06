// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Quality.Tests;

public class QualityMetricsTests
{
    private readonly DocumentQualityAnalyzer _analyzer = new();

    [Fact]
    public void TextDensity_AllTextRich_ReturnsHighValue()
    {
        var doc = TestDocumentBuilder.CreateCleanDocument();

        var report = _analyzer.Analyze(doc);

        // Clean doc has 4 paragraphs all with >10 words across 8 blocks (4 paragraphs + 4 headings)
        report.Metrics.TextDensity.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void EmptyBlockRatio_AllEmpty_IsHigh()
    {
        var doc = TestDocumentBuilder.CreateEmptyBlocksDocument();

        var report = _analyzer.Analyze(doc);

        // 3 empty + 1 non-empty = 75% empty
        report.Metrics.EmptyBlockRatio.Should().Be(0.75);
    }

    [Fact]
    public void DuplicateLineRatio_WithDuplicates_IsNonZero()
    {
        var doc = TestDocumentBuilder.CreateDuplicateContentDocument();

        var report = _analyzer.Analyze(doc);

        // 3 identical paragraphs out of 4 non-empty; 2 duplicates / 4 non-empty = 0.5
        report.Metrics.DuplicateLineRatio.Should().Be(0.5);
    }

    [Fact]
    public void OcrSuspicionScore_CleanDocument_IsZero()
    {
        var doc = TestDocumentBuilder.CreateCleanDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.OcrSuspicionScore.Should().Be(0.0);
    }

    [Fact]
    public void ReadingOrderScore_ConsistentHeadings_IsOne()
    {
        var doc = TestDocumentBuilder.CreateCleanDocument();

        var report = _analyzer.Analyze(doc);

        // Clean doc headings: H1 → H2 → H2 (no skips)
        report.Metrics.ReadingOrderScore.Should().Be(1.0);
    }

    [Fact]
    public void ReadingOrderScore_SkippedLevels_IsLessThanOne()
    {
        var doc = TestDocumentBuilder.CreateBrokenHeadingOrderDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.ReadingOrderScore.Should().BeLessThan(1.0);
    }

    [Fact]
    public void AverageBlockLength_NonEmpty_IsPositive()
    {
        var doc = TestDocumentBuilder.CreateCleanDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.AverageBlockLength.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void HeadingConsistencyScore_ConsistentCapitalization_IsOne()
    {
        var doc = TestDocumentBuilder.CreateCleanDocument();

        var report = _analyzer.Analyze(doc);

        // Clean doc: H2 headings are "Background" and "Conclusion" — both Title case
        report.Metrics.HeadingConsistencyScore.Should().Be(1.0);
    }

    [Fact]
    public void TableWarningCount_ProblematicTables_IsPositive()
    {
        var doc = TestDocumentBuilder.CreateProblematicTablesDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.TableWarningCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AllMetrics_EmptyDocument_AreZeroOrDefault()
    {
        var doc = TestDocumentBuilder.CreateEmptyDocument();

        var report = _analyzer.Analyze(doc);

        report.Metrics.TextDensity.Should().Be(0.0);
        report.Metrics.AverageBlockLength.Should().Be(0.0);
        report.Metrics.EmptyBlockRatio.Should().Be(0.0);
        report.Metrics.DuplicateLineRatio.Should().Be(0.0);
        report.Metrics.TableWarningCount.Should().Be(0);
        report.Metrics.OcrSuspicionScore.Should().Be(0.0);
        report.Metrics.ReadingOrderScore.Should().Be(1.0);
        report.Metrics.HeadingConsistencyScore.Should().Be(1.0);
    }
}
