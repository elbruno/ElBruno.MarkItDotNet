#!/usr/bin/env pwsh

# Create Phase 3 GitHub Issues for ElBruno.MarkItDotNet
# Usage: ./create-github-issues.ps1
#
# Prerequisites:
#   1. Set GITHUB_TOKEN environment variable with a personal access token
#   2. Token must have 'repo' scope
#
# Steps:
#   $env:GITHUB_TOKEN = "your_token_here"
#   ./create-github-issues.ps1

param(
    [string]$Owner = "elbruno",
    [string]$Repo = "ElBruno.MarkItDotNet",
    [string]$Token = $env:GITHUB_TOKEN
)

if (-not $Token) {
    Write-Error @"
Error: GITHUB_TOKEN environment variable not set.

Please set your GitHub personal access token:
  `$env:GITHUB_TOKEN = 'ghp_xxxxxxxxxxxxxxxxxxxx'

Then run this script again.

To create a personal access token:
  1. Go to https://github.com/settings/tokens
  2. Click "Generate new token (classic)"
  3. Select scopes: 'repo' (full control)
  4. Copy the token and set it as above
"@
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $Token"
    "Accept"        = "application/vnd.github.v3+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

function Create-GitHubIssue {
    param(
        [string]$Title,
        [string]$Body,
        [string[]]$Labels = @()
    )
    
    $bodyJson = @{
        title  = $Title
        body   = $Body
        labels = $Labels
    } | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod `
            -Uri "https://api.github.com/repos/$Owner/$Repo/issues" `
            -Method Post `
            -Headers $headers `
            -Body $bodyJson `
            -ContentType "application/json"
        
        Write-Host "✅ Issue #$($response.number) created: $($response.title)" -ForegroundColor Green
        Write-Host "   URL: $($response.html_url)" -ForegroundColor Cyan
        return $response
    } catch {
        Write-Error "Failed to create issue '$Title': $($_)"
        return $null
    }
}

Write-Host "Creating Phase 3 GitHub Issues for $Owner/$Repo..." -ForegroundColor Cyan
Write-Host ""

# Issue 1: Connectors Foundation
$issue1 = Create-GitHubIssue `
    -Title "Phase 3: Connectors Foundation" `
    -Body @"
# Phase 3: Connectors Foundation

## Overview
Implement the abstraction layer and initial connectors for document sourcing, enabling MarkItDotNet to pull from multiple data sources beyond local file systems.

## Implementation Details

### 1. IDocumentSource Interface (Core Abstraction)
Create the foundational interface for document sources.

**Location:** ``src/ElBruno.MarkItDotNet.Connectors/IDocumentSource.cs``

### 2. FileSystem Connector
Implement local directory scanning with:
- Recursive directory traversal
- File type filtering (configurable include/exclude patterns)
- Metadata extraction (size, modified date, permissions)
- Stream-based file reading (memory efficient)

**Location:** ``src/ElBruno.MarkItDotNet.Connectors/FileSystemConnector.cs``

### 3. Azure Blob Storage Connector
Implement Azure Blob sourcing with:
- BlobContainerClient integration
- Blob metadata mapping (tags, properties)
- Streaming blob content (no download-to-memory)
- Resumable iteration (continuation tokens)

**Location:** ``src/ElBruno.MarkItDotNet.Connectors/AzureBlobConnector.cs``

### 4. Dependency Injection Registration
Create ServiceCollectionExtensions for transparent connector wiring.

## Package Details
- **Package ID:** ``ElBruno.MarkItDotNet.Connectors``
- **Version:** 0.7.0
- **Target Frameworks:** net8.0; net10.0
- **Dependencies:** Azure.Storage.Blobs (for Azure connector)

## Testing Requirements
- FileSystemConnector: 20+ tests covering directory traversal, filtering, metadata
- AzureBlobConnector: 15+ tests with mock Azure SDK
- IDocumentSource: Contract tests ensuring all implementations honor interface

