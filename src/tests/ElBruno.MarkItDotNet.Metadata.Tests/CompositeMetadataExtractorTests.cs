// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Metadata.Tests;

public class CompositeMetadataExtractorTests
{
    [Fact]
    public void Extract_WithNoEnrichers_ReturnsBaseResult()
    {
        var baseExtractor = new DocumentMetadataExtractor();
        var composite = new CompositeMetadataExtractor(baseExtractor);

        var doc = new Document
        {
            Metadata = new DocumentMetadata { Title = "Test" },
        };

        var result = composite.Extract(doc);

        result.Title.Should().Be("Test");
    }

    [Fact]
    public void Extract_AppliesEnrichersInOrder()
    {
        var baseExtractor = new DocumentMetadataExtractor();
        var enricher1 = new TagAddingEnricher("tag1");
        var enricher2 = new TagAddingEnricher("tag2");
        var composite = new CompositeMetadataExtractor(baseExtractor, [enricher1, enricher2]);

        var doc = new Document
        {
            Metadata = new DocumentMetadata { Title = "Test" },
        };

        var result = composite.Extract(doc);

        result.Tags.Should().Contain("tag1");
        result.Tags.Should().Contain("tag2");
    }

    [Fact]
    public void Extract_EnricherCanModifyMetadata()
    {
        var baseExtractor = new DocumentMetadataExtractor();
        var enricher = new LanguageOverrideEnricher("ja");
        var composite = new CompositeMetadataExtractor(baseExtractor, [enricher]);

        var doc = new Document
        {
            Metadata = new DocumentMetadata { Title = "Test" },
        };

        var result = composite.Extract(doc);

        result.Language.Should().Be("ja");
    }

    [Fact]
    public void Constructor_ThrowsForNullBaseExtractor()
    {
        var action = () => new CompositeMetadataExtractor(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Extract_ThrowsForNullDocument()
    {
        var composite = new CompositeMetadataExtractor(new DocumentMetadataExtractor());

        var action = () => composite.Extract(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    #region Test enrichers

    private sealed class TagAddingEnricher(string tag) : IMetadataEnricher
    {
        public MetadataResult Enrich(MetadataResult result, Document document)
        {
            var tags = result.Tags.ToList();
            tags.Add(tag);
            return result with { Tags = tags };
        }
    }

    private sealed class LanguageOverrideEnricher(string language) : IMetadataEnricher
    {
        public MetadataResult Enrich(MetadataResult result, Document document)
        {
            return result with { Language = language };
        }
    }

    #endregion
}
