# Work Handoff

## TẠM DỪNG THEO YÊU CẦU — bàn giao phiên sau (2026-10-15)

**Trạng thái hiện tại: tạm dừng, không tích hợp embedding.** Mục này thay thế các dòng “Latest/next” lịch sử phía dưới khi xác định việc cần tiếp tục. Không tự resume hoặc tiếp tục sửa theo đề xuất cũ.

### 1. Đã hoàn thành và ranh giới kiểm chứng

- Đã audit Mock AI và sửa source AI-001/003/004; giữ trạng thái **Đã sửa source, chờ kiểm chứng runtime**. Bỏ mount ChatBot bếp, backend loại kitchen khỏi endpoint AI tương ứng; giữ KitchenHub/KDS/notification/service bếp. Không coi thay đổi source là runtime PASS.
- Bộ test tay chính ở `ba-test-frontend/ai-chat/` có **104 ID**: OWN-AI-01..54, CUS-AI-01..35, QR-AI-01..15. Case kỹ thuật AI-01..12 và liên vai trò vẫn riêng, không cộng vào 104; không xóa evidence cũ, các case chưa chạy vẫn **Chưa test**.
- E5 O4 chạy trong spike .NET ONNX; tokenizer, masked mean pooling, L2 và consistency với Python cùng artifact đã kiểm chứng trước đây. Điều đó **không chứng minh O4 tương đương model gốc** hoặc chất lượng intent tiếng Việt.
- Adapter dùng session lại, MaxConcurrency=1, admission/queue bounded (các lượt review dùng QueueCapacity=2), cancellation/dispose đã có kiểm tra kỹ thuật trước. Native inference không bảo đảm dừng ngay khi cancellation. Không suy hosting đủ RAM.
- **Runtime chat vẫn dùng MockAiProvider/recognizer theo luật hiện tại.** Spike chưa nối provider/orchestrator/permission/tool/frontend. EmployeeType/BasicSalary giữ nguyên; không thêm write intent hoặc thay quyền.

### 2. Mốc thử nghiệm, phiên bản và số liệu

Giữ `.tools/mock-ai-embedding-spike/profiles/max-targeted-revision-v2.json`, SHA256 `4BE05424E8DAC532BAF6E9CD74443B4BFD6D12E586CE1E6392AA93426CD5E8C2`: targeted-revision-v2, **max cosine / score .85 / margin .01**, 37 retained, 35 competing, 9 label. Kiểm kê khi tạm dừng: cả 9 hash source/data trong manifest còn khớp. Đây là mốc thử nghiệm, không phải cấu hình runtime; CLI default cũ không tự đổi theo manifest.

| Mốc | Bằng chứng chính / giới hạn |
|---|---|
| full-baseline max/.85/.02 | CAL accepted đúng7/24, SUP1/18; các tập là development. |
| So sánh max vs top-2 mean ở .85/.02 | Mean CAL8/24, SUP3/18, nhưng có hồi quy discrimination; không chọn mean làm baseline. |
| `threshold-trials-v1.json`, max/full-baseline | T0 .85/.02, T1 .85/.015, T2 .85/.01, T3 .85/.005: accepted đúng8/12/14/17 trên19 ready /42 tổng; 0 false accept quan sát được, không accuracy độc lập. |
| `max-full-baseline-candidate-v1.json` | Chọn T2 .85/.01; 14/19 ready, CAL23 còn sai. Manifest giữ nguyên lịch sử; binary/pipeline hiện là v2 nên không tự tuyên bố tái lập v1. |
| EMB v1 chưa sửa | Candidate21/26 single-target; accepted đúng10/17 ready (30 tổng), 0 sai đã chốt, 1 accepted unknown E22; 12 câu có exact/near overlap. |
| targeted-revision-v2 hồi quy | CAL9/11 ready (24 tổng), SUP6/8 (18 tổng), EMB12/17 (30 tổng): tổng27/36 ready; 0 accepted sai quan sát được, E22 policy unknown. |
| `check-targeted-v2.json`, 15 câu mới lúc đó | Candidate9/14 single-target, ready candidate4/6; accepted đúng3/6 ready =3/15 tổng, sai0 quan sát được; 3 valid bị chặn, non-accept đúng8/9. Không đánh giá độc lập bên ngoài. |

CAL24/SUP18/EMB30 và NEW-V2-01..15 **đều đã được quan sát**, không gọi là independent clean evaluation hoặc dùng lại làm bằng chứng tuning độc lập. Không đổi câu/nhãn/output cũ.

Lỗi còn: E28/CAL21/SUP07 routing revenue chưa đúng; CAL10/menu và SUP02/comparison còn nhầm; E03/E05/E11/E13/E15 còn bị margin chặn. Parser E08/E11 lấy đúng alias nhưng biên “trong tuần thứ…” còn nguy cơ khớp this_week chưa được hỗ trợ. E24 vẫn candidate write, chỉ sửa hành động thành hỏi lại. CAL23 vẫn raw MY_ORDERS, loyalty veto chặn tool đúng capability, không coi semantic đã sửa. Tập mới: N03 dấu phẩy/tiền cảnh chặn nhầm multi-intent, N02 menu→ranking, N05 orders→inventory-unsupported, N14 other-orders→loyalty-unsupported.

**E22 “Báo cáo bán hàng hôm nay” chưa chốt:** mặc định KPI summary, revenue-only hay hỏi lại? Không tự gán nhãn chính thức/tính đúng. Candidate v2 là comparison bất hợp lý; không lấy quyền backend che sai intent.

### 3. Model/artifact thực tế và tác vụ tải

