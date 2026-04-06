// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Generates deterministic identifiers for documents and blocks
/// based on content and optional positional context.
/// Uses SHA256 hashing truncated to 12 hex characters for stability.
/// </summary>
public static class DocumentIdGenerator
{
    private const int IdHexLength = 12;

    /// <summary>
    /// Generates a deterministic ID from one or more content strings.
    /// The same inputs always produce the same output.
    /// </summary>
    /// <param name="parts">Content strings to hash together.</param>
    /// <returns>A 12-character lowercase hex string identifier.</returns>
    public static string Generate(params string[] parts)
    {
        var combined = string.Join("|", parts);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hash)[..IdHexLength].ToLowerInvariant();
    }

    /// <summary>
    /// Generates a deterministic ID for a document based on its source path and title.
    /// </summary>
    /// <param name="sourcePath">The file path or URI of the source document.</param>
    /// <param name="title">The document title, if available.</param>
    /// <returns>A 12-character lowercase hex string identifier.</returns>
    public static string ForDocument(string? sourcePath, string? title)
    {
        return Generate(sourcePath ?? string.Empty, title ?? string.Empty);
    }

    /// <summary>
    /// Generates a deterministic ID for a block based on its content and position.
    /// </summary>
    /// <param name="blockType">The type discriminator of the block (e.g., "paragraph", "heading").</param>
    /// <param name="content">The primary text content of the block.</param>
    /// <param name="position">A positional indicator (e.g., section index, block index).</param>
    /// <returns>A 12-character lowercase hex string identifier.</returns>
    public static string ForBlock(string blockType, string content, int position)
    {
        return Generate(blockType, content, position.ToString());
    }
}
