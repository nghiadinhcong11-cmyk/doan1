# AI Integration (current)

The current runtime uses deterministic local Mock AI. `AiController` accepts `POST /api/Ai/chat` for authenticated users and `POST /api/Ai/guest-chat` for the development-only guest scope. The frontend chat components call those contracts and do not select a provider or send API keys.

The active path is:

`ChatBot.tsx` → `AiController` → `AiOrchestrator` → `IAiProvider` registered as `MockAiProvider` → `AiToolRegistry` / `AiPermissionService` → application services → Mock AI response.

The orchestrator preserves request-local history, sequential function calls, server-side role and branch checks, read-only mode, guest menu scope and the product-price confirmation boundary. Mock AI is deterministic and only recognizes its allowlisted intents; it is not a general-purpose model and does not call an external service.

`AiToolRegistry`, `AiPermissionService`, `AiAuthorization`, `AiContextService`, the tool implementations, audit sink and controller contracts are shared AI infrastructure and remain in use. Function declaration and response parsing are the existing internal contract consumed by Mock AI and the orchestrator, not an outbound provider integration.

No external model client, HTTP registration, routing/fallback path or provider-specific dependency is part of the current registration. No external AI request is made by the current registration. Runtime verification is limited to static inspection/build evidence; no external endpoint was contacted.

This document is the current source-facing AI description. Older audit wording must be updated to this Mock-only boundary rather than reused as an implementation claim.

The detailed static review, intent allowlist, complete registered-tool inventory, confirmation matrix and open findings are in [`02-static-review/MOCK-AI-REVIEW.md`](02-static-review/MOCK-AI-REVIEW.md). The consolidated findings are `AI-001..AI-004` in [`06-findings/FINDINGS-REGISTER.md`](06-findings/FINDINGS-REGISTER.md); all remain open and runtime-unverified.
## Current reconciliation — 17/10/2026

This file preserves the earlier source-facing Mock AI audit. The current runtime additionally has a Development/Test-only `ManagerEmbeddingPilot` for the frozen manager-two-intent-v3.3 baseline. MockAiProvider remains active for flag-OFF and non-pilot paths. Current acceptance evidence is in `../AI-EMBEDDING-MANAGER-ACCEPTANCE-FINAL.md`; this does not claim a general external AI provider or full-role Sentence Embedding rollout.