Trước yêu cầu tạm dừng đã tra cứu **đúng một ứng viên** `sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2`, revision `e8f8c211226b894fcb81acc59f3b34ba3efd5f42`. Model card/API chính thức khai Apache-2.0, có vi; metadata local xác nhận max_seq_length128, mean pooling384, do_lower_case=false. Hướng dùng không thêm prefix query của E5. Đây là **metadata, chưa là inference chạy đúng**.

Tác vụ PowerShell tải session71907 đã **exit0 hoàn tất trước lệnh Ctrl+C khi bàn giao**; tool trả toàn bộ checksum. Không còn session tải đó đang chạy. Không bắt đầu tải mới hoặc dừng API/process khác. Chưa tạo project so sánh mới, chưa sửa adapter để chạy MiniLM, chưa build/inference/scoring/benchmark MiniLM, **chưa có kết quả E5–MiniLM**.

Model/artifact và trạng thái đầy đủ (đường dẫn repository-relative, checksum, bytes) ở mục kiểm kê tạm dừng đầu `MOCK-AI-EMBEDDING-CALIBRATION.md`. Tóm tắt:
- E5 revision `614241f622f53c4eeff9890bdc4f31cfecc418b3`: `artifacts/onnx/model_O4.onnx` **235,052,531 bytes**, SHA256 `4654C156F3E4171ABC9C716CDB771BF9116455D15AC1AAB364AEEEDE0E3205B0`, đã dùng; tokenizer17,082,730 bytes, SHA256 `0B44A9D7B51C3C62626640CDA0E2C2F70FDACDC25BBBD68038369D14EBDF4C39`.
- E5 `artifacts/onnx/model.onnx` **370,938,763 bytes**, SHA256 `BF8ADC716255C5ECDFCD7A10039FCCC11EB1F98BCFEE0F9AB6007F99C24702C3`: **file tải dở theo spike cũ**, không sử dụng; có file/hash không đồng nghĩa artifact hoàn chỉnh.
- MiniLM `artifacts/minilm/onnx__model_O4.onnx` **235,166,264 bytes**, SHA256 `307BBA13F9F5708461169C9F2D633C76C5572919BCC998606B6E5AEA46F05DB4`, hoàn tất/match LFS metadata chính thức; tokenizer9,081,518 bytes SHA256 `2C3387BE76557BD40970CEC13153B3BBF80407865484B209E655E5E4729076B8`, hoàn tất. Sáu metadata/README file đi kèm cũng tải xong. **Không có MiniLM partial được phát hiện trong folder này**; không khẳng định mọi cache ngoài folder đều sạch.
- Chỉ tra metadata `model_quint8_avx2.onnx` của **cùng model MiniLM**, không tải: size118,453,870, advertised SHA256 `98A01D88B7DE996CDEA58C32CA71208C09968D143798814B2EA09D3439DC334F`. Không có chạy/tải int8 để so sánh. Không tự tải thêm biến thể/xóa file trong bàn giao.

Các đường dẫn `artifacts/...` trên tương đối `.tools/mock-ai-embedding-spike/`. Artifact/cache/output/venv/bin/obj được ignore; `git check-ignore` xác nhận cả hai O4, output mới và binary build. Không đưa binary lớn vào Git.

### 4. Môi trường, tài nguyên và Git

Máy local Windows11 Home Single Language10.0.26200; CPU i3-1125G4 4core/8logical; RAM lắp8,268,136,448 bytes (~7.70GiB). Trong bước chuẩn bị so sánh, CIM quan sát RAM trống574,308 rồi460,548 KiB (~561→450MiB), free virtual3,261,072KiB ở lần thứ hai. Đây là **hai snapshot áp lực bộ nhớ**, không peak/benchmark hay kết luận máy đủ deploy; không load hai model đồng thời.

Số E5 **lịch sử** median warm5.422ms và post-load working set~616.8MB thuộc spike trước trên máy local; không đo lại trong bước chuẩn bị so sánh/tạm dừng, không so trực tiếp với MiniLM chưa chạy. Nguồn/công cụ: Get-CimInstance (đã cần quyền ngoài sandbox), Get-FileHash SHA256, Get-ChildItem; HTTP chính thức Hugging Face lấy artifact. Network/CIM trong sandbox ban đầu lỗi, lần được cấp quyền mới thành công; không dùng kết quả lỗi làm số liệu.

HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`. `git status --short`: **31 tracked modified +7 untracked entries cấp mặc định** (bao gồm directory, không phải tổng số file -uall). Nhóm dirty: AI backend/controller/orchestrator/tool validator, frontend ChatBot/kitchen route; checklist role/coverage/technical; audit/findings/index/context; .gitignore; spike và tài liệu AI mới. Các thay đổi này đã tồn tại trước bàn giao, không revert/commit/push. Lượt tạm dừng **chỉ sửa tài liệu**.

Đã kiểm chứng trước: build spike Release0warning/error, scoring hồi quy CAL/SUP/EMB và tập15, adapter lifecycle; source Mock fixes có build/evidence lịch sử riêng, không diễn giải thành runtime PASS. Phiên chuẩn bị so sánh chỉ tra metadata/tải file/kiểm kê; **không build mới/inference/benchmark/API/browser/database/EF/migration/seed**. Không xác nhận migration nào đã được áp dụng bởi việc bàn giao này.

### 5. Quy tắc tiếp tục và prompt phiên sau

1. Đọc AGENTS.md, AI_RULES.md, mục bàn giao này, calibration và manifest; kiểm tra HEAD/status/hash/model file hiện có **trước** lệnh tải hoặc inference.
2. Quyết định có tiếp tục so sánh **đúng một ứng viên MiniLM** hay không sau kiểm tra RAM/commit budget. Không tự tải nhiều biến thể; không chạy hai model cùng lúc hoặc dừng process người dùng để giải phóng RAM. Không đủ môi trường an toàn thì ghi **BLOCKED**, hướng dẫn làm sau, không giả lập kết quả.
3. Nếu được tiếp tục, giữ cùng catalog/data v2 và max cosine, không chỉnh mẫu/nhãn/parser/guard. Dùng tokenizer/prefix/pooling/max length đúng từng model; baseline adapter hiện bắt buộc prefix không rỗng nên **không dùng query: cho MiniLM chỉ để qua validation**. Phải thiết kế harness so sánh riêng/opt-in mà không thay mốc v2; chưa làm thiết kế/implementation này.
4. So sánh top-1/second/margin **trước guard/threshold**, tách parser/guard và tài nguyên. Không bê .85/.01 E5 sang MiniLM rồi kết luận chất lượng; E22 báo unknown. Không tiếp tục vá catalog/threshold nếu chưa có mục tiêu/bằng chứng.
5. UI/chat kiểm tra được bằng tay → `ba-test-frontend/` đúng role/chức năng, đủ điều kiện/bước/kỳ vọng, giữ Chưa test. Không tạo chat runner. Kỹ thuật → được build/EF inspection read-only/inference/scoring/resource/adapter; API/tool/browser chỉ khi thực sự cần và môi trường local/test đã xác minh. Thao tác ghi phải có dữ liệu test và DB an toàn; không tự migration/seed/delete hoặc tác động dữ liệu vận hành.
6. Ghi evidence theo ba tầng **embedding component / API-tool-permission / UI end-to-end**. PASS một tầng không chuyển sang tầng khác; chưa chạy hoặc không có môi trường thì Chưa test/BLOCKED đúng nghĩa.

**Prompt gợi ý:** “Đọc mục TẠM DỪNG trong chatgpt-context/04-WORK-HANDOFF.md, CONTEXT-PACK và calibration report. Kiểm tra HEAD/status, hash profile max-targeted-revision-v2 và artifact E5/MiniLM đã có; bảo toàn working tree. Đánh giá RAM khả dụng rồi đề xuất/tiếp tục phép so sánh có kiểm soát đúng một MiniLM đã tải nếu khả thi, không tải nhiều biến thể hoặc load hai model đồng thời. Giữ catalog/data/parser/guard và max cosine, không áp ngưỡng E5 thành chính sách MiniLM; E22 vẫn unknown. Không tích hợp runtime hoặc vá theo từng câu. Kiểm chứng kỹ thuật bằng công cụ phù hợp, checklist UI để Chưa test; nếu môi trường không an toàn thì ghi BLOCKED. Không migration/seed/commit/push.”

## Latest targeted spike revision — stopped after new check

- EMB-E01..E30 is now development/regression data; original questions/labels/outputs remain unchanged. Worked only in isolated spike, no runtime/provider/orchestrator/permission/tool/frontend integration. Baseline catalog Examples and manifest v1 remain historical; pipeline source is now v2, so current binary is not a reproduction of v1.
- Added catalog profile targeted-revision-v2: clarify 3 broad/template-heavy prototypes, add exactly 3 monetary/aggregate/comparison prototypes; 37 retained samples, same 9 labels. Max cosine/.85/.01/O4/tokenizer/adapter remain fixed. Parser adds week/current-month aliases without calculating dates. Write-candidate needs action+target evidence or asks clarification; loyalty domain veto returns unsupported, preserving raw candidate.
- Frozen manifest: .tools/mock-ai-embedding-spike/profiles/max-targeted-revision-v2.json. E02 now accepted; E03/E05 candidate corrected but margin still blocks; E08 week accepted; E11 period parsed but margin blocks. E24 still WRITE candidate, now asks clarification rather than asserting write. CAL-23 still MY_ORDERS raw but LoyaltyCapabilityNotSupported/tool=null, not ownership denial.
- At .85/.01: CAL correct accepted 8→9/11 ready (24 total), SUP 6→6/8 (18 total), EMB 10→12/17 (30 total). No observed confirmed false accepts; E22 stays policy-unknown, now wrong COMPARISON candidate/clarification. Overall development ready coverage 24→27/36. Old raw outputs and all 72 before/after rows retained in calibration report.
- Build Release passed 0 warning/error. After source freeze, prepared 15 new internal checks (check-targeted-v2.json), recorded labels/behavior before inference and audited lexical/structural overlap; no exact/near Dice≥.8, but shared request structures explicitly documented. Ran once only, no subsequent fixes. Candidate correct 9/14 single-target (1 multi excluded), ready candidate4/6; accepted correct3/6 ready (3/15 total), 0 wrong accepted, 3 valid blocked; non-accept8/9. Not an external independent evaluation.
- New blockers: N03 whole-text revenue correct but comma-preface falsely triggers multi-intent guard; N02 menu→ranking; N05 orders→inventory-unsupported; N14 other-orders→loyalty-unsupported. New loyalty2/2 and write refusal correct, but CAL23 semantic error remains. Do not integrate tool-selecting runtime even with feature flag yet.
- E22 needs owner decision: default unspecified “báo cáo bán hàng” to KPI summary, revenue only, or ask choice? Schemas/services support different outputs but do not settle this language policy. Do not relabel as correct or change runtime without that decision.
- Next proposed task: inspect independent-request clause detection (N03 vs N11) and unsupported-template attraction (N05/N14), not threshold tuning or a new sample campaign. If revising from these new checks, mark them development. Stop this round here. All manual end-to-end cases remain Chưa test; no API/browser/database/business tool/migration/seed, model download, dependency, commit/push or process stop.

## Latest frozen EMB evaluation — component only

- Ran exactly EMB-E01..E30 once with frozen `max-full-baseline-candidate-v1`, max/full-baseline/.85/.01. All 8 manifest source/data hashes plus O4/tokenizer/profile verified unchanged. No catalog/parser/guard/threshold/model/runtime changes; profile v1 remains immutable. Input: `.tools/mock-ai-embedding-spike/evaluation-EMB-E01-E30.json`; results and case-by-case review at the top of `MOCK-AI-EMBEDDING-CALIBRATION.md`.
- Pre-run provenance audit found 12 exact/near-overlap IDs (including E04 exact active sample, and 9 prior smoke questions). Only 18 had no detected overlap; do not describe all 30 as an independent held-out accuracy test. Original questions/labels retained, semantic/decision grading metadata declared before scoring. E22 policy-dependent remains unresolved.
- Candidate correct 21/26 single-target cases; 5 wrong, 4 without a single gold label. Ready-request top-1 14/17. Accepted: 10 correct, 0 confirmed incorrect, 1 unresolved E22; accepted correctness 10/10 assessable, not 11/11. Ready coverage 10/17; 7 valid deferred. Correct non-accept 11/12, E24 mismatch (read ranking/menu classified as write). Raw runner FalseAccept=1 is E22, not an adjudicated business false accept.
- Period arguments correct 7/11 ready inputs with explicit periods; conditional on correct candidate 6/8, parser misses E08 “trong tuần” and E11 “tháng hiện tại”. E02/03/05/28 routing and menu E13/15 margin remain issues. No-overlap-found subset ready coverage only 5/10.
- Do not yet integrate as runtime tool selector, even configuration-gated: wrong revenue/comparison routing, parser misses, low menu coverage, E24 wrong refusal reason and unresolved E22 are blockers. No loyalty question exists in the final 30; CAL-23 is not retested/fixed by E29 other-orders. Backend permissions/ownership/guest gates/business outcomes were not executed. All UI/manual cases remain Chưa test.
- Stop after evaluation: no fixes, tuning or model download; no build (no C# change), chat runner, API/browser/database/tool calls, migration/seed, commit/push or process stop. A future revision based on these errors must mark EMB as development data and obtain a new independent evaluation set. Current chat still uses deterministic Mock recognition.

## Current objective

Perform the static authentication, authorization and branch-isolation review and preserve the evidence boundary for later runtime tests.

## Recently completed

- Surveyed the repository and existing `ba-test-frontend/`, audit, evidence and diagnostic material.
- Established the audit preparation index and source-of-truth rule under `bao-cao/tai-lieu-ky-thuat/system-audit/`.
- Added the audit baseline, scope/environment, evidence index and single findings register.
- Recorded `BASE-001` for the dirty working tree and `DOC-001` for the source/document migration-count discrepancy.
- Created this `chatgpt-context/` pack for future sessions.
- Confirmed current AI registration is `IAiProvider` Ă¢â€ â€™ `MockAiProvider` and updated current context/audit guidance to the Mock-only boundary.
- Reorganized `ba-test-frontend/` into role/page folders, copied legacy case content into role-indexed files, added coverage/runtime/cross-role guides and replaced old root checklists with links.
- Removed an obsolete audit/archive document that could be deleted without touching canonical migration history.

These actions are documentation preparation and static review only. They do not mean the system passed an audit, build or runtime test.

## Not completed

- No runtime API/browser/database session, migration, seed, SignalR test, performance test or backup/restore was run.
- Canonical EF migration files and model snapshot were not rewritten because several migration/designer files contain mixed historical schema steps; database state is not claimed to be cleaned.
- No finding was closed and no feature was certified.
- At the start of the current static review the working tree was clean; the documentation changes from this review are now uncommitted user changes.

## Open issues

- `BASE-001`: current working tree is not a reproducible clean snapshot. See `bao-cao/tai-lieu-ky-thuat/system-audit/06-findings/FINDINGS-REGISTER.md`.
- `DOC-001`: current source migration count differs from an older state document. This is a verified documentation/source-count discrepancy, not a database failure conclusion.
- Existing evidence has not been revalidated against the current working tree.
- `PAY-001`: employee profile fields are a confirmed retained decision; only historical migration metadata remains under separate review.

## Decisions already made

- Keep one active checklist location under `ba-test-frontend/`; legacy copies are preserved only for ID/evidence mapping.
- Use current executable source as the first reference when documents conflict.
- Keep findings in one register and distinguish execution status from conclusion.
- Treat current AI registration as Mock AI unless source/configuration proves otherwise.
- Keep runtime and database claims unverified until evidence is generated in an approved test environment.
- Keep customer lookup/loyalty scope, product availability scope and missing-branch SignalR fallback as explicit policy/runtime questions; do not infer a manager global scope.

## Suggested next steps

The following are suggestions, not new user instructions:

1. Freeze or name a review snapshot for the audit.
2. Perform static review of payment/order state and migration/model alignment after this security review.
3. Run only the approved build/typecheck/codegen checks and record their evidence.
4. Prepare an isolated runtime plan before any API/database/SignalR verification.

## Handoff metadata

Last updated: `2026-10-15`  
Reference branch: `main`  
Reference commit: `4abeb1bf2e646df837745713a495b164eed60488`
Reference tree: clean at review start; `main` was ahead of `origin/main` by one commit. The current review changes are intentionally not committed.

## Source-fix handoff — 2026-10-15

- SEC-006: public customer registration now uses `CustomerRegistrationDto` with only full name, phone and password. Server owns group/rank, loyalty, spending, order count, activity and timestamps; frontend registration sends the allowlisted contract.
- SEC-004: kitchen request status validates status, request/order, active branch and legal transition before mutation. Invalid transitions are returned as 400; no runtime proof yet.
- SEC-003: branch-aware availability reads use the `(ProductId, BranchId)` row and explicit `Available` fallback when missing. Legacy `Products.AvailabilityStatus` remains migration/legacy data, not branch operational state. Kitchen consumes branch-scoped availability events and refetches after reconnect.
- Migration `20261015051548_AddProductBranchAvailability.cs` has explicit EF context/migration metadata. It has not been applied; no database or migration history was changed.
- Inactive-branch open-order recovery remains a business decision. Current source blocks create/accept/payment/status/send-to-kitchen when the branch is inactive, so an existing open order may require reactivation or an approved administrative recovery path.

All related findings remain open as “Đã sửa source, chờ kiểm chứng runtime/migration”; build success is not a PASS. Manual cases remain `Chưa test`.

## Migration/tooling handoff — 2026-10-15

- EF tooling was run against a rebuilt isolated assembly, with `--no-connect`; no database, seed or application was run.
- `migrations list` discovered `20261015051548_AddProductBranchAvailability`.
- `has-pending-model-changes` reported no model changes pending relative to the current snapshot.
- Offline SQL generation succeeded and was inspected without committing the generated output. It contains the new table, composite key, cascade FKs, index, cross-join seed and migration-history insert.
- Direct `DbContext`/`Migration` metadata is sufficient for discovery and script generation. The migration still has no conventional generated `BuildTargetModel` designer; this did not block current tooling, but future migration scaffolding should use the normal project tooling and review the target model.
- Branch-missing availability currently returns `Available`; this is implementation behavior pending user confirmation, not a finalized business policy.
- POS and DigitalMenu do not subscribe to `ProductAvailabilityChanged`; they refresh through existing fetch/reload flows. Kitchen realtime does not cover all frontend consumers.

Next session: decide the missing-row default and inactive-branch order recovery policy, run migration/runtime tests in an approved isolated environment, and test POS/customer availability live-vs-reload behavior.
## Latest handoff update

- Confirmed decision: retain EmployeeType and BasicSalary as employee profile fields in source/API/frontend; no salary-processing module is implied.
- Added employee profile checklist cases for create, edit, save, reload, display, current form validation and branch authorization.
- Moved role legacy case tables into active role files 00-CASES-BY-PAGE.md; legacy files now provide ID lookup. Rebuilt the cross-role and refresh tables in their active files.
- Updated the coverage matrix with route/page/function/case/file/gap columns.
- PAY-001 remains open for historical migration-chain review only. No database was connected or modified.
- No runtime test was executed. The next static review should validate links, residual historical migration references and the role/page map before manual runtime work.

## Authentication and branch-isolation review update

- Added `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/SECURITY-AND-BRANCH-REVIEW.md` with controller/action, SignalR and AI tool inventories, visible controls, evidence limits and policy questions.
- Added findings `SEC-001` through `SEC-005`. They remain open; adding manual cases does not close them.
- Added technical/API cases `AUTH-01..04`, `BRANCH-01..04`, `SIG-01..02` and `AI-01..03` in `ba-test-frontend/ky-thuat-runtime/README.md`. All remain `ChĂ†Â°a test`.
- Static review did not run API, browser, database, migration, seed, SignalR or business transactions. MockAiProvider remains primary; EmployeeType and BasicSalary remain profile fields.
## Reconciliation update

- The current model snapshot contains EmployeeType and BasicSalary only for Employee; no Payroll entity/table/relationship mapping was found in the snapshot.
- Historical migration source/designer references remain and are tracked by PAY-001. No migration, snapshot, database or EF history was changed.
- Route/function coverage is now maintained in ba-test-frontend/00-ROUTE-FUNCTION-MAP.md; the coverage matrix remains the summary index.
- NET-01..NET-05 and TECH-01..TECH-05 remain separate, unchanged, and ChĂ†Â°a test.
## Checklist gap-completion update

- Added page checklists for admin /customers and /support, kitchen /kitchen/tables, inventory/HR/settings subpages, and guest direct /reservation and /profile.
- Added COV-001 because source renders guest/customer shell behavior but does not establish the intended guest policy for reservation/profile data.
- Added case IDs ADM-CUS-01..09, ADM-SUP-01..03, KIT-TBL-01..04, INV-SUB-01..08, HR-SUB-01..08, SET-SUB-01..08 and KQR-01..06.
- Updated route-function map and coverage matrix. Cases remain ChĂ†Â°a test; this is checklist preparation, not runtime verification.
- EmployeeType, BasicSalary, MockAiProvider and migration history remain unchanged.



## Implementation handoff update â€” 2026-10-15

### Completed in source

- Fixed guest profile infinite loading by recognizing guest session modes, ending profile loading immediately, showing a login invitation, and avoiding customer profile API calls for guests.
- Applied source-level customer/loyalty least-privilege response changes, per-branch product availability model/controller callers, and active-branch order processing checks.
- Added source migration 20261015051548_AddProductBranchAvailability.cs; it is not applied and the database was not connected.
- Added manual/runtime follow-up cases KQR-07..08, AUTH-05, BRANCH-05..07, and SIG-03; all remain ChÆ°a test.

### Not completed

- No API, browser, SignalR, database, migration apply, seed or business transaction was run.
- Customer reservation guest policy remains open. Runtime confirmation of customer scope, availability, branch invariant and guest profile behavior remains required.
- Migration history/database state was not cleaned or changed. PAY-001 remains historical migration-chain review only.

### Current verification

- Isolated backend build succeeded with no compile errors in .artifacts/api; normal output was previously blocked by a locked running API output. Customer and admin frontend production builds also completed; none of these builds is runtime evidence.
- Working tree contains pre-existing and current uncommitted changes; no commit/push was performed.




## Static diff review follow-up — 2026-10-15

- Reviewed the actual HEAD and dirty diff; no application code was changed in this review.
- New static report: bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/GUEST-CUSTOMER-AVAILABILITY-ORDER-DIFF-REVIEW.md.
- Customer profile DTO ownership checks are present, but public Customer entity binding leaves CustomerGroup client-controlled on new registration. This is SEC-006 and needs a DTO/allowlist fix before acceptance.
- Availability is branch-scoped in the new model, but missing rows fall back to global Product.AvailabilityStatus; Product/Branch creation does not initialize rows, and no ProductAvailabilityChanged frontend consumer was found. This remains SEC-003 residual risk.
- Kitchen request status suppresses invalid-branch broadcast but mutates OrderRequest before validating the joined order branch. SEC-004 remains open until ordering is corrected.
- Migration source was reviewed only; explicit `DbContext`/`Migration` metadata was added to the uncommitted migration, but no generated `BuildTargetModel` designer or database/schema application was performed.
- Added AUTH-06, BRANCH-08 and SIG-04. All remain Chưa test. Guest reservation policy remains open.
## Owner employee navigation static review — 2026-10-15

- Audited the actual admin/management “Nhân viên” navigation scope: `/employees`, `/attendance`, `/schedule`, `/shifts`.
- Static report: `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/OWNER-EMPLOYEE-NAV-REVIEW.md`.
- Added findings `HR-001..HR-003` to the single findings register. They remain open; no runtime, API, database or migration test was run.
- Added owner manual cases `ADM-EMP-01..05` in `ba-test-frontend/chu-quan/employees.md`; reused existing `ADM-11`, `ADM-17` and shared `HR-SUB-03..05`. All remain `Chưa test`.
- `EmployeeType` and `BasicSalary` remain profile-only fields. No payroll behavior was added or inferred.
- No application code, migration, snapshot, `AGENTS.md` or `AI_RULES.md` was changed.
- Owner navigation HR source-fix update (2026-10-15): HR-001 now blocks self-disable/self-delete from authenticated identity in `EmployeeController` and disables only those UI actions; self-edit remains available.
- HR-002 now checks response status and exposes safe error/loading feedback across `/employees`, `/attendance`, `/schedule`, `/shifts`, with pending guards for writes/refreshes.
- HR-003 now validates active branch on create and actual non-null branch transfer while allowing profile edits on an existing inactive branch.
- HR-001..003 are `Đã sửa source, chờ kiểm chứng runtime`; `ADM-EMP-01..05` remain `Chưa test`. Admin build passed; isolated backend build passed; no API/browser/database/migration/seed operation ran.

## Owner navigation HR static fix review — 2026-10-15

- Reviewed the actual HR application diff separately from pre-existing documentation changes.
- Confirmed JWT source mapping: `AuthController` passes `employee.Id` to `JwtService`, which writes `ClaimTypes.NameIdentifier`; backend self checks compare the employee key, while localStorage is UX-only.
- Residual findings: HR-004 self-disable through entity-shaped `PUT`; HR-005 omitted/null `BranchId` ambiguity; HR-006 handler-level duplicate-submit and mutation-success/refetch-failure distinction.
- Added page-specific owner cases: `ADM-ATT-01..03`, `ADM-SCH-01..04`, `ADM-SHF-01..03`; all remain `Chưa test`. `ADM-EMP-06..07` cover the HR-004/005 paths.
- HR handoff is not closed. No source, API, browser, runtime, database, migration or build operation was performed in this review.
- HR residuals fixed in source: `UpdateEmployee` now uses allow-listed `EmployeeUpdateRequest`, blocks self active→inactive PUT, distinguishes omitted/null/ID `BranchId`, preserves manager branch rules and checks active transfer before mutation. Employee/schedule mutations and attendance/shifts refreshes now have ref-backed in-flight guards; successful mutations report refetch failure separately.
- HR-004 is merged into HR-001, HR-005 into HR-003, HR-006 into HR-002. All three parent findings are `Đã sửa source, chờ kiểm chứng runtime`; no runtime/API/browser/database/migration/seed was run.
- Added `ADM-EMP-06..08`, `ADM-ATT-01..03`, `ADM-SCH-01..04`, `ADM-SHF-01..03`; all remain `Chưa test`. HR handoff is pending runtime verification.

## HR handoff clarification and closeout preparation - 2026-10-15

- Actor and target roles are separate in branch validation. Manager actors remain constrained to the authenticated JWT branch; admin actors may currently assign a branchless normalized staff target, including `manager`, because no existing admin target-role branch-required rule is present. This is preserved policy, not a new source defect.
- BranchId semantics are explicit: omitted preserves, same ID preserves including an inactive current branch, different non-null ID requires an active existing branch, and explicit null removes the branch under current admin behavior. Role and final branch are checked before mutation; historical links are untouched.
- Owner `/attendance` and `/shifts` are read/refresh-only. Attendance check-in/out and shift open/close endpoints exist in staff/POS controllers outside this owner-nav scope and were not runtime-tested as owner-page mutations.
- Owner writes reviewed are employee create/update/status-toggle/delete and schedule create/delete. Their source handlers include response checks, ref-backed in-flight protection and distinct mutation-success/refetch-failure feedback. Manual cases remain `Chưa test`.
- HR-001..003 are `Đã sửa source, chờ kiểm chứng runtime`; HR-004/005/006 are merged into those parent findings. Admin frontend and isolated backend builds passed; no API, browser, database, migration, seed or runtime operation was run. This is not runtime PASS or finding closure.
-
## Mock AI static review update — 2026-10-15

- Reviewed current source at HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`; before this documentation pass, `git status --short` was clean. No application code, migration, snapshot, API, browser, database, seed or build operation was run.
- Added `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/MOCK-AI-REVIEW.md` covering admin/staff/customer/guest UI, endpoints, claims context, Mock provider/recognizer, orchestrator, registry, permission, confirmation and all 14 registered tools.
- Added findings `AI-001..AI-004`: tool failures can surface as `Success=true`; write/price confirmation is not reachable through normal Mock intent flow; both ChatBot UIs allow concurrent sends and retain component history across actor/branch transitions; selected tool paths expose some caught messages without a strict safe-message boundary. All remain `Mở`, runtime-unverified.
- Reconfirmed existing `SEC-005` without duplicating it: `AiToolValidator.ValidateBranchIsolation` has no tool call site, while current tools generally pass context-derived branch values to services.
- Added technical cases `AI-04..AI-11` in `ba-test-frontend/ky-thuat-runtime/README.md` and mapped them in `00-COVERAGE-MATRIX.md`; all remain `Chưa test`.
- The main policy decision still needed is whether Mock remains read-only (then align UI/checklist wording) or supports narrowly scoped writes with explicit confirmation. Do not infer that every write must use confirmation.
-
## Mock AI source-fix and manual-case update — 2026-10-15

