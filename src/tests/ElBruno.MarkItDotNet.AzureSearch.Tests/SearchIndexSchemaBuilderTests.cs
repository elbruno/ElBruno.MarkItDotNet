// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents.Indexes.Models;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.AzureSearch.Tests;

public class SearchIndexSchemaBuilderTests
{
    [Fact]
    public void Build_ReturnsIndexWithCorrectName()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        index.Name.Should().Be("test-index");
    }

    [Fact]
    public void Build_ContainsAllExpectedFields()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        var fieldNames = index.Fields.Select(f => f.Name).ToList();

        fieldNames.Should().Contain(nameof(SearchDocument.Id));
        fieldNames.Should().Contain(nameof(SearchDocument.Content));
        fieldNames.Should().Contain(nameof(SearchDocument.Title));
        fieldNames.Should().Contain(nameof(SearchDocument.HeadingPath));
        fieldNames.Should().Contain(nameof(SearchDocument.FilePath));
        fieldNames.Should().Contain(nameof(SearchDocument.PageNumber));
        fieldNames.Should().Contain(nameof(SearchDocument.ChunkIndex));
        fieldNames.Should().Contain(nameof(SearchDocument.DocumentId));
        fieldNames.Should().Contain(nameof(SearchDocument.ContentVector));
        fieldNames.Should().Contain(nameof(SearchDocument.Tags));
        fieldNames.Should().Contain(nameof(SearchDocument.Metadata));
        fieldNames.Should().Contain(nameof(SearchDocument.CitationText));
        fieldNames.Should().Contain(nameof(SearchDocument.LastUpdated));
    }

    [Fact]
    public void Build_IdFieldIsKey()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        var idField = index.Fields.First(f => f.Name == nameof(SearchDocument.Id));
        idField.IsKey.Should().BeTrue();
    }

    [Fact]
    public void Build_ContentFieldIsSearchable()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        var contentField = index.Fields.First(f => f.Name == nameof(SearchDocument.Content));
        contentField.IsSearchable.Should().BeTrue();
    }

    [Fact]
    public void Build_ContentVectorFieldHasCorrectDefaultDimensions()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        var vectorField = index.Fields.First(f => f.Name == nameof(SearchDocument.ContentVector));
        vectorField.Should().NotBeNull();
        vectorField.VectorSearchDimensions.Should().Be(1536);
    }

    [Fact]
    public void Build_ContentVectorFieldHasCustomDimensions()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index", vectorDimensions: 3072);

        var vectorField = index.Fields.First(f => f.Name == nameof(SearchDocument.ContentVector));
        vectorField.Should().NotBeNull();
        vectorField.VectorSearchDimensions.Should().Be(3072);
    }

    [Fact]
    public void Build_HasHnswAlgorithmConfiguration()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        index.VectorSearch.Should().NotBeNull();
        index.VectorSearch.Algorithms.Should().ContainSingle()
            .Which.Should().BeOfType<HnswAlgorithmConfiguration>();
    }

    [Fact]
    public void Build_HasVectorSearchProfile()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        index.VectorSearch.Profiles.Should().ContainSingle()
            .Which.Name.Should().Be(SearchIndexSchemaBuilder.VectorSearchProfileName);
    }

    [Fact]
    public void Build_VectorProfileReferencesHnswAlgorithm()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        var profile = index.VectorSearch.Profiles.Single();
        profile.AlgorithmConfigurationName.Should().Be(SearchIndexSchemaBuilder.HnswAlgorithmConfigName);
    }

    [Fact]
    public void Build_TagsFieldIsFilterable()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        var tagsField = index.Fields.First(f => f.Name == nameof(SearchDocument.Tags));
        tagsField.IsFilterable.Should().BeTrue();
    }

    [Fact]
    public void Build_NullIndexName_Throws()
    {
        var act = () => SearchIndexSchemaBuilder.Build(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Build_EmptyIndexName_Throws()
    {
        var act = () => SearchIndexSchemaBuilder.Build("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Build_ZeroVectorDimensions_Throws()
    {
        var act = () => SearchIndexSchemaBuilder.Build("test-index", vectorDimensions: 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Build_NegativeVectorDimensions_Throws()
    {
        var act = () => SearchIndexSchemaBuilder.Build("test-index", vectorDimensions: -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Build_FilePathFieldIsFilterable()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        var field = index.Fields.First(f => f.Name == nameof(SearchDocument.FilePath));
        field.IsFilterable.Should().BeTrue();
    }

    [Fact]
    public void Build_LastUpdatedFieldIsSortable()
    {
        var index = SearchIndexSchemaBuilder.Build("test-index");

        var field = index.Fields.First(f => f.Name == nameof(SearchDocument.LastUpdated));
        field.IsSortable.Should().BeTrue();
    }
}
