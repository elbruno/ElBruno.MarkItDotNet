# Project Context

- **Owner:** Bruno Capuano
- **Project:** ElBruno.MarkItDotNet — .NET library converting 15+ file formats to Markdown. Building a CLI tool (`markitdown`) and AI skills.
- **Stack:** C#, .NET 8/10, System.CommandLine, NuGet, xUnit, FluentAssertions
- **Created:** 2026-04-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
- **CLI test project created**: `src/tests/ElBruno.MarkItDotNet.Cli.Tests/` with 28 passing tests across 5 test classes (FormatsCommand, ConvertCommand, BatchCommand, UrlCommand, OutputFormatter).
- **CLI tests use process-based integration testing**: Tests invoke `dotnet run --project ...` via `CliRunner` helper, capturing stdout/stderr/exit codes. This tests the full CLI pipeline including argument parsing.
- **CLI exit codes**: 0=success, 2=file not found, 3=unsupported format, 1=general error.
- **UrlConverter handles invalid URLs gracefully**: The UrlConverter returns success (exit 0) for non-fetchable URLs like "not-a-valid-url" rather than failing.
- **System.CommandLine beta4 requires InvocationContext lambdas**: `SetHandler` with method groups doesn't support `CancellationToken` binding; use `async (InvocationContext ctx) => { ... }` pattern instead.
- **Security test coverage is minimal**: Of 699 total tests, only ~14 (2%) are security-relevant. 67% are happy-path. The highest-risk gaps are SSRF (UrlConverter), path traversal (CLI + MarkdownService), XML bombs/XXE (XmlConverter), credential leakage (Azure integrations), and corrupt binary file handling (PDF/DOCX/EPUB converters).
- **Full gap analysis written**: `docs/security-test-gaps.md` — 40 recommended security tests across 4 phases. Phase 1 has 11 critical tests that should block any public release.
- **Test project inventory**: 17 test projects, 100 .cs test files. Largest: MarkItDotNet.Tests (188 tests, 19 files). Smallest: PowerPoint.Tests (6 tests, 1 file), GoldenTests (5 tests, 4 files).
