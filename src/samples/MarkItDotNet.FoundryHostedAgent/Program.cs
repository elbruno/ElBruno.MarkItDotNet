using ElBruno.MarkItDotNet;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8088");

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
    var result = await markdownService.ConvertAsync(stream, request.Input.Extension);

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

public sealed record InvocationEnvelope(InvocationInput Input);

public sealed record InvocationInput(
    string? FileName,
    string Extension,
    string ContentBase64);

public sealed record InvocationResponse(InvocationOutput Output);

public sealed record InvocationOutput(
    string FileName,
    string Extension,
    string Markdown);
