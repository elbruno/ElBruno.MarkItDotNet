# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| CLI commands, tool packaging, System.CommandLine | Fenster | Implement commands, argument parsing, DI setup, output formatting |
| Architecture, scope, code review | Keaton | Review PRs, design decisions, project structure |
| Tests, quality, edge cases | Hockney | xUnit tests, CLI integration tests, exit code verification |
| Documentation, skills, README | McManus | CLI docs, AI skills, README updates, copilot instructions |
| Security review, vulnerability analysis, threat modeling | Kobayashi | Code audits, dependency scanning, security findings |
| Session logging | Scribe | Automatic — never needs routing |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Keaton |
| `squad:keaton` | Architecture/review tasks | Keaton |
| `squad:fenster` | CLI implementation tasks | Fenster |
| `squad:hockney` | Testing tasks | Hockney |
| `squad:mcmanus` | Documentation tasks | McManus |
| `squad:kobayashi` | Security review tasks | Kobayashi |

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn the tester to write test cases from requirements simultaneously.
