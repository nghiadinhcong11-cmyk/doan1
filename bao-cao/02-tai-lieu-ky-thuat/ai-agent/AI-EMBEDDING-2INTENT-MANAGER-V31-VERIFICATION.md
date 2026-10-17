# AI Embedding 2 Intent Manager v3.1 — Policy Correctness & Regression

Ngày audit: 2026-10-17
Phạm vi: `.tools/mock-ai-embedding-spike/`. Không tích hợp API, không gọi database, không sửa backend/frontend.

## 1. Kết luận

**NO-GO cho tích hợp.** Policy v3.1 đã sửa được phủ định, nhận diện compound request và một lỗi false capability do từ khóa quá rộng. Bộ holdout mới đạt 16/16 quyết định đúng, gồm 7/7 in-scope ACCEPT và 3/3 OOD bị từ chối. Tuy nhiên historical regression còn 7/20 dòng không PASS, trong đó có một false acceptance; semantic candidate vẫn có lỗi trên holdout (6/7) dù final policy resolve đúng nhờ capability policy. Vì vậy kết quả chưa đủ để xem là regression an toàn hoặc APPROVED.

## 2. Nguyên nhân lỗi được xác định

- Metric cũ trộn số lượng ACCEPT với accuracy và không nêu mẫu số. v3.1 tách `semanticCandidateCorrect`, `finalIntentCorrect`, `finalDecisionCorrect`, `inScopeAcceptanceRate`, `safeAbstention`, `falseAcceptance`, `oodRejectionRate`, `capabilityCompliantAccepts` và `toolRoutingEligible`.
- Phủ định `Đừng` không được xử lý ổn định sau chuẩn hóa Unicode. v3.1 ánh xạ `đ/Đ`, loại dấu rồi kiểm tra token phủ định ở đầu hoặc giữa clause; mọi phủ định không chắc chắn đều CLARIFY.
- Regex `kiem` trước đây bắt nhầm “kiểm tra tồn kho” thành revenue. v3.1 thu hẹp thành `kiem duoc`, tránh nhận nhầm capability.
- `AmbiguousSubject` coi “tóm tắt tình hình kinh doanh” là mơ hồ dù summary capability đáp ứng được; policy đã loại các marker summary rõ ràng khỏi nhánh ambiguous.
- Composition được đánh giá độc lập với semantic score: revenue + menu là `MULTI_TOOL_COMPOUND`; revenue + order count là `SINGLE_TOOL_MULTI_FIELD`. Không quyết định multi-tool chỉ vì có chữ “và”.

## 3. Thay đổi

- `.tools/mock-ai-embedding-spike/V3ExperimentRunner.cs`
  - thêm `--holdout-root` và xuất kết quả holdout.
  - chuẩn hóa Unicode và phủ định an toàn hơn.
  - sửa output parsing và ambiguous-subject policy.
  - metric có mẫu số và tỷ lệ riêng.
- `.tools/mock-ai-embedding-spike/PolicyUnitChecks.cs`
  - 8 policy checks cho ASCII/Unicode negation, phủ định giữa clause, câu điều kiện, single-tool multi-field, multi-tool compound và ambiguous subject.
- `.tools/mock-ai-embedding-spike/Program.cs`
  - thêm `--v31-policy-checks` và `--v31-verification`.
- `.tools/mock-ai-embedding-spike/experiments/manager-revenue-business-summary-v31/holdout.json`
  - holdout mới, tách khỏi Reference/Development/Test historical.
- `.tools/mock-ai-embedding-spike/artifacts/manager-revenue-business-summary-v31-result.json`
  - bằng chứng inference ONNX và từng row.

Capability contract vẫn chỉ là metadata mô phỏng trong spike: role `manager`, branch `userContext.BranchId`, read-only. Đây không phải kiểm chứng `AiPermissionService` hoặc branch authorization runtime.

## 4. Verification commands

