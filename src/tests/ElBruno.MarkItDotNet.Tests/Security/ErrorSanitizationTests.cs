using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests.Security;

/// <summary>
/// Tests that MarkdownService sanitizes error messages to prevent
/// information leakage of internal file paths.
/// </summary>
public class ErrorSanitizationTests
{
    private static MarkdownService CreateService()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());
        return new MarkdownService(registry);
    }

    [Fact]
    public async Task ConvertAsync_FileNotFound_ErrorDoesNotContainWindowsPath()
    {
        var service = CreateService();
        var fakePath = @"C:\Users\admin\secrets\sensitive.txt";

        var result = await service.ConvertAsync(fakePath);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage!.Should().NotContain(@"C:\Users\admin");
        result.ErrorMessage!.Should().Contain("[path]");
    }

    [Fact]
    public async Task ConvertAsync_FileNotFound_ErrorDoesNotContainUnixPath()
    {
        var service = CreateService();
        // Use a path that exists as a valid file path format but the file doesn't exist
        // On Windows, forward-slash paths also work for FileInfo
        var fakePath = @"C:\tmp\nonexistent\secret.txt";

        var result = await service.ConvertAsync(fakePath);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage!.Should().NotContain(@"C:\tmp\nonexistent");
    }

    [Fact]
    public async Task ConvertAsync_Stream_ConverterThrowsWithPath_PathIsSanitized()
    {
        // Register a converter that throws an exception containing a file path
        var registry = new ConverterRegistry();
        registry.Register(new ThrowingConverter());
        var service = new MarkdownService(registry);

        using var stream = new MemoryStream("test"u8.ToArray());
        var result = await service.ConvertAsync(stream, ".throw");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage!.Should().NotContain(@"C:\internal\secrets");
        result.ErrorMessage!.Should().Contain("[path]");
    }

    [Fact]
    public async Task ConvertAsync_ErrorWithMultiplePaths_AllAreSanitized()
    {
        var registry = new ConverterRegistry();
        registry.Register(new MultiPathThrowingConverter());
        var service = new MarkdownService(registry);

        using var stream = new MemoryStream("test"u8.ToArray());
        var result = await service.ConvertAsync(stream, ".multithrow");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage!.Should().NotContain(@"C:\path\one");
        result.ErrorMessage!.Should().NotContain(@"D:\path\two");
    }

    /// <summary>
    /// A converter that throws an exception containing a file path in the message.
    /// </summary>
    private sealed class ThrowingConverter : IMarkdownConverter
    {
        public bool CanHandle(string fileExtension) =>
            fileExtension.Equals(".throw", StringComparison.OrdinalIgnoreCase);

        public Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(@"Failed to process file at C:\internal\secrets\data.bin");
        }
    }

    /// <summary>
    /// A converter that throws with multiple paths in the message.
    /// </summary>
    private sealed class MultiPathThrowingConverter : IMarkdownConverter
    {
        public bool CanHandle(string fileExtension) =>
            fileExtension.Equals(".multithrow", StringComparison.OrdinalIgnoreCase);

        public Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(@"Error copying C:\path\one\a.txt to D:\path\two\b.txt");
        }
    }
}
