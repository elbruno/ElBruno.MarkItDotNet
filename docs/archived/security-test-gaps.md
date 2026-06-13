# Security Test Gap Analysis

> **Analyst:** Hockney (Tester)
> **Date:** 2025-07-17
> **Scope:** All test projects under `src/tests/`

---

## 1. Current Test Coverage Summary

### Test Counts by Project

| Test Project | Test Count | Files | Primary Focus |
|---|---:|---:|---|
| ElBruno.MarkItDotNet.Tests | 188 | 19 | Core converters (CSV, DOCX, EPUB, HTML, JSON, PDF, RTF, TXT, URL, XML, YAML), service, registry |
| ElBruno.MarkItDotNet.AzureSearch.Tests | 58 | 5 | Search index builder, uploader, mapper, options |
| ElBruno.MarkItDotNet.Metadata.Tests | 54 | 5 | Metadata extraction, language detection, enrichment |
| ElBruno.MarkItDotNet.CoreModel.Tests | 51 | 5 | Document model, blocks, renderer |
| ElBruno.MarkItDotNet.Sync.Tests | 46 | 7 | Sync planner, executor, state stores, hasher |
| ElBruno.MarkItDotNet.Chunking.Tests | 42 | 7 | Heading/paragraph/token-aware chunkers, options |
| ElBruno.MarkItDotNet.Citations.Tests | 41 | 5 | Citation builder, serializer, formatter |
| ElBruno.MarkItDotNet.Integration.Tests | 41 | 7 | Full pipeline, serialization, quality-gated pipeline |
| ElBruno.MarkItDotNet.Quality.Tests | 40 | 6 | Quality analyzer, metrics, report |
| ElBruno.MarkItDotNet.VectorData.Tests | 40 | 6 | Vector records, JSONL export, batch processor |
| ElBruno.MarkItDotNet.DocumentIntelligence.Tests | 33 | 4 | Azure DI converter, mapper, options |
| ElBruno.MarkItDotNet.Cli.Tests | 22 | 6 | CLI commands (convert, batch, url, formats) |
| ElBruno.MarkItDotNet.AI.Tests | 15 | 5 | AI converters (image, PDF, audio) |
| ElBruno.MarkItDotNet.Whisper.Tests | 10 | 3 | Whisper audio transcription converter |
| ElBruno.MarkItDotNet.Excel.Tests | 7 | 1 | Excel converter |
| ElBruno.MarkItDotNet.PowerPoint.Tests | 6 | 1 | PowerPoint converter |
| ElBruno.MarkItDotNet.GoldenTests | 5 | 4 | Golden file regression tests |
| **TOTAL** | **699** | **100** | |

### Test Classification Breakdown

| Category | Count | % of Total | Description |
|---|---:|---:|---|
| Happy-path | ~470 | 67% | Normal expected behavior, feature verification |
| Edge-case | ~120 | 17% | Boundary conditions, empty inputs, case sensitivity |
| Error-handling | ~95 | 14% | Null guards, unsupported format rejection, missing files |
| Security | ~14 | 2% | Script/style stripping, invalid URL rejection, FTP rejection |

### Existing Security-Adjacent Tests

These are the only tests with explicit security relevance:

| Test File | Test Method | What It Validates |
|---|---|---|
| HtmlConverterTests.cs | `ConvertAsync_ScriptTag_IsStrippedOrIgnored` | `<script>` tag removal |
| HtmlConverterTests.cs | `ConvertAsync_StyleTag_IsStrippedOrIgnored` | `<style>` tag removal |
| HtmlConverterTests.cs | `ConvertAsync_FullDocument_StripsHeadAndScript` | Full doc sanitization |
| UrlConverterTests.cs | `ConvertUrlAsync_StripsScriptTags` | Script removal from fetched HTML |
| UrlConverterTests.cs | `ConvertUrlAsync_StripsStyleTags` | Style removal from fetched HTML |
| UrlConverterTests.cs | `ConvertUrlAsync_StripsNavFooterHeaderTags` | Boilerplate stripping |
| UrlConverterTests.cs | `ConvertUrlAsync_InvalidUrl_ReturnsInvalidMessage` | Invalid URL rejection |
| UrlConverterTests.cs | `ConvertUrlAsync_FtpUrl_ReturnsInvalidMessage` | Non-HTTP protocol rejection |
| UrlConverterTests.cs | `ConvertUrlAsync_HttpError_ReturnsFailureMessage` | HTTP error handling |
| UrlConverterTests.cs | `ConvertUrlAsync_NullOrWhitespace_ThrowsArgumentException` | Null/whitespace URL guard |
| UrlCommandTests.cs | `Url_InvalidUrl_ExecutesWithoutCrash` | CLI doesn't crash on bad URL |
| XmlConverterTests.cs | `ConvertAsync_MalformedXml_ReturnsPlainCodeBlock` | Malformed XML fallback |
| JsonConverterTests.cs | `ConvertAsync_InvalidJson_ReturnsNoteWithRawContent` | Malformed JSON fallback |

