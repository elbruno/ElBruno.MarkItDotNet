using System.Globalization;
using System.Text.Json;

namespace IngestionWorkflowSample;

public sealed class WorkflowOptions
{
    public string InputPath { get; set; } = "input";
    public string OutputPath { get; set; } = "output";
    public bool Recursive { get; set; } = true;
    public int MaxDepth { get; set; } = 3;
    public List<string> IncludePatterns { get; set; } = ["*.txt", "*.json", "*.html"];
    public long MaxFileSizeBytes { get; set; } = 50_000_000;
    public double PassThreshold { get; set; } = 0.70;
    public bool DryRun { get; set; }
    public bool SeedInput { get; set; } = true;
    public string ChunkingStrategy { get; set; } = "paragraph";
    public int ChunkSize { get; set; } = 512;
    public int ChunkOverlap { get; set; } = 50;
    public AzureSearchWorkflowOptions AzureSearch { get; set; } = new();
    public bool ShowHelp { get; set; }
}

public sealed class AzureSearchWorkflowOptions
{
    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public int BatchSize { get; set; } = 100;
    public int VectorDimensions { get; set; } = 1536;
    public bool CreateIndexIfMissing { get; set; } = true;
}

public static class WorkflowOptionsParser
{
    public static WorkflowOptions Parse(string[] args, string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(baseDirectory);

        var options = LoadDefaults(baseDirectory);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase))
            {
                options.ShowHelp = true;
                continue;
            }

            if (string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase))
            {
                options.DryRun = true;
                continue;
            }

            if (string.Equals(arg, "--skip-seed", StringComparison.OrdinalIgnoreCase))
            {
                options.SeedInput = false;
                continue;
            }

            if (string.Equals(arg, "--enable-azure-search", StringComparison.OrdinalIgnoreCase))
            {
                options.AzureSearch.Enabled = true;
                continue;
            }

            if (TryReadValue(arg, "--input", out var inputPath))
            {
                options.InputPath = inputPath ?? ReadNext(args, ref i, "--input");
                continue;
            }

            if (TryReadValue(arg, "--output", out var outputPath))
            {
                options.OutputPath = outputPath ?? ReadNext(args, ref i, "--output");
                continue;
            }

            if (TryReadValue(arg, "--max-depth", out var maxDepthRaw))
            {
                options.MaxDepth = int.Parse(maxDepthRaw ?? ReadNext(args, ref i, "--max-depth"), CultureInfo.InvariantCulture);
                continue;
            }

            if (TryReadValue(arg, "--max-file-size", out var maxFileSizeRaw))
            {
                options.MaxFileSizeBytes = long.Parse(maxFileSizeRaw ?? ReadNext(args, ref i, "--max-file-size"), CultureInfo.InvariantCulture);
                continue;
            }

            if (TryReadValue(arg, "--threshold", out var thresholdRaw))
            {
                options.PassThreshold = double.Parse(thresholdRaw ?? ReadNext(args, ref i, "--threshold"), CultureInfo.InvariantCulture);
                continue;
            }

            if (TryReadValue(arg, "--pattern", out var patternRaw))
            {
                var pattern = patternRaw ?? ReadNext(args, ref i, "--pattern");
                options.IncludePatterns.Add(pattern);
                continue;
            }

            if (TryReadValue(arg, "--chunk-strategy", out var chunkStrategyRaw))
            {
                options.ChunkingStrategy = chunkStrategyRaw ?? ReadNext(args, ref i, "--chunk-strategy");
                continue;
            }

            if (TryReadValue(arg, "--chunk-size", out var chunkSizeRaw))
            {
                options.ChunkSize = int.Parse(chunkSizeRaw ?? ReadNext(args, ref i, "--chunk-size"), CultureInfo.InvariantCulture);
                continue;
            }

            if (TryReadValue(arg, "--chunk-overlap", out var overlapRaw))
            {
                options.ChunkOverlap = int.Parse(overlapRaw ?? ReadNext(args, ref i, "--chunk-overlap"), CultureInfo.InvariantCulture);
                continue;
            }

            if (TryReadValue(arg, "--search-endpoint", out var endpointRaw))
            {
                options.AzureSearch.Endpoint = endpointRaw ?? ReadNext(args, ref i, "--search-endpoint");
                options.AzureSearch.Enabled = true;
                continue;
            }

            if (TryReadValue(arg, "--search-index", out var indexRaw))
            {
                options.AzureSearch.IndexName = indexRaw ?? ReadNext(args, ref i, "--search-index");
                options.AzureSearch.Enabled = true;
                continue;
            }

            if (TryReadValue(arg, "--search-api-key", out var apiKeyRaw))
            {
                options.AzureSearch.ApiKey = apiKeyRaw ?? ReadNext(args, ref i, "--search-api-key");
                options.AzureSearch.Enabled = true;
                continue;
            }

            throw new ArgumentException($"Unknown argument: '{arg}'.");
        }

        if (options.MaxDepth < 0)
        {
            throw new ArgumentException("--max-depth must be greater than or equal to 0.");
        }

        if (options.MaxFileSizeBytes <= 0)
        {
            throw new ArgumentException("--max-file-size must be greater than 0.");
        }

        var normalizedStrategy = options.ChunkingStrategy.Trim().ToLowerInvariant();
        if (normalizedStrategy is not ("heading" or "paragraph" or "token"))
        {
            throw new ArgumentException("--chunk-strategy must be one of: heading, paragraph, token.");
        }

        options.ChunkingStrategy = normalizedStrategy;

        if (options.ChunkSize <= 0)
        {
            throw new ArgumentException("--chunk-size must be greater than 0.");
        }

        if (options.ChunkOverlap < 0)
        {
            throw new ArgumentException("--chunk-overlap must be greater than or equal to 0.");
        }

        if (options.PassThreshold is < 0 or > 1)
        {
            throw new ArgumentException("--threshold must be between 0.0 and 1.0.");
        }

        if (options.AzureSearch.BatchSize <= 0)
        {
            throw new ArgumentException("AzureSearch.BatchSize must be greater than 0.");
        }

        if (options.AzureSearch.VectorDimensions <= 0)
        {
            throw new ArgumentException("AzureSearch.VectorDimensions must be greater than 0.");
        }

        if (options.IncludePatterns.Count == 0)
        {
            options.IncludePatterns.AddRange(["*.txt", "*.json", "*.html"]);
        }

        options.InputPath = Path.GetFullPath(options.InputPath, baseDirectory);
        options.OutputPath = Path.GetFullPath(options.OutputPath, baseDirectory);

        return options;
    }

    private static WorkflowOptions LoadDefaults(string baseDirectory)
    {
        var options = new WorkflowOptions();
        var appSettingsPath = Path.Combine(baseDirectory, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            return options;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
        if (!document.RootElement.TryGetProperty("IngestionWorkflow", out var workflow))
        {
            return options;
        }

        if (TryGetString(workflow, "InputPath", out var inputPath))
        {
            options.InputPath = inputPath!;
        }

        if (TryGetString(workflow, "OutputPath", out var outputPath))
        {
            options.OutputPath = outputPath!;
        }

        if (TryGetBool(workflow, "Recursive", out var recursive))
        {
            options.Recursive = recursive;
        }

        if (TryGetInt(workflow, "MaxDepth", out var maxDepth))
        {
            options.MaxDepth = maxDepth;
        }

        if (TryGetLong(workflow, "MaxFileSizeBytes", out var maxFileSize))
        {
            options.MaxFileSizeBytes = maxFileSize;
        }

        if (TryGetDouble(workflow, "PassThreshold", out var passThreshold))
        {
            options.PassThreshold = passThreshold;
        }

        if (TryGetBool(workflow, "DryRun", out var dryRun))
        {
            options.DryRun = dryRun;
        }

        if (TryGetBool(workflow, "SeedInput", out var seedInput))
        {
            options.SeedInput = seedInput;
        }

        if (TryGetString(workflow, "ChunkingStrategy", out var chunkingStrategy))
        {
            options.ChunkingStrategy = chunkingStrategy!;
        }

        if (TryGetInt(workflow, "ChunkSize", out var chunkSize))
        {
            options.ChunkSize = chunkSize;
        }

        if (TryGetInt(workflow, "ChunkOverlap", out var chunkOverlap))
        {
            options.ChunkOverlap = chunkOverlap;
        }

        if (workflow.TryGetProperty("IncludePatterns", out var patternsElement) &&
            patternsElement.ValueKind == JsonValueKind.Array)
        {
            var patterns = patternsElement
                .EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();

            if (patterns.Count > 0)
            {
                options.IncludePatterns = patterns!;
            }
        }

        if (workflow.TryGetProperty("AzureSearch", out var azureSearch) &&
            azureSearch.ValueKind == JsonValueKind.Object)
        {
            if (TryGetBool(azureSearch, "Enabled", out var enabled))
            {
                options.AzureSearch.Enabled = enabled;
            }

            if (TryGetString(azureSearch, "Endpoint", out var endpoint))
            {
                options.AzureSearch.Endpoint = endpoint!;
            }

            if (TryGetString(azureSearch, "IndexName", out var indexName))
            {
                options.AzureSearch.IndexName = indexName!;
            }

            if (TryGetString(azureSearch, "ApiKey", out var apiKey))
            {
                options.AzureSearch.ApiKey = apiKey;
            }

            if (TryGetInt(azureSearch, "BatchSize", out var batchSize))
            {
                options.AzureSearch.BatchSize = batchSize;
            }

            if (TryGetInt(azureSearch, "VectorDimensions", out var vectorDimensions))
            {
                options.AzureSearch.VectorDimensions = vectorDimensions;
            }

            if (TryGetBool(azureSearch, "CreateIndexIfMissing", out var createIndex))
            {
                options.AzureSearch.CreateIndexIfMissing = createIndex;
            }
        }

        return options;
    }

    private static bool TryReadValue(string arg, string optionName, out string? value)
    {
        var prefix = $"{optionName}=";
        if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = arg[prefix.Length..];
            return true;
        }

        if (string.Equals(arg, optionName, StringComparison.OrdinalIgnoreCase))
        {
            value = null;
            return true;
        }

        value = null;
        return false;
    }

    private static string ReadNext(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length || args[index + 1].StartsWith("-", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        index++;
        return args[index];
    }

    private static bool TryGetString(JsonElement element, string name, out string? value)
    {
        if (element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryGetBool(JsonElement element, string name, out bool value)
    {
        if (element.TryGetProperty(name, out var property) &&
            (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False))
        {
            value = property.GetBoolean();
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryGetInt(JsonElement element, string name, out int value)
    {
        if (element.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt32(out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryGetLong(JsonElement element, string name, out long value)
    {
        if (element.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt64(out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryGetDouble(JsonElement element, string name, out double value)
    {
        if (element.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetDouble(out value))
        {
            return true;
        }

        value = default;
        return false;
    }
}
