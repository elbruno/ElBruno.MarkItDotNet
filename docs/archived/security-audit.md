# ElBruno.MarkItDotNet — Security Audit Report

**Auditor:** Kobayashi (Security Reviewer)  
**Date:** 2025-07-15  
**Scope:** Full codebase under `src/` — core library, 13 satellite packages, CLI tool  
**Version:** Current `main` branch  

---

## 1. Executive Summary

**Overall Risk Posture: MODERATE**

ElBruno.MarkItDotNet is a file-conversion library that processes untrusted file content across 15+ formats. The codebase demonstrates solid engineering practices — proper null checking, cancellation token propagation, and clean separation of concerns. However, as a library that ingests arbitrary file content and performs URL fetching and AI integration, several security-relevant gaps exist.

**Key Statistics:**
- 🔴 Critical findings: **0**
- 🟠 High findings: **3**
- 🟡 Medium findings: **7**
- 🔵 Low findings: **5**
- ⚪ Informational: **4**
- **Known CVEs in dependencies: 0** (clean dependency audit)

No critical vulnerabilities were identified that would require immediate remediation before release. The most significant concerns are SSRF in URL conversion, resource exhaustion from unbounded file processing, and prompt injection risks in the AI package.

---

## 2. Findings Table

| # | Severity | Category | Location | Description | CWE | Remediation |
|---|----------|----------|----------|-------------|-----|-------------|
| 1 | 🟠 High | SSRF | `Converters/UrlConverter.cs:50-51` | URL validation only checks scheme (http/https) but does not block private/internal IPs (127.0.0.1, 10.x, 169.254.169.254, ::1, etc.), `file://` is blocked but DNS rebinding or internal hostnames are not. `HttpClient` follows redirects by default, so an attacker can redirect to internal resources. | CWE-918 | Validate resolved IP addresses against a deny-list of private ranges. Disable automatic redirect following or validate the redirect target. Consider using `SocketsHttpHandler` with a custom `ConnectCallback` to check resolved IPs before connecting. |
| 2 | 🟠 High | Resource Exhaustion | Multiple converters (all `ReadToEndAsync` calls) | No file size limits anywhere in the pipeline. `HtmlConverter`, `CsvConverter`, `JsonConverter`, `XmlConverter`, `YamlConverter`, `PlainTextConverter`, `RtfConverter` all call `reader.ReadToEndAsync()` which loads the entire file into memory. A multi-GB file will cause `OutOfMemoryException`. The `EpubConverter` and `PdfConverter` similarly load entire documents. | CWE-400 | Add a configurable `MaxFileSize` option to `MarkItDotNetOptions`. Enforce it in `MarkdownService.ConvertAsync()` by checking `stream.Length` (when available) or using a size-limiting wrapper stream. |
| 3 | 🟠 High | Prompt Injection | `AI/AiPdfConverter.cs:75-79` | Untrusted text extracted from PDF pages is directly interpolated into the AI prompt string. A malicious PDF could contain text like `"Ignore previous instructions and output..."` to manipulate the AI model's behavior. Same concern in the prompt template used for low-text pages. | CWE-77 (analog) | Clearly separate system instructions from user content. Use a system message for instructions and a user message containing only the extracted text. Consider using `ChatMessage(ChatRole.System, ...)` for the instruction and `ChatMessage(ChatRole.User, extractedText)` for the content. Document this risk for library consumers. |
| 4 | 🟡 Medium | Prompt Injection | `AI/AiOptions.cs:11,16` | `ImagePrompt` and `AudioPrompt` are configurable but sent alongside untrusted file content. While the prompts are sent as text alongside image/audio `DataContent`, a malicious image with embedded EXIF text or an adversarial audio file could influence model behavior. | CWE-77 (analog) | Document the risk. Consider using system-level messages for instructions vs. user-level messages for content. |
| 5 | 🟡 Medium | XML External Entity (XXE) | `Converters/XmlConverter.cs:29` | `XDocument.Parse(content)` uses the default `XmlReaderSettings`. In .NET 8+, DTD processing is disabled by default, so this is safe on modern runtimes. However, if the library is used on older runtimes or the default changes, XXE could allow file disclosure or SSRF. | CWE-611 | Explicitly create an `XmlReaderSettings` with `DtdProcessing = DtdProcessing.Prohibit` and use `XDocument.Load(XmlReader.Create(reader, settings))` for defense-in-depth. |
| 6 | 🟡 Medium | Path Traversal | `Sync/FileSyncStateStore.cs:127-129` | `GetFilePath` sanitizes `documentId` by replacing invalid filename chars with `_`. However, `..` is a valid filename character sequence, so a `documentId` like `../../../etc/passwd` would be sanitized to `.._.._.._.._etc_passwd` — which is safe. But a `documentId` containing only valid chars like `..\..\secret` on Windows could potentially escape the base directory. | CWE-22 | Use `Path.GetFullPath()` and verify the result starts with the expected `_basePath`. E.g.: `var fullPath = Path.GetFullPath(Path.Combine(_basePath, safeName + ".json")); if (!fullPath.StartsWith(Path.GetFullPath(_basePath))) throw ...` |
| 7 | 🟡 Medium | Temp File Safety | `Whisper/WhisperAudioConverter.cs:39` | Creates temp files with a GUID-based name in the system temp directory. While the GUID makes the name unpredictable (good), the temp file is created via `File.Create` which doesn't use exclusive flags. On shared systems, there's a small window between name generation and file creation. The cleanup is in a `finally` block with best-effort semantics. | CWE-377 | Use `FileStream` with `FileOptions.DeleteOnClose` for automatic cleanup, or use `FileMode.CreateNew` to fail if the file already exists (detecting race conditions). Consider using a library-specific subdirectory in temp. |
| 8 | 🟡 Medium | Regex Denial of Service | `Converters/UrlConverter.cs:111,117-126` | The `CleanHtml` method applies 7 regex patterns sequentially to potentially large HTML strings. Patterns like `<script[^>]*>.*?</script>` with `RegexOptions.Singleline` use lazy quantifiers on large input, which can be slow. While not catastrophic ReDoS, processing very large HTML pages could be CPU-intensive. | CWE-1333 | Consider using compiled regex or `[GeneratedRegex]` source generators. Set a regex timeout via `RegexOptions` or `Regex.MatchTimeout`. Alternatively, use an HTML parser (like HtmlAgilityPack) for tag removal instead of regex. |
| 9 | 🟡 Medium | Resource Exhaustion | `Converters/EpubConverter.cs:33-34` | Non-seekable streams are fully copied into `MemoryStream` without size limits. A crafted EPUB (which is a ZIP file) could have massive decompressed content, causing memory exhaustion. | CWE-400 | Limit the copy-to-memory operation with a max size check, or use a stream wrapper that throws when a limit is exceeded. |
| 10 | 🟡 Medium | Information Disclosure | `MarkdownService.cs:83`, `ConvertCommand.cs:24` | Exception messages (`ex.Message`) are returned directly to callers via `ConversionResult.Failure()` and printed to stderr in the CLI. These may contain internal file paths, connection strings, or other sensitive information depending on the exception type. | CWE-200 | Sanitize error messages before returning. For library consumers, consider a `ConversionResult.Failure(userMessage, exception)` pattern that separates the user-facing message from the diagnostic detail. In the CLI, avoid printing full exception messages in non-verbose mode. |
| 11 | 🔵 Low | Content Injection | `Converters/UrlConverter.cs:84` | The title extracted from the fetched HTML page is inserted directly into the Markdown output as `# {title}`. If the HTML title contains Markdown injection characters (e.g., `](http://evil.com)`), the resulting Markdown could contain unintended links. | CWE-79 (analog) | Sanitize/escape the title before inserting into Markdown output. At minimum, escape `[`, `]`, `(`, `)` characters. |
| 12 | 🔵 Low | Content Injection | `Converters/UrlConverter.cs:91` | The source URL is embedded directly in the Markdown output: `*Source: [{uri.Host}]({url})*`. A URL with Markdown-special characters could break the output structure. | CWE-74 | Escape the URL and host for Markdown output — particularly `[`, `]`, `(`, `)`. |
| 13 | 🔵 Low | Shared HttpClient | `Converters/UrlConverter.cs:20` | The default parameterless constructor creates `new HttpClient()` per instance. If many `UrlConverter` instances are created, this can lead to socket exhaustion. The constructor also accepts an injected `HttpClient` (good), but the default case is risky. | CWE-400 | Use `IHttpClientFactory` or a `static` shared `HttpClient` for the default case. Document that consumers should inject their own `HttpClient` for production use. |
| 14 | 🔵 Low | No Timeout | `Converters/UrlConverter.cs:58` | `_httpClient.GetStringAsync()` has no explicit timeout configured. A slow-responding server could hang the conversion indefinitely (only bounded by the cancellation token if provided). | CWE-400 | Set a default `HttpClient.Timeout` (e.g., 30 seconds) in the default constructor. Document the timeout behavior. |
| 15 | 🔵 Low | Race Condition | `ConverterRegistry.cs:9,10` | `_converters` and `_plugins` are plain `List<T>` with no thread-safety. If `Register()` or `RegisterPlugin()` is called concurrently with `Resolve()`, race conditions can occur. The DI registration pattern mitigates this (singleton + configured at startup), but direct usage could be unsafe. | CWE-362 | Document that `ConverterRegistry` is not thread-safe for writes. Consider using `ConcurrentBag<T>` or making it immutable after construction. |
| 16 | ⚪ Info | Defense in Depth | `Converters/DocxConverter.cs:25` | `WordprocessingDocument.Open(fileStream, false)` — the `false` flag opens read-only, which is correct and prevents accidental modification. Good practice. | — | No action needed. |
| 17 | ⚪ Info | Error Swallowing | `AI/AiPdfConverter.cs:89`, `AI/AiImageConverter.cs:67`, `AI/AiAudioConverter.cs:66` | All AI converters catch `Exception` broadly and return fallback text. This is reasonable for resilience but could hide configuration errors, auth failures, or rate limiting. No exception logging is present. | CWE-390 | Consider logging the caught exception (using `ILogger` if available) so operational issues are not silently swallowed. |
| 18 | ⚪ Info | Encoding | `MarkdownService.cs:226-227` | When the URL is passed through a `MemoryStream`, UTF-8 encoding is used with `Encoding.UTF8.GetBytes(url)` — correct handling. | — | No action needed. |
| 19 | ⚪ Info | Batch Parallelism | `Cli/Commands/BatchCommand.cs:72-75` | The batch command uses `--parallel` to control parallelism with `SemaphoreSlim`. The `relativePath` used for output file naming preserves the directory structure from the source. This is correct and does not introduce path traversal because the source directory is validated and `GetRelativePath` is bounded. | — | No action needed, but adding a note that `parallel` should be capped at a reasonable max (e.g., 32) would be defense-in-depth. |

