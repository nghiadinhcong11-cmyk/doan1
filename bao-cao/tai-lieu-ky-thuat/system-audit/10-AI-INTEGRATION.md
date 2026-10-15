# AI Integration (current)

The current runtime uses deterministic local Mock AI. `AiController` accepts `POST /api/Ai/chat` for authenticated users and `POST /api/Ai/guest-chat` for the development-only guest scope. The frontend chat components call those contracts and do not select a provider or send API keys.

The active path is:

`ChatBot.tsx` → `AiController` → `AiOrchestrator` → `IAiProvider` registered as `MockAiProvider` → `AiToolRegistry` / `AiPermissionService` → application services → Mock AI response.

The orchestrator preserves request-local history, sequential function calls, server-side role and branch checks, read-only mode, guest menu scope and the product-price confirmation boundary. Mock AI is deterministic and only recognizes its allowlisted intents; it is not a general-purpose model and does not call an external service.

`AiToolRegistry`, `AiPermissionService`, `AiAuthorization`, `AiContextService`, the tool implementations, audit sink and controller contracts are shared AI infrastructure and remain in use. Function declaration and response parsing are the existing internal contract consumed by Mock AI and the orchestrator, not an outbound provider integration.

No external model client, HTTP registration, routing/fallback path or provider-specific dependency is part of the current registration. No external AI request is made by the current registration. Runtime verification is limited to static inspection/build evidence; no external endpoint was contacted.

This document is the current source-facing AI description. Older audit wording must be updated to this Mock-only boundary rather than reused as an implementation claim.
