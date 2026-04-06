# PRD — ElBruno.MarkItDotNet Ingestion Expansion Roadmap

## Document Status
- **Product**: ElBruno.MarkItDotNet ecosystem expansion
- **Document Type**: Product Requirements Document
- **Version**: 1.0
- **Date**: 2026-04-06
- **Owner**: Bruno / ElBruno OSS
- **Audience**: Repo agents, maintainers, contributors, reviewers

---

## 1. Executive Summary

`ElBruno.MarkItDotNet` already solves an important problem: converting many kinds of files into Markdown in .NET. That is a strong base, but the broader ingestion scenario for AI, search, RAG, and agentic applications requires more than file-to-Markdown conversion.

This PRD defines a **3-phase roadmap** to evolve the project from a Markdown conversion library into a **trustworthy ingestion framework for .NET**.

The roadmap focuses on these three phases:

1. **Phase 1 — Core Ingestion Foundations**
   - `DocumentIntelligence`
   - `CoreModel`
   - `Chunking`
   - `Citations`

2. **Phase 2 — Production Readiness**
   - `Metadata`
   - `Quality`
   - `VectorData` / `AzureSearch`
   - `Sync`

3. **Phase 3 — Ecosystem Expansion**
   - `Connectors`
   - `Security`
   - `Evals`

This document is designed so repo agents can use it as an implementation blueprint.

---

## 2. Problem Statement

Today, developers can convert documents into Markdown, but building a real ingestion pipeline still requires a lot of custom work:

- preserving document structure
- creating chunking strategies
- tracking citations and source locations
- enriching metadata
- routing difficult documents to better extraction pipelines
- loading content into vector stores or search indexes
- re-ingesting content efficiently
- applying security, governance, and evaluation

This causes duplication across projects and increases time-to-value for .NET developers building AI-enabled applications.

The opportunity is to provide a clean, modular, .NET-first ingestion ecosystem.

---

## 3. Product Vision

Build the most practical and developer-friendly **document ingestion toolkit for .NET**, starting with Markdown conversion and expanding into:

- structured extraction
- layout-aware chunking
- citations and traceability
- enrichment
- sinks/loaders
- incremental sync
- governance and evaluation

The project should serve:

- AI and RAG developers
- search and knowledge platform builders
- enterprise teams with compliance needs
- beginners who want batteries-included ingestion pipelines
- Cloud/AI/.NET community demos and samples

---

## 4. Product Goals

### Primary Goals
- Turn `MarkItDotNet` into a reusable ingestion platform
- Preserve more document structure beyond plain Markdown
- Support trustworthy citations and chunk traceability
- Enable direct integration with vector/search systems
- Improve outcomes for RAG and agent workflows
- Keep the architecture modular and package-based

### Secondary Goals
- Provide great developer experience
- Support local-first and Azure-first scenarios
- Offer sensible defaults with extension points
- Make packages demo-friendly and beginner-friendly

### Non-Goals
- Building a full document management system
- Replacing all specialized OCR or enterprise ETL products
- Building every possible connector in the first release
- Locking the system to a single AI/vector provider

---

## 5. Users and Personas

### Persona A — AI App Developer
Wants to ingest PDFs, Office files, and web content into a RAG pipeline quickly.

### Persona B — Enterprise Developer
Needs traceability, quality gates, indexing targets, and security hooks.

### Persona C — OSS / Community Builder
Wants easy demos, samples, and reusable pipelines for workshops and videos.

### Persona D — Beginner .NET Developer
Needs a simple “one package + one sample + one result” approach.

---

## 6. Core Product Principles

- **Markdown-first, but not Markdown-only**
- **Structure matters**
- **Citations are first-class**
- **Chunking is a product, not a utility**
- **Provider-neutral where possible**
- **Azure integrations should feel native**
- **Good defaults, open extension points**
- **Composable packages, not one giant blob**

---

## 7. Proposed Package Architecture

### Existing Base
- `ElBruno.MarkItDotNet`

### Phase 1
- `ElBruno.MarkItDotNet.CoreModel`
- `ElBruno.MarkItDotNet.DocumentIntelligence`
- `ElBruno.MarkItDotNet.Chunking`
- `ElBruno.MarkItDotNet.Citations`

### Phase 2
- `ElBruno.MarkItDotNet.Metadata`
- `ElBruno.MarkItDotNet.Quality`
- `ElBruno.MarkItDotNet.VectorData`
- `ElBruno.MarkItDotNet.AzureSearch`
- `ElBruno.MarkItDotNet.Sync`

### Phase 3
- `ElBruno.MarkItDotNet.Connectors.*`
- `ElBruno.MarkItDotNet.Security`
- `ElBruno.MarkItDotNet.Evals`

---

