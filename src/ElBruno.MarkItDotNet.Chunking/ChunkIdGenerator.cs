// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;

namespace ElBruno.MarkItDotNet.Chunking;

/// <summary>
/// Generates stable, deterministic chunk identifiers based on document ID,
/// chunk index, and a content hash.
/// </summary>
public static class ChunkIdGenerator
{
    /// <summary>
    /// Generates a deterministic chunk ID using the document ID, chunk index,
    /// and the first 8 characters of a SHA-256 hash of the content.
    /// </summary>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="chunkIndex">The zero-based index of the chunk.</param>
    /// <param name="content">The text content of the chunk.</param>
    /// <returns>A stable, deterministic chunk identifier.</returns>
    public static string Generate(string documentId, int chunkIndex, string content)
    {
        var contentHash = ComputeContentHash(content);
        return $"{documentId}-chunk-{chunkIndex}-{contentHash}";
    }

    private static string ComputeContentHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes)[..8].ToLowerInvariant();
    }
}