- Applied source fixes for AI-001, AI-003 and AI-004; statuses remain `Đã sửa source, chờ kiểm chứng runtime`.
- Kept AI-002 open: no write intent or new permission was added. Registered write/confirmation tools remain distinct from intents reachable through Mock chat, pending a product policy decision.
- Removed the admin `ChatBot` mount from `apps/admin-web/src/app/routes/KitchenRoutes.tsx`. Added server authorization that excludes `kitchen` from `/api/Ai/chat` and `/api/Ai/live-read-only-status`; KitchenHub, KDS, notification and kitchen services were not changed.
- Initial baseline was 100 manual cases (`OWN-AI-01..50`, `CUS-AI-01..35`, `QR-AI-01..15`); it was subsequently merged into the 104-ID primary suite recorded below. All remain `Chưa test`; no automation or API/browser/database execution was performed.
- AI test-suite merge completed: the primary UI suite now has 104 IDs (`OWN-AI-01..54`, `CUS-AI-01..35`, `QR-AI-01..15`). `OWN-AI-51..54` cover manager branch scope, employee `get_my_shift`, cashier read/deny and employee/cashier write refusal. Technical gate `GST-10` is represented by `AI-12`; legacy IDs remain mapped for history and no old evidence was deleted.
- Added links in the role READMEs, route/function map and coverage matrix. Technical/API/confirmation checks continue to use `ba-test-frontend/ky-thuat-runtime/README.md` and existing cross-role cases.
- Backend/frontend verification is still pending in this handoff; no migration, seed, process stop, commit or push was performed.
- Sentence embedding is the selected direction for Mock intent recognition. The design is recorded in `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/MOCK-AI-EMBEDDING-PROPOSAL.md`; it is under evaluation and has not been integrated. The proposal keeps `MockAiProvider`, orchestrator, registry, permission, confirmation and service boundaries, and does not add write intent or restore kitchen AI chat.
- Isolated embedding spike completed in `.tools/mock-ai-embedding-spike/`; report: `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/MOCK-AI-EMBEDDING-SPIKE.md`. `model_O4.onnx` + Hugging Face tokenizer ran in .NET ONNX Runtime and matched the minimal Python reference (token IDs/masks equal; cross-implementation cosine >= 0.999999881). Warm .NET single inference median was 5.422 ms and post-load working set was about 616.8 MB on the test machine. This is spike evidence only, not chat integration or production capacity approval.
- Follow-up adapter/calibration: `MOCK-AI-EMBEDDING-CALIBRATION.md` and `.tools/mock-ai-embedding-spike/SentenceEmbeddingAdapter.cs` now provide an isolated reusable encoder, catalog, scoring and 24-case calibration. Build passed; calibration top-1 label match was 18/24 with experimental `0.85` score/`0.02` margin, not a production policy. Adapter uses local artifacts, masked mean + L2 normalization, cached catalog vectors and bounded default concurrency 1. It is not registered in runtime chat.
- Historical calibration review: the `0.85/0.02` output had 10 accepted cases with 9 matching intent labels; `0.03` had 7 accepted with all 7 matching labels. These were label-only counts, superseded by the contract-based decision review below because several accepted cases lacked supported periods. Adapter queue is bounded (`MaxConcurrency=1`, `QueueCapacity=2` in review commands); native inference remains synchronous.
- No label was changed to improve metrics. CAL-06/10 remain close-intent clarification cases; CAL-15/17/21 require multi-intent/history/parameter policy; CAL-23 must fail closed after separate ownership/capability checks. Runtime integration remains blocked until these policies and a new independent calibration set are reviewed.
- Important limitation retained: .NET/Python consistency is only for the selected ONNX O4 artifact; it does not prove O4 equivalent to the original model or Vietnamese intent quality. Memory observed after adapter load was about 594 MB RSS increase on the local machine; deployment capacity is unknown.