## 8. Phase 1 — Core Ingestion Foundations

## 8.1 Objectives
Build the minimum architecture required to move from file conversion to trustworthy ingestion.

## 8.2 Scope
- canonical internal document model
- Azure Document Intelligence integration
- chunking framework
- citations/source maps

## 8.3 Packages

### 8.3.1 `ElBruno.MarkItDotNet.CoreModel`

#### Purpose
Define the internal document model used across all ingestion packages.

#### Why
Markdown alone is not enough for chunking, citations, figures, or structured loading.

#### Required Capabilities
- `Document`
- `DocumentSection`
- `DocumentBlock`
- `ParagraphBlock`
- `HeadingBlock`
- `TableBlock`
- `FigureBlock`
- `PageBlock`
- `SpanReference`
- `SourceReference`
- `DocumentMetadata`
- document-level and block-level IDs

#### Functional Requirements
- Support a tree or graph structure for document layout
- Preserve ordering of blocks
- Preserve page references when available
- Support custom metadata bags
- Support serialization to JSON

#### Acceptance Criteria
- A parsed document can be represented in `CoreModel`
- Markdown renderers can map from `CoreModel`
- Blocks can preserve source references and page numbers
- JSON round-trip works with sample documents

---

### 8.3.2 `ElBruno.MarkItDotNet.DocumentIntelligence`

#### Purpose
Provide first-class integration with Azure Document Intelligence.

#### Why
Some documents need layout-aware extraction, OCR, table preservation, and structured output.

#### Required Capabilities
- ingest file/stream/URL where supported
- call Azure Document Intelligence layout model
- map results into `CoreModel`
- emit Markdown and structured document output
- preserve page, table, figure, caption, formula, and span metadata where possible

#### Functional Requirements
- authentication via Azure credentials / endpoint + key
- async extraction API
- DI-friendly registration
- resilience/error wrapping
- configurable extraction options
- fallback-friendly result model

#### Acceptance Criteria
- sample PDF can be converted through Azure DI into `CoreModel`
- table-heavy and scanned documents preserve more structure than local-only fallback
- page and source references are available on mapped blocks
- package includes one minimal sample and one advanced sample

---

### 8.3.3 `ElBruno.MarkItDotNet.Chunking`

#### Purpose
Provide layout-aware and token-aware chunking for downstream AI/search use.

#### Why
Good ingestion depends heavily on chunk quality.

#### Required Capabilities
- heading-based chunking
- paragraph-based chunking
- token-aware chunking
- overlap settings
- preserve table atomicity
- preserve figure + caption atomicity
- document-specific strategies for slides/sheets/pages

#### Functional Requirements
- chunk from `CoreModel`, not raw text only
- allow chunk metadata enrichment
- configurable max size and overlap
- pluggable token counter
- stable chunk IDs
- include source references in output chunks

#### Acceptance Criteria
- same document can be chunked using multiple strategies
- chunks preserve heading path and page/source metadata
- tables are not arbitrarily split unless explicitly allowed
- token-aware chunking respects configured limits within tolerance

---

### 8.3.4 `ElBruno.MarkItDotNet.Citations`

#### Purpose
Track source-to-chunk and source-to-answer traceability.

#### Why
Trustworthy AI/search scenarios require citations.

#### Required Capabilities
- page references
- block references
- source file references
- offset/span references
- heading path references
- normalized citation payload for downstream apps

#### Functional Requirements
- attach citation metadata to `CoreModel` blocks and output chunks
- generate human-readable citation strings
- support exact-source and coarse-source modes
- allow citation serialization and deserialization

#### Acceptance Criteria
- each chunk can return one or more citations
- citations can reference page + section + source
- citations can survive JSON persistence
- downstream app sample can display citation links or text references

---

## 8.4 Phase 1 Deliverables
- package implementations
- unit tests
- integration tests
- sample apps
- docs
- diagrams
- at least one end-to-end ingestion sample: PDF -> CoreModel -> chunks -> citations

## 8.5 Phase 1 Success Metrics
- create a structured ingestion pipeline with less than 30 lines of sample code
- preserve citations for at least PDF-based scenarios
- produce chunk outputs that can be consumed by downstream vector/search code

---

## 9. Phase 2 — Production Readiness

## 9.1 Objectives
Make the ingestion stack more practical for real-world systems.

## 9.2 Scope
- metadata extraction and enrichment
- quality analysis and routing
- loading into vector/search systems
- incremental ingestion and change detection

## 9.3 Packages

### 9.3.1 `ElBruno.MarkItDotNet.Metadata`

#### Purpose
Extract and normalize metadata and optional enrichments.

#### Required Capabilities
- title extraction
- author extraction when available
- language detection
- created/modified dates when available
- document type tagging
- heading normalization
- optional section summary support
- entity/tag extensibility hooks

