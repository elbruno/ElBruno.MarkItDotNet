# Building & Testing

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Git

## Clone the Repository

```bash
git clone https://github.com/elbruno/ElBruno.MarkItDotNet.git
cd ElBruno.MarkItDotNet
```

## Build

```bash
dotnet build ElBruno.MarkItDotNet.slnx
```

To build for a specific framework (e.g., when .NET 10 SDK is not installed):

```bash
dotnet build -p:TargetFrameworks=net8.0
```

## Run Tests

```bash
dotnet test ElBruno.MarkItDotNet.slnx
```

Or targeting a specific framework:

```bash
dotnet test -p:TargetFrameworks=net8.0
```

The test suite uses [xUnit](https://xunit.net/) with [FluentAssertions](https://fluentassertions.com/) and covers converters, connectors, ingestion workflows, and package-level integrations.

Golden-file comparisons normalize line endings (`CRLF`/`LF`) so results are stable across Windows and Linux runners.

## Run the Sample App

```bash
dotnet run --project src/samples/BasicConversion/BasicConversion.csproj
```

Run the hosted-agent + web UI sample:

```bash
dotnet run --project src/samples/MarkItDotNet.FoundryHostedAgent/MarkItDotNet.FoundryHostedAgent.csproj
```

Open `http://localhost:8088` to upload a file, set an agent URL, and view converted Markdown output.

Run with Aspire orchestration reference:

```bash
dotnet run src/samples/MarkItDotNet.FoundryHostedAgent/apphost.cs
```

## Project Structure

```
ElBruno.MarkItDotNet/
├── src/
│   ├── ElBruno.MarkItDotNet/          # Main library (packable NuGet)
│   │   ├── Converters/                # Built-in converters
│   │   ├── IMarkdownConverter.cs      # Core interface
│   │   ├── MarkdownService.cs         # Main service
│   │   ├── ConversionResult.cs        # Result type
│   │   └── ConverterRegistry.cs       # Converter resolver
│   ├── tests/
│   │   └── ElBruno.MarkItDotNet.Tests/  # xUnit test project
│   └── samples/
│       └── BasicConversion/           # Demo console app
├── docs/                              # Documentation
├── images/                            # Branding assets
├── Directory.Build.props              # Shared MSBuild properties
├── global.json                        # SDK version
└── ElBruno.MarkItDotNet.slnx         # Solution file
```

## CI/CD

- **CI** (`ci.yml`) — builds and tests on every push/PR to `main`
- **Publish** (`publish.yml`) — packs and pushes to NuGet.org on GitHub release (OIDC auth)

## Creating a Release

1. Ensure all tests pass
2. Create a GitHub release with a tag like `v0.1.0`
3. The publish workflow automatically builds, packs, and pushes to NuGet.org

## Azure Container Apps Notes (FoundryHostedAgent sample)

The `MarkItDotNet.FoundryHostedAgent` sample is container-ready:

- Dockerfile builds and publishes the app
- Runtime listens on port `8088` and honors `PORT`-based hosting configuration
- Configure `AgentUi__DefaultAgentUrl` to point the web UI at your deployed hosted-agent endpoint

When deploying to Azure Container Apps:

1. Configure ingress target port to `8088`
2. Set environment variables as needed (`PORT`, `AgentUi__DefaultAgentUrl`, `AgentUi__MaxUploadBytes`)
3. Validate `/health` and then test browser upload flow
