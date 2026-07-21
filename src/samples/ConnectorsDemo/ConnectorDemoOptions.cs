using System.Globalization;
using System.Text.Json;

namespace ConnectorsDemoSample;

public sealed class ConnectorDemoOptions
{
    public string InputPath { get; set; } = "input";
    public string OutputPath { get; set; } = "output";
    public bool Recursive { get; set; } = true;
    public int MaxDepth { get; set; } = 3;
    public List<string> IncludePatterns { get; set; } = ["*.txt", "*.json", "*.html"];
    public long MaxFileSizeBytes { get; set; } = 50_000_000;
    public bool DryRun { get; set; }
    public bool SeedInput { get; set; } = true;
    public AzureBlobSourceOptions AzureBlob { get; set; } = new();
    public bool ShowHelp { get; set; }
}

public sealed class AzureBlobSourceOptions
{
    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
    public string? AccountName { get; set; }
    public string? ServiceUri { get; set; }
    public string? ContainerName { get; set; }
    public string? BlobPrefix { get; set; }
    public int? PageSize { get; set; }
    public int? MaxRetries { get; set; }
    public int? NetworkTimeoutSeconds { get; set; }
}

public static class ConnectorDemoOptionsParser
{
    public static ConnectorDemoOptions Parse(string[] args, string baseDirectory)
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

            if (string.Equals(arg, "--enable-azure", StringComparison.OrdinalIgnoreCase))
            {
                options.AzureBlob.Enabled = true;
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

            if (TryReadValue(arg, "--pattern", out var patternRaw))
            {
                var pattern = patternRaw ?? ReadNext(args, ref i, "--pattern");
                options.IncludePatterns.Add(pattern);
                continue;
            }

            if (TryReadValue(arg, "--azure-container", out var containerRaw))
            {
                options.AzureBlob.ContainerName = containerRaw ?? ReadNext(args, ref i, "--azure-container");
                options.AzureBlob.Enabled = true;
                continue;
            }

            if (TryReadValue(arg, "--azure-connection-string", out var connectionStringRaw))
            {
                options.AzureBlob.ConnectionString = connectionStringRaw ?? ReadNext(args, ref i, "--azure-connection-string");
                options.AzureBlob.Enabled = true;
                continue;
            }

            if (TryReadValue(arg, "--azure-account", out var accountRaw))
            {
                options.AzureBlob.AccountName = accountRaw ?? ReadNext(args, ref i, "--azure-account");
                options.AzureBlob.Enabled = true;
                continue;
            }

            if (TryReadValue(arg, "--azure-service-uri", out var serviceUriRaw))
            {
                options.AzureBlob.ServiceUri = serviceUriRaw ?? ReadNext(args, ref i, "--azure-service-uri");
                options.AzureBlob.Enabled = true;
                continue;
            }

            if (TryReadValue(arg, "--azure-prefix", out var prefixRaw))
            {
                options.AzureBlob.BlobPrefix = prefixRaw ?? ReadNext(args, ref i, "--azure-prefix");
                options.AzureBlob.Enabled = true;
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

        if (options.IncludePatterns.Count == 0)
        {
            options.IncludePatterns.AddRange(["*.txt", "*.json", "*.html"]);
        }

        options.InputPath = Path.GetFullPath(options.InputPath, baseDirectory);
        options.OutputPath = Path.GetFullPath(options.OutputPath, baseDirectory);

        return options;
    }

    private static ConnectorDemoOptions LoadDefaults(string baseDirectory)
    {
        var options = new ConnectorDemoOptions();
        var appSettingsPath = Path.Combine(baseDirectory, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            return options;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
        if (!document.RootElement.TryGetProperty("ConnectorsDemo", out var root))
        {
            return options;
        }

        if (TryGetString(root, "InputPath", out var inputPath))
        {
            options.InputPath = inputPath!;
        }

        if (TryGetString(root, "OutputPath", out var outputPath))
        {
            options.OutputPath = outputPath!;
        }

        if (TryGetBool(root, "Recursive", out var recursive))
        {
            options.Recursive = recursive;
        }

        if (TryGetInt(root, "MaxDepth", out var maxDepth))
        {
            options.MaxDepth = maxDepth;
        }

        if (TryGetLong(root, "MaxFileSizeBytes", out var maxFileSize))
        {
            options.MaxFileSizeBytes = maxFileSize;
        }

        if (TryGetBool(root, "DryRun", out var dryRun))
        {
            options.DryRun = dryRun;
        }

        if (TryGetBool(root, "SeedInput", out var seedInput))
        {
            options.SeedInput = seedInput;
        }

        if (root.TryGetProperty("IncludePatterns", out var patternsElement) &&
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

        if (root.TryGetProperty("AzureBlob", out var azureRoot) &&
            azureRoot.ValueKind == JsonValueKind.Object)
        {
            if (TryGetBool(azureRoot, "Enabled", out var enabled))
            {
                options.AzureBlob.Enabled = enabled;
            }

            if (TryGetString(azureRoot, "ConnectionString", out var connectionString))
            {
                options.AzureBlob.ConnectionString = connectionString;
            }

            if (TryGetString(azureRoot, "AccountName", out var accountName))
            {
                options.AzureBlob.AccountName = accountName;
            }

            if (TryGetString(azureRoot, "ServiceUri", out var serviceUri))
            {
                options.AzureBlob.ServiceUri = serviceUri;
            }

            if (TryGetString(azureRoot, "ContainerName", out var container))
            {
                options.AzureBlob.ContainerName = container;
            }

            if (TryGetString(azureRoot, "BlobPrefix", out var blobPrefix))
            {
                options.AzureBlob.BlobPrefix = blobPrefix;
            }

            if (TryGetInt(azureRoot, "PageSize", out var pageSize))
            {
                options.AzureBlob.PageSize = pageSize;
            }

            if (TryGetInt(azureRoot, "MaxRetries", out var maxRetries))
            {
                options.AzureBlob.MaxRetries = maxRetries;
            }

            if (TryGetInt(azureRoot, "NetworkTimeoutSeconds", out var networkTimeoutSeconds))
            {
                options.AzureBlob.NetworkTimeoutSeconds = networkTimeoutSeconds;
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
}
