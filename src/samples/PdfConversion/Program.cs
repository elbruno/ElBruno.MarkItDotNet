using ElBruno.MarkItDotNet;
using Microsoft.Extensions.DependencyInjection;
using UglyToad.PdfPig.Writer;

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  📕 PDF Conversion Sample                                ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

var services = new ServiceCollection();
services.AddMarkItDotNet();
var sp = services.BuildServiceProvider();
var markdownService = sp.GetRequiredService<MarkdownService>();

// Build a 2-page PDF in memory
var pdfBytes = CreateSamplePdf();
Console.WriteLine($"📝 Created in-memory PDF ({pdfBytes.Length} bytes, 2 pages)\n");

// ── Demo 1: Full conversion ────────────────────────────────────────────
Console.WriteLine("📄 DEMO 1: Full PDF → Markdown");
Console.WriteLine("─────────────────────────────────────────────────────────\n");

using (var stream = new MemoryStream(pdfBytes))
{
    var result = await markdownService.ConvertAsync(stream, ".pdf");
    if (result.Success)
    {
        Console.WriteLine("✅ PDF conversion succeeded!");
        Console.WriteLine($"   Format: {result.SourceFormat}");
        if (result.Metadata is not null)
            Console.WriteLine($"   Words: {result.Metadata.WordCount} | Time: {result.Metadata.ProcessingTime.TotalMilliseconds:F1}ms");
        Console.WriteLine($"\n{result.Markdown}");
    }
    else
    {
        Console.WriteLine($"❌ Failed: {result.ErrorMessage}");
    }
}

Console.WriteLine("\n");

// ── Demo 2: Streaming page-by-page ─────────────────────────────────────
Console.WriteLine("🔄 DEMO 2: Streaming PDF (page-by-page)");
Console.WriteLine("─────────────────────────────────────────────────────────\n");

using (var stream = new MemoryStream(pdfBytes))
{
    var pageNum = 0;
    await foreach (var chunk in markdownService.ConvertStreamingAsync(stream, ".pdf"))
    {
        pageNum++;
        Console.WriteLine($"── 📃 Page {pageNum} ──");
        Console.WriteLine(chunk.TrimEnd());
        Console.WriteLine();
    }
    Console.WriteLine($"✅ Streamed {pageNum} pages successfully!");
}

Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ✅ PDF Sample Complete!                                  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

static byte[] CreateSamplePdf()
{
    var builder = new PdfDocumentBuilder();

    var font = builder.AddStandard14Font(UglyToad.PdfPig.Fonts.Standard14Fonts.Standard14Font.Helvetica);

    // Page 1
    var page1 = builder.AddPage(UglyToad.PdfPig.Content.PageSize.Letter);
    page1.AddText("MarkItDotNet PDF Demo", 18, new UglyToad.PdfPig.Core.PdfPoint(72, 700), font);
    page1.AddText("This is page one of the sample PDF document.", 12, new UglyToad.PdfPig.Core.PdfPoint(72, 660), font);
    page1.AddText("It demonstrates in-memory PDF creation and conversion.", 12, new UglyToad.PdfPig.Core.PdfPoint(72, 640), font);
    page1.AddText("Features: PDF to Markdown, streaming support, heading detection.", 12, new UglyToad.PdfPig.Core.PdfPoint(72, 620), font);
    page1.AddText("- Paragraph lines are joined using their typical line spacing.", 12, new UglyToad.PdfPig.Core.PdfPoint(72, 580), font);
    page1.AddText("- List items remain separate in the Markdown output.", 12, new UglyToad.PdfPig.Core.PdfPoint(72, 560), font);

    // Page 2
    var page2 = builder.AddPage(UglyToad.PdfPig.Content.PageSize.Letter);
    page2.AddText("Second Page", 16, new UglyToad.PdfPig.Core.PdfPoint(72, 700), font);
    page2.AddText("This is the second page with additional content.", 12, new UglyToad.PdfPig.Core.PdfPoint(72, 660), font);
    page2.AddText("The streaming API yields each page as a separate chunk.", 12, new UglyToad.PdfPig.Core.PdfPoint(72, 640), font);

    return builder.Build();
}
