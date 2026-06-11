using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace MarkItDotNet.FoundryHostedAgent.WebUi;

public sealed class HostedAgentClient(HttpClient httpClient, IHttpClientFactory httpClientFactory, IOptions<AgentUiOptions> options)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly AgentUiOptions _options = options.Value;

    public string AgentServiceName => _options.AgentServiceName;
    public long MaxUploadBytes => _options.MaxUploadBytes;

    public async Task<AgentConversionResult> ConvertAsync(
        IBrowserFile file, string? customUrl, CancellationToken cancellationToken)
    {
        if (file is null)
            return new AgentConversionResult(false, null, "No file was selected.", null, null);

        var extension = Path.GetExtension(file.Name);
        if (string.IsNullOrWhiteSpace(extension))
            return new AgentConversionResult(false, null, "The selected file must have an extension.", file.Name, null);

        if (file.Size <= 0)
            return new AgentConversionResult(false, null, "The selected file is empty.", file.Name, extension);

        if (file.Size > _options.MaxUploadBytes)
            return new AgentConversionResult(
                false, null,
                $"File exceeds the maximum upload size of {Math.Round(_options.MaxUploadBytes / 1024d / 1024d, 2)} MB.",
                file.Name, extension);

        await using var stream = file.OpenReadStream(_options.MaxUploadBytes, cancellationToken);
        await using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var payload = new InvocationEnvelope(new InvocationInput(
            file.Name,
            extension,
            Convert.ToBase64String(memory.ToArray())));

        // Use custom URL if provided, otherwise use the pre-configured HTTP client
        HttpClient client;
        if (!string.IsNullOrWhiteSpace(customUrl))
        {
            client = new HttpClient { BaseAddress = new Uri(customUrl.TrimEnd('/')) };
        }
        else
        {
            client = _httpClient;
        }

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/invocations", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            var endpoint = string.IsNullOrWhiteSpace(customUrl)
                ? $"service '{_options.AgentServiceName}'"
                : $"custom URL '{customUrl}'";
            return new AgentConversionResult(
                false, null, $"Could not reach the agent at {endpoint}: {ex.Message}", file.Name, extension);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new AgentConversionResult(
                false, null,
                $"Agent returned {(int)response.StatusCode} ({response.ReasonPhrase}). {body}",
                file.Name, extension, (int)response.StatusCode);
        }

        InvocationResponse? output;
        try
        {
            output = await response.Content.ReadFromJsonAsync<InvocationResponse>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            return new AgentConversionResult(
                false, null, $"Could not parse agent response: {ex.Message}", file.Name, extension);
        }

        if (output?.Output is null)
            return new AgentConversionResult(false, null, "Agent response did not contain output.", file.Name, extension);

        return new AgentConversionResult(
            true,
            output.Output.Markdown,
            null,
            output.Output.FileName,
            output.Output.Extension,
            (int)response.StatusCode);
    }
}
