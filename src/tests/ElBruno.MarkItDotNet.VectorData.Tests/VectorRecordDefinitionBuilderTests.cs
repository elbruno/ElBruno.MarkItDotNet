// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Microsoft.Extensions.VectorData;
using Xunit;

namespace ElBruno.MarkItDotNet.VectorData.Tests;

public class VectorRecordDefinitionBuilderTests
{
    [Fact]
    public void Build_ReturnsDefinitionWithAllProperties()
    {
        var definition = VectorRecordDefinitionBuilder.Build();

        definition.Should().NotBeNull();
        definition.Properties.Should().HaveCount(11);
    }

    [Fact]
    public void Build_HasKeyPropertyForId()
    {
        var definition = VectorRecordDefinitionBuilder.Build();

        var keyProperty = definition.Properties
            .OfType<VectorStoreKeyProperty>()
            .SingleOrDefault();

        keyProperty.Should().NotBeNull();
        keyProperty!.Name.Should().Be("Id");
    }

    [Fact]
    public void Build_HasVectorPropertyForEmbedding()
    {
        var definition = VectorRecordDefinitionBuilder.Build();

        var vectorProperty = definition.Properties
            .OfType<VectorStoreVectorProperty>()
            .SingleOrDefault();

        vectorProperty.Should().NotBeNull();
        vectorProperty!.Name.Should().Be("Embedding");
        vectorProperty.Dimensions.Should().Be(1536);
    }

    [Fact]
    public void Build_CustomDimensions_SetsEmbeddingDimensions()
    {
        var definition = VectorRecordDefinitionBuilder.Build(embeddingDimensions: 768);

        var vectorProperty = definition.Properties
            .OfType<VectorStoreVectorProperty>()
            .Single();

        vectorProperty.Dimensions.Should().Be(768);
    }

    [Fact]
    public void Build_ContainsAllExpectedDataProperties()
    {
        var definition = VectorRecordDefinitionBuilder.Build();

        var dataPropertyNames = definition.Properties
            .OfType<VectorStoreDataProperty>()
            .Select(p => p.Name)
            .ToList();

        dataPropertyNames.Should().Contain("Content");
        dataPropertyNames.Should().Contain("DocumentId");
        dataPropertyNames.Should().Contain("DocumentTitle");
        dataPropertyNames.Should().Contain("HeadingPath");
        dataPropertyNames.Should().Contain("PageNumber");
        dataPropertyNames.Should().Contain("FilePath");
        dataPropertyNames.Should().Contain("ChunkIndex");
        dataPropertyNames.Should().Contain("Tags");
        dataPropertyNames.Should().Contain("Metadata");
    }

    [Fact]
    public void Build_DataPropertyCount_MatchesExpected()
    {
        var definition = VectorRecordDefinitionBuilder.Build();

        var dataProperties = definition.Properties
            .OfType<VectorStoreDataProperty>()
            .ToList();

        // 9 data properties: Content, DocumentId, DocumentTitle, HeadingPath,
        // PageNumber, FilePath, ChunkIndex, Tags, Metadata
        dataProperties.Should().HaveCount(9);
    }
}
