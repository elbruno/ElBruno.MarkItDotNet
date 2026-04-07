# Security Findings — Kobayashi (2025-07-15)

## Key Decisions for Team Awareness

### DECISION: SSRF mitigation needed in UrlConverter before production use
- **Finding:** `UrlConverter` validates HTTP/HTTPS scheme but does not block private IPs, metadata endpoints (169.254.169.254), or redirect-based SSRF.
- **Recommendation:** Add IP deny-list validation and disable/validate HTTP redirects.
- **Priority:** 🟠 High — Phase 1

### DECISION: Add configurable MaxFileSize limit
- **Finding:** All converters load entire files into memory via `ReadToEndAsync()` with no size bounds. A multi-GB file causes OOM.
- **Recommendation:** Add `MaxFileSize` to `MarkItDotNetOptions` (default 100MB), enforce in `MarkdownService`.
- **Priority:** 🟠 High — Phase 1

### DECISION: Restructure AI prompts to mitigate prompt injection
- **Finding:** `AiPdfConverter` interpolates untrusted extracted PDF text directly into prompt strings. Malicious PDFs can hijack AI behavior.
- **Recommendation:** Use `ChatRole.System` for instructions, `ChatRole.User` for untrusted content.
- **Priority:** 🟠 High — Phase 1

### DECISION: Add explicit XXE protection in XmlConverter
- **Finding:** `XDocument.Parse()` is safe on .NET 8+ by default, but not explicitly configured. Defense-in-depth requires explicit `DtdProcessing.Prohibit`.
- **Priority:** 🟡 Medium — Phase 2

### DECISION: Canonicalize paths in FileSyncStateStore
- **Finding:** `documentId` sanitization strips invalid chars but doesn't verify the resolved path stays within the base directory.
- **Recommendation:** Add `Path.GetFullPath()` + prefix check.
- **Priority:** 🟡 Medium — Phase 2

## Summary
- **19 total findings**: 0 Critical, 3 High, 7 Medium, 5 Low, 4 Informational
- **0 known CVEs** in dependencies
- Full report: `docs/security-audit.md`