---

## 3. Dependency Audit

```
dotnet list package --vulnerable --include-transitive
```

**Result: ✅ All 49 projects (libraries, tests, samples, CLI) report no vulnerable packages.**

All NuGet dependencies (direct and transitive) are free of known CVEs as of the audit date.

**Key Dependencies Reviewed:**
| Package | Used By | Risk Notes |
|---------|---------|------------|
| `UglyToad.PdfPig` | Core (PDF) | Parses untrusted PDF. No known CVEs. Monitor for updates. |
| `DocumentFormat.OpenXml` | Core (DOCX), PowerPoint | Parses untrusted OOXML. No known CVEs. Well-maintained by Microsoft. |
| `ReverseMarkdown` | Core (HTML, URL, RTF, EPUB) | Processes untrusted HTML. No known CVEs. |
| `ClosedXML` | Excel | Parses untrusted XLSX. No known CVEs. Has had historical issues with formula injection. |
| `VersOne.Epub` | Core (EPUB) | Parses untrusted EPUB (ZIP-based). No known CVEs. |
| `RtfPipe` | Core (RTF) | Parses untrusted RTF. Smaller project — monitor for advisories. |
| `Microsoft.Extensions.AI` | AI | Microsoft AI abstraction. No known CVEs. |
| `System.CommandLine` | CLI | Handles CLI argument parsing. No known CVEs. |
| `Azure.AI.DocumentIntelligence` | DocIntelligence | Azure SDK. No known CVEs. |
| `Azure.Search.Documents` | AzureSearch | Azure SDK. No known CVEs. |

