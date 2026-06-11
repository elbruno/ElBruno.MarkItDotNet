# MarkItDotNet.FoundryHostedAgent

Blazor Server + hosted agent sample that converts document bytes to Markdown with `ElBruno.MarkItDotNet`.

## What this sample includes

- `Program.cs`: Blazor Server host + `POST /invocations` endpoint (Foundry hosted-agent style payload)
- `Components/Pages/Home.razor`: web UI to upload a file, set agent URL, and view converted Markdown
- `HostedAgentClient.cs`: HTTP client that forwards uploaded files to the configured hosted agent URL
- `appsettings.json`: default agent URL and max upload settings for local/cloud configuration
- `agent.yaml`: hosted agent metadata contract
- `Dockerfile`: container build for deployment
- `apphost.cs`: Aspire 13 single-file AppHost reference for local orchestration

## Local run

```bash
dotnet run --project src/samples/MarkItDotNet.FoundryHostedAgent/MarkItDotNet.FoundryHostedAgent.csproj
```

The API listens on `http://localhost:8088`.

Open `http://localhost:8088` to use the web UI.

The page contains:

- a textbox for the agent endpoint URL (for example, `http://localhost:8088/invocations` locally, or your Azure URL)
- a file picker
- a conversion button
- a Markdown preview area

By default, the UI reads `AgentUi:DefaultAgentUrl` from `appsettings.json`.

Health check:

```bash
curl http://localhost:8088/health
```

Invocation example:

```bash
curl -X POST http://localhost:8088/invocations ^
  -H "Content-Type: application/json" ^
  -d "{ \"input\": { \"fileName\": \"sample.txt\", \"extension\": \".txt\", \"contentBase64\": \"SGVsbG8gTWFya2Rvd24h\" } }"
```

## Aspire reference (latest line)

Use the single-file AppHost with Aspire 13:

```bash
dotnet run src/samples/MarkItDotNet.FoundryHostedAgent/apphost.cs
```

This starts the sample in Aspire orchestration mode and keeps `PORT`-based endpoint wiring compatible for local and cloud hosting.

## Configuration

`appsettings.json` exposes web UI settings:

- `AgentUi:DefaultAgentUrl` — default URL shown in the textbox
- `AgentUi:MaxUploadBytes` — max allowed upload size in bytes

You can override these with environment variables, for example:

- `AgentUi__DefaultAgentUrl`
- `AgentUi__MaxUploadBytes`

## Deploy as Hosted Agent to Microsoft Foundry (azd-first)

Use the current hosted-agent quickstart workflow:

- Quickstart: [Hosted agent quickstart](https://learn.microsoft.com/azure/foundry/agents/quickstarts/quickstart-hosted-agent)
- Source-code deploy: [Deploy hosted agent from source code](https://learn.microsoft.com/azure/foundry/agents/how-to/deploy-hosted-agent-code)

Suggested flow:

1. Initialize hosted-agent project/deployment configuration with `azd ai agent init`.
2. Ensure `agent.yaml` points to hosted + `invocations` protocol.
3. Validate locally (`azd ai agent run` or direct `dotnet run`).
4. Deploy with `azd deploy`.

## Azure Container Apps readiness

This sample is container-ready out of the box:

- `Dockerfile` publishes and runs the app on port `8088`
- the app supports `PORT` environment-based binding
- web UI agent endpoint can be switched at runtime using the textbox or environment configuration

For Azure Container Apps, set the ingress target port to `8088`, and set `AgentUi__DefaultAgentUrl` to your deployed hosted agent endpoint.
