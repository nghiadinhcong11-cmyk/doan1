# Scope and Guardrails

## Current project scope

In scope are the existing restaurant POS and management capabilities: branches, catalog, tables/reservations, staff operations, orders, kitchen workflow, payment/invoice/receipt, expenses/reporting, customer/QR ordering, loyalty, inventory, notifications, system settings, realtime kitchen communication and the current AI assistant boundary.

Use existing architecture and patterns first. Backend business rules, authorization, branch isolation, database schema and persisted financial values remain authoritative.

## Out of scope, historical or not established

- Staff profile, attendance, schedule and shift functions are in scope where current source exposes them. Do not infer additional financial staff processing from profile fields.
- Docker/Compose, CI/CD and external deployment services are not assumed to be active. Old documents or deleted paths are historical unless current source/configuration proves otherwise.
- A real external AI provider is not assumed. Current registration is Mock AI.
- Automated xUnit, Node or Playwright coverage is not assumed merely because old test files or reports exist. Check current scripts and project files.
- Production data, production database state, backup validity and operational SLA are not assumed.
- New mobile apps, microservices, distributed cache, new payment provider or new AI provider are not part of the current architecture unless the user explicitly requests a scoped change.

## Evidence classification

Keep these categories distinct:

1. User requirement: explicitly requested in the current task or repository instructions.
2. Proposal: an optional design suggested by Codex.
3. Design/documentation: described intent without proof of executable implementation.
4. Source implementation: code/configuration currently present.
5. Runtime evidence: behavior observed in a named environment and time.

When context and source disagree, inspect the relevant source and record the difference. Never convert a proposal, old report or code path into a verified behavior claim.

## Guardrails for changes

- Read `AGENTS.md` before changing code.
- Search for existing consumers before adding an entity, endpoint, DTO, hook, tool or API client.
- Preserve authentication, authorization, branch isolation, financial authority, SignalR events and database migration history.
- Do not trust frontend prices, totals, roles, branch IDs, payment amounts or AI-generated IDs as authoritative.
- Do not create or modify schema outside an EF Core migration process, and do not run migration/seed against an unverified target.
- Do not expose secrets or sensitive data in source, logs, evidence or context files.
- Do not mark a feature PASS because code compiles or was read. Use the appropriate technical, runtime or manual evidence.
- Avoid large refactors and new dependencies unless the user explicitly requests them and the benefit/cost is clear.

## Handling ambiguity

Ask for clarification only when missing information affects money, permissions, security, deletion, schema, order state or another material decision. Continue safe, read-only or well-defined parts of the task. Mark unknowns explicitly instead of filling them with assumptions.

## Optional expansion rule

Any proposed feature, architecture expansion, automation or new technology must be labeled `Tùy chọn`, explain its reason, expected benefit and implementation/testing cost. It must remain separate from the current-state description and must not silently become a project requirement.

## Context maintenance

Update only the context file(s) affected by a real change:

- system behavior/architecture: `01-SYSTEM-CURRENT-STATE.md`;
- scope or safety boundary: this file;
- source/document location: `03-CODE-AND-DOCS-MAP.md`;
- task status/decision: `04-WORK-HANDOFF.md`;
- reusable session instructions: `00-START-HERE.md` or `05-NEW-SESSION-PROMPT.md`;
- compact attachment summary: `CHATGPT-CONTEXT-PACK.md`.

Do not require every small task to rewrite the entire pack. Refresh the pack when a change materially affects shared assumptions.
## Confirmed employee-profile decision

Retain EmployeeType and BasicSalary in the employee profile across entity, persistence, API and frontend. Treat them only as recorded profile data. Do not propose deleting them or infer salary-processing functionality from their presence. Historical migration metadata that mentions broader staff-processing schema remains a separate open review item in PAY-001.
