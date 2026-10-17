# AI Embedding v3.3 — Capability Boundary & Regression Verification

Ngày kiểm thử: 2026-10-17
Phạm vi: .tools/mock-ai-embedding-spike/. Không sửa backend/frontend, MockAiProvider, AiOrchestrator, Tool Registry hoặc database.

## 1. Kết luận

**GO có điều kiện cho bước xem xét thử nghiệm giới hạn; chưa APPROVED và chưa tích hợp AI Chat.**

Capability contract đã được áp dụng:

- REVENUE_QUERY → get_revenue: chỉ today|month|year.
- BUSINESS_SUMMARY → get_business_summary: today|yesterday|this_week|last_week|this_month|last_month.
- Câu ngoài hai capability trả UNSUPPORTED.
- Kỳ không được revenue tool hỗ trợ trả CLARIFY với lý do UNSUPPORTED_PERIOD.
- Không chuyển revenue-only sang summary chỉ vì runner-up.
- Negation vẫn CLARIFY an toàn.
- Fresh holdout mới đạt 16/16.
- Adjudicated historical đạt 20/20.
- Raw historical giữ nguyên nhãn gốc và còn 4 dòng không PASS; đây là kết quả được báo riêng, không bị che bởi adjudication.

GO ở đây chỉ có nghĩa đủ điều kiện chuyển sang bước xem xét nghiên cứu giới hạn. Chưa có bằng chứng authorization/branch runtime và chưa được phép tích hợp.

## 2. Thay đổi so với v3.1

### Policy

File .tools/mock-ai-embedding-spike/V3ExperimentRunner.cs:

- Thêm kiểm tra IsClearlyOutOfScope trước missing period và mixed-capability.
- Tách OOD khỏi in-domain unsupported period.
- Các mẫu capability ngoài phạm vi như menu, giờ phục vụ, loyalty, ca làm, tồn kho, cập nhật trạng thái đơn hàng, ranking và đơn chờ trả UNSUPPORTED.
- Giữ nguyên negation safety, single-tool multi-field và multi-tool composition.
- Không fallback period và không chọn runner-up để vượt capability validation.
- Thêm đánh giá adjudicated historical song song raw historical.

### Dataset và mapping

- Giữ nguyên v1/v2/v3.
- Thêm experiments/manager-revenue-business-summary-v33/historical-adjudication.json.
- Mỗi mapping có intent, expected decision, capability classification, period, required outputs, composition và lý do nghiệp vụ.
- Thêm holdout độc lập mới tại experiments/manager-revenue-business-summary-v33/holdout.json.
- Holdout không được dùng trong calibration.

### Unit checks

PolicyUnitChecks.cs có 11 checks:

- 5 negation/Unicode checks.
- 3 composition checks.
- 3 OOD boundary checks.

Kết quả: **PASS 11/11**.

## 3. Kết quả metric

Artifact: .tools/mock-ai-embedding-spike/artifacts/manager-revenue-business-summary-v33-result.json.

| Tập | N | In-scope ACCEPT | Semantic candidate | Final intent | OOD rejection | False acceptance | Final decision |
|---|---:|---:|---:|---:|---:|---:|---:|
| Development cũ | 13 | 7/7 (100%) | 7/7 | 7/7 | 2/2 (100%) | 0 | 12/13 |
| Raw v3 Test | 17 | 10/10 (100%) | 9/10 | 10/10 | 3/3 (100%) | 0 | 16/17 |
| Raw historical v2 | 20 | 5/8 (62.5%) | 7/8 | 5/8 | 8/8 (100%) | 1 | 16/20 |
| Adjudicated historical | 20 | 6/6 (100%) | 5/6 | 6/6 | 8/8 (100%) | 0 | 20/20 |
| Fresh v3.3 holdout | 16 | 6/6 (100%) | 6/6 | 6/6 | 5/5 (100%) | 0 | 16/16 |

Mẫu số:

- In-scope ACCEPT = số dòng expected ACCEPT được ACCEPT.
- Semantic candidate = top predicted intent đúng trên các dòng expected ACCEPT.
- Final intent = dòng expected ACCEPT vừa ACCEPT vừa resolve đúng intent.
- OOD rejection = số dòng expected intent OUT_OF_SCOPE không ACCEPT / tổng OUT_OF_SCOPE.
- Final decision = expected decision và resolved intent đúng theo nhãn của tập.
- Capability-compliant accepts và tool-routing eligible được tính riêng trong JSON.

Raw historical vẫn giữ nguyên để truy vết. Adjudicated historical không phải test độc lập mới.

## 4. Adjudication historical

Các thay đổi nhãn được lưu riêng, không sửa manager-revenue-business-summary-v2/test.json.

