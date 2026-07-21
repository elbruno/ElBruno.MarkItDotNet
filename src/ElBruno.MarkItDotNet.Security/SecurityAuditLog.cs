using System.Text.Json;

namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Appends structured audit entries to an append-only JSONL file.
/// Each entry captures a policy evaluation outcome for compliance tracking.
/// </summary>
public sealed class SecurityAuditLog
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Creates a <see cref="SecurityAuditLog"/> that writes to <paramref name="filePath"/>.
    /// The directory is created if it does not exist.
    /// </summary>
    public SecurityAuditLog(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
    }

    /// <summary>Appends an audit entry for the given policy evaluation result.</summary>
    /// <param name="sourceIdentifier">A name or path identifying the content source (e.g. filename).</param>
    /// <param name="policyName">The name of the policy that was evaluated.</param>
    /// <param name="result">The policy result to record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task AppendAsync(
        string sourceIdentifier,
        string policyName,
        PolicyResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var entry = new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            Source: sourceIdentifier ?? string.Empty,
            Policy: policyName ?? string.Empty,
            Passed: result.Passed,
            ViolationCount: result.Violations.Count,
            Violations: result.Violations
                .Select(v => new AuditViolation(v.RuleName, v.Message, v.LineNumber))
                .ToList(),
            RedactionApplied: result.RedactedContent is not null);

        var line = JsonSerializer.Serialize(entry, _jsonOptions);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Reads all audit entries from the log file.</summary>
    public async Task<IReadOnlyList<AuditEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath)) return [];

        var lines = await File.ReadAllLinesAsync(_filePath, cancellationToken);
        var entries = new List<AuditEntry>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var entry = JsonSerializer.Deserialize<AuditEntry>(line, _jsonOptions);
            if (entry is not null) entries.Add(entry);
        }

        return entries;
    }

    // --- nested record types ---

    /// <summary>A single audit log entry.</summary>
    public sealed record AuditEntry(
        DateTimeOffset Timestamp,
        string Source,
        string Policy,
        bool Passed,
        int ViolationCount,
        List<AuditViolation> Violations,
        bool RedactionApplied);

    /// <summary>Condensed violation summary stored in an audit entry.</summary>
    public sealed record AuditViolation(string RuleName, string Message, int LineNumber);
}
