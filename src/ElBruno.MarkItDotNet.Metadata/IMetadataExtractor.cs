// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Extracts normalized metadata from a <see cref="Document"/>.
/// </summary>
public interface IMetadataExtractor
{
    /// <summary>
    /// Extracts metadata from the given document.
    /// </summary>
    /// <param name="document">The document to extract metadata from.</param>
    /// <returns>A <see cref="MetadataResult"/> containing extracted metadata.</returns>
    MetadataResult Extract(Document document);
}
