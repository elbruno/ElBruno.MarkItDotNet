using System.Text;
using ElBruno.MarkItDotNet;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  MarkItDotNet - URL to Markdown Conversion Sample         ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

// --- Approach 1: Using MarkdownService.ConvertUrlAsync() directly ---
Console.WriteLine("── Approach 1: ConvertUrlAsync (direct URL) ─────────────────\n");

var services = new ServiceCollection();
services.AddMarkItDotNet();
var sp = services.BuildServiceProvider();

var markdownService = sp.GetRequiredService<MarkdownService>();

var url = args.Length > 0 ? args[0] : "https://example.com";
Console.WriteLine($"🌐 Fetching: {url}\n");

var result = await markdownService.ConvertUrlAsync(url);

if (result.Success)
{
    Console.WriteLine($"✅ Conversion succeeded! Words: {result.Metadata?.WordCount}, Time: {result.Metadata?.ProcessingTime.TotalMilliseconds:F0}ms\n");
    Console.WriteLine("── Markdown Output ──────────────────────────────────────");
    Console.WriteLine(result.Markdown);
}
else
{
    Console.WriteLine($"❌ Conversion failed: {result.ErrorMessage}");
}

// --- Approach 2: Using the .url stream approach ---
Console.WriteLine("\n── Approach 2: ConvertAsync with .url stream ────────────────\n");

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(url));
var streamResult = await markdownService.ConvertAsync(stream, ".url");

if (streamResult.Success)
{
    Console.WriteLine($"✅ Stream-based conversion succeeded! Words: {streamResult.Metadata?.WordCount}\n");
    Console.WriteLine("── Markdown Output (first 200 chars) ────────────────────");
    var preview = streamResult.Markdown.Length > 200
        ? streamResult.Markdown[..200] + "..."
        : streamResult.Markdown;
    Console.WriteLine(preview);
}
else
{
    Console.WriteLine($"❌ Conversion failed: {streamResult.ErrorMessage}");
}

Console.WriteLine("\n💡 Tip: Pass a URL as a command-line argument to convert any web page.");