## Latest embedding decision review

- Report: `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/MOCK-AI-EMBEDDING-CALIBRATION.md`, current review section at the top. HEAD remains `38bbd423b963aaf5779d4ae3356f942ff236643b`; pre-existing changes preserved.
- CAL-23 is loyalty mapped incorrectly to `MY_ORDERS/get_my_orders`; no loyalty tool is registered. It is not a correct intent rescued by ownership denial. Candidate is still wrong after this review, but the spike decision now asks again without forwarding a self-only tool. This is recorded as a decision mismatch, not a fully fixed intent.
- Added `IntentDecisionPipeline`: separate semantic candidate/score/margin, capability, parameters, decision reason, pending backend authorization and tool eligible for backend checks. Clause embeddings plus narrow negation/continuation rules and strict period allowlists; no role filtering or identity/period guessing. Catalog now 34 retained samples, of which 32 compete; multi-intent examples keep IDs but no longer compete as business capabilities.
- Corrected historic ready-for-backend counts: old .85/.02 is 6 correct + 4 false accept out of 10, not 9/10; old .03 is 6/7, not 7/7. Original case text/labels/IDs remain unchanged; `calibration-behavior.json` records expected decisions separately.
- Current 24-case development run: 7 correct accept, 0 false accept, 12 correct non-accept, 4 valid deferred and 1 decision mismatch. The supplemental 18 cases were frozen before execution and scored once: 1 correct accept, 0 false accept, 10 correct non-accept, 7 valid deferred. Coverage 7/24 and 1/18; supplemental generalization is inadequate. No tuning used the final EMB manual evaluation set; do not reuse the supplemental set as independent evidence if it is used to tune further.
- Adapter concrete fixes: enforce concurrency 1, dispose model session on constructor validation failure, guard empty batch after dispose, cancellation check after tokenization, private encoder. Local lifecycle checks passed 7 observations for busy/cancel/slot recovery/dispose; slot ownership does not prove native Run entry or immediate cancellation. Release build passed with 0 warning/0 error.
- Keep .85/.02 as a development reference only. No production threshold proposed. Runtime integration/tool-selecting feature flag is not recommended yet: improve semantic confusion and affirmative coverage on fresh data, define parser/capability policy and check hosting resources first. Runtime provider/orchestrator/permission/tools/frontend, EmployeeType/BasicSalary and database remain untouched; no API/browser/database/migration/seed/chat tests or commit/push ran.

