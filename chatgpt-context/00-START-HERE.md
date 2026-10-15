# ChatGPT/Codex Context — Start Here

## Purpose

This folder is a compact orientation pack for a new ChatGPT/Codex session working on the repository. It describes the current system, boundaries, source map and handoff state. It is not a replacement for source code, `AGENTS.md`, the audit reports or the manual test checklist.

## Read order

1. Read `AGENTS.md` at repository root.
2. Read this file.
3. Read `01-SYSTEM-CURRENT-STATE.md` for current implementation facts.
4. Read `02-SCOPE-AND-GUARDRAILS.md` before proposing or changing behavior.
5. Read `03-CODE-AND-DOCS-MAP.md` to locate the relevant source.
6. Read `04-WORK-HANDOFF.md` for the current task boundary.
7. Read the audit baseline when the task concerns verification, evidence or contradictions: `bao-cao/tai-lieu-ky-thuat/system-audit/01-baseline/`.

Then read only the source and documents relevant to the requested task. Do not treat this pack as proof that a feature works.

## By task type

| Task | Read next |
|---|---|
| Order, money or payment | `AGENTS.md`, `OrderService`, `OrderController`, `Order`, relevant DTOs, `02-SCOPE-AND-GUARDRAILS.md` |
| Auth, role or branch | `Program.cs`, `JwtService`, `RoleNames`, relevant controller/service and the security audit documents |
| Database or migration | `ApplicationDbContext`, entity/configuration, canonical migrations, audit baseline and migration findings |
| Frontend/API contract | relevant `App.tsx`/route, API consumer, controller/DTO and `packages/shared/swagger.json` |
| Kitchen/realtime | `KitchenHub`, `KitchenNotifier`, `PaymentEventRouting`, `KitchenService`, frontend SignalR consumers |
| AI | `AI_RULES.md`, `services/api/src/AI/`, provider registration, registry and permission service |
| Manual verification | `ba-test-frontend/README.md`, `00-COVERAGE-MATRIX.md` and the relevant role/page/flow file |
| Thesis/evidence | `05-operations-and-thesis/EVIDENCE-INDEX.md`, audit findings and current source |

## Working rules

- Do not claim to have verified a fact until the relevant source, configuration or runtime evidence has been checked.
- Static source inspection describes implementation; runtime evidence describes observed behavior in a named environment. Neither substitutes for the other.
- If the source, schema, configuration and documentation disagree, inspect the affected path and record the discrepancy. Do not silently choose an old report.
- Do not print or copy secrets, tokens, passwords, personal data, full connection strings or private certificate material.
- Preserve existing user changes. Make the smallest safe change and inspect consumers before changing a contract.

## Update metadata

Last updated: `2026-10-15`  
Reference commit: `0a480965f5a582c6da5c30fb70d9a5a2d1cd3d4e` (`main`)  
Working tree at survey: not clean; the snapshot recorded 137 modified, 149 deleted and 19,474 untracked entries when using `git status --porcelain=v1 -uall`.

The commit is only the last committed base. It does not fully represent the uncommitted working tree. Recheck the relevant paths before relying on this pack for a later change.

## Using this pack

Codex opening the repository should read this pack, inspect the relevant current source and check changes since the reference snapshot before acting.

ChatGPT receiving only attachments should use the supplied files as context, state that repository access is unavailable, and request the exact missing source excerpt when a decision depends on it.
