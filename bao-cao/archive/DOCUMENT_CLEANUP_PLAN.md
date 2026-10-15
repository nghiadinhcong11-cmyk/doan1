# Documentation Cleanup Plan

Audit snapshot: 2026-09-30. Phase 10 review completed: 2026-09-30. Cleanup uses archive-first handling; no source, migration, test, runtime configuration, or protected repository file is touched.

Classification: `KEEP` = authoritative/useful and should remain; `MERGE` = preserve useful content in `docs/system-audit/`; `ARCHIVE` = retain as historical/design material but remove from the primary documentation path; `DELETE CANDIDATE` = only after human confirmation that it contains no unique information.

| File | Classification | Reason | Information to preserve | Proposed action |
|---|---|---|---|---|
| `README.md` | KEEP | Project-level entry point and repository navigation | Setup and navigation | Kept; linked to `docs/system-audit/00-README.md` and marked as supplementary documentation |
| `AGENTS.md` | KEEP | Repository engineering rules | All instructions | Do not change |
| `AI_RULES.md` | KEEP | AI-specific engineering constraints | Permission and safety rules | Keep as policy, cross-reference audit |
| `docs/PROJECT_CONTEXT.md` | ARCHIVE | Prior code-oriented context with stale role/production claims | Historical evidence and prior findings | Moved unchanged to `docs/archive/legacy-audits/` |
| `docs/AUDIT_GUIDANCE_FILES.md` | ARCHIVE | Prior audit guidance/report | Audit method and historical discrepancy analysis | Moved unchanged to `docs/archive/legacy-audits/` |
| `docs/AUDIT_EXPECTED_VS_ACTUAL_USE_CASE.md` | ARCHIVE | Earlier comparison audit, superseded by current inventory/conflicts | Historical use-case evidence | Moved unchanged to `docs/archive/legacy-audits/` |
| `docs/USE_CASE_EXPECTED_VS_ACTUAL_AUDIT.md` | ARCHIVE | Duplicate/related use-case audit | Historical use-case evidence | Moved unchanged to `docs/archive/legacy-audits/` |
| `docs/FINAL_*_UML_AUDIT.md` | ARCHIVE | Multiple historical final UML audits overlap | Decisions about diagrams and semantic corrections | Moved unchanged to `docs/archive/legacy-audits/` |
| `docs/AUTHORIZATION_GAP_FIX_REPORT.md` | ARCHIVE | Historical remediation report, superseded by current security audit | Security history and remediation context | Moved unchanged to `docs/archive/legacy-audits/` |
| `docs/AI_TASK20_ANALYTICS_DESIGN.md` | ARCHIVE | Design artifact; not necessarily current implementation | Planned analytics concepts | Moved unchanged to `docs/archive/design-history/` |
| `docs/AI_TASK21_BUSINESS_ADVISOR_DESIGN.md` | ARCHIVE | Design artifact | Planned AI concepts | Moved unchanged to `docs/archive/design-history/` |
| `docs/AI_TASK22_FINANCIAL_INTELLIGENCE_DESIGN.md` | ARCHIVE | Design artifact partially reflected by current analytics code | Design rationale and assumptions | Moved unchanged to `docs/archive/design-history/` |
| `docs/AI_TASK22_INVENTORY_DESIGN.md` | ARCHIVE | Inventory design without confirmed end-to-end implementation | Planned scope | Mark planned; retain |
| `docs/AI_TASK23_PROACTIVE_INSIGHTS_DESIGN.md` | ARCHIVE | Design artifact partially reflected by current insight code | Design rationale | Moved unchanged to `docs/archive/design-history/` |
| `docs/Hinh_3_1_Use_Case_Tong_Quat.drawio` | KEEP | Diagram source may be used by report | Diagram structure | Validate against use-case inventory |
| `baocaodoan/README.md` and `baocaodoan/00...16` | KEEP | Existing thesis/report pack referenced by root README | Narrative, report structure and historical context | Kept; source-of-truth precedence is documented in root README |
| `docs/API_CLIENT.md`, `docs/COST_MANAGEMENT.md`, `docs/JWT_AUTH_UPDATE.md`, `docs/PROGRESS.md`, `docs/REFACTORING_SUMMARY.md` | PRE-EXISTING DELETED | Already had `D` status before Phase 10 | Historical decisions if recoverable from Git | Not restored, not deleted by Phase 10 |
| `output.txt`, `output_utf8.txt` | ARCHIVE | Generated test-output files; duplicate historical result, not application inputs | Historical test result | Moved unchanged to `docs/archive/generated-output/` |

Phase 10 result: `DELETE = none`. All current cleanup candidates with any historical value were archived or kept. The five `D` files above are explicitly pre-existing working-tree deletions and were not touched.

Decision gate: future deletion of archived material still requires human review. The audit set remains the authoritative current snapshot; archived files are preserved for traceability and are not normative.
