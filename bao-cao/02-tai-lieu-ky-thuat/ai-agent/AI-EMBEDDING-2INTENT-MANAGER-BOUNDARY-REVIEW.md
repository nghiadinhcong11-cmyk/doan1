# Sentence Embedding v3 — Rà soát ranh giới ngữ nghĩa hai intent

**Phạm vi:** audit và thiết kế, không chạy tối ưu mới
**Role:** `manager`
**Intent đang xem xét:** `REVENUE_QUERY` → `get_revenue`; `BUSINESS_SUMMARY` → `get_business_summary`
**Kết luận:** chưa đủ cơ sở GO hoặc APPROVED cho v3.

## 1. Nghiệp vụ thực tế của hai tool

### `get_revenue`

Source: `services/api/src/AI/Tools/Admin/GetRevenueTool.cs:12-66`.

- Input bắt buộc: `period`.
- Allowlist runtime: `today`, `month`, `year` (`:42-48`).
- Gọi `IDashboardService.GetRevenueByPeriodAsync(period, userContext.BranchId)` (`:56`).
- Output: một chuỗi doanh thu theo kỳ, không trả số đơn, AOV, chi phí hay sản phẩm.
- Role: `admin`, `manager`; risk `Read` (`:15-17`).
- Branch: truyền `userContext.BranchId` xuống service.

`DashboardService.GetRevenueByPeriodAsync` quy đổi thời gian Việt Nam và lấy `PaidAmount` của order hoàn thành, có thanh toán (`DashboardService.cs:357-383`, `407-421`). Với tool này, câu hỏi số tiền/doanh thu thuần theo ngày, tháng hoặc năm là single intent rõ ràng.

### `get_business_summary`

Source: `services/api/src/AI/Tools/Admin/GetBusinessSummaryTool.cs:11-87`.

- Input schema bắt buộc: `period` (`:24-33`).
- Các nhánh được xử lý: `yesterday`, `this_week`, `last_week`, `this_month`, `last_month`, `today` (`:42-69`). Giá trị khác rơi vào `default` và bị xử lý như `today`; đây là điểm cần validation riêng trước khi tích hợp.
- Gọi `IDashboardService.GetBusinessSummaryAsync(startUtc, endUtc, userContext.BranchId)` (`:74-81`).
- Output JSON gồm `Revenue`, `OrderCount`, `AverageOrderValue`, `TotalExpenses`, `EstimatedCOGS`, `EstimatedNetProfit`, `TopProducts` và thời gian kỳ (`:83-87`).
- Role: `admin`, `manager`; risk `Read` (`:14-16`).
- Branch: truyền cùng `userContext.BranchId`.

`DashboardService.GetBusinessSummaryAsync` tính doanh thu từ cùng tập order hoàn thành/đã thanh toán, sau đó tính số đơn, AOV, expenses, COGS ước tính, lợi nhuận ước tính và top products (`DashboardService.cs:424-497`). Vì vậy **Business Summary có chứa chỉ số doanh thu**. Đây là overlap nghiệp vụ thực tế, không phải lỗi riêng của embedding.

### Khi nào một tool đủ?

| Yêu cầu | Tool phù hợp | Lý do |
|---|---|---|
| “Doanh thu hôm nay/tháng/năm là bao nhiêu?” | `get_revenue` | Trả đúng một giá trị và kỳ được hỗ trợ. |
| “Tổng quan/KPI, số đơn, AOV, chi phí, lợi nhuận, top món” | `get_business_summary` | Chỉ summary có các trường này. |
| “Tổng hợp doanh thu và số đơn” | `get_business_summary` | Một tool đã trả cả `Revenue` và `OrderCount`; không bắt buộc gọi hai tool. |
| “Doanh thu tuần này” | Không có mapping trực tiếp tới `get_revenue` | `get_revenue` không hỗ trợ week; summary có week và chứa revenue. Cần chính sách mapping hoặc làm rõ, không tự gắn là revenue-only. |
| Một câu yêu cầu số tiền thuần **và** một báo cáo KPI độc lập | Có thể cần hai tool | Đây mới là compound request thực sự nếu summary không đáp ứng yêu cầu định dạng/độ chính xác của phần đầu. |

Cả hai tool đều lấy cùng branch context và đều cho `manager` đọc dữ liệu. Khác biệt chính là **độ rộng output và miền thời gian**, không phải chỉ từ khóa “doanh thu”.

## 2. Rà soát nhãn và ranh giới intent

Không thay đổi bất kỳ nhãn hoặc file dataset nào. Kết quả audit:

### Single intent rõ ràng

- Revenue: các câu có mục tiêu duy nhất là một số doanh thu và kỳ `today/month/year`, ví dụ REF-R01..R06 và phần lớn `NEW-R02`, `NEW-R03`.
- Summary: các câu yêu cầu KPI/tổng quan/số đơn/AOV/chi phí/top products, ví dụ REF-S01..S06 và NEW-S01..S04.

### Có thể được đáp ứng bởi cả hai tool

