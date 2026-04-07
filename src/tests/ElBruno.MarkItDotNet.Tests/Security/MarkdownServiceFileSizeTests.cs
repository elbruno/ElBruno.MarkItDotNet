using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests.Security;

/// <summary>
/// Tests that MarkdownService enforces file size limits to prevent
/// resource exhaustion attacks via oversized files.
/// </summary>
public class MarkdownServiceFileSizeTests : IDisposable
{
    private readonly List<string> _tempFiles = [];

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
        GC.SuppressFinalize(this);
    }

    private string CreateTempFile(string extension, int sizeBytes)
    {
        var path = Path.Combine(Path.GetTempPath(), $"security-test-{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(path, new byte[sizeBytes]);
        _tempFiles.Add(path);
        return path;
    }

    private static MarkdownService CreateService(long maxFileSizeBytes)
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());
        var options = new MarkItDotNetOptions { MaxFileSizeBytes = maxFileSizeBytes };
        return new MarkdownService(registry, null, options);
    }

    [Fact]
    public async Task ConvertAsync_FilePath_ExceedsMaxSize_ReturnsFailure()
    {
        var service = CreateService(maxFileSizeBytes: 10);
        var path = CreateTempFile(".txt", 100);

        var result = await service.ConvertAsync(path);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeds maximum allowed size");
    }

    [Fact]
    public async Task ConvertAsync_FilePath_UnderMaxSize_Succeeds()
    {
        var service = CreateService(maxFileSizeBytes: 1000);
        var path = CreateTempFile(".txt", 5);
        File.WriteAllText(path, "Hello");

        var result = await service.ConvertAsync(path);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConvertAsync_Stream_SeekableExceedsMaxSize_ReturnsFailure()
    {
        var service = CreateService(maxFileSizeBytes: 10);
        using var stream = new MemoryStream(new byte[100]);

        var result = await service.ConvertAsync(stream, ".txt");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeds maximum allowed size");
    }

    [Fact]
    public async Task ConvertAsync_Stream_SeekableUnderMaxSize_Succeeds()
    {
        var service = CreateService(maxFileSizeBytes: 1000);
        using var stream = new MemoryStream("Hello"u8.ToArray());

        var result = await service.ConvertAsync(stream, ".txt");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConvertAsync_FileSizeLimitDisabled_LargeFileAllowed()
    {
        var service = CreateService(maxFileSizeBytes: 0); // disabled
        var path = CreateTempFile(".txt", 200);
        File.WriteAllText(path, "Some content that is reasonably long for the test");

        var result = await service.ConvertAsync(path);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConvertAsync_DefaultOptions_Has100MBLimit()
    {
        var options = new MarkItDotNetOptions();

        options.MaxFileSizeBytes.Should().Be(100 * 1024 * 1024);
    }

    [Fact]
    public async Task ConvertAsync_ExactlyAtLimit_Succeeds()
    {
        var service = CreateService(maxFileSizeBytes: 50);
        var path = CreateTempFile(".txt", 50);
        // Write exactly 50 bytes
        File.WriteAllBytes(path, new byte[50]);

        var result = await service.ConvertAsync(path);

        // File size equals limit (not exceeds), so it should succeed
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConvertAsync_OneByteOverLimit_ReturnsFailure()
    {
        var service = CreateService(maxFileSizeBytes: 50);
        var path = CreateTempFile(".txt", 51);

        var result = await service.ConvertAsync(path);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeds maximum allowed size");
    }
}
