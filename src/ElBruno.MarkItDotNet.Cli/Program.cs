using System.CommandLine;
using ElBruno.MarkItDotNet.Cli.Commands;

// === Root command: markitdown <file> ===
var fileArg = new Argument<FileInfo>(
    "file")
{
    Description = "File to convert to Markdown"
};

var outputOption = new Option<FileInfo?>("--output", "-o")
{
    Description = "Write output to file instead of stdout"
};

var formatOption = new Option<string>("--format")
{
    Description = "Output format: markdown or json",
    DefaultValueFactory = _ => "markdown"
};

var streamingOption = new Option<bool>("--streaming")
{
    Description = "Use streaming API for supported formats"
};

var quietOption = new Option<bool>("--quiet", "-q")
{
    Description = "Suppress status messages on stderr"
};

var verboseOption = new Option<bool>("--verbose", "-v")
{
    Description = "Show metadata (word count, timing, format)"
};

var rootCommand = new RootCommand("Convert files to Markdown — powered by ElBruno.MarkItDotNet");
rootCommand.Add(fileArg);
rootCommand.Add(outputOption);
rootCommand.Add(formatOption);
rootCommand.Add(streamingOption);
rootCommand.Add(quietOption);
rootCommand.Add(verboseOption);

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    return await ConvertCommand.HandleAsync(
        parseResult.GetValue(fileArg)!,
        parseResult.GetValue(outputOption),
        parseResult.GetValue(formatOption)!,
        parseResult.GetValue(streamingOption),
        parseResult.GetValue(quietOption),
        parseResult.GetValue(verboseOption),
        cancellationToken);
});

// === batch subcommand ===
var batchDirArg = new Argument<DirectoryInfo>(
    "directory")
{
    Description = "Directory containing files to convert"
};

var batchOutputOption = new Option<DirectoryInfo>("--output", "-o")
{
    Description = "Output directory for converted files",
    Required = true
};

var recursiveOption = new Option<bool>("--recursive", "-r")
{
    Description = "Process subdirectories recursively"
};

var patternOption = new Option<string?>("--pattern")
{
    Description = "File glob pattern (e.g. *.pdf)"
};

var parallelOption = new Option<int>("--parallel")
{
    Description = "Max parallel conversions",
    DefaultValueFactory = _ => Environment.ProcessorCount
};

var batchFormatOption = new Option<string>("--format")
{
    Description = "Output format: markdown or json",
    DefaultValueFactory = _ => "markdown"
};

var batchQuietOption = new Option<bool>("--quiet", "-q")
{
    Description = "Suppress status messages on stderr"
};

var batchCommand = new Command("batch", "Batch-convert all files in a directory");
batchCommand.Add(batchDirArg);
batchCommand.Add(batchOutputOption);
batchCommand.Add(recursiveOption);
batchCommand.Add(patternOption);
batchCommand.Add(parallelOption);
batchCommand.Add(batchFormatOption);
batchCommand.Add(batchQuietOption);

batchCommand.SetAction(async (parseResult, cancellationToken) =>
{
    return await BatchCommand.HandleAsync(
        parseResult.GetValue(batchDirArg)!,
        parseResult.GetValue(batchOutputOption)!,
        parseResult.GetValue(recursiveOption),
        parseResult.GetValue(patternOption),
        parseResult.GetValue(parallelOption),
        parseResult.GetValue(batchFormatOption)!,
        parseResult.GetValue(batchQuietOption),
        cancellationToken);
});

rootCommand.Add(batchCommand);

// === url subcommand ===
var urlArg = new Argument<string>(
    "url")
{
    Description = "HTTP/HTTPS URL to fetch and convert"
};

var urlOutputOption = new Option<FileInfo?>("--output", "-o")
{
    Description = "Write output to file instead of stdout"
};

var urlFormatOption = new Option<string>("--format")
{
    Description = "Output format: markdown or json",
    DefaultValueFactory = _ => "markdown"
};

var urlQuietOption = new Option<bool>("--quiet", "-q")
{
    Description = "Suppress status messages on stderr"
};

var urlCommand = new Command("url", "Convert a web page URL to Markdown");
urlCommand.Add(urlArg);
urlCommand.Add(urlOutputOption);
urlCommand.Add(urlFormatOption);
urlCommand.Add(urlQuietOption);

urlCommand.SetAction(async (parseResult, cancellationToken) =>
{
    return await UrlCommand.HandleAsync(
        parseResult.GetValue(urlArg)!,
        parseResult.GetValue(urlOutputOption),
        parseResult.GetValue(urlFormatOption)!,
        parseResult.GetValue(urlQuietOption),
        cancellationToken);
});

rootCommand.Add(urlCommand);

// === formats subcommand ===
var formatsCommand = new Command("formats", "List all supported file formats and their converters");

formatsCommand.SetAction((_, _) =>
{
    return FormatsCommand.HandleAsync();
});

rootCommand.Add(formatsCommand);

// Run
return await rootCommand.Parse(args).InvokeAsync();
