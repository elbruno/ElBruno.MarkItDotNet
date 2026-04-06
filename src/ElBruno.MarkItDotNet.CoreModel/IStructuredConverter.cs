// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Contract for converting files into structured CoreModel documents.
/// Parallel to IMarkdownConverter but produces rich document structure.
/// </summary>
public interface IStructuredConverter
{
    /// <summary>
    /// Determines whether this converter can handle the given file.
    /// </summary>
    /// <param name="filePath">Path to the file to evaluate.</param>
    /// <returns><see langword="true"/> if this converter supports the file; otherwise <see langword="false"/>.</returns>
    bool CanHandle(string filePath);

    /// <summary>
    /// Converts a file on disk into a structured <see cref="Document"/>.
    /// </summary>
    /// <param name="filePath">Path to the source file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A structured document representation of the file.</returns>
    Task<Document> ConvertToDocumentAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a stream into a structured <see cref="Document"/>.
    /// </summary>
    /// <param name="stream">The input stream containing file data.</param>
    /// <param name="fileName">The original file name, used to determine format.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A structured document representation of the stream content.</returns>
    Task<Document> ConvertToDocumentAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}
