# ElBruno .NET Repository Conventions

These instructions define the conventions for ElBruno's .NET NuGet library repositories. Apply them when creating, modifying, or reviewing any project in this repo.

---

## Repository Structure

```
{ProjectName}/
├── README.md                    # Project overview, badges, quick start
├── LICENSE                      # MIT License
├── Directory.Build.props        # Shared MSBuild properties (all projects)
├── global.json                  # SDK version with rollForward
├── .gitignore                   # .NET + IDE + OS ignores
├── {ProjectName}.slnx           # XML-based solution file
├── src/
│   ├── {Owner}.{ProjectName}.{Package}/   # Library projects
│   ├── tests/{Package}.Tests/             # xUnit test projects
│   └── samples/{SampleName}/              # Sample/demo apps
├── docs/                        # All documentation except README.md and LICENSE
├── images/                      # All images including nuget_logo.png
└── .github/workflows/           # CI/CD workflows
```

- **Root files only:** README.md, LICENSE, Directory.Build.props, global.json, .gitignore, solution file
- **All source code** (libraries, tests, samples) lives under `src/`
- **All documentation** (except README.md and LICENSE) lives under `docs/`
- **All images** live under `images/`, including `nuget_logo.png` for NuGet packages

---

## ⚠️ Critical Rule: All Code Lives Under `src/`

**Every** project — libraries, tests, samples, benchmarks, tools — MUST be placed under the `src/` directory. Nothing gets created at the repo root except:
- README.md
- LICENSE
- Directory.Build.props
- global.json
- .gitignore
- .editorconfig
- {ProjectName}.slnx

**Structure under `src/`:**
- `src/{Owner}.{ProjectName}.{Feature}/` — Library projects (packable)
- `src/tests/{LibraryProject}.Tests/` — Test projects
- `src/samples/{SampleName}/` — Sample/demo apps
- `src/tools/{ToolName}/` — Tool projects (dotnet global tools)

**Golden rule: NEVER create project directories at the repo root. NEVER put tests or samples outside `src/`.**

---

## Solution Format

- Use `.slnx` (XML-based solution format), **not** `.sln`
- All projects (libraries, tests, samples) must be included in the solution file
- Use `<Folder>` elements for logical grouping: `/src/`, `/src/tests/`, `/src/samples/`

---

## Project Conventions

### Library Projects
- Target `net8.0;net10.0;net11.0` (multi-target: existing targets + latest .NET)
- Package naming: `{Owner}.{ProjectName}.{Feature}` (e.g., `ElBruno.QRCodeGenerator.Svg`)
- Each packable project must include the NuGet icon:
  ```xml
  <None Include="$(MSBuildThisFileDirectory)..\..\images\nuget_logo.png" Pack="true" PackagePath="" />
  ```
- Include `<InternalsVisibleTo>` for the corresponding test project
- Enable symbol packages: `<IncludeSymbols>true</IncludeSymbols>`, `<SymbolPackageFormat>snupkg</SymbolPackageFormat>`
- Enable deterministic CI builds: `<ContinuousIntegrationBuild Condition="'$(GITHUB_ACTIONS)' == 'true'">true</ContinuousIntegrationBuild>`

### Test Projects
- Target `net8.0;net10.0;net11.0`
- Framework: xUnit with coverlet.collector
- Naming: `{LibraryProject}.Tests`
- Location: `src/tests/{LibraryProject}.Tests/`
- Set `<IsPackable>false</IsPackable>` and `<IsTestProject>true</IsTestProject>`

### Tool Projects (dotnet global tools)
- Target `net8.0` only (single target — CI runners may not have preview SDKs)
- Set `<PackAsTool>true</PackAsTool>` and `<ToolCommandName>{toolname}</ToolCommandName>`

### Sample Projects
- Location: `src/samples/{SampleName}/`
- Reference library projects via `<ProjectReference>`

---

## Directory.Build.props

Place at repo root. Shared by all projects:

```xml
<Project>
  <PropertyGroup>
    <!-- Language -->
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <WarningsAsErrors>nullable</WarningsAsErrors>

    <!-- Code analysis -->
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>

    <!-- Repository information -->
    <RepositoryUrl>https://github.com/{owner}/{repo}</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PublishRepositoryUrl>true</PublishRepositoryUrl>

    <!-- Package defaults -->
    <Authors>Bruno Capuano (ElBruno)</Authors>
    <Company>Bruno Capuano</Company>
    <Copyright>Copyright © Bruno Capuano $([System.DateTime]::Now.Year)</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageIcon>nuget_logo.png</PackageIcon>
  </PropertyGroup>
</Project>
```

---

## global.json

Use `"rollForward": "latestMajor"` for flexibility across dev machines and CI:

```json
{
  "sdk": {
    "version": "11.0.100",
    "rollForward": "latestMajor"
  }
}
```

