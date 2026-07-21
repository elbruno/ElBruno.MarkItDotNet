# Security Policies Guide

This guide explains the policy-chain approach used in `SecurityPoliciesDemo`.

## What the sample applies

1. **Scanner rules** from `ElBruno.MarkItDotNet.Security`:
   - JavaScript links
   - Secret-like token detection
   - Control character detection
2. **Custom policy rules**:
   - Deny keyword checks (for example `confidential`)
   - Max content length guardrail
   - PII pattern detection (email, phone, SSN)
   - Optional redaction with a configurable mask
3. **Audit output**:
   - JSONL entry per scenario with scanner and policy outcomes

## Run

```bash
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj
```

Dry-run:

```bash
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj -- --dry-run
```

## Configuration

`src/samples/SecurityPoliciesDemo/appsettings.json` (`SecurityPoliciesDemo` section):

- `OutputPath`
- `AuditLogPath`
- `MaxIssues`
- `MaxContentLength`
- `EnablePiiRedaction`
- `RedactionMask`
- `DenyKeywords`
- scanner toggles (`DetectJavaScriptLinks`, `DetectSecretLikeTokens`, `DetectControlCharacters`)

CLI arguments override these values:

```bash
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj -- --output ./security-output --audit-log ./security-output/audit.jsonl --deny-keyword internal-only
```

## Integration notes

- Use `--dry-run` in CI or experimentation flows when you only need policy results.
- Keep deny keywords domain-specific (legal, compliance, tenant terms).
- Treat PII regex matching as baseline heuristics; production apps can plug in stronger detectors upstream.
