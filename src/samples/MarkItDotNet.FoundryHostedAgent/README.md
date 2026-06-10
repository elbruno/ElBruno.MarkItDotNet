# MarkItDotNet.FoundryHostedAgent

Sample hosted agent service that converts document bytes to Markdown with `ElBruno.MarkItDotNet`.

## What this sample includes

- `Program.cs`: `POST /invocations` endpoint (Foundry hosted-agent style payload)
- `agent.yaml`: hosted agent metadata contract
- `Dockerfile`: container build for deployment
- `apphost.cs`: Aspire 13 single-file AppHost reference for local orchestration

## Local run

```bash
dotnet run --project src/samples/MarkItDotNet.FoundryHostedAgent/MarkItDotNet.FoundryHostedAgent.csproj
```

The API listens on `http://localhost:8088`.

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

## Deploy as Hosted Agent to Microsoft Foundry (azd-first)

Use the current hosted-agent quickstart workflow:

- Quickstart: https://learn.microsoft.com/azure/foundry/agents/quickstarts/quickstart-hosted-agent
- Source-code deploy: https://learn.microsoft.com/azure/foundry/agents/how-to/deploy-hosted-agent-code

Suggested flow:

1. Initialize hosted-agent project/deployment configuration with `azd ai agent init`.
2. Ensure `agent.yaml` points to hosted + `invocations` protocol.
3. Validate locally (`azd ai agent run` or direct `dotnet run`).
4. Deploy with `azd deploy`.
