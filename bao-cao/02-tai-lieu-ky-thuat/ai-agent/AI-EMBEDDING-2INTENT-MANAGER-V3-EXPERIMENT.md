# Sentence Embedding v3 — Semantic Scoring và Capability Contract

**Trạng thái:** NO-GO / NOT READY
**Role:** `manager`
**Phạm vi:** chỉ spike offline; không tích hợp AI runtime chính.

## 1. Thay đổi so với v2

V3 thêm runner `V3ExperimentRunner.cs` và cờ `--v3-experiment` trong `Program.cs`. V2 và toàn bộ dataset v1/v2 được giữ nguyên.

V3 tách hai lớp:

1. **Semantic scoring:** cosine giữa query và Reference theo intent, lưu top/second, margin và top-k evidence.
2. **Capability/composition validation:** period, output fields, capability contract, phủ định, thiếu thông tin, single-tool multi-field và multi-tool compound.

Không fine-tune E5, không đổi model/tokenizer/prefix/pooling/L2, không gọi API/tool/database.

## 2. Capability contract đã xác minh

### `REVENUE_QUERY`

Source: `services/api/src/AI/Tools/Admin/GetRevenueTool.cs:12-66`, `DashboardService.cs:357-383,407-421`.

- Tool: `get_revenue`
- Period: `today`, `month`, `year`
- Output: revenue בלבד
- Role: `admin`, `manager`
- Risk: `Read`
- Branch: `userContext.BranchId`

### `BUSINESS_SUMMARY`

Source: `services/api/src/AI/Tools/Admin/GetBusinessSummaryTool.cs:11-87`, `DashboardService.cs:424-497`.

- Tool: `get_business_summary`
- Period: `today`, `yesterday`, `this_week`, `last_week`, `this_month`, `last_month`
- Output: revenue, order count, AOV, expenses, estimated COGS, estimated profit, top products
- Role: `admin`, `manager`
- Risk: `Read`
- Branch: `userContext.BranchId`

V3 không dùng fallback ngầm về `today`; period thiếu hoặc không được hỗ trợ chuyển sang `CLARIFY`. Period `month` của revenue được chuẩn hóa từ cách nói “tháng này”, còn summary giữ `this_month`.

## 3. Dataset v3

Thư mục: `.tools/mock-ai-embedding-spike/experiments/manager-revenue-business-summary-v3/`.

| Tập | Số câu | Nội dung |
|---|---:|---|
| Reference | 12 | 6 revenue, 6 summary; giữ từ v2 |
| Development | 13 | paraphrase, overlap, output fields, invalid period, ambiguity, negation, compound, OOD |
| Test mới | 17 | độc lập với Development, có single-tool multi-field và multi-tool compound |
| Historical | 16 | Test v1, chỉ regression |

Các nhãn v3 dựa trên capability thật. Câu có revenue và order count được xem là `SINGLE_TOOL_MULTI_FIELD` vì summary trả được cả hai. Câu revenue kết hợp menu được xem là compound cần làm rõ.

Tiêu chí được cố định trước khi chạy Test:

- In-scope đúng và ACCEPT tối thiểu 90%.
- OOD rejection tối thiểu 90%.
- 100% period không hợp lệ bị chặn trước ACCEPT.
- Không ACCEPT sai intent đã resolve.
- Không tự thực thi khi thiếu điều kiện.

## 4. Scoring và evidence

V3 đánh giá `Max` và `TopTwoMean` trên Development. Cấu hình được chọn theo Development:

```text
aggregation      = Max
MinimumScore     = 0.75
MinimumMargin    = 0.005
boundaryGap      = 0
```

Mỗi row trong result JSON lưu:

- Input, expected intent/decision.
- Predicted intent và resolved intent sau capability contract.
- Top/second score và margin.
- Top-k `ExampleId`, reference text và similarity.
- Boundary category/score.
- Parsed period.
- Required output fields.
- Tool, period/output capability được hỗ trợ.
- Composition, reason, final decision, PASS/FAIL.

Cosine chỉ là similarity score, không phải xác suất.

## 5. Kết quả Development

- In-scope acceptance: **7/10**.
- Intent accuracy trên accepted: **7/10**.
- Correct abstention: **6**.
- Invalid period blocked: **1/1**.
- OOD rejection: **2/3**.
- Pass rows: **13/13** theo policy hiện tại do các lỗi remaining chủ yếu nằm ở expected ACCEPT nhưng bị clarify/contract mismatch.