## Latest coverage diagnosis — three intents

- Completed the 18-row source/output diagnosis before changing samples: of 8 valid SUP requests, 6 correct top-1 candidates were rejected by margin, 1 comparison was misranked as revenue, 1 ranking accepted. No demonstrated false capability block or missing parser extraction on a correct candidate. Three target intents had correct top-1 5/5 valid SUP requests but accepted 0/5.
- Ran one separate `three-intent-examples` profile: 34 full baseline samples + 6 examples only for MENU_QUERY/REVENUE_QUERY/MY_ORDERS. Baseline remains the default, all competitor/negative intents remain. Threshold .85/.02, scorer, guards, parser and adapter unchanged.
- CAL accepted correct 7/24 → 8/24 (valid coverage 7/11 → 8/11). SUP 1/18 → 3/18 (valid coverage 1/8 → 3/8); zero false accepts observed. However SUP valid top-1 fell 7/8 → 5/8: added revenue example displaced summary SUP-05, added orders example displaced affirmative menu SUP-18. This is a mixed/negative diagnostic result, not a profile to promote.
- Both datasets are now observed development data. Full before/after tables and root-cause counts are in `MOCK-AI-EMBEDDING-CALIBRATION.md`. Build passed 0 warning/error; full baseline reproduced historical outputs. At that handoff the proposed next hypothesis was max versus top-two aggregation; the subsequent comparison below tested it. Runtime integration remains deferred.

