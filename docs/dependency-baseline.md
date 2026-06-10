# Dependency Baseline Report

Date: 2026-06-10

This baseline captures package usage across all projects in `ElBruno.MarkItDotNet.slnx` and compares current versions against latest stable versions on NuGet.

## Scope summary

- 14 library/tool projects
- 17 test projects
- 20 sample projects
- 27 unique NuGet packages referenced

## Package matrix (current vs latest stable)

| Package | Current versions in repo | Latest stable | Library/Tool refs | Test refs | Sample refs | Total refs |
|---|---|---:|---:|---:|---:|---:|
| Azure.AI.DocumentIntelligence | 1.0.0 (1) | 1.0.0 | 1 | 0 | 0 | 1 |
| Azure.Identity | 1.13.2 (2) | 1.21.0 | 2 | 0 | 0 | 2 |
| Azure.Search.Documents | 11.7.0 (1) | 12.0.0 | 1 | 0 | 0 | 1 |
| ClosedXML | 0.105.0 (2) | 0.105.0 | 1 | 0 | 1 | 2 |
| coverlet.collector | 6.0.4 (17) | 10.0.1 | 0 | 17 | 0 | 17 |
| DocumentFormat.OpenXml | 3.3.0 (5) | 3.5.1 | 2 | 1 | 2 | 5 |
| ElBruno.Whisper | 0.1.5 (1) | 0.2.0 | 1 | 0 | 0 | 1 |
| FluentAssertions | 8.3.0 (17) | 8.10.0 | 0 | 17 | 0 | 17 |
| Microsoft.AspNetCore.OpenApi | 8.0.16 (1) | 10.0.9 | 0 | 0 | 1 | 1 |
| Microsoft.Extensions.AI | 9.5.0 (1) | 10.7.0 | 0 | 0 | 1 | 1 |
| Microsoft.Extensions.AI.Abstractions | 9.5.0 (2) | 10.7.0 | 1 | 1 | 0 | 2 |
| Microsoft.Extensions.DependencyInjection | 8.0.1 (2); 9.0.6 (25) | 10.0.9 | 1 | 10 | 16 | 27 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.6 (9) | 10.0.9 | 9 | 0 | 0 | 9 |
| Microsoft.Extensions.Hosting | 9.0.6 (1) | 10.0.9 | 0 | 0 | 1 | 1 |
| Microsoft.Extensions.VectorData.Abstractions | 10.1.0 (1) | 10.7.0 | 1 | 0 | 0 | 1 |
| Microsoft.NET.Test.Sdk | 18.3.0 (17) | 18.6.0 | 0 | 17 | 0 | 17 |
| NSubstitute | 5.1.0 (1); 5.3.0 (1) | 5.3.0 | 0 | 2 | 0 | 2 |
| PdfPig | 0.1.14 (4) | 0.1.14 | 2 | 0 | 2 | 4 |
| ReverseMarkdown | 4.6.0 (1) | 5.3.0 | 1 | 0 | 0 | 1 |
| RtfPipe | 2.0.0 (1) | 2.0.7677.4303 | 1 | 0 | 0 | 1 |
| Swashbuckle.AspNetCore | 6.6.2 (1) | 10.2.1 | 0 | 0 | 1 | 1 |
| System.CommandLine | 2.0.0-beta4.22272.1 (1) | 2.0.9 | 1 | 0 | 0 | 1 |
| System.Text.Encoding.CodePages | 9.0.6 (1) | 10.0.9 | 1 | 0 | 0 | 1 |
| System.Text.Json | 9.0.6 (1) | 10.0.9 | 0 | 0 | 1 | 1 |
| VersOne.Epub | 3.3.2 (1) | 3.3.6 | 1 | 0 | 0 | 1 |
| xunit | 2.9.0 (17) | 2.9.3 | 0 | 17 | 0 | 17 |
| xunit.runner.visualstudio | 3.1.0 (1); 3.1.5 (16) | 3.1.5 | 0 | 17 | 0 | 17 |

## Initial upgrade priorities

1. Centralize shared versions with `Directory.Packages.props` (reduce drift and duplication first).
2. Upgrade shared Microsoft runtime and DI packages used by many projects:
   - `Microsoft.Extensions.DependencyInjection*`
   - `Microsoft.Extensions.Hosting`
   - `System.Text.*`
3. Upgrade test stack uniformly:
   - `Microsoft.NET.Test.Sdk`
   - `xunit`, `xunit.runner.visualstudio`
   - `coverlet.collector`, `FluentAssertions`
4. Upgrade converter/runtime dependencies:
   - `DocumentFormat.OpenXml`
   - `ReverseMarkdown`
   - `RtfPipe`
   - `Azure.Identity`, `Azure.Search.Documents`
5. Keep stable versions across existing projects; allow preview only in the new Aspire/Foundry sample when required.