#### Acceptance Criteria
- sample documents produce normalized metadata objects
- enrichments are optional and pluggable
- metadata can be attached at document and chunk level

---

### 9.3.2 `ElBruno.MarkItDotNet.Quality`

#### Purpose
Score extraction/chunking quality and enable fallback decisions.

#### Required Capabilities
- extraction confidence scoring
- OCR suspicion flags
- low-text-density detection
- duplicate-line detection
- table extraction warnings
- broken reading-order heuristics
- quality report output

#### Acceptance Criteria
- weak documents can be flagged automatically
- quality report can guide fallback routing
- diagnostics are exposed cleanly to developers

---

### 9.3.3 `ElBruno.MarkItDotNet.VectorData`

#### Purpose
Provide provider-neutral loading support for vector-oriented storage.

#### Required Capabilities
- chunk-to-record mapping
- metadata payload mapping
- embedding-ready payload support
- interfaces friendly to `Microsoft.Extensions.VectorData`
- JSONL export fallback

#### Acceptance Criteria
- chunks can be transformed into vector records consistently
- package supports provider-neutral abstractions first
- docs include at least one local vector sink sample

---

### 9.3.4 `ElBruno.MarkItDotNet.AzureSearch`

#### Purpose
Provide first-class Azure AI Search integration.

#### Required Capabilities
- index schema helpers
- document upload helpers
- field mapping for content, metadata, vectors, citations
- hybrid-search-friendly record shape guidance

#### Acceptance Criteria
- chunks and citations can be loaded into Azure AI Search
- sample app shows end-to-end indexing
- docs explain recommended field mappings

---

### 9.3.5 `ElBruno.MarkItDotNet.Sync`

#### Purpose
Support incremental re-ingestion and change-aware updates.

#### Required Capabilities
- source hash
- chunk hash
- changed-content detection
- soft-delete support
- version markers
- sync plan output

#### Acceptance Criteria
- re-running ingestion can skip unchanged content
- changed sections can be reprocessed selectively when possible
- sync summary is easy to inspect

---

## 9.4 Phase 2 Deliverables
- metadata/enrichment system
- quality scoring/reporting
- vector/search loaders
- incremental sync primitives
- end-to-end sample: ingest -> chunk -> enrich -> index -> re-sync

## 9.5 Phase 2 Success Metrics
- support production-style demos with re-index scenarios
- reduce duplicate ingestion work
- expose enough metadata for realistic RAG/search apps

---

## 10. Phase 3 — Ecosystem Expansion

## 10.1 Objectives
Expand the ecosystem toward enterprise and community scenarios.

## 10.2 Scope
- source connectors
- security/compliance helpers
- evaluation/benchmark tooling

## 10.3 Packages

### 10.3.1 `ElBruno.MarkItDotNet.Connectors.*`

#### Purpose
Make ingestion easier from common real-world sources.

#### Initial Connector Candidates
- local file system
- GitHub docs/content
- Azure Blob Storage
- website / sitemap crawler
- SharePoint / OneDrive
- RSS / feed ingestion

#### Acceptance Criteria
- at least two connectors shipped first
- connectors produce normalized ingestion inputs
- connectors fit cleanly into the pipeline

---

### 10.3.2 `ElBruno.MarkItDotNet.Security`

#### Purpose
Support safe ingestion for enterprise use cases.

#### Required Capabilities
- PII detection hooks
- redaction pipeline hooks
- file type allow/deny
- file size/page count limits
- secret detection hooks
- record tagging for access/governance

#### Acceptance Criteria
- developers can inject security checks before indexing
- redaction can be applied at document or chunk level
- policies are configurable

---

### 10.3.3 `ElBruno.MarkItDotNet.Evals`

#### Purpose
Measure ingestion quality and downstream usefulness.

#### Required Capabilities
- extraction benchmark harness
- chunk quality benchmark harness
- citation coverage metrics
- latency and memory metrics
- retrieval-oriented evaluation hooks
- comparison mode across chunking/extraction strategies

#### Acceptance Criteria
- package can compare at least two strategies on one corpus
- metrics are exportable
- docs include benchmark scenarios

---

## 10.4 Phase 3 Deliverables
- connectors foundation
- security hooks and policies
- benchmark/evaluation tooling
- realistic enterprise and OSS scenario samples

## 10.5 Phase 3 Success Metrics
- easier integration with real document sources
- safer ingestion for enterprise teams
- measurable evaluation story for community and product demos

---

## 11. Cross-Cutting Requirements

### 11.1 Developer Experience
- idiomatic .NET APIs
- DI-first where useful
- async-first
- sensible defaults
- minimal sample code paths
- strong XML docs and README examples

