# Test Patterns for ElBruno.MarkItDotNet

**Author:** Parker  
**Date:** 2025-07-18  
**Status:** Proposed

## Converter Test Structure

Each converter test class follows a consistent pattern:

1. **CanHandle** — verify supported extensions (including case-insensitive), and verify rejection of unsupported extensions via `[Theory]`/`[InlineData]`
2. **NullStream** — verify `ArgumentNullException` for null stream input
3. **Empty/Whitespace input** — verify graceful handling (empty string or no crash)
4. **Happy path** — convert known input, assert key markers in output (don't assert exact strings — converters may evolve formatting)
5. **TestData golden files** — when practical, read from `TestData/` directory and compare against `.expected.md` files

## Key Patterns

- **In-memory file creation** for binary formats (DOCX via OpenXml, PNG/GIF/BMP via raw headers, PDF via raw syntax) — no test fixture files on disk needed for most tests
- **Contract-first testing** — test against `IMarkdownConverter` interface behavior, not implementation details
- **StubConverter** inner class in ConverterRegistryTests for testing registry behavior without depending on real converters
- **Loose assertions** — use `Should().Contain()` over `Should().Be()` for converter output to tolerate formatting changes

## TestData Directory

- Located at `src/tests/ElBruno.MarkItDotNet.Tests/TestData/`
- Requires `<None Update="TestData\**\*" CopyToOutputDirectory="PreserveNewest" />` in test csproj
- Tests gracefully skip if TestData files aren't found (for CI resilience)
