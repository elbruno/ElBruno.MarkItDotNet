// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// A file-based implementation of <see cref="ISyncStateStore"/> that stores each document's
/// sync state as a JSON file at <c>{basePath}/{documentId}.json</c>.
/// </summary>
public class FileSyncStateStore : ISyncStateStore
{
    private readonly string _basePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSyncStateStore"/> class.
    /// </summary>
    /// <param name="basePath">The directory path where state files are stored.</param>
    public FileSyncStateStore(string basePath)
    {
        ArgumentNullException.ThrowIfNull(basePath);
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    /// <inheritdoc />
    public async Task<SyncState?> GetStateAsync(string documentId, CancellationToken ct = default)
    {
        var filePath = GetFilePath(documentId);
        if (!File.Exists(filePath))
        {
            return null;
        }

        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var json = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<SyncState>(json, JsonOptions);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveStateAsync(SyncState state, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        var filePath = GetFilePath(state.DocumentId);

        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var json = JsonSerializer.Serialize(state, JsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteStateAsync(string documentId, CancellationToken ct = default)
    {
        var filePath = GetFilePath(documentId);

        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            _lock.Release();
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SyncState>> GetAllStatesAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var states = new List<SyncState>();
            if (!Directory.Exists(_basePath))
            {
                return states;
            }

            foreach (var file in Directory.GetFiles(_basePath, "*.json"))
            {
                var json = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);
                var state = JsonSerializer.Deserialize<SyncState>(json, JsonOptions);
                if (state is not null)
                {
                    states.Add(state);
                }
            }

            return states;
        }
        finally
        {
            _lock.Release();
        }
    }

    private string GetFilePath(string documentId)
    {
        // Sanitize documentId for use as a filename
        var safeName = string.Join("_", documentId.Split(Path.GetInvalidFileNameChars()));
        var filePath = Path.Combine(_basePath, safeName + ".json");
        var fullPath = Path.GetFullPath(filePath);
        var baseFullPath = Path.GetFullPath(_basePath);

        if (!fullPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Document ID would result in a path outside the base directory.");
        }

        return fullPath;
    }
}
