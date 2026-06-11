using MarkItDotNet.FoundryHostedAgent.WebUi;
using MarkItDotNet.FoundryHostedAgent.WebUi.Components;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AgentUiOptions>(builder.Configuration.GetSection(AgentUiOptions.SectionName));

// Add HttpClient for the agent
// Aspire sets environment variables for referenced services, e.g.:
// MARKITDOTNET_AGENT_HTTP_PORT, MARKITDOTNET_AGENT_HTTP_HOST, or in connection string format
builder.Services.AddHttpClient<HostedAgentClient>((serviceProvider, client) =>
{
    // Try various environment variable naming patterns that Aspire might use
    var agentPort = Environment.GetEnvironmentVariable("MARKITDOTNET_AGENT_HTTP_PORT")
                    ?? Environment.GetEnvironmentVariable("Services__markitdotnet-agent__http__0__port")
                    ?? Environment.GetEnvironmentVariable("Services__markitdotnet-agent__http__port")
                    ?? "8088";
    var agentHost = Environment.GetEnvironmentVariable("MARKITDOTNET_AGENT_HTTP_HOST")
                    ?? Environment.GetEnvironmentVariable("Services__markitdotnet-agent__http__host")
                    ?? "localhost";
    var agentScheme = Environment.GetEnvironmentVariable("MARKITDOTNET_AGENT_HTTP_SCHEME")
                      ?? Environment.GetEnvironmentVariable("Services__markitdotnet-agent__http__scheme")
                      ?? "http";

    client.BaseAddress = new Uri($"{agentScheme}://{agentHost}:{agentPort}");
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
