// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;
using ElBruno.MarkItDotNet.Quality;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Integration.Tests;

/// <summary>
/// Tests quality gates that control whether a document proceeds through the pipeline.
/// </summary>
public class QualityGatedPipelineTests
{
    [Fact]
    public void HighQualityDocument_PassesQualityGate()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var analyzer = new DocumentQualityAnalyzer();

        var report = analyzer.Analyze(document);

        report.OverallScore.Should().BeGreaterThanOrEqualTo(0.7,
            "a well-structured report should have a quality score >= 0.7");
        report.SuggestedAction.Should().BeOneOf(QualityAction.None, QualityAction.Review);
        report.Metrics.TextDensity.Should().BeGreaterThan(0);
        report.Metrics.ReadingOrderScore.Should().BeGreaterThanOrEqualTo(0.5);
    }

    [Fact]
    public void LowQualityDocument_FailsQualityGate()
    {
        var document = TestDocumentFactory.CreateLowQualityDocument();
        var analyzer = new DocumentQualityAnalyzer();

        var report = analyzer.Analyze(document);

        report.OverallScore.Should().BeLessThan(0.7,
            "a garbled document should have a low quality score");
        report.SuggestedAction.Should().NotBe(QualityAction.None,
            "a low-quality document should require fallback or rejection");
    }

    [Fact]
    public void LowQualityDocument_ReportsIssuesWithSeverity()
    {
        var document = TestDocumentFactory.CreateLowQualityDocument();
        var analyzer = new DocumentQualityAnalyzer();

        var report = analyzer.Analyze(document);

        report.Issues.Should().NotBeEmpty("garbled text and empty blocks should produce issues");

        var issuesBySeverity = report.Issues.GroupBy(i => i.Severity).ToDictionary(g => g.Key, g => g.Count());

        // Should have at least some warnings or errors
        var warningsAndErrors = report.Issues.Count(i =>
            i.Severity is QualitySeverity.Warning or QualitySeverity.Error);
        warningsAndErrors.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LowQualityDocument_HasOcrRelatedIssues()
    {
        var document = TestDocumentFactory.CreateLowQualityDocument();
        var analyzer = new DocumentQualityAnalyzer();

        var report = analyzer.Analyze(document);

        var ocrIssues = report.Issues.Where(i =>
            i.Code.Contains("OCR", StringComparison.OrdinalIgnoreCase) ||
            i.Code.Contains("GARBLED", StringComparison.OrdinalIgnoreCase)).ToList();

        // The garbled text should trigger OCR suspicion
        report.Metrics.OcrSuspicionScore.Should().BeGreaterThan(0,
            "garbled text should produce a non-zero OCR suspicion score");
    }

    [Fact]
    public void EmptyBlocksInLowQuality_AreReported()
    {
        var document = TestDocumentFactory.CreateLowQualityDocument();
        var analyzer = new DocumentQualityAnalyzer();

        var report = analyzer.Analyze(document);

        var emptyBlockIssues = report.Issues.Where(i => i.Code == "EMPTY_BLOCK").ToList();
        emptyBlockIssues.Should().NotBeEmpty("the low-quality document has empty paragraph blocks");
    }

    [Fact]
    public void QualityReport_MetricsArePopulated()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var analyzer = new DocumentQualityAnalyzer();

        var report = analyzer.Analyze(document);

        report.Metrics.TextDensity.Should().BeInRange(0.0, 1.0);
        report.Metrics.AverageBlockLength.Should().BeGreaterThan(0);
        report.Metrics.EmptyBlockRatio.Should().BeInRange(0.0, 1.0);
        report.Metrics.DuplicateLineRatio.Should().BeInRange(0.0, 1.0);
        report.Metrics.OcrSuspicionScore.Should().BeInRange(0.0, 1.0);
        report.Metrics.ReadingOrderScore.Should().BeInRange(0.0, 1.0);
        report.Metrics.HeadingConsistencyScore.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void SimpleDocument_HasAcceptableQuality()
    {
        var document = TestDocumentFactory.CreateSimpleDocument();
        var analyzer = new DocumentQualityAnalyzer();

        var report = analyzer.Analyze(document);

        report.OverallScore.Should().BeGreaterThanOrEqualTo(0.5);
        report.SuggestedAction.Should().BeOneOf(QualityAction.None, QualityAction.Review);
    }

    [Fact]
    public void QualityGate_ConditionallyAllowsPipeline()
    {
        var highQualityDoc = TestDocumentFactory.CreateRealisticReport();
        var lowQualityDoc = TestDocumentFactory.CreateLowQualityDocument();
        var analyzer = new DocumentQualityAnalyzer();

        var highReport = analyzer.Analyze(highQualityDoc);
        var lowReport = analyzer.Analyze(lowQualityDoc);

        // The high-quality document should score significantly higher
        highReport.OverallScore.Should().BeGreaterThan(lowReport.OverallScore,
            "a well-structured report should score higher than garbled text");

        // Verify the quality gap is meaningful
        (highReport.OverallScore - lowReport.OverallScore).Should().BeGreaterThan(0.1,
            "there should be a meaningful quality difference between good and bad documents");
    }

    [Fact]
    public void HeadingOrderViolation_ReportsWarning()
    {
        // Create a document with heading level skip (H1 → H4)
        var document = new Document
        {
            Id = DocumentIdGenerator.ForDocument("test/heading-skip.md", "Heading Skip"),
            Metadata = new DocumentMetadata { Title = "Heading Skip" },
            Sections =
            [
                new DocumentSection
                {
                    Heading = new HeadingBlock
                    {
                        Id = "h1",
                        Text = "Top Level",
                        Level = 1,
                    },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = "p1",
                            Text = "This is a well-formed paragraph with enough words to be considered text-rich for quality analysis purposes.",
                        },
                    ],
                    SubSections =
                    [
                        new DocumentSection
                        {
                            Heading = new HeadingBlock
                            {
                                Id = "h4",
                                Text = "Deeply Nested",
                                Level = 4,
                            },
                            Blocks =
                            [
                                new ParagraphBlock
                                {
                                    Id = "p2",
                                    Text = "Content under a heading that skips levels, which is a reading order violation detected by the quality analyzer.",
                                },
                            ],
                            SubSections = [],
                        },
                    ],
                },
            ],
        };

        var analyzer = new DocumentQualityAnalyzer();
        var report = analyzer.Analyze(document);

        var headingOrderIssues = report.Issues.Where(i => i.Code == "HEADING_ORDER_SKIP").ToList();
        headingOrderIssues.Should().NotBeEmpty("skipping heading levels should be flagged");
    }
}