## Acceptance Criteria
- [ ] IDocumentSource interface is stable and documented
- [ ] FileSystemConnector handles all tested edge cases (nested dirs, symlinks, permissions)
- [ ] AzureBlobConnector streams efficiently (no buffering entire blobs)
- [ ] All tests passing (40+)
- [ ] Package builds and publishes with v0.7.0
- [ ] Demo sample runs end-to-end
- [ ] Documentation complete with examples
"@ `
    -Labels @("Phase 3", "connectors")

Write-Host ""

# Issue 2: Security Hardening
$issue2 = Create-GitHubIssue `
    -Title "Phase 3: Security Hardening & Policy Engine" `
    -Body @"
# Phase 3: Security Hardening & Policy Engine

## Overview
Expand the Security package foundation with policy-driven content filtering, PII detection/redaction, and production guardrails for large-scale ingestion pipelines.

## Implementation Details

### 1. Policy Definition & Enforcement
Create a policy abstraction for consistent security rules via ``ISecurityPolicy`` interface and ``PolicyResult`` class.

### 2. PII Detection & Redaction Pipeline
- Detect common PII: emails, phone numbers, SSN, credit card patterns
- Configurable redaction strategies (mask, replace, remove)
- Metadata tagging (confidence levels, detection method)
- Integration with Citations package

**Location:** ``src/ElBruno.MarkItDotNet.Security/PiiDetector.cs``

### 3. Content Allow/Deny Policies
- Source-level policies (e.g., allow only specific domains in URLs)
- Content-type policies (block certain media types)
- Path patterns for file ingestion (e.g., exclude /node_modules)
- Policy evaluation result propagation into downstream chunking

**Location:** ``src/ElBruno.MarkItDotNet.Security/ContentPolicyEngine.cs``

### 4. File-Size & Page-Count Guardrails
- Configurable limits on max file size, max page count (for PDFs)
- Streaming validation (reject before full download)
- Quota tracking per source/session

**Location:** ``src/ElBruno.MarkItDotNet.Security/GuardrailsPolicy.cs``

## Testing Requirements
- PiiDetector: 25+ tests with real PII patterns (SSN, phone, email, credit card)
- ContentPolicyEngine: 15+ tests for path patterns, domain filtering
- GuardrailsPolicy: 10+ tests for size/quota enforcement
- Integration: 5+ end-to-end tests with Security + Citations + Chunking

## Acceptance Criteria
- [ ] ISecurityPolicy interface is extensible and documented
- [ ] PII detection covers common patterns with >90% accuracy
- [ ] Allow/deny policies can be chained and composed
- [ ] Guardrails enforce limits without memory spikes
- [ ] Policy metadata flows seamlessly into Citations
- [ ] All tests passing (50+)
- [ ] Performance: policy evaluation <10ms per chunk
"@ `
    -Labels @("Phase 3", "security")

Write-Host ""

# Issue 3: Evaluation Tooling
$issue3 = Create-GitHubIssue `
    -Title "Phase 3: Evaluation Tooling & Benchmark Suite" `
    -Body @"
# Phase 3: Evaluation Tooling & Benchmark Suite

## Overview
Expand the Evals package foundation with corpus-based benchmarking, multi-strategy comparison reports, and exportable metrics for CI/CD integration.

## Implementation Details

### 1. Benchmark Corpus & Fixtures
- Create test corpus: diverse file formats, document types, sizes (10KB to 100MB)
- Immutable fixtures for reproducible measurements
- Metadata: expected output characteristics, known edge cases

**Location:** ``src/tests/ElBruno.MarkItDotNet.Evals.Tests/Fixtures/BenchmarkCorpus.cs``

### 2. Multi-Strategy Evaluation Reports
Extend ConversionEvaluationEngine to compare chunking strategies and generate ranking reports with recommendations.

**Location:** ``src/ElBruno.MarkItDotNet.Evals/StrategyComparison.cs``

### 3. Citation Coverage Metrics
- Measure % of original citations retained post-conversion
- Track citation accuracy (correct mapping of references)
- Correlation with chunking strategy (heading-based vs. paragraph-based)

**Location:** ``src/ElBruno.MarkItDotNet.Evals/CitationCoverageEvaluator.cs``

### 4. Performance Metrics Collection
- Latency: time-to-conversion by format type
- Memory: peak memory usage during conversion and chunking
- Throughput: documents/second for batch operations
- Exportable JSON/CSV for CI dashboards