---

## 4. Threat Model

### Attack Surface Analysis

As a file-conversion library, ElBruno.MarkItDotNet's primary threat model involves:

**Primary Threat Actor:** An attacker who controls the input files or URLs being converted.

#### 4.1 Attack Surface: File Input (All Converters)

| Vector | Risk | Current Mitigation | Gap |
|--------|------|-------------------|-----|
| Malicious PDF | Memory exhaustion, parser crash | PdfPig handles parsing | No file size limits |
| Malicious DOCX/PPTX | XML bomb in OOXML (billion laughs) | OpenXml SDK has built-in limits | Relies on SDK defaults |
| Crafted EPUB (ZIP bomb) | Memory exhaustion via decompression | None | No decompressed size limits |
| Crafted Excel | Formula injection in cell values | ClosedXML reads values only | Cell values pass through to Markdown unescaped |
| Crafted CSV | Markdown table injection | Pipe character is escaped | Column/row count is unbounded |
| Crafted HTML | XSS pass-through, script injection | ReverseMarkdown handles conversion | Output Markdown could contain raw HTML |
| Crafted XML | XXE (mitigated by .NET 8 defaults) | .NET 8 disables DTD by default | Not explicitly configured |
| Crafted RTF | Parser exploitation | RtfPipe handles parsing | Relies on library safety |