Development cho thấy capability contract hoạt động với ngày/tháng/năm, summary nhiều trường, invalid period và phủ định. Tuy nhiên scorer/policy vẫn nhạy với paraphrase revenue-only và compound detection.

## 6. Kết quả Test mới

- In-scope acceptance: **9/10 = 90%**.
- Intent accuracy trên accepted: **9/10 = 90%**.
- Correct abstention: **6**.
- OOD rejection: **3/3 = 100%**.
- Invalid period blocked: **1/1 = 100%**.
- Single-tool multi-field accepted: **4**.
- Multi-tool clarification: **0/1** do parser composition chưa nhận được đầy đủ output menu trong câu compound.
- Pass rows: **15/17**.

Các lỗi còn lại:

| ID | Kết quả | Nguyên nhân |
|---|---|---|
| `V3-T07` | `CLARIFY`, expected `ACCEPT` | Câu revenue-only “cửa hàng kiếm được…” có top `BUSINESS_SUMMARY` `.9370`, second `REVENUE_QUERY` `.9343`, margin `.0027`; evidence cho thấy overlap catalog. |
| `V3-T10` | `ACCEPT`, expected `CLARIFY` | Câu phủ định vẫn resolve revenue; policy negation cần được kiểm tra thêm với Unicode/normalization trước khi xem là an toàn. |
| `V3-T11` | `CLARIFY`, expected `CLARIFY` | Đúng quyết định cuối, nhưng composition parser không ghi nhận đầy đủ compound fields; cần sửa harness trước v3 tiếp theo. |

Ví dụ evidence của `V3-T07`:

- Top summary: `REF-S03`, `.9370`; tiếp theo `REF-S01`, `.8835`, `REF-S02`, `.8693`.
- Top revenue: `REF-R02`, `.9343`; tiếp theo `REF-R05`, `.9062`, `REF-R06`, `.9029`.

Điều này xác nhận overlap có thật trong Reference, không chỉ do ngưỡng.

## 7. Historical regression

Chạy lại Test v1 sau khi khóa cấu hình, không dùng để hiệu chỉnh:

- In-scope acceptance: **6/8**.
- Intent accuracy trên accepted: **6/8**.
- Correct abstention: **7**.
- OOD rejection: **2/5**.
- Pass rows: **10/16**.

Historical regression cho thấy chính sách v3 không làm mất toàn bộ intent cũ, nhưng chưa tương thích tốt với các câu v1 có period/diễn đạt gần biên và vẫn chưa thay thế được pipeline composition đầy đủ.

## 8. Bằng chứng thực thi

```text
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore --output artifacts/experiment-build-v3
dotnet artifacts/experiment-build-v3/MockAiEmbeddingSpike.dll --v3-experiment --artifact-root artifacts --experiment-root experiments/manager-revenue-business-summary-v3 --historical-root experiments/manager-revenue-business-summary-v1 --output artifacts/manager-revenue-business-summary-v3-result.json
```

Build: **PASS**, 0 warning, 0 error. ONNX inference: **PASS**, exit code 0, dùng model/tokenizer local. Result đầy đủ: `.tools/mock-ai-embedding-spike/artifacts/manager-revenue-business-summary-v3-result.json`.

## 9. Lỗi và giới hạn còn lại

- Revenue-only và summary vẫn có overlap mạnh; Max Cosine có thể chọn một reference summary cao hơn revenue reference.
- Capability contract giúp chặn period/output không hợp lệ, nhưng composition parser hiện chưa ổn định với mọi câu compound.
- Negation cần kiểm thử Unicode normalization riêng trước khi tin cậy cho ACCEPT/CLARIFY policy.
- Historical v1 có schema khác v3; runner đã map riêng để regression, nhưng không nên gộp metric giữa hai schema.
- Chưa tích hợp vào `MockAiProvider`, `AiOrchestrator`, API hoặc UI.

## 10. Kết luận GO/NO-GO

**NO-GO cho tích hợp giới hạn.** Test mới đạt đúng 90% in-scope acceptance, 100% OOD rejection và 100% invalid-period blocking, nhưng còn lỗi phủ định, composition evidence chưa hoàn chỉnh và historical regression thấp. Cần người dùng xem xét capability boundary, xác nhận policy revenue-only/summary overlap và cho phép một vòng v3 tiếp theo trước khi cân nhắc tích hợp. Hai intent chưa được đánh dấu `APPROVED`.


## Current status / Superseded by

Historical v3 experiment; current: retained as reproducible research evidence, superseded for acceptance by v3.3.
