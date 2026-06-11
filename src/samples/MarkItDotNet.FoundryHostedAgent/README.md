# MarkItDotNet.FoundryHostedAgent Scenario

Complete sample scenario for a hosted-agent backend plus a Blazor test frontend, orchestrated with Aspire.

## Folder layout

- `apphost.cs` — Aspire AppHost entry point (orchestrates both projects)
- `Agent/` — hosted agent backend (`/health`, `/invocations`)
- `WebUi/` — Blazor Server frontend to upload files, call the agent, and preview Markdown

## Run with Aspire (recommended)

From repository root:

```bash
dotnet run src/samples/MarkItDotNet.FoundryHostedAgent/apphost.cs
```

Aspire starts:

- `markitdotnet-agent`
- `markitdotnet-webapp`

The UI gets the backend endpoint automatically through environment wiring (`AgentUi__DefaultAgentUrl`).

## Run projects independently

Backend:

```bash
dotnet run --project src/samples/MarkItDotNet.FoundryHostedAgent/Agent/MarkItDotNet.FoundryHostedAgent.csproj
```

Frontend:

```bash
dotnet run --project src/samples/MarkItDotNet.FoundryHostedAgent/WebUi/MarkItDotNet.FoundryHostedAgent.WebUi.csproj
```

When running independently, set the agent URL in the UI textbox (or configure `AgentUi__DefaultAgentUrl`).

## Deployment notes

- `Agent/agent.yaml` and `Agent/Dockerfile` are used for hosted-agent/container deployment.
- The Web UI can target local or cloud endpoints by changing the agent URL textbox or `AgentUi__DefaultAgentUrl`.
