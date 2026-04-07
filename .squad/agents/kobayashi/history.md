# Kobayashi — History

## Project Context
- **Owner:** Bruno Capuano
- **Stack:** C#, .NET 8/10, NuGet library ecosystem
- **Description:** ElBruno.MarkItDotNet — .NET library converting 15+ file formats to Markdown, with satellite NuGet packages for Excel, PowerPoint, AI, Whisper, CLI tool, and more.
- **Key areas:** File I/O converters, URL fetching, PDF/DOCX/HTML parsing, AI integration (IChatClient), CLI tool (System.CommandLine)

## Learnings

### 2025-07-15 — First Full Security Audit
- **Dependency audit is clean**: All 49 projects report zero known CVEs across direct and transitive NuGet dependencies. Key dependencies: PdfPig, OpenXml, ReverseMarkdown, ClosedXML, VersOne.Epub, RtfPipe.
- **Top 3 concerns**: (1) SSRF in `UrlConverter` — validates scheme but not IP/hostnames, follows redirects; (2) No file size limits — all converters load entire files into memory; (3) Prompt injection in AI package — untrusted PDF text interpolated directly into prompts.
- **Good practices observed**: Consistent argument validation, CancellationToken propagation, read-only document opening, `leaveOpen: true` on StreamReaders, `[GeneratedRegex]` in newer packages.
- **`FileSyncStateStore` path traversal**: Sanitizes invalid filename chars but doesn't fully canonicalize paths with `Path.GetFullPath()`.
- **`XmlConverter` XXE**: Safe on .NET 8+ by default, but not explicitly configured for defense-in-depth.
- **Whisper temp files**: Uses GUID naming (good) but standard `File.Create` (no exclusive flags).
- **Report delivered**: `docs/security-audit.md` with 19 findings across 5 severity levels.
