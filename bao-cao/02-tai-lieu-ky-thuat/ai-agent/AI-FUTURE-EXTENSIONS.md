# AI Future Extensions

This document records possible future research directions only. They are outside the current AI scope and were not implemented.

## Kitchen

Potential future capability:

- Read-only summary of active kitchen requests and preparation status.
- Branch-scoped notification or backlog explanation.
- Safe status lookup through `KitchenService` and existing `KitchenHub` boundaries.

Required safeguards:

- Keep KDS UI and SignalR as the authoritative operational flow.
- Use trusted role/branch context.
- Never write order or kitchen state from an ambiguous prompt.
- Require explicit permission and confirmation for any status mutation.
- Add regression tests for branch groups and unauthorized users.

## Employee

Potential future capability:

- Own shift and schedule lookup.
- Own assigned table/order summary.
- Read-only explanation of assigned work.

Required safeguards:

- Scope results to the authenticated employee and branch.
- Do not expose management reports or other employees' data.
- Keep `get_my_shift` and shared tools behind a dedicated capability contract.
- Add role and branch isolation tests before enabling a feature flag.

## Cashier

Potential future capability:

- Read-only order/payment status lookup.
- Explanation of a persisted order total.
- Confirmation-gated workflows for supported cashier actions.

Required safeguards:

- Use persisted backend totals and payment state.
- Never accept client totals or infer payment completion from chat.
- Preserve AiPermissionService and confirmation boundaries.
- Keep mutations disabled until a separate approved intent catalog exists.

## Common prerequisites

Before any future role is enabled:

1. Define an intent and capability contract.
2. Separate reference, development and holdout data.
3. Verify tool permissions, branch scope and parameter validation.
4. Add feature-gated runtime wiring with fail-closed behavior.
5. Run isolated harness, regression and UAT logging checks.
6. Obtain explicit review before changing the frozen manager baseline or extending the pilot.

No future role, intent or backend behavior was added by this audit.




## Current status / Superseded by

Current future-scope roadmap; not an acceptance report.
