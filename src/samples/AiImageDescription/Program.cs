using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.AI;
using Microsoft.Extensions.AI;

Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  MarkItDotNet - AI Image Description Sample               ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

// --- Mock IChatClient for demo purposes ---
// In production, replace with a real client:
//   services.AddSingleton<IChatClient>(new OpenAIChatClient("gpt-4o", apiKey));
//   services.AddSingleton<IChatClient>(new AzureOpenAIChatClient(...));
var mockClient = new MockChatClient();

// Use a fresh registry so the AI converter handles images instead of the built-in one
var registry = new ConverterRegistry();
registry.RegisterPlugin(new AiConverterPlugin(mockClient));
var markdownService = new MarkdownService(registry);

// Create a minimal valid 1x1 PNG in-memory (PNG header + IHDR + IDAT + IEND)
byte[] pngBytes =
[
    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
    0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk (13 bytes)
    0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // 1x1 pixel
    0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, // 8-bit RGB
    0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, 0x54, // IDAT chunk
    0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, 0x00, // compressed pixel
    0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, // CRC
    0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, // IEND chunk
    0xAE, 0x42, 0x60, 0x82
];

Console.WriteLine("🖼️  Converting a 1x1 PNG image using AI-powered converter...\n");

using var imageStream = new MemoryStream(pngBytes);
var result = await markdownService.ConvertAsync(imageStream, ".png");

if (result.Success)
{
    Console.WriteLine("✅ AI image conversion succeeded!");
    Console.WriteLine($"   Words: {result.Metadata?.WordCount}\n");
    Console.WriteLine("── Markdown Output ──────────────────────────────────────");
    Console.WriteLine(result.Markdown);
}
else
{
    Console.WriteLine($"❌ Conversion failed: {result.ErrorMessage}");
}

Console.WriteLine("\n💡 Tip: Replace MockChatClient with a real IChatClient for actual AI descriptions.");

// --- Mock implementation returns a canned response ---
sealed class MockChatClient : IChatClient
{
    public ChatClientMetadata Metadata => new("MockChatClient");

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant,
            "## Image Analysis\n\nThis is a **1×1 pixel** PNG image with a single red pixel.\n\n" +
            "### Details\n- **Dimensions:** 1×1\n- **Color depth:** 8-bit RGB\n- **Content:** Solid color swatch\n\n" +
            "*No text detected via OCR.*"));
        return Task.FromResult(response);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
