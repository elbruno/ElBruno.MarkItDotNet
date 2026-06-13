# Plan: `markitdown-dotnet-cli` Skill for awesome-copilot

## Goal

Create an [Agent Skill](https://agentskills.io/specification) that teaches AI agents how to use the `markitdown` CLI tool from ElBruno.MarkItDotNet. Contribute it to [github/awesome-copilot](https://github.com/github/awesome-copilot).

## Background

The `markitdown` CLI converts 15+ file formats to Markdown — ideal for AI pipelines, RAG ingestion, and documentation workflows. An Agent Skill would let any Copilot-enabled agent discover and use the tool automatically.

- **NuGet:** [ElBruno.MarkItDotNet.Cli](https://www.nuget.org/packages/ElBruno.MarkItDotNet.Cli)
- **Source:** [elbruno/ElBruno.MarkItDotNet](https://github.com/elbruno/ElBruno.MarkItDotNet)
- **CLI Docs:** [docs/cli.md](../docs/cli.md)

## Repos & Branches

| Repo | Branch | Purpose |
|------|--------|---------|
| `ElBruno.MarkItDotNet` | `feature/markitdown-cli-skill` | This plan + reference material |
| `awesome-copilot` (fork) | `skills/markitdown-dotnet-cli` (from `upstream/staged`) | Skill file for PR |

## Skill File Location

```
awesome-copilot/skills/markitdown-dotnet-cli/SKILL.md
```

## SKILL.md Structure

### Frontmatter

```yaml
---
name: markitdown-dotnet-cli
description: >-
  Convert 15+ file formats to Markdown using the markitdown .NET CLI tool.
  Use when converting PDF, DOCX, XLSX, PPTX, HTML, CSV, XML, YAML, RTF,
  EPUB, images, or URLs to Markdown — for AI pipelines, RAG ingestion,
  documentation workflows, or batch processing.
---
```

### Sections to Include

1. **Overview** — What the tool does, why agents should use it (convert files to clean Markdown for AI consumption)
2. **Installation** — `dotnet tool install -g ElBruno.MarkItDotNet.Cli` + verification
3. **Commands Reference**
   - `markitdown <file>` — single file conversion with options (`-o`, `--format`, `--streaming`, `-q`, `-v`)
   - `markitdown batch <directory>` — batch conversion (`-o`, `-r`, `--pattern`, `--parallel`, `--format`)
   - `markitdown url <url>` — web page to Markdown (`-o`, `--format`)
   - `markitdown formats` — list supported formats
4. **Supported Formats Table** — All 18 formats (core + satellite packages) with extensions and package info
5. **Exit Codes** — 0 (success), 1 (conversion error), 2 (file not found), 3 (unsupported format)
6. **Agent Workflow Patterns**
   - RAG ingestion: batch convert docs folder → vector DB
   - JSON metadata extraction: `--format json | jq .metadata`
   - Pipeline chaining: stdout piping to other tools
   - Batch with glob patterns for selective conversion
7. **Troubleshooting** — Common errors and fixes (file not found, unsupported format, memory issues)

### No Bundled Assets

The skill is self-contained in SKILL.md — no scripts or reference files needed.

## Implementation Steps

1. **Write `SKILL.md`** in `D:\github\awesome-copilot\skills\markitdown-dotnet-cli\`
2. **Validate** — `npm run skill:validate`
3. **Build** — `npm run build` (updates README tables)
4. **Commit** — Descriptive message
5. **Push** — To `origin` (elbruno/awesome-copilot)
6. **Open PR** — Target: `staged` branch on `github/awesome-copilot`
   - Title: `Add markitdown-dotnet-cli skill 🤖🤖🤖` (AI agent marker for fast-track)
   - Description: link to NuGet, source repo, supported formats

## References

- [awesome-copilot CONTRIBUTING.md — Adding Skills](https://github.com/github/awesome-copilot/blob/main/CONTRIBUTING.md#adding-skills)
- [awesome-copilot Skills README](https://github.com/github/awesome-copilot/blob/main/docs/README.skills.md)
- [Agent Skills Specification](https://agentskills.io/specification)
- Existing skill examples: `aspire/SKILL.md`, `nuget-manager/SKILL.md`
