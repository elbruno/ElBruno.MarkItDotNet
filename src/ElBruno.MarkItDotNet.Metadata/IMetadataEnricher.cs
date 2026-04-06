// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Enriches an existing <see cref="MetadataResult"/> with additional computed properties.
/// Implementations can add tags, detect entities, or perform other custom enrichments.
/// </summary>
public interface IMetadataEnricher
{
    /// <summary>
    /// Enriches the given metadata result using additional analysis of the document.
    /// </summary>
    /// <param name="result">The existing metadata result to enrich.</param>
    /// <param name="document">The source document for additional analysis.</param>
    /// <returns>An enriched <see cref="MetadataResult"/>.</returns>
    MetadataResult Enrich(MetadataResult result, Document document);
}
