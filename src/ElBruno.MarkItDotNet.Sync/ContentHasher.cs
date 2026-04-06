// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using ElBruno.MarkItDotNet.Chunking;

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Provides SHA-256 hashing utilities for source content and chunks.
/// </summary>
public static class ContentHasher
{
    /// <summary>
    /// Computes a SHA-256 hash of the source content from a stream.
    /// </summary>
    /// <param name="content">The source content stream.</param>
    /// <returns>A lowercase hex-encoded SHA-256 hash string.</returns>
    public static string ComputeSourceHash(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var hashBytes = SHA256.HashData(content);
        return ToHexLower(hashBytes);
    }

    /// <summary>
    /// Computes a SHA-256 hash of the source content from a byte array.
    /// </summary>
    /// <param name="content">The source content bytes.</param>
    /// <returns>A lowercase hex-encoded SHA-256 hash string.</returns>
    public static string ComputeSourceHash(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var hashBytes = SHA256.HashData(content);
        return ToHexLower(hashBytes);
    }

    /// <summary>
    /// Computes a SHA-256 hash of a chunk's content combined with its heading path.
    /// </summary>
    /// <param name="chunk">The chunk result to hash.</param>
    /// <returns>A lowercase hex-encoded SHA-256 hash string.</returns>
    public static string ComputeChunkHash(ChunkResult chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        var combined = chunk.Content + "|" + (chunk.HeadingPath ?? string.Empty);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return ToHexLower(hashBytes);
    }

    /// <summary>
    /// Computes SHA-256 hashes for a collection of chunks, keyed by chunk ID.
    /// </summary>
    /// <param name="chunks">The chunks to hash.</param>
    /// <returns>A dictionary mapping chunk ID to its content hash.</returns>
    public static Dictionary<string, string> ComputeChunkHashes(IEnumerable<ChunkResult> chunks)
    {
        ArgumentNullException.ThrowIfNull(chunks);
        var result = new Dictionary<string, string>();
        foreach (var chunk in chunks)
        {
            result[chunk.Id] = ComputeChunkHash(chunk);
        }

        return result;
    }

    private static string ToHexLower(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
