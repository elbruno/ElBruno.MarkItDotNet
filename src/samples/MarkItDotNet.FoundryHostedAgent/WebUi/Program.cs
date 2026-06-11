using MarkItDotNet.FoundryHostedAgent.WebUi;
using MarkItDotNet.FoundryHostedAgent.WebUi.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AgentUiOptions>(builder.Configuration.GetSection(AgentUiOptions.SectionName));
builder.Services.AddHttpClient<HostedAgentClient>();
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
