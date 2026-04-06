// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Chunking.Tests;

public class ChunkingOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new ChunkingOptions();

        options.MaxChunkSize.Should().Be(512);
        options.OverlapSize.Should().Be(50);
        options.PreserveTableAtomicity.Should().BeTrue();
        options.PreserveFigureAtomicity.Should().BeTrue();
        options.TokenCounter.Should().BeNull();
    }

    [Fact]
    public void MaxChunkSize_CanBeCustomized()
    {
        var options = new ChunkingOptions { MaxChunkSize = 1024 };

        options.MaxChunkSize.Should().Be(1024);
    }

    [Fact]
    public void OverlapSize_CanBeCustomized()
    {
        var options = new ChunkingOptions { OverlapSize = 100 };

        options.OverlapSize.Should().Be(100);
    }

    [Fact]
    public void PreserveTableAtomicity_CanBeDisabled()
    {
        var options = new ChunkingOptions { PreserveTableAtomicity = false };

        options.PreserveTableAtomicity.Should().BeFalse();
    }

    [Fact]
    public void PreserveFigureAtomicity_CanBeDisabled()
    {
        var options = new ChunkingOptions { PreserveFigureAtomicity = false };

        options.PreserveFigureAtomicity.Should().BeFalse();
    }

    [Fact]
    public void TokenCounter_CanBeSetToCustomFunction()
    {
        Func<string, int> counter = text => text.Length;
        var options = new ChunkingOptions { TokenCounter = counter };

        options.TokenCounter.Should().NotBeNull();
        options.TokenCounter!("hello").Should().Be(5);
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        Func<string, int> counter = text => text.Length / 4;
        var options = new ChunkingOptions
        {
            MaxChunkSize = 256,
            OverlapSize = 25,
            PreserveTableAtomicity = false,
            PreserveFigureAtomicity = false,
            TokenCounter = counter,
        };

        options.MaxChunkSize.Should().Be(256);
        options.OverlapSize.Should().Be(25);
        options.PreserveTableAtomicity.Should().BeFalse();
        options.PreserveFigureAtomicity.Should().BeFalse();
        options.TokenCounter.Should().NotBeNull();
    }
}
