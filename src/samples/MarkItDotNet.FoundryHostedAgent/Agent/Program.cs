using ElBruno.MarkItDotNet;
using MarkItDotNet.FoundryHostedAgent;

var builder = WebApplication.CreateBuilder(args);

var portValue = builder.Configuration["PORT"];
var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 8088;
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddMarkItDotNet();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/invocations", async (InvocationEnvelope request, MarkdownService markdownService, CancellationToken cancellationToken) =>
{
    if (request.Input is null)
    {
        return Results.BadRequest(new { error = "Missing input payload." });
    }

    if (string.IsNullOrWhiteSpace(request.Input.Extension))
    {
        return Results.BadRequest(new { error = "Input.extension is required." });
    }

    if (string.IsNullOrWhiteSpace(request.Input.ContentBase64))
    {
        return Results.BadRequest(new { error = "Input.contentBase64 is required." });
    }

    byte[] bytes;
    try
    {
        bytes = Convert.FromBase64String(request.Input.ContentBase64);
    }
    catch (FormatException)
    {
        return Results.BadRequest(new { error = "Input.contentBase64 is not valid Base64." });
    }

    await using var stream = new MemoryStream(bytes);
    var result = await markdownService.ConvertAsync(stream, request.Input.Extension, cancellationToken);

    if (!result.Success)
    {
        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.Ok(new InvocationResponse(
        new InvocationOutput(
            request.Input.FileName ?? "document",
            request.Input.Extension,
            result.Markdown)));
});

await app.RunAsync();