---

## 2. Security Test Gaps

### 2.1 Input Validation Gaps

| Component | Gap | Priority | Test Description |
|---|---|---|---|
| **UrlConverter** | No SSRF tests for internal network URLs | 🔴 Critical | Test that `http://127.0.0.1`, `http://localhost`, `http://169.254.169.254`, `http://[::1]`, `http://0x7f000001` are rejected |
| **UrlConverter** | No test for redirect-to-internal-IP | 🔴 Critical | Test that HTTP redirects to private IPs/localhost are blocked |
| **UrlConverter** | No test for `file://` protocol | 🔴 Critical | Test that `file:///etc/passwd` or `file:///C:/Windows/` URLs are rejected |
| **UrlConverter** | No test for extremely long URLs | 🟡 Medium | Test URL > 8KB doesn't cause resource exhaustion |
| **CLI ConvertCommand** | No path traversal tests | 🔴 Critical | Test that `../../etc/passwd`, `..\..\..\Windows\System32\` paths are handled safely |
| **CLI BatchCommand** | No path traversal in output dir | 🔴 Critical | Test that `--output ../../../somewhere` doesn't write outside intended directory |
| **CLI BatchCommand** | No glob injection tests | 🟡 Medium | Test that `--pattern` with special characters doesn't cause unexpected behavior |
| **MarkdownService** | No path traversal tests | 🔴 Critical | Test that file paths with `..` segments are sanitized or rejected |
| **All converters** | No tests for files with path traversal in names | 🟡 Medium | Test filenames like `../../../etc/passwd` or `..\..\Windows\` |

### 2.2 Malicious Input Gaps

| Component | Gap | Priority | Test Description |
|---|---|---|---|
| **PdfConverter** | No malformed/corrupt PDF tests | 🔴 Critical | Test that truncated, corrupt, or crafted PDFs don't crash or hang |
| **DocxConverter** | No malformed DOCX tests | 🔴 Critical | Test corrupt/truncated DOCX (invalid ZIP, missing XML parts) |
| **EpubConverter** | No malformed EPUB tests | 🔴 Critical | Test corrupt EPUB files (invalid ZIP, malicious HTML inside) |
| **ExcelConverter** | No malformed XLSX tests | 🟡 Medium | Test corrupt Excel files, extreme row/column counts |
| **PowerPointConverter** | No malformed PPTX tests | 🟡 Medium | Test corrupt PowerPoint files |
| **HtmlConverter** | No deeply nested HTML test | 🟡 Medium | Test HTML with 10,000+ nested `<div>` tags (stack overflow risk) |
| **HtmlConverter** | No HTML bomb test | 🟡 Medium | Test extremely large HTML (e.g., 100MB) for memory exhaustion |
| **XmlConverter** | No XML bomb test (Billion Laughs) | 🔴 Critical | Test XML entity expansion attack: `<!ENTITY lol "lol">...` |
| **XmlConverter** | No XXE test (External Entity) | 🔴 Critical | Test `<!ENTITY xxe SYSTEM "file:///etc/passwd">` is rejected |
| **JsonConverter** | No deeply nested JSON test | 🟡 Medium | Test JSON with 10,000+ nesting levels (stack overflow risk) |
| **JsonConverter** | No large JSON test | 🟡 Medium | Test JSON file > 1GB for memory exhaustion |
| **CsvConverter** | No CSV with extreme column count | 🟡 Medium | Test CSV with 100,000+ columns |
| **CsvConverter** | No CSV injection test | 🟡 Medium | Test CSV cells containing `=CMD()`, `@SUM()` formula injection payloads are escaped in output |
| **RtfConverter** | No malformed RTF test | 🟡 Medium | Test corrupt RTF with unclosed groups |
| **YamlConverter** | No YAML bomb test | 🟡 Medium | Test YAML with billion-laughs-style anchors/aliases |
| **AI converters** | No prompt injection via file content | 🟡 Medium | Test that file content doesn't inject malicious instructions into AI prompts |

### 2.3 Resource Exhaustion Gaps

| Component | Gap | Priority | Test Description |
|---|---|---|---|
| **All converters** | No oversized input tests | 🟡 Medium | Test behavior with files > 100MB, > 1GB — should fail gracefully, not OOM |
| **Chunking (all strategies)** | No extreme document size tests | 🟡 Medium | Test chunking on document with 100,000+ blocks |
| **TokenAwareChunker** | No test with adversarial token counter | 🟢 Low | Test with token counter that always returns 0 or max int |
| **VectorBatchProcessor** | No extreme batch test | 🟢 Low | Test with batch size of 1 and 1M records |
| **SearchIndexUploader** | No large batch upload test | 🟢 Low | Test upload with extremely large document count |
| **CLI BatchCommand** | No test with massive directory | 🟢 Low | Test batch conversion on directory with 100,000+ files |

### 2.4 Concurrency & Race Condition Gaps

| Component | Gap | Priority | Test Description |
|---|---|---|---|
| **FileSyncStateStore** | No concurrent read/write tests | 🟡 Medium | Test parallel `SaveStateAsync`/`GetStateAsync` calls don't corrupt state |
| **InMemorySyncStateStore** | No concurrent access tests | 🟡 Medium | Test thread-safety of the in-memory store under parallel access |
| **ConverterRegistry** | No concurrent registration/resolution | 🟢 Low | Test parallel `Register`/`Resolve` calls |
| **MarkdownService** | No concurrent conversion tests | 🟢 Low | Test parallel `ConvertAsync` calls for thread-safety |

### 2.5 Error Information Leakage Gaps

| Component | Gap | Priority | Test Description |
|---|---|---|---|
| **All converters** | No error message content tests | 🟡 Medium | Verify error messages don't leak full file paths, stack traces, or internal details |
| **CLI tool** | No error output sanitization tests | 🟡 Medium | Verify stderr doesn't expose internal paths, versions, or system info |
| **UrlConverter** | No DNS/network error leakage test | 🟡 Medium | Verify DNS resolution errors don't leak internal network topology |
| **DocumentIntelligence** | No API key leakage in errors | 🔴 Critical | Verify Azure API keys don't appear in error messages or logs |
| **AzureSearch** | No API key leakage in errors | 🔴 Critical | Verify search API keys don't appear in error messages |

### 2.6 Special Character & Encoding Gaps

| Component | Gap | Priority | Test Description |
|---|---|---|---|
| **FileSyncStateStore** | No special char filename tests | 🟡 Medium | Test filenames with `<>:"/\|?*`, unicode, null bytes, very long names |
| **CLI tool** | No special char in file paths | 🟡 Medium | Test paths with spaces, unicode, quotes, pipes, semicolons |
| **All converters** | No null byte in filename tests | 🟡 Medium | Test filenames containing `\0` characters |
| **MarkdownService** | No extension spoofing tests | 🟡 Medium | Test double extensions like `malware.exe.txt`, `file.pdf.html` |
| **HtmlConverter** | No encoding mismatch test | 🟢 Low | Test HTML with mismatched charset declarations |

