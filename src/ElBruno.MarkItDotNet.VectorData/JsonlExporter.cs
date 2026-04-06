// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;

namespace ElBruno.MarkItDotNet.VectorData;

/// <summary>
/// Exports <see cref="VectorRecord"/> collections to JSONL (JSON Lines) format,
/// a universal fallback format for vector data ingestion.
/// </summary>
public static class JsonlExporter
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Exports vector records to a stream in JSONL format (one JSON object per line).
    /// </summary>
    /// <param name="records">The records to export.</param>
    /// <param name="output">The output stream to write to.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    public static void ExportToStream(IEnumerable<VectorRecord> records, Stream output, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(output);

        var opts = options ?? DefaultOptions;
        using var writer = new StreamWriter(output, Encoding.UTF8, leaveOpen: true);

        foreach (var record in records)
        {
            var json = JsonSerializer.Serialize(record, opts);
            writer.WriteLine(json);
        }

        writer.Flush();
    }

    /// <summary>
    /// Exports vector records to a file in JSONL format (one JSON object per line).
    /// </summary>
    /// <param name="records">The records to export.</param>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    public static void ExportToFile(IEnumerable<VectorRecord> records, string filePath, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(filePath);

        using var stream = File.Create(filePath);
        ExportToStream(records, stream, options);
    }

    /// <summary>
    /// Exports vector records to a JSONL string (one JSON object per line).
    /// </summary>
    /// <param name="records">The records to export.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>A string containing the JSONL-formatted records.</returns>
    public static string ExportToString(IEnumerable<VectorRecord> records, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(records);

        var opts = options ?? DefaultOptions;
        var sb = new StringBuilder();

        foreach (var record in records)
        {
            var json = JsonSerializer.Serialize(record, opts);
            sb.AppendLine(json);
        }

        return sb.ToString();
    }
}
