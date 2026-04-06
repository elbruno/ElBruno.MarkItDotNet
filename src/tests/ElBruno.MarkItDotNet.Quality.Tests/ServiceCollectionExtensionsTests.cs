// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Quality.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkItDotNetQuality_RegistersQualityAnalyzerOptions()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetQuality();

        using var provider = services.BuildServiceProvider();
        provider.GetService<QualityAnalyzerOptions>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetQuality_RegistersIQualityAnalyzer()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetQuality();

        using var provider = services.BuildServiceProvider();
        provider.GetService<IQualityAnalyzer>().Should().NotBeNull()
            .And.BeOfType<DocumentQualityAnalyzer>();
    }

    [Fact]
    public void AddMarkItDotNetQuality_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetQuality(options =>
        {
            options.MinWordsForTextRich = 100;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<QualityAnalyzerOptions>();
        options.MinWordsForTextRich.Should().Be(100);
    }

    [Fact]
    public void AddMarkItDotNetQuality_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetQuality();

        result.Should().BeSameAs(services);
    }
}