### 11.2 Extensibility
- interfaces for parsers, enrichers, chunkers, sinks, evaluators
- pipeline composition support
- custom metadata bags
- provider-neutral abstractions first when feasible

### 11.3 Packaging
- keep packages modular
- avoid giant transitive dependency footprint
- clearly separate Azure-specific packages from neutral packages

### 11.4 Performance
- support streaming where possible
- avoid unnecessary document duplication in memory
- support large-document scenarios with incremental processing patterns

### 11.5 Testing
- unit tests per package
- integration tests for Azure packages
- golden-file tests for document structure and chunk results
- benchmark tests where relevant

### 11.6 Samples and Documentation
Each package should ship with:
- README
- quickstart
- minimal code sample
- advanced sample where relevant
- sample input documents if licensing allows
- architecture notes

---

## 12. Suggested Repository Structure

> **Convention:** All source code, tests, and samples live under `src/`. Only root-level config files (README.md, LICENSE, Directory.Build.props, global.json, .slnx) live at the repo root. Documentation lives under `docs/`.

```text
src/
  ElBruno.MarkItDotNet/                          # existing core library
  ElBruno.MarkItDotNet.AI/                       # existing AI satellite
  ElBruno.MarkItDotNet.Excel/                    # existing Excel satellite
  ElBruno.MarkItDotNet.PowerPoint/               # existing PowerPoint satellite
  ElBruno.MarkItDotNet.Whisper/                  # existing Whisper satellite
  ElBruno.MarkItDotNet.CoreModel/                # Phase 1 — new
  ElBruno.MarkItDotNet.DocumentIntelligence/     # Phase 1 — new
  ElBruno.MarkItDotNet.Chunking/                 # Phase 1 — new
  ElBruno.MarkItDotNet.Citations/                # Phase 1 — new
  ElBruno.MarkItDotNet.Metadata/                 # Phase 2 — new
  ElBruno.MarkItDotNet.Quality/                  # Phase 2 — new
  ElBruno.MarkItDotNet.VectorData/               # Phase 2 — new
  ElBruno.MarkItDotNet.AzureSearch/              # Phase 2 — new
  ElBruno.MarkItDotNet.Sync/                     # Phase 2 — new
  ElBruno.MarkItDotNet.Security/                 # Phase 3 — new
  ElBruno.MarkItDotNet.Evals/                    # Phase 3 — new
  ElBruno.MarkItDotNet.Connectors.FileSystem/    # Phase 3 — new
  ElBruno.MarkItDotNet.Connectors.AzureBlob/     # Phase 3 — new
  tests/
    ElBruno.MarkItDotNet.Tests/                  # existing
    ElBruno.MarkItDotNet.GoldenTests/            # existing
    ElBruno.MarkItDotNet.CoreModel.Tests/        # Phase 1 — new
    ElBruno.MarkItDotNet.Chunking.Tests/         # Phase 1 — new
    ElBruno.MarkItDotNet.Citations.Tests/        # Phase 1 — new
    ElBruno.MarkItDotNet.DocumentIntelligence.Tests/  # Phase 1 — new (integration)
    ElBruno.MarkItDotNet.Metadata.Tests/         # Phase 2 — new
    ElBruno.MarkItDotNet.Quality.Tests/          # Phase 2 — new
    ElBruno.MarkItDotNet.VectorData.Tests/       # Phase 2 — new
    ElBruno.MarkItDotNet.AzureSearch.Tests/      # Phase 2 — new (integration)
    ElBruno.MarkItDotNet.Sync.Tests/             # Phase 2 — new
    # Phase 3 test projects follow the same pattern
  samples/
    BasicConversion/                             # existing
    RagPipeline/                                 # existing — will be updated
    Phase1.PdfToChunks/                          # Phase 1 — new
    Phase1.PdfToChunksWithCitations/             # Phase 1 — new
    Phase2.IndexToAzureSearch/                   # Phase 2 — new
    Phase2.SyncChangedDocuments/                 # Phase 2 — new
    Phase3.ConnectorsDemo/                       # Phase 3 — new
    Phase3.SecurityAndEvals/                     # Phase 3 — new
docs/
  architecture/
  roadmap/
  package-guides/
```

---

## 13. Suggested Delivery Order

> **Feasibility-informed sequencing.** Milestones are ordered to minimize risk and maximize early value. CoreModel is the foundation — nothing else starts until it exists. Bridge mappers (Milestone 2) prove CoreModel works with real documents before building Chunking/Citations on top.

### Milestone 0 — Prerequisite: DI Consistency Fix
- Fix `AddMarkItDotNetAI()` to auto-register the AI plugin into `ConverterRegistry` (matching the Excel/PowerPoint pattern)
- This establishes the correct satellite package registration pattern for all new packages

