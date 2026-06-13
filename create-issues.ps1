#!/usr/bin/env pwsh

# GitHub GraphQL queries to create 4 Phase 3 issues
# Requires GITHUB_TOKEN environment variable

param(
    [string]$Owner = "elbruno",
    [string]$Repo = "ElBruno.MarkItDotNet"
)

$token = $env:GITHUB_TOKEN
if (-not $token) {
    Write-Error "GITHUB_TOKEN environment variable not set. Please set it and try again."
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
}

function Create-Issue {
    param(
        [string]$Title,
        [string]$Body,
        [string[]]$Labels
    )
    
    $query = @"
mutation {
  createIssue(input: {
    repositoryId: "R_kgDOBuHKnQ"
    title: "$($Title -replace '"', '\"')"
    body: "$($Body -replace '"', '\"' -replace "`n", '\n')"
    labelIds: $($Labels | ForEach-Object { "`"$_`"" } | Join-String -Separator ",")
  }) {
    issue {
      number
      title
      url
    }
  }
}
"@
    
    try {
        $response = Invoke-RestMethod -Uri "https://api.github.com/graphql" -Method Post -Headers $headers -Body (ConvertTo-Json @{ query = $query })
        if ($response.errors) {
            Write-Error "GraphQL Error: $($response.errors | ConvertTo-Json)"
            return $null
        }
        return $response.data.createIssue.issue
    } catch {
        Write-Error "Failed to create issue: $_"
        return $null
    }
}

# Issue 1: Connectors Foundation
$issue1Title = "Phase 3: Connectors Foundation"
$issue1Body = @"
# Phase 3: Connectors Foundation

## Overview
Implement the abstraction layer and initial connectors for document sourcing, enabling MarkItDotNet to pull from multiple data sources beyond local file systems.

## Implementation Details