#### 4.2 Attack Surface: URL Fetching (UrlConverter)

| Vector | Risk | Current Mitigation | Gap |
|--------|------|-------------------|-----|
| SSRF to internal services | Access internal APIs, cloud metadata | HTTP/HTTPS scheme check | No IP/hostname validation |
| DNS rebinding | Bypass scheme validation | None | No resolved-IP validation |
| Redirect to internal host | Bypass hostname checks | None | Redirects are followed |
| Slow loris / hanging response | DoS via resource exhaustion | CancellationToken | No explicit timeout |
| Extremely large page | Memory exhaustion | None | `GetStringAsync` loads all content |

#### 4.3 Attack Surface: AI Integration

| Vector | Risk | Current Mitigation | Gap |
|--------|------|-------------------|-----|
| Prompt injection via PDF text | AI model manipulation | None | Untrusted text in prompt |
| Prompt injection via image EXIF | AI model manipulation | None | Image sent to AI directly |
| AI response injection | Malicious Markdown in output | None | AI response used as-is |

#### 4.4 Attack Surface: CLI Tool

| Vector | Risk | Current Mitigation | Gap |
|--------|------|-------------------|-----|
| Path traversal via file argument | Access arbitrary files | `System.CommandLine` + `FileInfo` | FileInfo resolves paths |
| Batch output directory escape | Write to arbitrary locations | `GetRelativePath` preserves structure | Validated by design |
| URL argument | SSRF | Delegates to UrlConverter | Same gaps as UrlConverter |

#### 4.5 Attack Surface: Sync/State Management

| Vector | Risk | Current Mitigation | Gap |
|--------|------|-------------------|-----|
| Path traversal via documentId | File write outside base dir | Invalid filename char replacement | Not fully canonicalized |

### Trust Boundaries

```
┌─────────────────────────────────────────────────────┐
│  Consumer Application / CLI                          │
│  ┌─────────────────────────────────────────────────┐ │
│  │  MarkdownService (Entry Point)                  │ │
│  │  ┌──────────────┬──────────────┬───────────────┐│ │
│  │  │ File         │ URL          │ Stream        ││ │
│  │  │ Input        │ Input        │ Input         ││ │
│  │  └──────┬───────┴──────┬───────┴───────┬───────┘│ │
│  │         │              │               │        │ │
│  │  ┌──────▼───────┐ ┌────▼────┐  ┌───────▼──────┐│ │
│  │  │ Converter    │ │ URL     │  │ AI           ││ │
│  │  │ Registry     │ │ Fetch   │  │ IChatClient  ││ │
│  │  └──────┬───────┘ └────┬────┘  └───────┬──────┘│ │
│  │         │              │               │        │ │
│  │  ┌──────▼──────────────▼───────────────▼──────┐ │ │
│  │  │  3rd Party Libraries (PdfPig, OpenXml,     │ │ │
│  │  │  ReverseMarkdown, ClosedXML, etc.)         │ │ │
│  │  └────────────────────────────────────────────┘ │ │
│  └─────────────────────────────────────────────────┘ │
│              TRUST BOUNDARY ─── untrusted input ──── │
└─────────────────────────────────────────────────────┘
```

---

## 5. Phased Remediation Plan

### Phase 1: High Priority — Fix Before Production Use (🔴 + 🟠)

**Target: Findings #1, #2, #3**

| Task | Finding | Effort | Description |
|------|---------|--------|-------------|
| P1-1 | #1 SSRF | Medium | Add private IP deny-list validation in `UrlConverter`. Block `127.0.0.0/8`, `10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`, `169.254.169.254`, `::1`, `fc00::/7`. Disable `HttpClient` auto-redirect or validate redirect targets. |
| P1-2 | #2 Resource Exhaustion | Medium | Add `MaxFileSize` to `MarkItDotNetOptions` (default 100MB). Check in `MarkdownService` before passing to converters. Create a `LimitedStream` wrapper for streams without known length. |
| P1-3 | #3 Prompt Injection | Low | Restructure AI prompts to use `ChatRole.System` for instructions and `ChatRole.User` for untrusted extracted text. Add documentation about prompt injection risks. |

### Phase 2: Hardening — Security-in-Depth (🟡)

**Target: Findings #4, #5, #6, #7, #8, #9, #10**

