using System.Diagnostics;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Excel;
using ElBruno.MarkItDotNet.PowerPoint;

var builder = WebApplication.CreateBuilder(args);

// Register MarkItDotNet converters
builder.Services.AddMarkItDotNet();
builder.Services.AddMarkItDotNetExcel();
builder.Services.AddMarkItDotNetPowerPoint();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------------------------------------
// Conversion endpoints
// ---------------------------------------------------------------------------

/// <summary>
/// Converts an uploaded file to Markdown and returns the result as JSON.
/// </summary>
app.MapPost("/api/convert", async (IFormFile file, MarkdownService markdownService, CancellationToken ct) =>
{
    if (file is null || file.Length == 0)
    {
        return Results.BadRequest(new { success = false, errorMessage = "No file uploaded." });
    }

    var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

    await using var stream = file.OpenReadStream();
    var result = await markdownService.ConvertAsync(stream, extension, ct);

    return Results.Ok(new
    {
        result.Success,
        result.Markdown,
        result.SourceFormat,
        result.ErrorMessage,
        Metadata = result.Metadata is not null
            ? new
            {
                result.Metadata.WordCount,
                ProcessingTime = result.Metadata.ProcessingTime.TotalMilliseconds + "ms"
            }
            : null
    });
})
.WithName("ConvertFile")
.WithOpenApi()
.DisableAntiforgery()
.WithTags("Conversion");

/// <summary>
/// Converts an uploaded file to Markdown and streams chunks as Server-Sent Events.
/// </summary>
app.MapPost("/api/convert/streaming", async (IFormFile file, MarkdownService markdownService, HttpContext httpContext, CancellationToken ct) =>
{
    if (file is null || file.Length == 0)
    {
        httpContext.Response.StatusCode = 400;
        await httpContext.Response.WriteAsync("No file uploaded.", ct);
        return;
    }

    var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

    httpContext.Response.ContentType = "text/event-stream";
    httpContext.Response.Headers.CacheControl = "no-cache";
    httpContext.Response.Headers.Connection = "keep-alive";

    await using var stream = file.OpenReadStream();

    try
    {
        await foreach (var chunk in markdownService.ConvertStreamingAsync(stream, extension, ct))
        {
            await httpContext.Response.WriteAsync($"data: {chunk}\n\n", ct);
            await httpContext.Response.Body.FlushAsync(ct);
        }

        await httpContext.Response.WriteAsync("event: done\ndata: [DONE]\n\n", ct);
        await httpContext.Response.Body.FlushAsync(ct);
    }
    catch (NotSupportedException ex)
    {
        await httpContext.Response.WriteAsync($"event: error\ndata: {ex.Message}\n\n", ct);
        await httpContext.Response.Body.FlushAsync(ct);
    }
})
.WithName("ConvertFileStreaming")
.WithOpenApi()
.DisableAntiforgery()
.WithTags("Conversion");

// ---------------------------------------------------------------------------
// Information endpoints
// ---------------------------------------------------------------------------

/// <summary>
/// Returns the list of all supported conversion formats.
/// </summary>
app.MapGet("/api/formats", (ConverterRegistry registry) =>
{
    var formats = registry.GetAll().Select(c => new
    {
        Type = c.GetType().Name,
        // Probe common extensions to discover which ones this converter handles
        SupportedExtensions = KnownExtensions.All
            .Where(ext => c.CanHandle(ext))
            .ToArray()
    });

    return Results.Ok(formats);
})
.WithName("GetFormats")
.WithOpenApi()
.WithTags("Information");

/// <summary>
/// Simple health-check endpoint.
/// </summary>
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi()
    .WithTags("Information");

app.Run();

/// <summary>
/// Well-known file extensions used to discover converter capabilities.
/// </summary>
static class KnownExtensions
{
    public static readonly string[] All =
    [
        ".txt", ".md", ".json", ".html", ".htm",
        ".docx", ".pdf", ".csv", ".xml", ".yaml", ".yml",
        ".rtf", ".epub",
        ".xlsx", ".xls",
        ".pptx", ".ppt",
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".webp", ".svg"
    ];
}
