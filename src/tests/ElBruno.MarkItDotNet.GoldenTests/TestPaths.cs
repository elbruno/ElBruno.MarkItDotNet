namespace ElBruno.MarkItDotNet.GoldenTests;

/// <summary>
/// Resolves paths to SharedTestData documents and expected output files.
/// </summary>
internal static class TestPaths
{
    private static readonly string BaseDir = AppContext.BaseDirectory;

    public static string DocumentsDir => Path.Combine(BaseDir, "SharedTestData", "documents");
    public static string MarkItDotNetExpectedDir => Path.Combine(BaseDir, "SharedTestData", "expected", "markitdotnet");
    public static string MarkItDownExpectedDir => Path.Combine(BaseDir, "SharedTestData", "expected", "markitdown");

    public static string GetDocument(string filename) => Path.Combine(DocumentsDir, filename);

    public static string GetExpectedMarkItDotNet(string sourceFilename)
    {
        var baseName = Path.GetFileNameWithoutExtension(sourceFilename);
        var ext = Path.GetExtension(sourceFilename).ToLowerInvariant().Replace(".", "_");
        return Path.Combine(MarkItDotNetExpectedDir, $"{baseName}{ext}.md");
    }

    public static string GetExpectedMarkItDown(string sourceFilename)
    {
        var baseName = Path.GetFileNameWithoutExtension(sourceFilename);
        var ext = Path.GetExtension(sourceFilename).ToLowerInvariant().Replace(".", "_");
        return Path.Combine(MarkItDownExpectedDir, $"{baseName}{ext}.md");
    }

    public static string ReadExpected(string path) =>
        File.Exists(path) ? File.ReadAllText(path) : throw new FileNotFoundException($"Expected file not found: {path}");
}