**Location:** ``src/ElBruno.MarkItDotNet.Evals/PerformanceMetrics.cs``

## Testing Requirements
- BenchmarkCorpus: reproducible, versioned test data
- StrategyComparison: 10+ tests comparing chunking strategies
- CitationCoverageEvaluator: 8+ tests with known citation patterns
- PerformanceMetrics: 5+ tests with synthetic load

## Acceptance Criteria
- [ ] Benchmark corpus is reproducible and comprehensive (50+ files)
- [ ] StrategyComparisonReport provides actionable rankings
- [ ] Citation coverage metrics >90% for valid conversions
- [ ] Performance metrics exportable in JSON/CSV
- [ ] All tests passing (30+)
- [ ] Demo benchmark runs in <5 seconds on single file
- [ ] CI integration shows metrics trending
"@ `
    -Labels @("Phase 3", "evals")

Write-Host ""

# Issue 4: End-to-End Samples
$issue4 = Create-GitHubIssue `
    -Title "Phase 3: End-to-End Samples & Integration" `
    -Body @"
# Phase 3: End-to-End Samples & Integration

## Overview
Create comprehensive runnable samples demonstrating the full Phase 3 ecosystem: connectors, security policies, evaluations, and real-world ingestion workflows.

## Implementation Details

### 1. Connectors Demo
**Path:** ``src/samples/ConnectorsDemo/Program.cs``

Demonstrates:
- FileSystem connector scanning a directory
- Azure Blob connector with connection string
- Mixing connectors in a unified ingestion loop
- Error handling and retry strategies

### 2. Security & Policies Demo
**Path:** ``src/samples/SecurityPoliciesDemo/Program.cs``

Demonstrates:
- Applying PII detection and redaction
- Chaining multiple policies (content + guardrails)
- Exporting policy violations for audit
- Integration with Security package

### 3. Evaluation & Benchmarking Demo
**Path:** ``src/samples/EvaluationDemo/Program.cs``

Demonstrates:
- Running ConversionEvaluationEngine on converted output
- Comparing multiple chunking strategies
- Generating strategy comparison reports
- Exporting metrics for dashboards

### 4. Full Ingestion Workflow
**Path:** ``src/samples/IngestionWorkflow/Program.cs``

End-to-end flow:
1. **Source (Connectors):** Read from FileSystem or Azure Blob
2. **Security (Policies):** Apply PII redaction + content policies
3. **Conversion:** Convert to Markdown
4. **Chunking:** Apply chunking strategy
5. **Evaluation:** Score output quality
6. **Output:** Export to Azure Search or save to disk

### 5. Updated Ingestion Walkthroughs
**Path:** ``docs/ingestion-workflows.md``

Create/update walkthroughs for:
- "Ingest a local folder"
- "Ingest from Azure Blob Storage"
- "Apply security policies to sensitive documents"
- "Compare chunking strategies and pick the best one"
- "Export evaluation metrics to CI dashboard"

## Testing Requirements
- Each sample compiles and runs without errors
- Sample output matches expected format
- Samples work with all supported .NET versions (8.0, 10.0)
- No hardcoded paths or secrets in samples

## Acceptance Criteria
- [ ] ConnectorsDemo compiles and runs
- [ ] SecurityPoliciesDemo compiles and runs
- [ ] EvaluationDemo compiles and runs
- [ ] IngestionWorkflow compiles and runs end-to-end
- [ ] Updated ingestion walkthroughs are clear and copy-paste-able
- [ ] README.md links to all new samples
- [ ] No sample depends on external services (use mocks/stubs)
- [ ] All samples include error handling and logging
"@ `
    -Labels @("Phase 3", "samples", "documentation")

Write-Host ""
Write-Host "✅ All 4 Phase 3 issues created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. ✅ v0.6.1 is published to NuGet"
Write-Host "2. ✅ 4 Phase 3 issues created in GitHub"
Write-Host "3. 📝 Review issues and begin Phase 3 implementation"
Write-Host ""
Write-Host "View issues at: https://github.com/$Owner/$Repo/issues" -ForegroundColor Cyan
