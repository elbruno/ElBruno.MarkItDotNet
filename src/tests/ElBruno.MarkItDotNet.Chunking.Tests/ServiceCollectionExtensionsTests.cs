// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Chunking.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkItDotNetChunking_RegistersChunkingOptions()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetChunking();

        using var provider = services.BuildServiceProvider();
        provider.GetService<ChunkingOptions>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetChunking_RegistersIChunkingStrategy()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetChunking();

        using var provider = services.BuildServiceProvider();
        provider.GetService<IChunkingStrategy>().Should().NotBeNull()
            .And.BeOfType<HeadingBasedChunker>();
    }

    [Fact]
    public void AddMarkItDotNetChunking_RegistersAllChunkerImplementations()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetChunking();

        using var provider = services.BuildServiceProvider();
        provider.GetService<HeadingBasedChunker>().Should().NotBeNull();
        provider.GetService<ParagraphBasedChunker>().Should().NotBeNull();
        provider.GetService<TokenAwareChunker>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetChunking_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetChunking(options =>
        {
            options.MaxChunkSize = 1024;
            options.OverlapSize = 100;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<ChunkingOptions>();
        options.MaxChunkSize.Should().Be(1024);
        options.OverlapSize.Should().Be(100);
    }

    [Fact]
    public void AddMarkItDotNetChunking_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetChunking();

        result.Should().BeSameAs(services);
    }
}
