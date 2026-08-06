# SharedTestData

Shared test documents for end-to-end and comparison testing of MarkItDotNet converters.

## Structure

```
SharedTestData/
├── documents/              # Source documents (all formats)
│   ├── sample.csv
│   ├── sample.docx
│   ├── sample.epub
│   ├── sample.html
│   ├── sample.json
│   ├── sample.pdf
│   ├── sample_headings.pdf
│   ├── sample.pptx
│   ├── sample.rtf
│   ├── sample.txt
│   ├── sample.xlsx
│   ├── sample.xml
│   └── sample.yaml
└── expected/               # Expected markdown output
    ├── markitdotnet/       # C# MarkItDotNet golden files
    └── markitdown/         # Python markitdown output (for comparison)
```

## Purpose

- **Unit & integration tests**: Each test project can reference these documents to verify converter output against known inputs.
- **Regression testing**: Golden files in `expected/markitdotnet/` catch unintended output changes.
- **Cross-library comparison**: Side-by-side comparison of Python `markitdown` vs C# `MarkItDotNet` output for the same inputs.

## Document Content

All sample documents contain **equivalent content** covering:

- Headings (H1, H2)
- Paragraphs with bold and italic text
- Bullet and numbered lists
- Tables (3 columns: Format, Extension, Supported)
- Links (where the format supports them)

This makes cross-format and cross-library comparisons meaningful.

## Adding New Test Documents

When adding new test files:

1. Keep documents **small** (1-2 pages) to minimize repo size
2. Use **deterministic content** — avoid timestamps, random data, or user-specific info
3. Add corresponding expected output in `expected/markitdotnet/`
4. Optionally generate Python markitdown output for `expected/markitdown/`
