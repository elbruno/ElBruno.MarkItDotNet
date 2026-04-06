using Xunit;
using FluentAssertions;

namespace ElBruno.MarkItDotNet.DocumentIntelligence.Tests;

/// <summary>
/// Tests for <see cref="DocumentIntelligenceOptions"/> default values and configuration.
/// </summary>
public class DocumentIntelligenceOptionsTests
{
    [Fact]
    public void DefaultModelId_ShouldBe_PrebuiltLayout()
    {
        var options = new DocumentIntelligenceOptions();

        options.ModelId.Should().Be("prebuilt-layout");
    }

    [Fact]
    public void DefaultEndpoint_ShouldBeNull()
    {
        var options = new DocumentIntelligenceOptions();

        options.Endpoint.Should().BeNull();
    }

    [Fact]
    public void DefaultApiKey_ShouldBeNull()
    {
        var options = new DocumentIntelligenceOptions();

        options.ApiKey.Should().BeNull();
    }

    [Fact]
    public void DefaultSupportedExtensions_ShouldContainExpectedFormats()
    {
        var options = new DocumentIntelligenceOptions();

        options.SupportedExtensions.Should().Contain(".pdf");
        options.SupportedExtensions.Should().Contain(".png");
        options.SupportedExtensions.Should().Contain(".jpg");
        options.SupportedExtensions.Should().Contain(".jpeg");
        options.SupportedExtensions.Should().Contain(".tiff");
        options.SupportedExtensions.Should().Contain(".bmp");
        options.SupportedExtensions.Should().Contain(".docx");
        options.SupportedExtensions.Should().Contain(".xlsx");
        options.SupportedExtensions.Should().Contain(".pptx");
    }

    [Fact]
    public void CustomConfiguration_ShouldOverrideDefaults()
    {
        var options = new DocumentIntelligenceOptions
        {
            Endpoint = "https://my-instance.cognitiveservices.azure.com",
            ApiKey = "test-key-123",
            ModelId = "prebuilt-read",
            SupportedExtensions = [".pdf", ".png"]
        };

        options.Endpoint.Should().Be("https://my-instance.cognitiveservices.azure.com");
        options.ApiKey.Should().Be("test-key-123");
        options.ModelId.Should().Be("prebuilt-read");
        options.SupportedExtensions.Should().HaveCount(2);
        options.SupportedExtensions.Should().Contain(".pdf");
        options.SupportedExtensions.Should().Contain(".png");
    }
}