### Milestone 1 — CoreModel
- `CoreModel` contracts (Document, Section, Block hierarchy)
- `IStructuredConverter` interface (parallel to existing `IMarkdownConverter`)
- JSON serialization round-trip
- MarkdownRenderer (CoreModel → Markdown string)
- Unit tests

### Milestone 2 — Bridge Mappers
- `PdfCoreModelMapper` — adapts existing PdfConverter's page/word extraction to CoreModel blocks
- `DocxCoreModelMapper` — adapts existing DocxConverter's heading/table/image parsing to CoreModel blocks
- Golden-file tests for structured output
- **Gate:** CoreModel must represent real PDF and DOCX documents correctly before proceeding

### Milestone 3 — Chunking + Citations (parallel)
- `Chunking`: heading-based, paragraph-based, token-aware strategies
- `Citations`: page refs, block refs, source file refs, heading path
- Both depend only on CoreModel — can be developed in parallel
- Unit tests for each

### Milestone 4 — DocumentIntelligence + End-to-End Sample
- `DocumentIntelligence`: Azure DI layout model → CoreModel mapper
- End-to-end sample: PDF → CoreModel → chunks → citations → JSON
- Integration tests (conditional on Azure DI endpoint availability)

### Milestone 5 — Metadata + Quality (parallel)
- `Metadata`: title, author, language, dates, document type extraction
- `Quality`: text density, OCR suspicion, duplicate lines, table warnings
- Both are independent — can be developed in parallel

### Milestone 6 — VectorData + AzureSearch (parallel)
- `VectorData`: chunk-to-record mapping, provider-neutral abstractions
- `AzureSearch`: index schema helpers, document upload, field mapping
- VectorData is provider-neutral; AzureSearch is Azure-specific — keep isolated

### Milestone 7 — Sync
- Source/chunk hashing, change detection, soft-delete, version markers
- Sync plan output
- End-to-end sample: ingest → chunk → enrich → index → re-sync

### Milestone 8 — Connectors
- Connector abstraction (`IDocumentSource` interface)
- `Connectors.FileSystem` — local file system crawling
- `Connectors.AzureBlob` — Azure Blob Storage integration

### Milestone 9 — Security + Evals (parallel)
- `Security`: PII detection hooks, redaction pipeline hooks, file type/size policies
- `Evals`: extraction benchmark harness, chunk quality metrics, comparison mode

---

## 14. Agent-Friendly Task Breakdown

> **Feasibility notes included.** Each task is annotated with its feasibility assessment and any design decisions that must be resolved before implementation.

## 14.1 Phase 1 Agent Tasks

### CoreModel Package (Milestone 1)
- [ ] Design `Document`, `DocumentSection`, `DocumentBlock` hierarchy as immutable records
- [ ] Implement block types: `ParagraphBlock`, `HeadingBlock`, `TableBlock`, `FigureBlock`, `PageBlock`
- [ ] Implement `SpanReference` and `SourceReference` for positional tracking
- [ ] Implement `DocumentMetadata` with custom metadata bag support
- [ ] Add deterministic document-level and block-level ID generation
- [ ] Implement JSON serialization round-trip (System.Text.Json)
- [ ] Implement `MarkdownRenderer` (CoreModel → Markdown string)
- [ ] Define `IStructuredConverter` interface: `Task<Document> ConvertToDocumentAsync(...)`
- [ ] Add unit tests for model construction, serialization, rendering
- [ ] **Decision required:** immutable records (recommended) vs. mutable classes
- [ ] **Decision required:** tree structure (sections contain blocks) vs. flat list with depth markers

### Bridge Mappers (Milestone 2)
- [ ] Implement `PdfCoreModelMapper` — leverages existing `PdfConverter` page/word/line extraction
- [ ] Implement `DocxCoreModelMapper` — leverages existing `DocxConverter` heading/table/image parsing
- [ ] Register structured converters in DI alongside existing string converters
- [ ] Add golden-file tests comparing CoreModel output to expected structures
- [ ] **Note:** Existing converters stay unchanged — mappers are additive adapters

### Chunking Package (Milestone 3)
- [ ] Define `IChunkingStrategy` interface and `ChunkResult` model
- [ ] Implement `HeadingBasedChunker` — splits at heading boundaries, preserves heading path
- [ ] Implement `ParagraphBasedChunker` — splits at paragraph boundaries
- [ ] Implement `TokenAwareChunker` — respects token limits with pluggable counter
- [ ] Implement overlap settings (configurable token/character overlap)
- [ ] Enforce table atomicity — tables are never split unless explicitly allowed
- [ ] Enforce figure + caption atomicity
- [ ] Generate stable chunk IDs (deterministic from content + position)
- [ ] Include source references in output chunks
- [ ] DI registration extension method
- [ ] Unit tests with various document shapes (headings, tables, mixed)
- [ ] **Note:** Replaces ad-hoc chunking in existing `RagPipeline` sample