---

## 3. Recommended Security Test Categories

### Category A: Input Validation & Sanitization
Tests that verify the system properly validates and sanitizes all user-provided input before processing.

- **Path traversal prevention** — All file path inputs reject `..` navigation
- **URL validation** — SSRF prevention, protocol whitelisting, redirect following limits
- **Extension validation** — Double-extension detection, case normalization
- **Filename sanitization** — Special characters, null bytes, length limits

### Category B: Malicious Document Handling
Tests that verify the system handles deliberately crafted malicious documents without crashing, hanging, or leaking information.

- **Corrupt/truncated files** — Every converter handles broken files gracefully
- **Document bombs** — XML entity expansion, deeply nested structures, ZIP bombs
- **Injection payloads** — CSV formula injection, XSS payloads in HTML, prompt injection in AI workflows

### Category C: Resource Exhaustion Resistance
Tests that verify the system fails gracefully (not catastrophically) when given oversized or adversarial input.

- **Memory limits** — Large files don't cause OutOfMemoryException
- **CPU limits** — Complex documents don't cause infinite loops or excessive processing
- **Disk limits** — Output generation doesn't fill disk unexpectedly

### Category D: Credential & Information Security
Tests that verify sensitive information is never exposed in error messages, logs, or output.

- **API key protection** — Azure keys never appear in errors or output
- **Path sanitization in errors** — Full filesystem paths not leaked
- **Network topology protection** — Internal DNS/IP not leaked in URL errors

### Category E: Concurrency Safety
Tests that verify shared state remains consistent under parallel access.

- **State store thread-safety** — File and in-memory stores handle concurrent access
- **Registry thread-safety** — Converter registry is safe under parallel use

---

## 4. Phased Test Plan

### Phase 1: Critical Security Tests (Immediate — Blocks Release)

These tests address the highest-severity gaps that could lead to data exfiltration, arbitrary file access, or crashes.

