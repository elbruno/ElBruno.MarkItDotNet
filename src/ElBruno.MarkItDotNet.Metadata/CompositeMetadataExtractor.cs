// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Chains a base <see cref="IMetadataExtractor"/> with zero or more <see cref="IMetadataEnricher"/>
/// instances to produce a fully enriched <see cref="MetadataResult"/>.
/// </summary>
public class CompositeMetadataExtractor : IMetadataExtractor
{
    private readonly IMetadataExtractor _baseExtractor;
    private readonly IReadOnlyList<IMetadataEnricher> _enrichers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMetadataExtractor"/> class.
    /// </summary>
    /// <param name="baseExtractor">The base extractor that produces the initial metadata.</param>
    /// <param name="enrichers">Optional enrichers applied in order after base extraction.</param>
    public CompositeMetadataExtractor(
        IMetadataExtractor baseExtractor,
        IEnumerable<IMetadataEnricher>? enrichers = null)
    {
        ArgumentNullException.ThrowIfNull(baseExtractor);
        _baseExtractor = baseExtractor;
        _enrichers = enrichers?.ToList() ?? [];
    }

    /// <inheritdoc />
    public MetadataResult Extract(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var result = _baseExtractor.Extract(document);

        foreach (var enricher in _enrichers)
        {
            result = enricher.Enrich(result, document);
        }

        return result;
    }
}