### Citations Package (Milestone 3)
- [ ] Define `CitationReference` and `CitationSet` models
- [ ] Implement citation attachment to CoreModel blocks
- [ ] Implement citation propagation through chunking (chunk inherits block citations)
- [ ] Implement human-readable citation string formatter
- [ ] Support exact-source and coarse-source modes
- [ ] Implement citation JSON serialization/deserialization
- [ ] Unit tests for citation persistence, formatting, propagation

### DocumentIntelligence Package (Milestone 4)
- [ ] Create project with `Azure.AI.DocumentIntelligence` SDK dependency
- [ ] Implement `DocumentIntelligenceConverter` (file/stream/URL input)
- [ ] Implement Azure DI layout result → CoreModel mapper (pages, tables, figures, captions, formulas)
- [ ] Preserve confidence scores and bounding boxes in extensible metadata bag
- [ ] Authentication via Azure credentials or endpoint+key
- [ ] DI registration: `AddMarkItDotNetDocumentIntelligence()`
- [ ] Async API with resilience/error wrapping
- [ ] Integration tests (conditional on Azure DI endpoint — use `[SkipIfNoAzure]` attribute)
- [ ] Minimal sample + advanced sample
- [ ] End-to-end sample: PDF → Azure DI → CoreModel → chunks → citations → JSON
- [ ] **Note:** Follows existing AI package pattern but with proper DI registration

### DI Prerequisite (Milestone 0)
- [ ] Fix `AddMarkItDotNetAI()` to auto-register the AI plugin into `ConverterRegistry`
- [ ] Verify Excel and PowerPoint DI patterns as the reference implementation
- [ ] **Note:** This is a bug fix, not a new feature — must be done before Phase 1

## 14.2 Phase 2 Agent Tasks

### Metadata Package (Milestone 5)
- [ ] Define metadata extraction interfaces (`IMetadataExtractor`, `MetadataResult`)
- [ ] Implement title extraction from CoreModel (first heading, document property, filename fallback)
- [ ] Implement author extraction from document properties when available
- [ ] Implement language detection (evaluate library options or Azure AI dependency)
- [ ] Implement date extraction (created/modified from file or document properties)
- [ ] Implement document type tagging
- [ ] Implement heading normalization
- [ ] Define extensibility hooks for entity/tag enrichment
- [ ] Optional section summary support (may depend on AI package)
- [ ] Unit tests with representative documents
- [ ] **Decision required:** language detection — local library vs. Azure AI dependency

### Quality Package (Milestone 5)
- [ ] Define `IQualityAnalyzer` interface and `QualityReport` model
- [ ] Implement extraction confidence scoring
- [ ] Implement OCR suspicion flag heuristics
- [ ] Implement low-text-density detection
- [ ] Implement duplicate-line detection
- [ ] Implement table extraction warning heuristics
- [ ] Implement broken reading-order detection
- [ ] Generate quality report with actionable diagnostics
- [ ] Support quality-based fallback routing (e.g., low quality → Azure DI)
- [ ] Unit tests with weak/strong document examples

### VectorData Package (Milestone 6)
- [ ] Define chunk-to-record mapping abstractions
- [ ] Implement metadata payload mapping
- [ ] Implement embedding-ready payload support
- [ ] Align interfaces with `Microsoft.Extensions.VectorData`
- [ ] Implement JSONL export fallback
- [ ] At least one local vector sink sample
- [ ] **Decision required:** which version of `Microsoft.Extensions.VectorData` to target
- [ ] **Risk:** `Microsoft.Extensions.VectorData` maturity — verify API stability at implementation time

### AzureSearch Package (Milestone 6)
- [ ] Create project with `Azure.Search.Documents` SDK dependency
- [ ] Implement index schema helpers
- [ ] Implement document upload helpers
- [ ] Implement field mapping for content, metadata, vectors, citations
- [ ] Provide hybrid-search-friendly record shape guidance
- [ ] DI registration extension method
- [ ] End-to-end indexing sample
- [ ] Integration tests (conditional on Azure Search endpoint)

### Sync Package (Milestone 7)
- [ ] Define `ISyncStateStore` abstraction for sync state persistence
- [ ] Implement source hash computation
- [ ] Implement chunk hash computation
- [ ] Implement changed-content detection (compare current vs. stored hashes)
- [ ] Implement soft-delete support
- [ ] Implement version markers
- [ ] Generate sync plan output (what to add, update, delete)
- [ ] End-to-end sample: ingest → chunk → enrich → index → re-sync
- [ ] **Risk:** sync state persistence location needs design — file-based? database? pluggable?