### 1. IDocumentSource Interface (Core Abstraction)
\`\`\`csharp
public interface IDocumentSource
{
    string Name { get; }
    IAsyncEnumerable<SourceDocument> GetDocumentsAsync(CancellationToken cancellationToken);
}

public class SourceDocument
{
    public string Id { get; set; }
    public string Path { get; set; }
    public string ContentType { get; set; }
    public Stream Content { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
\`\`\`

Location: \`src/ElBruno.MarkItDotNet.Connectors/IDocumentSource.cs\`

### 2. FileSystem Connector
- Recursive directory traversal
- File type filtering (configurable include/exclude patterns)
- Metadata extraction (size, modified date, permissions)
- Stream-based file reading (memory efficient)

Location: \`src/ElBruno.MarkItDotNet.Connectors/FileSystemConnector.cs\`

### 3. Azure Blob Storage Connector
- BlobContainerClient integration
- Streaming blob content (no download-to-memory)
- Resumable iteration (continuation tokens)
- Connection string and managed identity support

Location: \`src/ElBruno.MarkItDotNet.Connectors/AzureBlobConnector.cs\`

### 4. Dependency Injection Registration
Create ServiceCollectionExtensions for transparent connector wiring.

## Package Details
- **Package ID**: \`ElBruno.MarkItDotNet.Connectors\`
- **Version**: 0.7.0
- **Target Frameworks**: net8.0; net10.0
- **Dependencies**: Azure.Storage.Blobs (for Azure connector)

## Acceptance Criteria
- [ ] IDocumentSource interface is stable and documented
- [ ] FileSystemConnector handles edge cases (nested dirs, symlinks, permissions)
- [ ] AzureBlobConnector streams efficiently (no buffering entire blobs)
- [ ] All tests passing (40+)
- [ ] Demo sample runs end-to-end
- [ ] Documentation complete with examples
"@

# Issue 2: Security Hardening
$issue2Title = "Phase 3: Security Hardening & Policy Engine"
$issue2Body = @"
# Phase 3: Security Hardening & Policy Engine

## Overview
Expand the Security package foundation with policy-driven content filtering, PII detection/redaction, and production guardrails for large-scale ingestion pipelines.

## Implementation Details

### 1. Policy Definition & Enforcement
\`\`\`csharp
public interface ISecurityPolicy
{
    string Name { get; }
    Task<PolicyResult> EvaluateAsync(string markdown, CancellationToken cancellationToken);
}

public class PolicyResult
{
    public bool IsViolation { get; set; }
    public List<PolicyIssue> Issues { get; set; }
    public List<(int Start, int End)> ViolationRanges { get; set; }
}
\`\`\`

### 2. PII Detection & Redaction Pipeline
- Detect common PII: emails, phone numbers, SSN, credit card patterns
- Configurable redaction strategies (mask, replace, remove)
- Metadata tagging (confidence levels, detection method)
- Integration with Citations package

Location: \`src/ElBruno.MarkItDotNet.Security/PiiDetector.cs\`

### 3. Content Allow/Deny Policies
- Source-level policies (allow specific domains in URLs)
- Content-type policies (block certain media types)
- Path patterns for file ingestion
- Policy evaluation result propagation

Location: \`src/ElBruno.MarkItDotNet.Security/ContentPolicyEngine.cs\`

### 4. File-Size & Page-Count Guardrails
- Configurable limits on max file size, max page count
- Streaming validation (reject before full download)
- Quota tracking per source/session

Location: \`src/ElBruno.MarkItDotNet.Security/GuardrailsPolicy.cs\`

## Testing Requirements
- PiiDetector: 25+ tests with real PII patterns
- ContentPolicyEngine: 15+ tests for path patterns
- GuardrailsPolicy: 10+ tests for size/quota enforcement
- Integration: 5+ end-to-end tests

## Acceptance Criteria
- [ ] ISecurityPolicy interface is extensible and documented
- [ ] PII detection covers common patterns with >90% accuracy
- [ ] Allow/deny policies can be chained and composed
- [ ] Guardrails enforce limits without memory spikes
- [ ] All tests passing (50+)
- [ ] Performance: policy evaluation <10ms per chunk
"@

# Issue 3: Evaluation Tooling
$issue3Title = "Phase 3: Evaluation Tooling & Benchmark Suite"
$issue3Body = @"
# Phase 3: Evaluation Tooling & Benchmark Suite

## Overview
Expand the Evals package foundation with corpus-based benchmarking, multi-strategy comparison reports, and exportable metrics for CI/CD integration.

## Implementation Details

### 1. Benchmark Corpus & Fixtures
- Create test corpus: diverse file formats, document types, sizes (10KB to 100MB)
- Immutable fixtures for reproducible measurements

Location: \`src/tests/ElBruno.MarkItDotNet.Evals.Tests/Fixtures/BenchmarkCorpus.cs\`

### 2. Multi-Strategy Evaluation Reports
\`\`\`csharp
public class StrategyComparisonReport
{
    public List<EvaluationReport> StrategyResults { get; set; }
    public RankingResult Ranking { get; set; }
    public List<string> Recommendations { get; set; }
}
\`\`\`

### 3. Citation Coverage Metrics
- Measure % of original citations retained post-conversion
- Track citation accuracy
- Correlation with chunking strategy

Location: \`src/ElBruno.MarkItDotNet.Evals/CitationCoverageEvaluator.cs\`

### 4. Performance Metrics Collection
- Latency: time-to-conversion by format type
- Memory: peak memory usage during conversion and chunking
- Throughput: documents/second for batch operations
- Exportable JSON/CSV for CI dashboards

Location: \`src/ElBruno.MarkItDotNet.Evals/PerformanceMetrics.cs\`

## Testing Requirements
- BenchmarkCorpus: reproducible, versioned test data
- StrategyComparison: 10+ tests comparing chunking strategies
- CitationCoverageEvaluator: 8+ tests
- PerformanceMetrics: 5+ tests with synthetic load

## Acceptance Criteria
- [ ] Benchmark corpus is reproducible and comprehensive (50+ files)
- [ ] StrategyComparisonReport provides actionable rankings
- [ ] Citation coverage metrics >90% for valid conversions
- [ ] Performance metrics exportable in JSON/CSV
- [ ] All tests passing (30+)
- [ ] Demo benchmark runs in <5 seconds
"@

# Issue 4: End-to-End Samples
$issue4Title = "Phase 3: End-to-End Samples & Integration"
$issue4Body = @"
# Phase 3: End-to-End Samples & Integration

## Overview
Create comprehensive runnable samples demonstrating the full Phase 3 ecosystem: connectors, security policies, evaluations, and real-world ingestion workflows.

## Implementation Details

### 1. Connectors Demo
Path: \`src/samples/ConnectorsDemo/Program.cs\`

Demonstrates:
- FileSystem connector scanning a directory
- Azure Blob connector with connection string
- Mixing connectors in a unified ingestion loop
- Error handling and retry strategies

### 2. Security & Policies Demo
Path: \`src/samples/SecurityPoliciesDemo/Program.cs\`

Demonstrates:
- Applying PII detection and redaction
- Chaining multiple policies
- Exporting policy violations for audit

### 3. Evaluation & Benchmarking Demo
Path: \`src/samples/EvaluationDemo/Program.cs\`

Demonstrates:
- Running ConversionEvaluationEngine
- Comparing multiple chunking strategies
- Generating strategy comparison reports

### 4. Full Ingestion Workflow
Path: \`src/samples/IngestionWorkflow/Program.cs\`

End-to-end flow:
1. Source (Connectors) → Read from FileSystem or Azure Blob
2. Security (Policies) → Apply PII redaction + content policies
3. Conversion → Convert to Markdown
4. Chunking → Apply chunking strategy
5. Evaluation → Score output quality
6. Output → Export to Azure Search or save to disk

### 5. Updated Ingestion Walkthroughs
Path: \`docs/ingestion-workflows.md\`

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
- [ ] IngestionWorkflow compiles and runs with end-to-end flow
- [ ] Updated ingestion walkthroughs are clear and copy-paste-able
- [ ] README.md links to all new samples
- [ ] All samples include error handling and logging
"@

Write-Host "Creating GitHub Issues..." -ForegroundColor Cyan

# Note: Full GraphQL implementation would require proper setup
# For now, output the issues in a format that can be copied into GitHub UI
Write-Host "`n=== ISSUE 1: $issue1Title ===" -ForegroundColor Green
Write-Host $issue1Title
Write-Host "`n---BODY---"
Write-Host $issue1Body
Write-Host "`nLabels: Phase 3, connectors"

Write-Host "`n=== ISSUE 2: $issue2Title ===" -ForegroundColor Green
Write-Host $issue2Title
Write-Host "`n---BODY---"
Write-Host $issue2Body
Write-Host "`nLabels: Phase 3, security"

Write-Host "`n=== ISSUE 3: $issue3Title ===" -ForegroundColor Green
Write-Host $issue3Title
Write-Host "`n---BODY---"
Write-Host $issue3Body
Write-Host "`nLabels: Phase 3, evals"

Write-Host "`n=== ISSUE 4: $issue4Title ===" -ForegroundColor Green
Write-Host $issue4Title
Write-Host "`n---BODY---"
Write-Host $issue4Body
Write-Host "`nLabels: Phase 3, samples, documentation"

Write-Host "`n✅ Issue templates ready. Copy into GitHub UI or use 'gh issue create' with GITHUB_TOKEN." -ForegroundColor Green
