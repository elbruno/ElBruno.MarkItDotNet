// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.VectorData;

namespace ElBruno.MarkItDotNet.VectorData;

/// <summary>
/// Builds a <see cref="VectorStoreCollectionDefinition"/> that describes the schema
/// of <see cref="VectorRecord"/> for vector store providers.
/// </summary>
public static class VectorRecordDefinitionBuilder
{
    /// <summary>
    /// Creates a <see cref="VectorStoreCollectionDefinition"/> matching the <see cref="VectorRecord"/> shape.
    /// </summary>
    /// <param name="embeddingDimensions">The number of dimensions in the embedding vector. Defaults to 1536 (OpenAI ada-002).</param>
    /// <returns>A configured <see cref="VectorStoreCollectionDefinition"/>.</returns>
    public static VectorStoreCollectionDefinition Build(int embeddingDimensions = 1536)
    {
        return new VectorStoreCollectionDefinition
        {
            Properties =
            [
                new VectorStoreKeyProperty("Id", typeof(string)),
                new VectorStoreDataProperty("Content", typeof(string)),
                new VectorStoreVectorProperty("Embedding", typeof(ReadOnlyMemory<float>?), embeddingDimensions),
                new VectorStoreDataProperty("DocumentId", typeof(string)),
                new VectorStoreDataProperty("DocumentTitle", typeof(string)),
                new VectorStoreDataProperty("HeadingPath", typeof(string)),
                new VectorStoreDataProperty("PageNumber", typeof(int?)),
                new VectorStoreDataProperty("FilePath", typeof(string)),
                new VectorStoreDataProperty("ChunkIndex", typeof(int)),
                new VectorStoreDataProperty("Tags", typeof(IReadOnlyList<string>)),
                new VectorStoreDataProperty("Metadata", typeof(IReadOnlyDictionary<string, object>)),
            ],
        };
    }
}
