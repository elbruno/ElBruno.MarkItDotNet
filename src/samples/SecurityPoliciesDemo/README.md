# SecurityPoliciesDemo Sample

This sample demonstrates a configurable security policy chain:

1. Convert source text to Markdown.
2. Run `ISecurityScanner` checks (JavaScript links, secret-like tokens, control chars).
3. Apply custom policy rules (deny keywords + max content length guardrail).
4. Detect and redact PII patterns (email, phone, SSN).
5. Emit per-scenario markdown outputs and JSONL audit entries.

## Run

```bash
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj
```

## Useful options

```bash
# Dry-run (no output files or audit log writes)
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj -- --dry-run

# Override output and audit destinations
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj -- --output ./security-output --audit-log ./security-output/audit.jsonl

# Add an extra deny keyword
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj -- --deny-keyword internal-only
```

Defaults are configured in `appsettings.json` (`SecurityPoliciesDemo` section), and CLI options override those defaults.
