// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.CoreModel.Tests;

public class DocumentIdGeneratorTests
{
    [Fact]
    public void Generate_SameContent_ProducesSameId()
    {
        var id1 = DocumentIdGenerator.Generate("hello", "world");
        var id2 = DocumentIdGenerator.Generate("hello", "world");

        id1.Should().Be(id2);
    }

    [Fact]
    public void Generate_DifferentContent_ProducesDifferentId()
    {
        var id1 = DocumentIdGenerator.Generate("hello");
        var id2 = DocumentIdGenerator.Generate("goodbye");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Generate_Returns12HexCharacters()
    {
        var id = DocumentIdGenerator.Generate("test content");

        id.Should().HaveLength(12);
        id.Should().MatchRegex("^[0-9a-f]{12}$");
    }

    [Fact]
    public void Generate_IsLowercase()
    {
        var id = DocumentIdGenerator.Generate("UPPERCASE INPUT");

        id.Should().Be(id.ToLowerInvariant());
    }

    [Fact]
    public void ForDocument_GeneratesDeterministicId()
    {
        var id1 = DocumentIdGenerator.ForDocument("/path/file.pdf", "Title");
        var id2 = DocumentIdGenerator.ForDocument("/path/file.pdf", "Title");

        id1.Should().Be(id2);
    }

    [Fact]
    public void ForDocument_DifferentPaths_ProduceDifferentIds()
    {
        var id1 = DocumentIdGenerator.ForDocument("/path/a.pdf", "Title");
        var id2 = DocumentIdGenerator.ForDocument("/path/b.pdf", "Title");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ForDocument_HandlesNullValues()
    {
        var id = DocumentIdGenerator.ForDocument(null, null);

        id.Should().HaveLength(12);
    }

    [Fact]
    public void ForBlock_GeneratesDeterministicId()
    {
        var id1 = DocumentIdGenerator.ForBlock("paragraph", "Hello", 0);
        var id2 = DocumentIdGenerator.ForBlock("paragraph", "Hello", 0);

        id1.Should().Be(id2);
    }

    [Fact]
    public void ForBlock_DifferentPositions_ProduceDifferentIds()
    {
        var id1 = DocumentIdGenerator.ForBlock("paragraph", "Same text", 0);
        var id2 = DocumentIdGenerator.ForBlock("paragraph", "Same text", 1);

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ForBlock_DifferentTypes_ProduceDifferentIds()
    {
        var id1 = DocumentIdGenerator.ForBlock("paragraph", "text", 0);
        var id2 = DocumentIdGenerator.ForBlock("heading", "text", 0);

        id1.Should().NotBe(id2);
    }
}
