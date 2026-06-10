using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.PowerPoint;
using Microsoft.Extensions.DependencyInjection;
using A = DocumentFormat.OpenXml.Drawing;

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ElBruno.MarkItDotNet - PowerPoint Conversion Sample     ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

var services = new ServiceCollection();
services.AddMarkItDotNet();
services.AddMarkItDotNetPowerPoint();
var serviceProvider = services.BuildServiceProvider();
var markdownService = serviceProvider.GetRequiredService<MarkdownService>();

// ── Build a PPTX in memory ───────────────────────────────────────────────
Console.WriteLine("🎨 Creating PowerPoint presentation in memory...\n");

using var ms = new MemoryStream();
using (var doc = PresentationDocument.Create(ms, PresentationDocumentType.Presentation, true))
{
    var presPart = doc.AddPresentationPart();
    presPart.Presentation = new Presentation(new SlideIdList(), new SlideSize { Cx = 9144000, Cy = 6858000 });

    uint slideId = 256;

    // Slide 1 – Title
    AddSlide(presPart, ref slideId,
        ["MarkItDotNet Overview", "Convert any document to Markdown"],
        "This presentation demonstrates the MarkItDotNet library for .NET developers.");

    // Slide 2 – Bullet points
    AddSlide(presPart, ref slideId,
        ["Key Features", "PDF, DOCX, HTML conversion", "Excel tables to Markdown", "PowerPoint slides + notes", "Extensible plugin architecture"],
        null);
}

ms.Position = 0;

// ── Convert to Markdown ──────────────────────────────────────────────────
Console.WriteLine("📝 Converting to Markdown...");
Console.WriteLine("─────────────────────────────────────────────────────────\n");

var result = await markdownService.ConvertAsync(ms, ".pptx");
if (result.Success)
{
    Console.WriteLine("✅ PowerPoint conversion succeeded!");
    Console.WriteLine($"   Source format: {result.SourceFormat}\n");
    Console.WriteLine("Converted Markdown:");
    Console.WriteLine(result.Markdown);
}
else
{
    Console.WriteLine($"❌ Conversion failed: {result.ErrorMessage}");
}

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║           PowerPoint Sample Complete!                    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

// ── Helper: add a slide with text paragraphs and optional speaker notes ──
static void AddSlide(PresentationPart presPart, ref uint slideId, string[] texts, string? notes)
{
    var slidePart = presPart.AddNewPart<SlidePart>();
    var shape = new Shape(
        new NonVisualShapeProperties(
            new NonVisualDrawingProperties { Id = 1, Name = "Content" },
            new NonVisualShapeDrawingProperties(),
            new ApplicationNonVisualDrawingProperties()),
        new ShapeProperties(new A.Transform2D(
            new A.Offset { X = 457200, Y = 274638 },
            new A.Extents { Cx = 8229600, Cy = 5851525 })),
        new TextBody(texts.Select(t =>
            new A.Paragraph(new A.Run(
                new A.RunProperties { Language = "en-US" },
                new A.Text { Text = t }))).ToArray()));

    slidePart.Slide = new Slide(new CommonSlideData(new ShapeTree(
        new NonVisualGroupShapeProperties(
            new NonVisualDrawingProperties { Id = 1, Name = "" },
            new NonVisualGroupShapeDrawingProperties(),
            new ApplicationNonVisualDrawingProperties()),
        new GroupShapeProperties(), shape)));

    if (notes is not null)
    {
        var notesPart = slidePart.AddNewPart<NotesSlidePart>();
        var notesShape = new Shape(
            new NonVisualShapeProperties(
                new NonVisualDrawingProperties { Id = 2, Name = "Notes" },
                new NonVisualShapeDrawingProperties(),
                new ApplicationNonVisualDrawingProperties()),
            new ShapeProperties(),
            new TextBody(new A.Paragraph(new A.Run(
                new A.RunProperties { Language = "en-US" },
                new A.Text { Text = notes }))));
        notesPart.NotesSlide = new NotesSlide(new CommonSlideData(new ShapeTree(
            new NonVisualGroupShapeProperties(
                new NonVisualDrawingProperties { Id = 1, Name = "" },
                new NonVisualGroupShapeDrawingProperties(),
                new ApplicationNonVisualDrawingProperties()),
            new GroupShapeProperties(), notesShape)));
    }

    var presentation = presPart.Presentation ??= new Presentation(new SlideIdList());
    var slideIdList = presentation.SlideIdList ??= new SlideIdList();
    slideIdList.Append(new SlideId { Id = slideId++, RelationshipId = presPart.GetIdOfPart(slidePart) });
}