- Câu có “doanh thu” nhưng đồng thời yêu cầu “tổng quan”, “chỉ số”, “số đơn” hoặc KPI: summary đã có revenue nên không nhất thiết là hai intent.
- `NEW-R01` “Cửa hàng kiếm được bao nhiêu trong ngày hôm nay?” về nghiệp vụ là revenue-only, dù embedding v2 nghiêng sang summary.
- `NEW-R04` “Trong tuần này doanh số là bao nhiêu tiền?” là revenue-only về ý nghĩa, nhưng `get_revenue` không nhận week. Nhãn `REVENUE_QUERY/ACCEPT` hiện tại không khớp hoàn toàn khả năng tool; nên ở dataset phiên bản sau là `CLARIFY_UNSUPPORTED_PERIOD` hoặc mapping có chủ đích sang summary.

### Multi-intent thực sự và compound request

- `TEST-A03` và `DEV2-A03` có hai mục tiêu diễn đạt trong cùng câu, nhưng cả hai đều có thể được đáp ứng bởi một `get_business_summary` nếu người dùng chấp nhận báo cáo KPI có revenue. Vì vậy nhãn `MULTI_INTENT` hiện tại là **chưa chắc chắn về mặt nghiệp vụ**.
- Cần phân biệt `MULTI_TOOL_COMPOUND` (bắt buộc hai capability) với `SINGLE_TOOL_MULTI_FIELD` (nhiều trường nhưng một tool đủ). Không nên xem mọi liên từ “và” là hai intent.

### Thiếu tham số, mơ hồ, ngoài phạm vi

- Thiếu kỳ: “Doanh thu thế nào?”, “Cho biết tình hình.” → cần làm rõ kỳ và nội dung.
- Mơ hồ: “Tình hình hôm nay ra sao?” → chưa chỉ rõ chỉ số hay doanh thu thuần.
- Phủ định: “Không cần xem doanh thu…” → không phải yêu cầu thực thi; cần `CLARIFY` hoặc xác nhận ý định mới.
- Menu, ca làm, tồn kho, giờ mở cửa, loyalty, ranking món, trạng thái/đếm đơn → ngoài hai capability hiện tại.

## 3. Phân tích các lỗi được yêu cầu

Artifact v2 lưu `PredictedIntent`, điểm top/second và margin, nhưng không lưu `IntentCandidate.ExampleId`. `IntentScorer` có tạo `ExampleId` tại `IntentScorer.cs:36-42`, còn `OodAbstentionExperimentRunner.ResultRow` chỉ ghi điểm. Vì vậy **không thể xác minh chính xác Reference ID tạo cosine cao nhất từ artifact hiện có** mà không chạy lại inference hoặc sửa harness để lưu evidence; việc suy đoán ID sẽ không được coi là bằng chứng.

### `NEW-R01`

- Expected: `REVENUE_QUERY`, nhưng v2 top là `BUSINESS_SUMMARY` `.9370`, second `REVENUE_QUERY` `.9343`, margin `.0027`.
- Nghiệp vụ: câu hỏi số tiền thuần, một tool `get_revenue` đủ.
- Nguyên nhân khả dĩ: catalog summary có các mẫu nói về tổng quan doanh thu, trong khi revenue catalog cũng dùng “cửa hàng thu/kiếm được bao nhiêu”; hai catalog chồng ngữ nghĩa ở động từ “thu/kiếm được bao nhiêu”.
- Max cosine khuếch đại một reference gần ngẫu nhiên; không có prototype/consistency check để yêu cầu nhiều reference cùng ủng hộ intent.
- v2 đã chuyển thành `CLARIFY` vì margin thấp. Đây là hành vi an toàn hơn chọn nhầm summary, nhưng làm giảm recall.

### `NEW-R04`

- Expected: `REVENUE_QUERY`, top revenue `.9209`, second summary `.9165`, margin `.0044`; v2 `CLARIFY` do margin `.005`.
- Nghiệp vụ: “tuần” không nằm trong allowlist của `get_revenue`; summary mới là tool hiện có hỗ trợ `this_week` và vẫn trả Revenue.
- Vì vậy đây vừa là ranh giới semantic gần nhau, vừa là mismatch giữa nhãn intent và capability thật. Không nên chỉ hạ margin để ép `ACCEPT`.

### `TEST-A03`

- Top summary `.9243`, second revenue `.9144`, margin `.0099`; v2 historical regression vẫn `ACCEPT`.
- Câu yêu cầu doanh thu và tổng quan; summary trả được cả revenue và KPI, nên có thể là `SINGLE_TOOL_MULTI_FIELD`, không nhất thiết bắt buộc hai tool.
- Lỗi hiện tại là composition policy không có tầng “một tool đáp ứng nhiều trường”; chỉ dựa trên embedding câu đầy đủ nên chọn summary và bỏ qua tính compound.

### `DEV2-A03`

- Top summary `.9497`; Development row không được lưu trong result JSON nên second score và ExampleId của câu này không còn bằng chứng truy xuất được; quyết định v2 vẫn `ACCEPT`.
- Cùng hiện tượng với TEST-A03. Đây là thiếu phân biệt giữa intent recognition và composition/capability planning, không chỉ là threshold.

## 4. Có nên bỏ max cosine không?