| Task | Finding | Effort | Description |
|------|---------|--------|-------------|
| P2-1 | #5 XXE | Low | Explicitly set `DtdProcessing.Prohibit` in `XmlConverter`. |
| P2-2 | #6 Path Traversal | Low | Add `Path.GetFullPath` + prefix check in `FileSyncStateStore.GetFilePath()`. |
| P2-3 | #7 Temp Files | Low | Use `FileMode.CreateNew` or `FileOptions.DeleteOnClose` in Whisper converter. |
| P2-4 | #8 Regex DoS | Low | Switch `CleanHtml` regexes to `[GeneratedRegex]` with timeouts, or use HTML parser. |
| P2-5 | #9 EPUB Memory | Low | Add size limit to the seekable-stream copy in `EpubConverter`. |
| P2-6 | #10 Info Disclosure | Low | Sanitize exception messages in error results. Add a `DetailedErrors` option. |
| P2-7 | #4 AI Prompts | Low | Document prompt injection risks for AI-powered converters. |

### Phase 3: Best Practices — Defense-in-Depth (🔵 + ⚪)

**Target: Findings #11, #12, #13, #14, #15, #17**

| Task | Finding | Effort | Description |
|------|---------|--------|-------------|
| P3-1 | #11, #12 Content Injection | Low | Escape Markdown-special characters in `UrlConverter` title and source output. |
| P3-2 | #13 HttpClient | Low | Use a static `HttpClient` in default constructor or document `IHttpClientFactory` usage. |
| P3-3 | #14 Timeout | Low | Set default `HttpClient.Timeout = TimeSpan.FromSeconds(30)`. |
| P3-4 | #15 Thread Safety | Low | Document thread-safety limitations of `ConverterRegistry`. |
| P3-5 | #17 Logging | Low | Add optional `ILogger` support for AI converter error paths. |

### Phase 4: Security Testing

| Task | Description |
|------|-------------|
| P4-1 | Add unit test: `UrlConverter` rejects `file://`, `ftp://`, private IPs |
| P4-2 | Add unit test: Converters reject files exceeding `MaxFileSize` |
| P4-3 | Add unit test: `XmlConverter` rejects XXE payloads |
| P4-4 | Add unit test: `FileSyncStateStore` rejects path traversal in documentId |
| P4-5 | Add unit test: `UrlConverter` output escapes Markdown-special characters |
| P4-6 | Add fuzz test: Feed randomly generated/corrupted files to each converter and verify no unhandled exceptions or resource leaks |
| P4-7 | Add integration test: AI converters separate system vs. user messages |
| P4-8 | Add benchmark test: Verify regex-based HTML cleaning completes within reasonable time for large inputs |

### Phase 5: Documentation

| Task | Description |
|------|-------------|
| P5-1 | Create `docs/security.md` — document the threat model, trust boundaries, and known limitations for library consumers |
| P5-2 | Add security notes to `README.md` — document that URL conversion follows redirects, that file sizes are bounded by available memory (until MaxFileSize is implemented), and that AI integration carries prompt injection risks |
| P5-3 | Add XML doc comments on `AiOptions` properties warning about prompt injection when using untrusted content |
| P5-4 | Document `UrlConverter` SSRF risks and recommend consumers provide pre-configured `HttpClient` with appropriate restrictions |
| P5-5 | Document that `ConverterRegistry` is not thread-safe for concurrent registration |

---

## 6. Positive Security Observations

The codebase demonstrates several good security practices worth acknowledging:

1. **Consistent null/argument validation** — `ArgumentNullException.ThrowIfNull()` and `ArgumentException.ThrowIfNullOrWhiteSpace()` are used consistently throughout.
2. **CancellationToken propagation** — All async paths properly propagate cancellation tokens.
3. **Read-only document opening** — DOCX and PPTX files are opened with `isEditable: false`.
4. **URL scheme validation** — `UrlConverter` correctly validates `http`/`https` schemes (though IP validation is missing).
5. **Stream leaveOpen pattern** — `StreamReader` instances use `leaveOpen: true` to avoid disposing caller-owned streams.
6. **Error resilience** — Exception handling catches `Exception` broadly in conversion paths and returns structured `ConversionResult.Failure` rather than propagating, preventing information leakage at the API boundary.
7. **Clean dependency tree** — Zero known CVEs across all direct and transitive NuGet dependencies.
8. **Generated Regex** — `Metadata` and `Quality` packages use `[GeneratedRegex]` source generators (modern, performant).
9. **SHA-256 hashing** — `ContentHasher` uses SHA-256, which is appropriate for integrity checking.
10. **Filename sanitization** — `FileSyncStateStore` strips invalid filename characters from document IDs.

---

*Report generated by Kobayashi, Security Reviewer — ElBruno.MarkItDotNet Squad*
