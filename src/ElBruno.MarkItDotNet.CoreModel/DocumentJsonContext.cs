// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// JSON serialization context for the CoreModel document types.
/// </summary>
[JsonSerializable(typeof(Document))]
[JsonSerializable(typeof(DocumentSection))]
[JsonSerializable(typeof(DocumentBlock))]
[JsonSerializable(typeof(ParagraphBlock))]
[JsonSerializable(typeof(HeadingBlock))]
[JsonSerializable(typeof(TableBlock))]
[JsonSerializable(typeof(FigureBlock))]
[JsonSerializable(typeof(ListBlock))]
[JsonSerializable(typeof(ListItemBlock))]
[JsonSerializable(typeof(DocumentMetadata))]
[JsonSerializable(typeof(SourceReference))]
[JsonSerializable(typeof(SpanReference))]
public partial class DocumentJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Static helper for serializing and deserializing <see cref="Document"/> instances
/// with consistent JSON options including camelCase naming and polymorphic type support.
/// </summary>
public static class DocumentSerializer
{
    /// <summary>
    /// Shared serializer options for CoreModel JSON operations.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    /// <summary>
    /// Serializes a <see cref="Document"/> to a JSON string.
    /// </summary>
    /// <param name="document">The document to serialize.</param>
    /// <returns>A JSON string representation of the document.</returns>
    public static string Serialize(Document document)
    {
        return JsonSerializer.Serialize(document, Options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="Document"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized document, or <see langword="null"/> if deserialization fails.</returns>
    public static Document? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<Document>(json, Options);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }
}
