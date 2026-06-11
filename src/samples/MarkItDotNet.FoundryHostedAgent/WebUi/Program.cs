using MarkItDotNet.FoundryHostedAgent.WebUi;
using MarkItDotNet.FoundryHostedAgent.WebUi.Components;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire Service Defaults (includes OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

builder.Services.Configure<AgentUiOptions>(builder.Configuration.GetSection(AgentUiOptions.SectionName));

// Add HttpClient for the agent
// When using service discovery (default), the agent service name is resolved automatically
// by the ServiceDefaults AddServiceDiscovery() configuration.
// Custom URLs can be provided by the user in the UI.
builder.Services.AddHttpClient<HostedAgentClient>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Map default endpoints (health checks)
app.MapDefaultEndpoints();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("WebUi application starting");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

logger.LogInformation("WebUi application started successfully");
await app.RunAsync();