## 14.3 Phase 3 Agent Tasks

### Connector Abstractions (Milestone 8)
- [ ] Define `IDocumentSource` interface (yields file streams + metadata)
- [ ] Define `SourceDocument` model (stream, path, metadata, source type)
- [ ] Implement `FileSystemConnector` — directory scanning, glob patterns, recursive traversal
- [ ] Implement `AzureBlobConnector` — container listing, blob download, prefix filtering
- [ ] DI registration for each connector
- [ ] Unit tests for FileSystem; integration tests for AzureBlob (conditional)
- [ ] **Decision required:** confirm FileSystem + AzureBlob as first two connectors

### Security Package (Milestone 9)
- [ ] Define `ISecurityPolicy` interface and pipeline hook points
- [ ] Implement file type allow/deny policy
- [ ] Implement file size and page count limits
- [ ] Define PII detection hook interface (bring-your-own detector)
- [ ] Define redaction pipeline hook interface
- [ ] Define secret detection hook interface
- [ ] Implement record tagging for access/governance metadata
- [ ] Configuration-driven policy application
- [ ] Unit tests with policy scenarios

### Evals Package (Milestone 9)
- [ ] Define evaluation harness framework
- [ ] Implement extraction benchmark harness (compare extraction strategies)
- [ ] Implement chunk quality benchmark harness
- [ ] Implement citation coverage metrics
- [ ] Implement latency and memory metrics collection
- [ ] Implement comparison mode across strategies (side-by-side output)
- [ ] Exportable results (JSON, CSV)
- [ ] At least one benchmark scenario documented
- [ ] **Risk:** needs representative test corpora — licensing of test documents could be an issue

---

## 15. Risks and Mitigations

### Risk: Overbuilding too early
**Mitigation**: keep packages separate and phase-gated.

### Risk: Too much Azure-specific coupling
**Mitigation**: keep `DocumentIntelligence` and `AzureSearch` isolated from neutral core packages.

### Risk: Chunking becomes too simplistic
**Mitigation**: design around `CoreModel`, not only plain text.

### Risk: Hard-to-test document behavior
**Mitigation**: use golden files and representative corpora. The existing `GoldenTests` project provides a proven pattern.

### Risk: Package sprawl hurts discoverability
**Mitigation**: publish clear package map and starter bundles.

### Risk: CoreModel introduction breaks existing API
**Mitigation**: CoreModel is an additive layer. The existing `IMarkdownConverter` → `string` API stays untouched. `IStructuredConverter` is a new parallel interface. Existing converters are adapted via mapper classes, not rewritten. No breaking changes to the public surface.

### Risk: Converter refactor scope creep
**Mitigation**: only PDF and DOCX get CoreModel bridge mappers in Phase 1. These are the most structured converters with the best internal data (page-level extraction, heading/table parsing). Other converters can be adapted later as needed.

### Risk: DI registration inconsistency across satellite packages
**Mitigation**: fix the AI package's `AddMarkItDotNetAI()` registration gap (Milestone 0) before adding new packages. This establishes the correct pattern that all Phase 1+ packages will follow.

### Risk: Microsoft.Extensions.VectorData API instability
**Mitigation**: VectorData package is Phase 2 (Milestone 6). By that point, the API should be more stable. Include a JSONL export fallback that works regardless of VectorData maturity.

### Risk: Sync state persistence design
**Mitigation**: define `ISyncStateStore` as a pluggable abstraction from day one. Ship with a simple file-based implementation; allow database or cloud-based stores as extensions.

### Risk: Evaluation corpus licensing
**Mitigation**: use small golden files from existing test data in-repo. For larger corpora, consider a separate companion repo or use publicly available test documents.

---

## 16. Open Questions

- Should `CoreModel` be fully immutable or partially mutable?
  - **Recommendation:** Immutable records with `with` expressions for builder patterns. This aligns with modern C# conventions and makes CoreModel safe for parallel processing.
- Should Markdown stay the default renderer or become one of multiple renderers?
  - **Recommendation:** Keep Markdown as the primary renderer. CoreModel is a richer parallel output path, not a replacement. The existing string-based API stays untouched.
- How much Azure Document Intelligence metadata should be preserved in raw form?
  - **Recommendation:** Preserve confidence scores and bounding boxes in an extensible metadata bag on CoreModel blocks. Raw Azure-specific types should NOT leak into CoreModel contracts.
- Which vector abstraction should be the primary interface in Phase 2?
  - **Recommendation:** `Microsoft.Extensions.VectorData` interfaces, with a JSONL export fallback. Verify API maturity at implementation time (Phase 2, Milestone 6).