## Latest fixed-catalog scoring comparison

- Compared exactly max and top-2 mean on full-baseline (34 retained/32 competing samples), O4, existing parser/guards and unchanged CAL/SUP. Max stays default; `--scoring top2-mean` is optional spike-only. Single-sample intents would use their one score without duplication; none exists in the current competing catalog. JSON retains constituent evidence and all intent ranks.
- At the same .85/.02, CAL correct accepted 7→8 (of 24; valid coverage 7/11→8/11), SUP 1→3 (of 18; valid coverage 1/8→3/8), false accepted 0 in both methods. Effective top-1 label matches CAL 18→19, SUP 13→12; valid-request top-1 remained 10/11 and 7/8. Mean fixes CAL-23 loyalty classification but misclassifies SUP-15 order scope as unsupported; CAL-19/20 write refusals become clarification. Counts and full raw/effective ranking tables are in the report's current top section.
- Mean reduces the effect of a lone high sample, including legitimate subtype matches. Gains are due to scoring gaps at fixed thresholds, not relaxed thresholds, but do not justify replacing max. No further thresholds/algorithms tried; stopped after the two-method comparison. Build passed 0 warning/error; 1,512 stored intent-score arithmetic checks passed. Catalog/model/parser/guard/data hashes unchanged. No runtime integration or API/browser/database/tool execution occurred.

