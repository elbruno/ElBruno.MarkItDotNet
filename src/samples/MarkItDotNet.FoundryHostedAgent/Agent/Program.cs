using ElBruno.MarkItDotNet;
using MarkItDotNet.FoundryHostedAgent;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

var portValue = builder.Configuration["PORT"];
var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 8088;
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add Aspire Service Defaults (includes OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

builder.Services.AddMarkItDotNet();

var app = builder.Build();

// Map default endpoints (health checks)
app.MapDefaultEndpoints();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("MarkItDotNet Agent starting on port {Port}", port);

app.MapPost("/invocations", async (InvocationEnvelope request, MarkdownService markdownService, CancellationToken cancellationToken) =>
{
    logger.LogInformation("Invocation received for file: {FileName}", request.Input?.FileName);

    if (request.Input is null)
    {
        logger.LogWarning("Missing input payload in invocation request");
        return Results.BadRequest(new { error = "Missing input payload." });
    }

    if (string.IsNullOrWhiteSpace(request.Input.Extension))
    {
        logger.LogWarning("Missing extension in input");
        return Results.BadRequest(new { error = "Input.extension is required." });
    }

    if (string.IsNullOrWhiteSpace(request.Input.ContentBase64))
    {
        logger.LogWarning("Missing contentBase64 in input");
        return Results.BadRequest(new { error = "Input.contentBase64 is required." });
    }

    byte[] bytes;
    try
    {
        bytes = Convert.FromBase64String(request.Input.ContentBase64);
    }
    catch (FormatException)
    {
        logger.LogError("Invalid Base64 format in contentBase64");
        return Results.BadRequest(new { error = "Input.contentBase64 is not valid Base64." });
    }

    logger.LogInformation("Converting file {FileName} with extension {Extension}", request.Input.FileName, request.Input.Extension);

    await using var stream = new MemoryStream(bytes);
    var result = await markdownService.ConvertAsync(stream, request.Input.Extension, cancellationToken);

    if (!result.Success)
    {
        logger.LogError("Conversion failed: {Error}", result.ErrorMessage);
        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    logger.LogInformation("Conversion successful for {FileName}", request.Input.FileName);

    return Results.Ok(new InvocationResponse(
        new InvocationOutput(
            request.Input.FileName ?? "document",
            request.Input.Extension,
            result.Markdown)));
});

logger.LogInformation("MarkItDotNet Agent started successfully");
await app.RunAsync();
