using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElBruno.MarkItDotNet.Citations;

/// <summary>
/// Serializes and deserializes <see cref="CitationSet"/> instances to and from JSON.
/// </summary>
public static class CitationSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a <see cref="CitationSet"/> to a JSON string.
    /// </summary>
    /// <param name="citationSet">The citation set to serialize.</param>
    /// <returns>A JSON string representation of the citation set.</returns>
    public static string Serialize(CitationSet citationSet)
    {
        ArgumentNullException.ThrowIfNull(citationSet);
        return JsonSerializer.Serialize(citationSet, SerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CitationSet"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized citation set.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static CitationSet Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<CitationSet>(json, SerializerOptions)
               ?? throw new JsonException("Deserialization returned null.");
    }
}