| ID | Nhãn gốc | Nhãn adjudicated | Lý do |
|---|---|---|---|
| NEW-R03 | REVENUE_QUERY / ACCEPT | REVENUE_QUERY / CLARIFY | yesterday không nằm trong allowlist get_revenue |
| NEW-R04 | REVENUE_QUERY / ACCEPT | REVENUE_QUERY / CLARIFY | this_week không nằm trong allowlist get_revenue |
| NEW-S03 | BUSINESS_SUMMARY / ACCEPT | BUSINESS_SUMMARY / CLARIFY | subject “tình hình vận hành” không chỉ rõ output |
| NEW-X03 | OUT_OF_SCOPE / UNSUPPORTED | giữ nguyên | cập nhật trạng thái đơn hàng ngoài read-only contracts |
| NEW-X05 | OUT_OF_SCOPE / UNSUPPORTED | giữ nguyên | giờ phục vụ ngoài cả hai tool |
| NEW-X06 | OUT_OF_SCOPE / UNSUPPORTED | giữ nguyên | loyalty points ngoài cả hai tool |
| NEW-A03 | MULTI_INTENT / CLARIFY | BUSINESS_SUMMARY / ACCEPT | summary đáp ứng revenue và overview bằng một tool |

Adjudicated historical đạt 20/20. Kết quả này phản ánh contract đã chốt, không phải kết quả độc lập mới.

## 5. Các trường hợp FAIL còn lại

### Development cũ: V3-D13

Câu compound có menu và capability trong phạm vi. Policy v3.3 nhận diện phần ngoài phạm vi trước nên trả UNSUPPORTED; nhãn development v3 cũ kỳ vọng CLARIFY. Đây là sai lệch nhãn cũ, không phải ACCEPT sai và không làm phát sinh tool execution.

### Raw v3 Test: V3-T11

Cùng loại compound revenue + menu. Policy mới trả UNSUPPORTED theo boundary ngoài phạm vi, còn nhãn v3 cũ kỳ vọng CLARIFY. Đây là thay đổi chính sách có chủ đích; raw test được giữ nguyên.

### Raw historical

Bốn dòng không PASS còn lại do nhãn gốc revenue-period/ambiguous/multi-intent đã được adjudicate. Raw historical vẫn có một false acceptance NEW-A03 theo nhãn cũ; adjudicated historical không còn false acceptance.

## 6. Kiểm tra capability và an toàn

- Revenue yesterday/week không được chuyển sang summary.
- Summary hỗ trợ week và trả nhiều trường trong một tool.
- Invalid period không fallback về today trong policy spike.
- Negation không ACCEPT.
- OOD được chặn trước missing-period clarification.
- Không có tool nghiệp vụ nào được gọi trong lần kiểm thử này; toolRoutingEligible chỉ là kiểm tra contract metadata, không phải authorization runtime.
- Role manager và branch context chỉ được mô phỏng trong capability contract.

## 7. Lệnh và bằng chứng

dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore --output artifacts/experiment-build-v33
dotnet artifacts/experiment-build-v33/MockAiEmbeddingSpike.dll --v31-policy-checks
dotnet artifacts/experiment-build-v33/MockAiEmbeddingSpike.dll --v33-verification --artifact-root artifacts --experiment-root experiments/manager-revenue-business-summary-v3 --historical-root experiments/manager-revenue-business-summary-v2 --adjudication-root experiments/manager-revenue-business-summary-v33 --holdout-root experiments/manager-revenue-business-summary-v33 --output artifacts/manager-revenue-business-summary-v33-result.json

- Build: **PASS**, 0 warning, 0 error.
- Policy checks: **PASS 11/11**.
- ONNX inference và JSON result: **PASS**.
- Không tải model, không cài package, không kết nối database.

## 8. Điều kiện còn thiếu

1. Kiểm chứng permission và branch isolation ở runtime trước khi tích hợp.
2. Người dùng nghiệm thu boundary và mapping adjudication.
3. Giữ baseline dataset/adjudication version cố định nếu chuyển sang nghiên cứu tiếp.
4. Chưa đánh dấu APPROVED cho intent.

## 9. GO/NO-GO

**GO có điều kiện cho xem xét nghiên cứu giới hạn**, dựa trên:

- Fresh holdout in-scope ACCEPT 100%.
- Fresh holdout OOD rejection 100%.
- Không false acceptance trên holdout.
- Adjudicated historical 20/20.
- Không fallback period và không ACCEPT capability ngoài phạm vi.

**Chưa GO cho tích hợp runtime** vì spike chưa kiểm chứng authorization/branch execution và chưa có phê duyệt nghiệm thu của người dùng.



## Current status / Superseded by

Frozen v3.3 verification evidence; current: RESEARCH_BASELINE_LOCKED remains valid, operational acceptance is summarized in AI-EMBEDDING-MANAGER-ACCEPTANCE-FINAL.md.
