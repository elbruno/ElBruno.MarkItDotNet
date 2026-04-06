// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using ElBruno.MarkItDotNet.Chunking;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Sync.Tests;

public class ContentHasherTests
{
    [Fact]
    public void ComputeSourceHash_Stream_ReturnsSha256HexString()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
        var hash = ContentHasher.ComputeSourceHash(stream);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().HaveLength(64); // SHA-256 = 32 bytes = 64 hex chars
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void ComputeSourceHash_ByteArray_ReturnsSha256HexString()
    {
        var hash = ContentHasher.ComputeSourceHash(Encoding.UTF8.GetBytes("hello world"));

        hash.Should().NotBeNullOrEmpty();
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void ComputeSourceHash_IsDeterministic()
    {
        var data = Encoding.UTF8.GetBytes("deterministic content");

        var hash1 = ContentHasher.ComputeSourceHash(data);
        var hash2 = ContentHasher.ComputeSourceHash(data);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeSourceHash_StreamAndByteArray_ProduceSameHash()
    {
        var data = Encoding.UTF8.GetBytes("same content");
        using var stream = new MemoryStream(data);

        var hashFromBytes = ContentHasher.ComputeSourceHash(data);
        var hashFromStream = ContentHasher.ComputeSourceHash(stream);

        hashFromBytes.Should().Be(hashFromStream);
    }

    [Fact]
    public void ComputeSourceHash_DifferentContent_ProducesDifferentHashes()
    {
        var hash1 = ContentHasher.ComputeSourceHash(Encoding.UTF8.GetBytes("content A"));
        var hash2 = ContentHasher.ComputeSourceHash(Encoding.UTF8.GetBytes("content B"));

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void ComputeChunkHash_ReturnsSha256HexString()
    {
        var chunk = new ChunkResult { Id = "chunk-1", Content = "test content", HeadingPath = "Header" };
        var hash = ContentHasher.ComputeChunkHash(chunk);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().HaveLength(64);
    }

    [Fact]
    public void ComputeChunkHash_IsDeterministic()
    {
        var chunk = new ChunkResult { Id = "chunk-1", Content = "test content", HeadingPath = "Header" };

        var hash1 = ContentHasher.ComputeChunkHash(chunk);
        var hash2 = ContentHasher.ComputeChunkHash(chunk);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeChunkHash_DifferentContent_ProducesDifferentHashes()
    {
        var chunk1 = new ChunkResult { Id = "chunk-1", Content = "content A", HeadingPath = "Header" };
        var chunk2 = new ChunkResult { Id = "chunk-1", Content = "content B", HeadingPath = "Header" };

        var hash1 = ContentHasher.ComputeChunkHash(chunk1);
        var hash2 = ContentHasher.ComputeChunkHash(chunk2);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void ComputeChunkHash_DifferentHeadingPath_ProducesDifferentHashes()
    {
        var chunk1 = new ChunkResult { Id = "chunk-1", Content = "same content", HeadingPath = "Header A" };
        var chunk2 = new ChunkResult { Id = "chunk-1", Content = "same content", HeadingPath = "Header B" };

        var hash1 = ContentHasher.ComputeChunkHash(chunk1);
        var hash2 = ContentHasher.ComputeChunkHash(chunk2);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void ComputeChunkHash_NullHeadingPath_DoesNotThrow()
    {
        var chunk = new ChunkResult { Id = "chunk-1", Content = "test content", HeadingPath = null };
        var hash = ContentHasher.ComputeChunkHash(chunk);

        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ComputeChunkHashes_ReturnsHashForEachChunk()
    {
        var chunks = new List<ChunkResult>
        {
            new() { Id = "chunk-1", Content = "content 1", HeadingPath = "H1" },
            new() { Id = "chunk-2", Content = "content 2", HeadingPath = "H2" }
        };

        var hashes = ContentHasher.ComputeChunkHashes(chunks);

        hashes.Should().HaveCount(2);
        hashes.Should().ContainKey("chunk-1");
        hashes.Should().ContainKey("chunk-2");
        hashes["chunk-1"].Should().NotBe(hashes["chunk-2"]);
    }
}