```text
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore --output artifacts/experiment-build-v31
dotnet artifacts/experiment-build-v31/MockAiEmbeddingSpike.dll --v31-policy-checks
dotnet artifacts/experiment-build-v31/MockAiEmbeddingSpike.dll --v31-verification --artifact-root artifacts --experiment-root experiments/manager-revenue-business-summary-v3 --historical-root experiments/manager-revenue-business-summary-v2 --holdout-root experiments/manager-revenue-business-summary-v31 --output artifacts/manager-revenue-business-summary-v31-result.json
```

Kết quả build: **PASS** (0 warning, 0 error).
Policy unit checks: **PASS 8/8**.
ONNX inference: **PASS**, output JSON được tạo.

## 5. Metric trước/sau

| Tập | v3 trước | v3.1 sau |
|---|---:|---:|
| Development final decision correct | 13/13 | 13/13 |
| Historical regression final decision correct | 10/16 (báo cáo v3) | 13/20* |
| Historical false acceptance | chưa tách rõ | 1 |
| Fresh holdout | chưa có | 16/16 |
| Holdout in-scope ACCEPT | chưa có | 7/7 (100%) |
| Holdout OOD rejection | chưa có | 3/3 (100%) |
| Holdout invalid period blocked | chưa có | 1/1 |
| Holdout multi-tool clarification | chưa có | 1/1 |

* Historical v3.1 chạy toàn bộ `experiments/manager-revenue-business-summary-v2/test.json` gồm 20 dòng; đây là regression đã quan sát, không phải đánh giá độc lập.

Metrics chi tiết từ JSON:
- Development: in-scope 7/7 ACCEPT; OOD 2/2; pass 13/13.
- v3 Test: in-scope 10/10 ACCEPT; OOD 3/3; pass 16/17; semantic candidate đúng 9/10.
- Historical: in-scope 5/8 ACCEPT; OOD 8/8; pass 13/20; false acceptance 1.
- Fresh holdout: in-scope 7/7 ACCEPT; OOD 3/3; pass 16/16; semantic candidate đúng 6/7; final intent đúng 7/7; false acceptance 0.

`finalDecisionCorrect`/ `pass` yêu cầu đúng cả expected decision; một câu CLARIFY thay cho ACCEPT không được tính PASS chỉ vì tool không chạy.

## 6. Các lỗi còn lại

- Historical false acceptance `NEW-A03` vẫn ACCEPT trong khi nhãn lịch sử yêu cầu CLARIFY.
- Historical có các in-scope câu bị CLARIFY do margin/capability policy; đây là regression chưa giải thích hết.
- Holdout `V31-H02` được resolve đúng thành revenue nhờ narrower capability dù semantic top-1 là BUSINESS_SUMMARY. Điều này chứng minh policy composition/capability có tác dụng, nhưng semantic candidate chưa hoàn toàn ổn định.
- `semanticCandidateCorrect` và `finalIntentCorrect` khác nhau; không được gọi kết quả cuối là bằng chứng embedding tự phân biệt hoàn hảo.
- Không có kiểm chứng authorization/branch runtime, vì spike không gọi tool hoặc database.

## 7. Đánh giá điều kiện GO

| Điều kiện | Kết quả |
|---|---|
| Không ACCEPT sai intent đã resolve trên holdout | PASS |
| Không ACCEPT phủ định/capability không phù hợp | PASS trên holdout |
| Composition nhiều trường/nhiều capability | PASS các ca holdout đã định trước |
| In-scope >=90% trên holdout | PASS, 100% |
| OOD rejection >=90% trên holdout | PASS, 100% |
| Historical regression không còn lỗi nghiêm trọng | **FAIL**, còn 1 false acceptance và 7 dòng không PASS |
| Metric/evidence nhất quán | PASS sau khi tách mẫu số |

Do historical regression còn lỗi và semantic candidate có một lỗi, quyết định vẫn **NO-GO**. Chưa APPROVED intent và chưa đề xuất tích hợp AI Chat.




## Current status / Superseded by

Historical v3.1 verification; current: retained, superseded by v3.3 and final acceptance.
