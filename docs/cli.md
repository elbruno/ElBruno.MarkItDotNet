> **⚠️ Migration from v0.8.x:** The CLI command was renamed from `markitdown` to `markitdown-dotnet` in v0.9.0 to avoid collision with the [Microsoft Python markitdown CLI](https://github.com/microsoft/markitdown). Run `dotnet tool update -g ElBruno.MarkItDotNet.Cli` and replace all invocations of `markitdown` with `markitdown-dotnet`.

# MarkItDotNet CLI Tool

**Command-line interface for converting 15+ file formats to Markdown** — built on the ElBruno.MarkItDotNet library.

The CLI tool brings the full power of document conversion to your terminal, enabling batch processing, piping, and automated workflows.

## Quick Start

### Install as Global Tool

```bash
dotnet tool install -g ElBruno.MarkItDotNet.Cli
```

### Your First Conversion

```bash
markitdown-dotnet report.pdf
```

Output is printed to the terminal. Pipe it, save it, or process it further:

```bash
markitdown-dotnet report.pdf | head -20
markitdown-dotnet report.pdf > report.md
```

## Installation

### Global Tool Installation

Make `markitdown-dotnet` available system-wide:

```bash
dotnet tool install -g ElBruno.MarkItDotNet.Cli
```

Verify installation:

```bash
markitdown-dotnet --version
```

### Local Tool Installation

For project-scoped use, add to a project's `.config/dotnet-tools.json`:

```bash
dotnet tool install ElBruno.MarkItDotNet.Cli
```

Then invoke it via the project context:

```bash
dotnet markitdown-dotnet <file>
```

## Commands Reference

### `markitdown-dotnet <file>`

Convert a single file to Markdown and output to stdout (or to file with `-o`).

**Usage:**
```bash
markitdown-dotnet <file> [options]
```

**Arguments:**
- `<file>` — Path to the file to convert

**Options:**
- `-o, --output <path>` — Write Markdown to file instead of stdout
- `--format <format>` — Output format: `markdown` (default) or `json`
- `--streaming` — Stream large files chunk-by-chunk (for PDFs, useful for memory-efficiency)
- `-q, --quiet` — Suppress progress/debug output
- `-v, --verbose` — Show detailed conversion logs

**Examples:**

```bash
# Convert and print to terminal
markitdown-dotnet report.pdf

# Convert and save to file
markitdown-dotnet report.pdf -o report.md

# Convert with streaming (good for large PDFs)
markitdown-dotnet large.pdf --streaming -o large.md

# Get JSON output with metadata
markitdown-dotnet data.csv --format json | jq .metadata

# Quiet mode (no status messages)
markitdown-dotnet document.docx -q
```

---

### `markitdown-dotnet batch <directory>`

Convert multiple files in a directory or recursively in subdirectories.

**Usage:**
```bash
markitdown-dotnet batch <directory> [options]
```

**Arguments:**
- `<directory>` — Directory containing files to convert

**Options:**
- `-o, --output <path>` — Output directory (required). Files are named `{original}.md`
- `-r, --recursive` — Include subdirectories (default: immediate files only)
- `--pattern <glob>` — File glob pattern (default: `*.*`). Example: `*.pdf`, `*.docx`, `*.{pdf,docx}`
- `--parallel <count>` — Number of parallel conversions (default: number of CPU cores). Use `1` for sequential
- `--format <format>` — Output format: `markdown` (default) or `json`
- `-q, --quiet` — Suppress progress/debug output
- `-v, --verbose` — Show detailed conversion logs

**Examples:**

```bash
# Convert all files in a directory
markitdown-dotnet batch ./documents -o ./output

# Recursive: include subdirectories
markitdown-dotnet batch ./docs -o ./md -r

# Convert only PDF files
markitdown-dotnet batch ./reports -o ./reports-md -r --pattern "*.pdf"

# Convert Word and PDF only
markitdown-dotnet batch ./mixed -o ./converted -r --pattern "*.{docx,pdf}"

# Limit parallelism (slower but lower memory)
markitdown-dotnet batch ./large -o ./large-md -r --parallel 2

# Verbose output for troubleshooting
markitdown-dotnet batch ./docs -o ./md -r -v
```

---

### `markitdown-dotnet url <url>`

Convert a web page to Markdown. Fetches the page, strips navigation/scripts/styles, and extracts content.

**Usage:**
```bash
markitdown-dotnet url <url> [options]
```

**Arguments:**
- `<url>` — URL to convert (http:// or https://)

**Options:**
- `-o, --output <path>` — Save Markdown to file (instead of stdout)
- `--format <format>` — Output format: `markdown` (default) or `json`
- `-q, --quiet` — Suppress progress/debug output
- `-v, --verbose` — Show detailed conversion logs

**Examples:**

```bash
# Print web page as Markdown to terminal
markitdown-dotnet url https://example.com

# Save web page to Markdown file
markitdown-dotnet url https://example.com -o page.md

# Get JSON output with metadata (word count, title, etc.)
markitdown-dotnet url https://example.com --format json | jq .metadata

# Batch convert URLs from a file
cat urls.txt | while read url; do markitdown-dotnet url "$url" -o "$(echo "$url" | md5sum | cut -d' ' -f1).md"; done
```

---

### `markitdown-dotnet formats`

List all supported file formats with their extensions and converter details.

**Usage:**
```bash
markitdown-dotnet formats
```

**Examples:**

```bash
# Show all supported formats
markitdown-dotnet formats

# Filter by extension
markitdown-dotnet formats | grep pdf
```

**Output:**

Shows a table with columns: **Format**, **Extensions**, **Converter**, **Package**, **Notes**.

---

## Exit Codes

| Code | Meaning | Details |
|------|---------|---------|
| `0` | Success | File(s) converted without errors |
| `1` | Conversion Error | File format failed to convert (unsupported or corrupted content) |
| `2` | File Not Found | Input file or directory does not exist |
| `3` | Unsupported Format | File extension is not registered by any converter |

**Example:**

```bash
markitdown-dotnet missing.pdf
# Output: File not found: missing.pdf
# Exit code: 2
```

---

## Examples

### Basic Conversion

Convert a document to Markdown:

```bash
markitdown-dotnet report.pdf
```

### Save to File

Convert and explicitly save output:

```bash
markitdown-dotnet report.pdf -o report.md
cat report.md
```

### Batch Convert with Pattern

Convert all PDFs in a folder tree:

```bash
markitdown-dotnet batch ./documents -o ./output -r --pattern "*.pdf"
```

### Pipeline for Processing

Extract first 20 lines of a converted document:

```bash
markitdown-dotnet report.pdf | head -20
```

Count words in converted markdown:

```bash
markitdown-dotnet data.csv | wc -w
```

### JSON Output for Scripting

Extract metadata (word count, title, etc.) for further processing:

```bash
markitdown-dotnet data.csv --format json | jq .metadata.wordCount
```

### URL to Markdown

Save a web page as Markdown:

```bash
markitdown-dotnet url https://example.com/article -o article.md
```

### Recursive Batch with Multiple Formats

Convert all Office documents (Word, PowerPoint, Excel) recursively:

```bash
markitdown-dotnet batch ./office-docs -o ./converted -r --pattern "*.{docx,pptx,xlsx}"
```

### Parallel Batch Processing

Speed up large-scale conversions on multi-core machines:

```bash
markitdown-dotnet batch ./huge-corpus -o ./output -r --parallel 8
```

### Troubleshooting with Verbose Mode

Debug why a file isn't converting:

```bash
markitdown-dotnet document.pdf -v
```

---

## Supported Formats

| Format | Extensions | Converter | Package | Requirements |
|--------|-----------|-----------|---------|---|
| Plain Text | `.txt`, `.md`, `.log` | `PlainTextConverter` | Core | None |
| JSON | `.json` | `JsonConverter` | Core | None |
| HTML | `.html`, `.htm` | `HtmlConverter` | Core | `ReverseMarkdown` |
| URL (Web Pages) | `.url` | `UrlConverter` | Core | `ReverseMarkdown` |
| Word (DOCX) | `.docx` | `DocxConverter` | Core | `DocumentFormat.OpenXml` |
| PDF | `.pdf` | `PdfConverter` | Core | `PdfPig` |
| CSV / TSV | `.csv`, `.tsv` | `CsvConverter` | Core | None |
| XML | `.xml` | `XmlConverter` | Core | None |
| YAML | `.yaml`, `.yml` | `YamlConverter` | Core | None |
| RTF | `.rtf` | `RtfConverter` | Core | `RtfPipe` |
| EPUB | `.epub` | `EpubConverter` | Core | `VersOne.Epub` |
| Images | `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.webp`, `.svg` | `ImageConverter` | Core | None |
| Excel (XLSX) | `.xlsx` | `ExcelConverter` | **Excel** | `ClosedXML` |
| PowerPoint (PPTX) | `.pptx` | `PowerPointConverter` | **PowerPoint** | `DocumentFormat.OpenXml` |
| Images (AI-OCR) | All image formats | `AiImageConverter` | **AI** | `Microsoft.Extensions.AI` |
| Audio (AI Transcription) | `.mp3`, `.wav`, `.m4a`, `.ogg` | `AiAudioConverter` | **AI** | `Microsoft.Extensions.AI` |
| PDF (AI-OCR) | `.pdf` | `AiPdfConverter` | **AI** | `Microsoft.Extensions.AI` |
| Audio (Local Whisper) | `.wav`, `.mp3`, `.m4a`, `.ogg`, `.flac` | `WhisperAudioConverter` | **Whisper** | `ElBruno.Whisper` |

---

## Tips & Tricks

### Memory Efficiency with Streaming

For large PDFs (100+ MB), use `--streaming` to process page-by-page:

```bash
markitdown-dotnet large-document.pdf --streaming -o large.md
```

### Parallel Batch Processing

On a multi-core machine, increase `--parallel` for faster conversions:

```bash
markitdown-dotnet batch ./corpus -o ./md -r --parallel $(nproc)
```

### Capture Metadata in Scripts

Extract conversion metadata (success, word count, etc.) programmatically:

```bash
result=$(markitdown-dotnet document.pdf --format json)
word_count=$(echo "$result" | jq .metadata.wordCount)
echo "Converted document has $word_count words"
```

### Filter Batch by Multiple Extensions

Use glob patterns for flexible file selection:

```bash
# All Office documents
markitdown-dotnet batch ./mixed -o ./out -r --pattern "*.{docx,xlsx,pptx}"

# All documents except images
markitdown-dotnet batch ./docs -o ./out -r --pattern "*.{pdf,docx,txt}"
```

### Chaining with Other Unix Tools

Convert and then process:

```bash
# Extract links from converted HTML
markitdown-dotnet page.html | grep -E '^\[' | sort | uniq

# Count paragraphs in converted document
markitdown-dotnet article.docx | grep -c '^$'

# Find longest headings
markitdown-dotnet report.pdf | grep '^##' | sort -k2 -nr | head -5
```

---

## Integration with AI Pipelines

The CLI outputs clean Markdown optimized for AI consumption:

### RAG Ingestion

```bash
# Convert all company docs to Markdown for vector DB ingestion
markitdown-dotnet batch ./company-docs -o ./md -r --format markdown

# Or with JSON metadata for more control:
markitdown-dotnet batch ./company-docs -o ./json -r --format json
```

### Documentation Generation

```bash
# Convert design files to Markdown spec
markitdown-dotnet design.pdf -o design-spec.md

# Convert spreadsheet data to tables
markitdown-dotnet data.xlsx -o data-tables.md
```

### Batch Processing Pipelines

```bash
#!/bin/bash
# Process all documents and store in object storage

for file in results/*.{pdf,docx,xlsx}; do
    echo "Converting $file..."
    output="${file%.*}.md"
    markitdown-dotnet "$file" -o "$output"
    # Upload to S3, GCS, etc.
    gsutil cp "$output" gs://bucket/docs/
done
```

---

## Troubleshooting

### File Not Found

```bash
$ markitdown-dotnet missing.pdf
Error: File not found: missing.pdf
Exit code: 2
```

Check the file path:

```bash
ls -la missing.pdf
```

### Unsupported Format

```bash
$ markitdown-dotnet archive.rar
Error: Unsupported format: .rar
Exit code: 3
```

Check supported formats:

```bash
markitdown-dotnet formats | grep rar
```

### Conversion Failed

```bash
$ markitdown-dotnet corrupted.pdf
Error: Conversion failed for corrupted.pdf: PDF is corrupted
Exit code: 1
```

Enable verbose mode for details:

```bash
markitdown-dotnet corrupted.pdf -v
```

### Memory Issues on Large Batches

Reduce parallelism:

```bash
markitdown-dotnet batch ./huge-folder -o ./out -r --parallel 2
```

Or process in smaller chunks:

```bash
# First 100 files
markitdown-dotnet batch ./huge --pattern "*.pdf" --parallel 1
```

---

## Version & Help

```bash
# Show version
markitdown-dotnet --version

# Show help
markitdown-dotnet --help

# Show command-specific help
markitdown-dotnet batch --help
markitdown-dotnet url --help
```
