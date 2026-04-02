using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;
using ImageConverter = ElBruno.MarkItDotNet.Converters.ImageConverter;

namespace ElBruno.MarkItDotNet.Tests;

public class ImageConverterTests
{
    private readonly ImageConverter _converter = new();

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".gif")]
    [InlineData(".bmp")]
    [InlineData(".webp")]
    [InlineData(".svg")]
    public void CanHandle_SupportedExtensions_ReturnsTrue(string extension)
    {
        _converter.CanHandle(extension).Should().BeTrue();
    }

    [Theory]
    [InlineData(".JPG")]
    [InlineData(".PNG")]
    [InlineData(".Gif")]
    public void CanHandle_IsCaseInsensitive(string extension)
    {
        _converter.CanHandle(extension).Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    [InlineData(".html")]
    [InlineData(".docx")]
    [InlineData(".tiff")]
    public void CanHandle_UnsupportedExtensions_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".png");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertAsync_ReturnsMarkdownImageReference()
    {
        using var stream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });

        var result = await _converter.ConvertAsync(stream, ".png");

        result.Should().Contain("![Image]");
        result.Should().Contain(".png");
    }

    [Fact]
    public async Task ConvertAsync_ContainsFormatInfo()
    {
        using var stream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });

        var result = await _converter.ConvertAsync(stream, ".jpg");

        result.Should().Contain("JPG");
        result.Should().Contain("Image");
    }

    [Theory]
    [InlineData(".jpg", "image.jpg")]
    [InlineData(".png", "image.png")]
    [InlineData(".gif", "image.gif")]
    [InlineData(".webp", "image.webp")]
    public async Task ConvertAsync_DifferentExtensions_UseCorrectFilename(string extension, string expectedFilename)
    {
        using var stream = new MemoryStream(new byte[] { 0x00, 0x01 });

        var result = await _converter.ConvertAsync(stream, extension);

        result.Should().Contain($"![Image]({expectedFilename})");
    }

    [Fact]
    public async Task ConvertAsync_ValidPng_DetectsDimensions()
    {
        // Create minimal 1x1 PNG
        var pngBytes = CreateMinimalPng(100, 50);
        using var stream = new MemoryStream(pngBytes);

        var result = await _converter.ConvertAsync(stream, ".png");

        result.Should().Contain("100");
        result.Should().Contain("50");
        result.Should().Contain("pixels");
    }

    [Fact]
    public async Task ConvertAsync_ValidGif_DetectsDimensions()
    {
        // Create minimal GIF header (GIF89a, 200x100)
        var gifBytes = new byte[]
        {
            0x47, 0x49, 0x46, 0x38, 0x39, 0x61, // GIF89a
            0xC8, 0x00, // width: 200 (little-endian)
            0x64, 0x00, // height: 100 (little-endian)
            0x00, 0x00, 0x00 // flags, bg, aspect
        };
        using var stream = new MemoryStream(gifBytes);

        var result = await _converter.ConvertAsync(stream, ".gif");

        result.Should().Contain("200");
        result.Should().Contain("100");
    }

    [Fact]
    public async Task ConvertAsync_SvgFormat_DoesNotCrash()
    {
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\"></svg>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svg));

        var result = await _converter.ConvertAsync(stream, ".svg");

        result.Should().Contain("![Image]");
        result.Should().Contain("SVG");
    }

    /// <summary>
    /// Creates a minimal PNG file header with specific dimensions.
    /// </summary>
    private static byte[] CreateMinimalPng(int width, int height)
    {
        var header = new byte[33]; // PNG signature (8) + IHDR chunk (25)

        // PNG signature
        header[0] = 0x89;
        header[1] = 0x50; // P
        header[2] = 0x4E; // N
        header[3] = 0x47; // G
        header[4] = 0x0D;
        header[5] = 0x0A;
        header[6] = 0x1A;
        header[7] = 0x0A;

        // IHDR chunk length (13 bytes)
        header[8] = 0x00;
        header[9] = 0x00;
        header[10] = 0x00;
        header[11] = 0x0D;

        // IHDR type
        header[12] = 0x49; // I
        header[13] = 0x48; // H
        header[14] = 0x44; // D
        header[15] = 0x52; // R

        // Width (big-endian)
        header[16] = (byte)((width >> 24) & 0xFF);
        header[17] = (byte)((width >> 16) & 0xFF);
        header[18] = (byte)((width >> 8) & 0xFF);
        header[19] = (byte)(width & 0xFF);

        // Height (big-endian)
        header[20] = (byte)((height >> 24) & 0xFF);
        header[21] = (byte)((height >> 16) & 0xFF);
        header[22] = (byte)((height >> 8) & 0xFF);
        header[23] = (byte)(height & 0xFF);

        // Bit depth, color type, compression, filter, interlace
        header[24] = 0x08; // 8-bit
        header[25] = 0x02; // RGB
        header[26] = 0x00;
        header[27] = 0x00;
        header[28] = 0x00;

        // CRC (just fill with zeros for our dimension-reading test)
        header[29] = 0x00;
        header[30] = 0x00;
        header[31] = 0x00;
        header[32] = 0x00;

        return header;
    }
}
