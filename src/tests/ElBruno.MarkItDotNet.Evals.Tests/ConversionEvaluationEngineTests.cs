using FluentAssertions;
using ElBruno.MarkItDotNet;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Evals.Tests;

public class ConversionEvaluationEngineTests
{
    [Fact]
    public void Evaluate_FailedConversion_ReturnsZeroScoreAndErrorIssue()
    {
        var engine = new ConversionEvaluationEngine();
        var failed = ConversionResult.Failure("Boom", ".pdf");

        var report = engine.Evaluate(failed);

        report.Score.Should().Be(0.0);
        report.Issues.Should().Contain(i => i.Code == "CONVERSION_FAILED" && i.Severity == EvaluationIssueSeverity.Error);
    }

    [Fact]
    public void Evaluate_GoodMarkdown_ReturnsPassingScore()
    {
        var engine = new ConversionEvaluationEngine();
        var ok = ConversionResult.Succeeded("# Heading\n\nSome converted content with enough detail.", ".txt");

        var report = engine.Evaluate(ok, "Heading Some converted content with enough detail");

        report.Score.Should().BeGreaterThan(0.70);
        report.Passes(0.70).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_EmptyMarkdown_ReturnsErrorIssue()
    {
        var engine = new ConversionEvaluationEngine();
        var empty = ConversionResult.Succeeded(string.Empty, ".txt");

        var report = engine.Evaluate(empty);

        report.Issues.Should().Contain(i => i.Code == "EMPTY_MARKDOWN" && i.Severity == EvaluationIssueSeverity.Error);
    }

    [Fact]
    public void AddMarkItDotNetEvals_RegistersEngineAndOptions()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetEvals(options => options.PassThreshold = 0.85);

        using var provider = services.BuildServiceProvider();
        provider.GetService<IEvaluationEngine>().Should().NotBeNull().And.BeOfType<ConversionEvaluationEngine>();
        provider.GetRequiredService<EvaluationOptions>().PassThreshold.Should().Be(0.85);
    }

    [Fact]
    public void AddMarkItDotNetEvals_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetEvals();

        result.Should().BeSameAs(services);
    }
}