| # | Test | Component | Gap Addressed |
|---|---|---|---|
| 1 | SSRF: reject localhost, 127.0.0.1, 169.254.x, [::1], 0x7f000001 | UrlConverter | Internal network access |
| 2 | SSRF: reject file:// protocol URLs | UrlConverter | Local file access via URL |
| 3 | SSRF: reject HTTP redirects to private IPs | UrlConverter | Redirect-based SSRF |
| 4 | XML bomb (Billion Laughs entity expansion) | XmlConverter | Denial of service |
| 5 | XXE (external entity injection) | XmlConverter | File exfiltration via XML |
| 6 | Path traversal in file paths (`../../sensitive`) | MarkdownService, CLI | Arbitrary file read |
| 7 | Path traversal in CLI output path | CLI BatchCommand | Arbitrary file write |
| 8 | API key not leaked in error messages | DocumentIntelligence, AzureSearch | Credential exposure |
| 9 | Corrupt PDF doesn't crash | PdfConverter | Crash via malformed input |
| 10 | Corrupt DOCX doesn't crash | DocxConverter | Crash via malformed input |
| 11 | Corrupt EPUB doesn't crash | EpubConverter | Crash via malformed input |

**Estimated effort:** 2–3 days for one developer

### Phase 2: Hardening Validation Tests

These tests validate that the system handles adversarial but plausible inputs correctly.

| # | Test | Component | Gap Addressed |
|---|---|---|---|
| 12 | Corrupt XLSX doesn't crash | ExcelConverter | Malformed input |
| 13 | Corrupt PPTX doesn't crash | PowerPointConverter | Malformed input |
| 14 | Malformed RTF doesn't crash | RtfConverter | Malformed input |
| 15 | YAML bomb (anchor/alias expansion) | YamlConverter | Denial of service |
| 16 | Deeply nested JSON (10K+ levels) | JsonConverter | Stack overflow |
| 17 | Deeply nested HTML (10K+ divs) | HtmlConverter | Stack overflow |
| 18 | CSV formula injection escaped in output | CsvConverter | Output injection |
| 19 | Concurrent FileSyncStateStore access | FileSyncStateStore | Race condition / corruption |
| 20 | Special characters in filenames | FileSyncStateStore, CLI | File system errors |
| 21 | Error messages don't contain full paths | All converters | Information leakage |
| 22 | Error messages don't contain stack traces | CLI tool | Information leakage |

**Estimated effort:** 2–3 days for one developer

### Phase 3: Best Practice Validation Tests

These tests enforce security best practices and harden edge cases.

| # | Test | Component | Gap Addressed |
|---|---|---|---|
| 23 | Extension spoofing (double extensions) | MarkdownService | Input confusion |
| 24 | Filenames with null bytes | All converters | Null byte injection |
| 25 | Very long filenames (> 260 chars) | CLI, FileSyncStateStore | Path length overflow |
| 26 | Very long URLs (> 8KB) | UrlConverter | Buffer overflow risk |
| 27 | DNS error doesn't leak network info | UrlConverter | Network topology leak |
| 28 | Prompt injection via file content | AI converters | AI prompt manipulation |
| 29 | CLI paths with unicode, quotes, pipes | CLI tool | Command injection |
| 30 | CLI glob pattern injection | CLI BatchCommand | Unexpected expansion |
| 31 | HTML with encoding mismatches | HtmlConverter | Parsing confusion |

**Estimated effort:** 2–3 days for one developer

### Phase 4: Comprehensive Security Suite

These tests form the ongoing security regression suite and cover resource exhaustion and concurrency.

| # | Test | Component | Gap Addressed |
|---|---|---|---|
| 32 | Oversized file (> 100MB) per converter | All converters | Memory exhaustion |
| 33 | Extreme CSV (100K+ columns) | CsvConverter | Resource exhaustion |
| 34 | Extreme document (100K+ blocks) for chunking | All chunkers | Memory/CPU exhaustion |
| 35 | Adversarial token counter (returns 0, returns MaxInt) | TokenAwareChunker | Logic error |
| 36 | Massive batch directory (100K+ files) | CLI BatchCommand | Resource exhaustion |
| 37 | Concurrent MarkdownService conversions | MarkdownService | Thread-safety |
| 38 | Concurrent ConverterRegistry operations | ConverterRegistry | Thread-safety |
| 39 | Concurrent InMemorySyncStateStore operations | InMemorySyncStateStore | Thread-safety |
| 40 | Large batch upload (10K+ documents) | SearchIndexUploader | Resource exhaustion |

**Estimated effort:** 3–4 days for one developer

---

## Summary

- **699 tests** exist today; only **~14 (2%)** have explicit security relevance
- **67% are happy-path**, which is excellent for feature coverage but leaves the security perimeter largely untested
- **40 security tests** are recommended across 4 phases
- **Phase 1 is the most urgent** — it covers SSRF, XXE, path traversal, credential leakage, and corrupt file handling
- The core converters (PDF, DOCX, EPUB, HTML, XML) and the URL converter are the highest-risk components
- The CLI tool's file I/O operations have **zero path traversal tests**
- Azure integration components have **zero credential leakage tests**
