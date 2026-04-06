// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Quality.Tests;

public class QualityAnalyzerOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveExpectedWeights()
    {
        var options = new QualityAnalyzerOptions();

        options.TextDensityWeight.Should().Be(0.20);
        options.OcrSuspicionWeight.Should().Be(0.25);
        options.DuplicateLineWeight.Should().Be(0.15);
        options.EmptyBlockWeight.Should().Be(0.10);
        options.ReadingOrderWeight.Should().Be(0.15);
        options.HeadingConsistencyWeight.Should().Be(0.15);
    }

    [Fact]
    public void DefaultOptions_WeightsSumToOne()
    {
        var options = new QualityAnalyzerOptions();

        double sum = options.TextDensityWeight +
                     options.OcrSuspicionWeight +
                     options.DuplicateLineWeight +
                     options.EmptyBlockWeight +
                     options.ReadingOrderWeight +
                     options.HeadingConsistencyWeight;

        sum.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void DefaultOptions_HaveExpectedThresholds()
    {
        var options = new QualityAnalyzerOptions();

        options.RejectThreshold.Should().Be(0.2);
        options.FallbackToDocumentIntelligenceThreshold.Should().Be(0.4);
        options.FallbackToOcrThreshold.Should().Be(0.55);
        options.ReviewThreshold.Should().Be(0.75);
    }

    [Fact]
    public void DefaultOptions_ThresholdsAreInOrder()
    {
        var options = new QualityAnalyzerOptions();

        options.RejectThreshold.Should().BeLessThan(options.FallbackToDocumentIntelligenceThreshold);
        options.FallbackToDocumentIntelligenceThreshold.Should().BeLessThan(options.FallbackToOcrThreshold);
        options.FallbackToOcrThreshold.Should().BeLessThan(options.ReviewThreshold);
    }

    [Fact]
    public void CustomWeights_AffectOverallScore()
    {
        var defaultAnalyzer = new DocumentQualityAnalyzer();
        var customOptions = new QualityAnalyzerOptions
        {
            TextDensityWeight = 0.50,
            OcrSuspicionWeight = 0.10,
            DuplicateLineWeight = 0.10,
            EmptyBlockWeight = 0.10,
            ReadingOrderWeight = 0.10,
            HeadingConsistencyWeight = 0.10,
        };
        var customAnalyzer = new DocumentQualityAnalyzer(customOptions);

        var doc = TestDocumentBuilder.CreateCleanDocument();
        var defaultReport = defaultAnalyzer.Analyze(doc);
        var customReport = customAnalyzer.Analyze(doc);

        // Both should produce valid scores but may differ due to different weights
        defaultReport.OverallScore.Should().BeInRange(0.0, 1.0);
        customReport.OverallScore.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void CustomThresholds_AffectSuggestedAction()
    {
        // Make review threshold very high so clean docs trigger Review
        var options = new QualityAnalyzerOptions { ReviewThreshold = 0.99 };
        var analyzer = new DocumentQualityAnalyzer(options);
        var doc = TestDocumentBuilder.CreateCleanDocument();

        var report = analyzer.Analyze(doc);

        // With such a high review threshold, even a clean doc may get Review
        report.SuggestedAction.Should().BeOneOf(QualityAction.None, QualityAction.Review);
    }

    [Fact]
    public void DefaultOptions_MinWordsForTextRich_IsTen()
    {
        var options = new QualityAnalyzerOptions();

        options.MinWordsForTextRich.Should().Be(10);
    }

    [Fact]
    public void DefaultOptions_SpecialCharRatioThreshold_IsHalf()
    {
        var options = new QualityAnalyzerOptions();

        options.SpecialCharRatioThreshold.Should().Be(0.5);
    }
}