Max cosine hữu ích để bắt paraphrase khi Reference ít, nhưng có ba rủi ro:

1. Một câu gần duy nhất kéo cả intent lên dù các reference còn lại không ủng hộ.
2. Catalog có câu “doanh thu” trong summary khiến summary cạnh tranh với revenue-only.
3. Điểm cao không chứng minh tool đáp ứng đủ kỳ, schema hoặc độ rộng output.

Không nên bỏ max ngay. Cần ghi `top ExampleId`, top-2 evidence và độ phân tán điểm trong report; sau đó so sánh max với aggregation bền vững trên Development. Việc này chỉ nên làm trong v3, không làm thay đổi v2 hiện tại.

## 5. Tối đa ba phương án thiết kế v3

| Phương án | Paraphrase | Phân biệt hai intent | Compound | Recall risk | Phức tạp/mở rộng | Kiểm chứng |
|---|---|---|---|---|---|---|
| **A. Semantic scorer + capability contract** | Tốt | Dùng embedding trước, sau đó kiểm tra output/period mà tool thực sự hỗ trợ | Phân loại `SINGLE_TOOL_MULTI_FIELD` trước khi yêu cầu hai tool | Thấp-vừa; câu week revenue có thể clarify | Vừa; mở rộng bằng metadata tool | Tốt, vì mỗi quyết định có score, evidence, period và capability reason |
| **B. Prototype/Top-k aggregation** | Tốt hơn khi catalog đa dạng | Giảm ảnh hưởng một reference đơn lẻ bằng mean/top-k/consistency | Vẫn cần composition policy riêng | Vừa; threshold có thể làm mất paraphrase | Vừa; cần calibration theo intent | Tốt nếu lưu toàn bộ evidence và split độc lập |
| **C. Hai tầng: intent embedding → composition planner** | Tốt | Tầng một nhận capability; tầng hai quyết định một tool hay nhiều tool | Tốt nhất cho câu nhiều trường/mệnh đề | Vừa; planner có thể yêu cầu clarify nhiều hơn | Cao; dễ mở rộng nhưng cần schema/contract chuẩn | Tốt nếu có dataset riêng cho single-tool multi-field và multi-tool |

## 6. Đề xuất phù hợp nhất

Chọn **Phương án A làm bước v3 đầu tiên**, kết hợp một phần ghi nhận evidence của B:

1. Giữ embedding để nhận diện semantic candidate.
2. Lưu `TopExampleId`, top-k evidence, score và margin.
3. Gắn mỗi intent với capability contract: supported periods, output fields, branch scope và tool.
4. Tách quyết định `Intent` khỏi `Composition`: một summary có thể đáp ứng revenue + order count; chỉ yêu cầu hai tool khi contract chứng minh một tool không đủ.
5. Nếu semantic candidate gần nhau hoặc period không thuộc tool đã chọn, trả `CLARIFY` với lý do cụ thể; không tự chọn runner-up.

Phương án này không dùng keyword matching thay cho embedding. Các trường như period và output capability là validation độc lập sau semantic recognition.

## 7. Điều kiện trước khi thực nghiệm v3

- Xác nhận nghiệp vụ cho `NEW-R04`, `TEST-A03` và `DEV2-A03`: revenue-only, summary multi-field hay multi-tool thật sự.
- Định nghĩa contract chính thức cho từng tool, đặc biệt kỳ `week` và hành vi period không hợp lệ của `get_business_summary`.
- Tạo dataset v3 mới có nhãn riêng: `SINGLE_INTENT`, `SINGLE_TOOL_MULTI_FIELD`, `MULTI_TOOL_COMPOUND`, `AMBIGUOUS`, `UNSUPPORTED_PERIOD`, `OUT_OF_SCOPE`.
- Giữ nguyên toàn bộ v2 làm historical; không sửa âm thầm nhãn cũ.
- Sửa harness ở lần triển khai v3 để lưu ExampleId và top-k evidence; nếu không, phân tích cosine chi tiết sẽ tiếp tục BLOCKED.
- Chốt Development/Test độc lập và tiêu chí chấp nhận trước khi chạy inference.
- Chưa tích hợp `MockAiProvider`, `AiOrchestrator`, API hoặc UI; chưa APPROVED intent.

## 8. Kết luận

Hai intent không hoàn toàn tách rời về ngữ nghĩa vì `get_business_summary` cũng trả doanh thu. `NEW-R01` là revenue-only nhưng bị embedding kéo sang summary; `NEW-R04` có nhãn revenue nhưng kỳ tuần không được revenue tool hỗ trợ; `TEST-A03` và `DEV2-A03` có thể là một summary nhiều trường thay vì hai tool bắt buộc.

Do đó bước tiếp theo nên làm rõ boundary nghiệp vụ và composition contract trước khi tiếp tục chỉnh threshold hoặc Reference. **Chưa có GO, chưa chốt intent, chưa tích hợp AI Chat.**


## Current status / Superseded by

Historical semantic boundary review; current: retained as design evidence. Superseded for acceptance status by AI-EMBEDDING-MANAGER-ACCEPTANCE-FINAL.md.