---

## .gitignore

Standard .NET patterns:

```gitignore
# Build outputs
[Bb]in/
[Oo]bj/
[Dd]ebug/
[Rr]elease/

# NuGet
*.nupkg
*.snupkg
.nuget/
packages/

# IDE
.vs/
*.user
*.suo
*.cache
*.DotSettings.user

# OS
Thumbs.db
.DS_Store
```

---

## GitHub Actions — CI Build (`build.yml`)

- **Triggers:** push to `main`, PR to `main`
- **Runner:** `ubuntu-latest`
- **SDK:** `dotnet-version: 8.0.x`, `10.0.x`, and `11.0.x`
- **Commands use solution-level operations** (not per-project)
- Build all target frameworks with the installed SDKs
- Test each target framework: `net8.0`, `net10.0`, and `net11.0`

```yaml
name: CI Build

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            8.0.x
            10.0.x
            11.0.x
      - name: Restore
        run: dotnet restore {ProjectName}.slnx
      - name: Build
        run: dotnet build {ProjectName}.slnx --no-restore
      - name: Test
        run: |
          dotnet test {ProjectName}.slnx --no-build --framework net8.0
          dotnet test {ProjectName}.slnx --no-build --framework net10.0
          dotnet test {ProjectName}.slnx --no-build --framework net11.0
```

---

## GitHub Actions — NuGet Publishing (`publish.yml`)

### Trusted Publishing via OIDC — NO API keys stored as secrets

- **Triggers:** `release` event (type: published) + manual `workflow_dispatch`
- **Environment:** `release` (requires `NUGET_USER` secret with NuGet.org username)
- **Permissions:** `id-token: write` (for OIDC) and `contents: read`
- **OIDC login:** Uses `NuGet/login@v1` action for token exchange — no `NUGET_API_KEY` needed
- **Version extraction:** from release tag (strip `v` prefix), manual input, or csproj fallback
- **Version validation:** regex `^[0-9]+\.[0-9]+\.[0-9]+`
- **Each packable project gets its own `dotnet pack` line**
- **Push with `--skip-duplicate`** to handle re-runs gracefully

```yaml
name: Publish to NuGet

on:
  release:
    types: [published]
  workflow_dispatch:
    inputs:
      version:
        description: "Package version (leave empty to use csproj version)"
        required: false
        type: string

jobs:
  publish:
    runs-on: ubuntu-latest
    environment: release
    permissions:
      id-token: write
      contents: read
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            8.0.x
            10.0.x
            11.0.x
      - name: Determine version
        id: version
        run: |
          # Extract from release tag, input, or csproj
          # Validate: ^[0-9]+\.[0-9]+\.[0-9]+
      - name: Restore
        run: dotnet restore {Solution}.slnx
      - name: Build
        run: dotnet build {Solution}.slnx -c Release --no-restore -p:Version=${{ steps.version.outputs.version }}
      - name: Test
        run: |
          dotnet test {Solution}.slnx -c Release --no-build --framework net8.0
          dotnet test {Solution}.slnx -c Release --no-build --framework net10.0
          dotnet test {Solution}.slnx -c Release --no-build --framework net11.0
      - name: Pack
        run: |
          dotnet pack src/{Package}/{Package}.csproj -c Release --no-build -p:Version=${{ steps.version.outputs.version }} --output ./nupkgs
          # Repeat for each packable project
      - name: NuGet login (OIDC trusted publishing)
        uses: NuGet/login@v1
        id: nuget-login
        with:
          user: ${{ secrets.NUGET_USER }}
      - name: Push to NuGet
        run: dotnet nuget push "./nupkgs/*.nupkg" --api-key ${{ steps.nuget-login.outputs.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate
```

---

## README.md Structure

Follow this exact order:

1. **Title:** `# {ProjectName}` (first line)
2. **Badges row** (immediately after title):
   - CI Build, Publish to NuGet, License: MIT, GitHub stars
   - Format: `[![Name](badge-url)](link-url)`
3. **Tagline** with emoji (one line, e.g., `## A family of .NET libraries for QR code generation 🟦`)
4. **Description** (one paragraph)
5. **Packages table** (if multi-package repo):
   | Package | NuGet | Downloads | Description |
   Using shields.io badges for NuGet version and downloads
6. **Per-package sections:** name, description, installation, quick start, sample/doc links
7. **Building from Source** section
8. **Documentation** section (links to `docs/`)
9. **License** section
10. **Author section** with Bruno's links:
    - Blog: https://elbruno.com
    - YouTube: https://youtube.com/@inthelabs
    - LinkedIn: https://linkedin.com/in/inthelabs
    - Twitter: https://twitter.com/inthelabs
    - Podcast: https://inthelabs.dev
11. **Acknowledgments** section