## Latest bounded threshold round — closed, candidate frozen

- Max/full-baseline, O4, parser/guards and CAL/SUP frozen. Verified 17/19 effective candidates correct among 11 CAL + 8 SUP ready requests (raw whole-sentence only 16/19); 9 correct candidates were blocked by baseline margin. The 19 requests are ready for backend authorization checks, not proven authorized/business-success cases.
- Predeclared T0 .85/.02, T1 .85/.015, T2 .85/.01, T3 .85/.005 and ran exactly those four on both development sets. Correct accepted: 8,12,14,17 out of 19 valid /42 total; false accepts observed 0 for all; correct non-accepts 22 and decision mismatch CAL-23 1 remain unchanged. No additional algorithms, templates, data or C# changes/build in this round.
- Selected **max-full-baseline-candidate-v1**, .85/.01, manifest `.tools/mock-ai-embedding-spike/profiles/max-full-baseline-candidate-v1.json`, status “ứng viên thử nghiệm, chưa triển khai”. T2 valid coverage 14/19; T3 was not preferred because its boundary was only ~.000061 above unresolved multi-intent SUP-12. The manifest pins model/tokenizer/catalog/scorer/parser/guard/adapter/runner/data hashes; it is not auto-loaded. Default remains max/full-baseline/.85/.02; runtime chat unchanged.
- T2 still defers valid CAL-02/CAL-11/SUP-18 by margin and CAL-10/SUP-02 by wrong-intent routing/parser; CAL-23 still loyalty→MY_ORDERS with an incorrect clarification reason. Keep these errors visible. The final EMB set was not opened/run; the report's top section supplies component evaluation logging instructions and distinguishes later end-to-end chat checks. Do not tune further in this closed round. Future changes based on evaluation errors require a new profile and reclassifying that evaluation data as development.