- Which two connectors should ship first?
  - **Recommendation:** FileSystem (trivial, universally useful for demos) + AzureBlob (most common enterprise source). GitHub connector is a future candidate.
- Should evaluation datasets live in-repo or in a separate companion repo?
  - **Recommendation:** Small golden files in-repo (under `src/tests/`). Large corpora in a separate companion repo to avoid bloating the main repo.

---

## 17. Definition of Done

A phase is considered done when:
- all scoped packages for the phase are implemented at MVP level
- tests are in place
- docs and samples are published
- at least one end-to-end sample demonstrates real value
- APIs are reviewed for consistency
- packages are ready for preview publishing

---

## 18. Recommended First MVP Slice

If starting small, implement this first:

1. Fix AI package DI registration (Milestone 0 — establishes the pattern)
2. `CoreModel` contracts + JSON serialization + MarkdownRenderer
3. `IStructuredConverter` interface + PDF bridge mapper + DOCX bridge mapper
4. `Chunking` with heading + paragraph + token-aware strategies
5. `Citations` basic page/source/heading-path references
6. `DocumentIntelligence` Azure DI layout integration
7. Sample: PDF → CoreModel → chunks → citations → JSON export

This provides the strongest visible value with the least ambiguity. The bridge mappers (step 3) are the critical validation step — they prove CoreModel works with real documents before building Chunking/Citations on top.

---

## 19. Feasibility Analysis

> **Performed against:** current codebase as of 2026-04-06. This section documents the gap analysis between the existing architecture and the PRD requirements.

### Current Architecture Summary

| Aspect | Current State |
|--------|---------------|
| **Core API** | `IMarkdownConverter.ConvertAsync()` → `Task<string>` (Markdown) |
| **Streaming** | `IStreamingMarkdownConverter` → `IAsyncEnumerable<string>` |
| **Result model** | `ConversionResult` + `ConversionMetadata` (string, format, WordCount, ProcessingTime) |
| **Document model** | None — no blocks, sections, pages, or structured representation |
| **Target frameworks** | `net8.0;net10.0` (multi-target) |
| **DI pattern** | `ConverterRegistry` with manual plugin registration via `IConverterPlugin` |
| **Satellite packages** | Excel, PowerPoint (correct DI), AI (broken DI — doesn't auto-register), Whisper |
| **Test infrastructure** | xUnit + FluentAssertions, golden-file tests with comparison to Python markitdown |
| **Existing RAG sample** | `RagPipeline` sample does ad-hoc Markdown text chunking in app code |

### Feasibility by Package

| Package | Feasibility | Key Consideration |
|---------|-------------|-------------------|
| CoreModel | ✅ HIGH | Purely additive — new types, no changes to existing API |
| Bridge Mappers | ⚠️ MEDIUM | PDF and DOCX converters already parse structure internally — mappers extract it |
| Chunking | ✅ HIGH | Depends only on CoreModel; replaces ad-hoc sample code |
| Citations | ✅ HIGH | Depends only on CoreModel; no existing citation patterns to conflict with |
| DocumentIntelligence | ✅ HIGH | Follows existing AI package pattern; adds Azure SDK as isolated dependency |
| Metadata | ✅ HIGH | Additive enrichment layer on CoreModel |
| Quality | ✅ HIGH | Stateless analysis of CoreModel documents |
| VectorData | ⚠️ MEDIUM | Depends on `Microsoft.Extensions.VectorData` maturity |
| AzureSearch | ✅ HIGH | Isolated Azure package; follows DocumentIntelligence pattern |
| Sync | ⚠️ MEDIUM | Needs `ISyncStateStore` abstraction — persistence design required |
| Connectors | ✅ HIGH | Additive; `IDocumentSource` interface + implementations |
| Security | ⚠️ MEDIUM | Hook-based — depends on external PII/secret detection libraries |
| Evals | ⚠️ MEDIUM | Needs benchmark corpus with known-good expected outputs |

### Key Design Constraint

The existing library is **string-first**. The PRD's `CoreModel` must be introduced as an **additive parallel path**, not a replacement:

- `IMarkdownConverter` (existing) → returns `string` — stays unchanged
- `IStructuredConverter` (new) → returns `Document` (CoreModel) — new interface
- Bridge mappers adapt existing converters to produce CoreModel output without rewriting them
- No breaking changes to the public API surface

---

## 20. Summary

The next evolution of `ElBruno.MarkItDotNet` should not be “more converters only.”

It should become a modular ingestion ecosystem for .NET with:
- structured extraction
- smart chunking
- citations
- metadata
- quality
- vector/search sinks
- sync
- connectors
- security
- evals

This roadmap gives a practical, phase-based path to get there.