### README Rules
- Installation commands use `dotnet add package` — do NOT include `<PackageReference>` XML snippets
- Badge URLs use shields.io
- Sample links point to `src/samples/`
- Doc links point to `docs/`

---

## Testing

- Framework: **xUnit** with coverlet for coverage
- Test project naming: `{LibraryProject}.Tests`
- Tests live in `src/tests/{TestProject}/`
- All tests must pass before publishing (enforced in both CI and publish workflows)
- Test projects target `net8.0;net10.0;net11.0`

---

## NuGet Publishing — Complete Operational Guide

### Initial Setup (One-Time)

1. Create a GitHub **environment** named `release` in repository settings
2. Add `NUGET_USER` secret: your NuGet.org username (username only, not email)
3. Configure **NuGet trusted publisher** at nuget.org:
   - Sign in to nuget.org → Account settings → API keys → Trusted publishers
   - Add GitHub repository as a trusted publisher
   - **No API key needed** — OIDC token exchange handles authentication
4. Grant appropriate permissions:
   - CI/CD roles can have `id-token: write` and `contents: read` 
   - Environment protection rules can require manual approval for production releases

### Publishing Workflow (`publish.yml`)

The workflow triggers on GitHub releases or manual dispatch. Here's how it works:

**Trigger events:**
- `release` event (type: published) — automatic when a release is created on GitHub
- `workflow_dispatch` — manual trigger with optional version input

**Version extraction:**
1. **From release tag:** `refs/tags/vX.Y.Z` → strip `v` prefix to get `X.Y.Z`
2. **From manual input:** `workflow_dispatch` with `version` input parameter (optional)
3. **Fallback:** Read from csproj `<Version>` element if no tag or input provided
4. **Validation:** Must match regex `^[0-9]+\.[0-9]+\.[0-9]+` (semver format)

**Pre-release versions:**
- For pre-release builds, append `-preview.N` or `-rc.N` suffix (e.g., `0.1.0-preview.1`, `1.0.0-rc.2`)
- NuGet treats these as pre-release and excludes them from default install unless explicitly requested

**Build pipeline:**
1. **Restore** dependencies for every target framework
2. **Build** in Release configuration with injected version from steps above
3. **Test** to ensure all tests pass before publishing
4. **Pack** each packable library project separately:
   - Use `dotnet pack <project>.csproj -c Release --output ./nupkgs` for each packable project
   - Version is injected via `-p:Version=${{ steps.version.outputs.version }}`
5. **OIDC login** (NuGet/login@v1 action):
   - Exchanges GitHub OIDC token for temporary NuGet API key
   - No credentials are logged or stored
   - API key automatically expires and is never persisted
6. **Push to NuGet:**
   - Command: `dotnet nuget push "./nupkgs/*.nupkg" --api-key <api-key> --skip-duplicate`
   - `--skip-duplicate` prevents failures if version already exists on NuGet (safe for re-runs)
   - Symbol packages (.snupkg) are pushed automatically alongside .nupkg files

### Multi-Package Repositories

In repos with multiple packable projects, add one `dotnet pack` line per project in the workflow:

```yaml
- name: Pack
  run: |
    dotnet pack src/ElBruno.MarkItDotNet.Core/ElBruno.MarkItDotNet.Core.csproj -c Release --no-build -p:Version=${{ steps.version.outputs.version }} --output ./nupkgs
    dotnet pack src/ElBruno.MarkItDotNet.Extensions/ElBruno.MarkItDotNet.Extensions.csproj -c Release --no-build -p:Version=${{ steps.version.outputs.version }} --output ./nupkgs
```

All packages go to the same `./nupkgs` folder and are pushed in a single `dotnet nuget push` command.

### Environment Protection (Optional but Recommended)

The `release` environment can require manual approval before publishing:
- GitHub UI: Environment → "Deployment protection rules" → Add required reviewers
- This ensures a human reviews the version/release before it ships to NuGet

### Manual Publishing (`workflow_dispatch`)

The workflow can be triggered manually from the GitHub UI:
- Go to Actions → "Publish to NuGet" → Run workflow
- Optionally provide a version number; if left empty, uses csproj version
- Useful for hotfixes or re-publishing without creating a release

### Key Security Points

- ✅ **OIDC trusted publishing:** GitHub exchanges an OIDC token for a temporary API key; no long-lived secrets
- ✅ **Version in csproj:** Source of truth is the `<Version>` element in the library project
- ✅ **No hardcoded API keys:** Only `NUGET_USER` (username) is stored as a secret
- ✅ **Audit trail:** All publishes are linked to a GitHub release or workflow_dispatch action in the audit log

---

## Author Information

- **Author:** Bruno Capuano (ElBruno)
- **License:** MIT (all repos)
- **NuGet icon:** `nuget_logo.png` from `images/` folder, included via `Pack="true" PackagePath=""`
