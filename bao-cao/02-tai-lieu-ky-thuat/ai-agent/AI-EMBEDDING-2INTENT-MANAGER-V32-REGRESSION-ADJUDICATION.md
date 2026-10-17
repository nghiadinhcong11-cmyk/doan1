# AI Embedding v3.2 — Historical Regression Adjudication

Ngày audit: 2026-10-17
Phạm vi: chỉ đọc artifact v3.1, source tool và `DashboardService`. Không chạy inference mới, không sửa scorer/policy/dataset.

## 1. Nguồn và capability contract

- `GetRevenueTool.cs:12-18,44-58`: `get_revenue`, role admin/manager, Read, bắt buộc `period`, chỉ chấp nhận `today|month|year`, truyền `userContext.BranchId`.
- `GetBusinessSummaryTool.cs:11-18,39-99`: `get_business_summary`, role admin/manager, Read; các nhánh thực tế `today|yesterday|this_week|last_week|this_month|last_month`; nhánh `default` hiện rơi về today.
- `DashboardService.cs:357-383,407-421`: revenue lấy `PaidAmount` của order hoàn thành, đã thanh toán, theo branch; service revenue trực tiếp chỉ map today/month/year.
- `DashboardService.cs:424-497`: summary trả revenue, order count, AOV, expenses, estimated COGS, estimated net profit, top products và cùng lọc branch.

Contract cho adjudication vì vậy được đọc như sau: revenue-only với yesterday/week không thể ACCEPT qua `get_revenue`; summary có thể đáp ứng revenue cùng các trường KPI; câu không nêu output cụ thể phải được xem là thiếu bằng chứng, không tự suy ra.

## 2. Bảy dòng historical không PASS

Điểm và evidence lấy nguyên từ `.tools/mock-ai-embedding-spike/artifacts/manager-revenue-business-summary-v31-result.json`. Các ID evidence là bằng chứng top reference đã được lưu trong artifact.

| ID | Câu hỏi | Expected | Candidate → resolved | Decision | Scores (top/second, margin) | Period / outputs / capability | Composition | Nguyên nhân chính |
|---|---|---|---|---|---|---|---|---|
| NEW-R03 | “Hôm qua doanh thu của quán đạt mức nào?” | REVENUE_QUERY / ACCEPT | REVENUE_QUERY → REVENUE_QUERY | CLARIFY | 0.917855 / 0.888042, 0.029813 | yesterday / revenue / get_revenue không hỗ trợ yesterday | SINGLE_INTENT | **D. DATASET_LABEL_CONFLICT**; policy CLARIFY đúng contract. Evidence REF-R01 0.917855, REF-R02 0.905664. |
| NEW-R04 | “Trong tuần này doanh số là bao nhiêu tiền?” | REVENUE_QUERY / ACCEPT | REVENUE_QUERY → REVENUE_QUERY | CLARIFY | 0.920886 / 0.916497, 0.004388 | this_week / revenue / get_revenue không hỗ trợ week | SINGLE_INTENT | **D. DATASET_LABEL_CONFLICT** và phụ **A. SEMANTIC_ERROR** do margin rất sát. Evidence REF-R04 0.920886, REF-R02 0.919578. |
| NEW-S03 | “Cho biết tình hình vận hành trong tuần hiện tại.” | BUSINESS_SUMMARY / ACCEPT | BUSINESS_SUMMARY → BUSINESS_SUMMARY | CLARIFY | 0.915587 / 0.897675, 0.017912 | this_week / toàn bộ summary fields / get_business_summary hỗ trợ this_week | SINGLE_TOOL_MULTI_FIELD | **F. INSUFFICIENT_EVIDENCE**; “tình hình vận hành” không chỉ rõ output, policy ambiguous-subject chặn ACCEPT. Evidence REF-S02 0.915587, REF-S05 0.909875. |
| NEW-X03 | “Hãy cập nhật trạng thái đơn hàng.” | OUT_OF_SCOPE / UNSUPPORTED | REVENUE_QUERY → REVENUE_QUERY | CLARIFY | 0.897054 / 0.883526, 0.013528 | missing / order_count, order_status; không capability nào hỗ trợ update/status | OUT_OF_SCOPE | **A. SEMANTIC_ERROR** candidate bị kéo về revenue; **B. POLICY_ERROR** vì output không hỗ trợ bị đi qua nhánh mixed-capability và trả CLARIFY thay vì UNSUPPORTED. Evidence REF-R06 0.897054, REF-R03 0.868689. |
| NEW-X05 | “Tối nay nhà hàng phục vụ đến mấy giờ?” | OUT_OF_SCOPE / UNSUPPORTED | BUSINESS_SUMMARY → BUSINESS_SUMMARY | CLARIFY | 0.865511 / 0.858801, 0.006710 | missing / không output được parse; summary không có capability giờ mở cửa | SINGLE_INTENT | **A. SEMANTIC_ERROR** (candidate summary), **B. POLICY_ERROR** (missing period được ưu tiên trước unsupported subject). Evidence REF-S03 0.865511, REF-S01 0.851943. |
| NEW-X06 | “Khách này còn bao nhiêu điểm thành viên?” | OUT_OF_SCOPE / UNSUPPORTED | BUSINESS_SUMMARY → BUSINESS_SUMMARY | CLARIFY | 0.830604 / 0.822094, 0.008510 | missing / không output được parse; summary không có loyalty points | SINGLE_INTENT | **A. SEMANTIC_ERROR** và **B. POLICY_ERROR** cùng NEW-X05; đây là OOD nhưng không bị ACCEPT. Evidence REF-S03 0.830604, REF-S04 0.812482. |
| NEW-A03 | “Báo cáo doanh thu và cho biết tổng quan hôm nay.” | MULTI_INTENT / CLARIFY | BUSINESS_SUMMARY → BUSINESS_SUMMARY | ACCEPT | 0.945288 / 0.937752, 0.007535 | today / toàn bộ summary fields; summary có revenue và KPI | SINGLE_TOOL_MULTI_FIELD | **D. DATASET_LABEL_CONFLICT**; theo capability một tool đáp ứng đủ yêu cầu. Đây là false acceptance theo nhãn lịch sử, không phải capability violation. Evidence REF-S05 0.945288, REF-S02 0.939196. |

### Capability validation riêng

- NEW-R03 và NEW-R04: semantic candidate đúng revenue nhưng period không hợp lệ cho tool đã chọn; CLARIFY là kết quả an toàn.
- NEW-S03: period hợp lệ và tool đủ output, nhưng subject không đủ cụ thể để biết người dùng muốn KPI nào.
- NEW-X03: required outputs không thuộc một capability hợp lệ; không có quyền/branch/tool execution nào xảy ra.
- NEW-X05/X06: không có required output tương ứng; việc dừng ở thiếu period là policy precedence, không phải capability acceptance.
- NEW-A03: summary output bao gồm revenue, order count và KPI; single-tool multi-field phù hợp capability.

## 3. Adjudication theo nhóm nguyên nhân

### D. Dataset label conflict

- **NEW-R03**: nhãn ACCEPT revenue-only mâu thuẫn allowlist `get_revenue`; đề xuất nhãn tương lai: `REVENUE_QUERY / CLARIFY_UNSUPPORTED_PERIOD`.
- **NEW-R04**: cùng mâu thuẫn; nếu muốn hỗ trợ tuần thì phải chọn summary capability hoặc định nghĩa mapping mới, không sửa nhãn lịch sử âm thầm.
- **NEW-A03**: đề xuất `BUSINESS_SUMMARY / ACCEPT / SINGLE_TOOL_MULTI_FIELD` nếu người dùng chấp nhận summary chứa revenue; chỉ giữ CLARIFY nếu nghiệp vụ yêu cầu một output revenue độc lập với định dạng khác.

### A/B: lỗi thực sự cần xử lý trong policy/scorer

- NEW-R04 có margin 0.004388 và hai catalog gần nhau; cần evidence/top-k hoặc aggregation bền vững hơn, nhưng không được hạ margin để ép ACCEPT.
- NEW-X03/X05/X06 cho thấy semantic candidate vẫn nhận OOD gần các reference nghiệp vụ.
- NEW-X03 cho thấy thứ tự policy cần phân biệt “mọi output unsupported” với “một output vô tình trùng capability khác”; hiện tại composition reason là mixed-capability.
- NEW-X05/X06 cho thấy kiểm tra OOD/subject nên có thể xảy ra trước khi yêu cầu period, nếu không sẽ trả CLARIFY vì thiếu kỳ cho một câu vốn ngoài phạm vi.

### F. Insufficient evidence

- NEW-S03 chưa nêu chỉ số cụ thể. Không có cơ sở khẳng định người dùng muốn doanh thu, KPI đầy đủ hay trạng thái vận hành. Không nên đổi thành ACCEPT chỉ vì embedding đứng đầu summary.

### E. Metric error

Không phát hiện thêm lỗi tính toán trong artifact v3.1 cho bảy dòng này. v3.1 đã phân biệt final decision và safe abstention. Sai khác lịch sử chủ yếu do nhãn/capability, không được dùng để tuyên bố accuracy mới.

## 4. Đề xuất nhãn adjudication riêng

Không sửa dataset lịch sử. Bảng đề xuất cho phiên bản dữ liệu sau:

| ID | Nhãn đề xuất | Lý do |
|---|---|---|
| NEW-R03 | REVENUE_QUERY + CLARIFY_UNSUPPORTED_PERIOD | Revenue-only nhưng revenue tool không có yesterday |
| NEW-R04 | REVENUE_QUERY + CLARIFY_UNSUPPORTED_PERIOD, hoặc BUSINESS_SUMMARY nếu chấp nhận summary revenue theo tuần | Cần quyết định nghiệp vụ trước |
| NEW-S03 | BUSINESS_SUMMARY + CLARIFY_AMBIGUOUS_SUBJECT | Period hợp lệ nhưng thiếu output cụ thể |
| NEW-X03 | OUT_OF_SCOPE + UNSUPPORTED | Cập nhật trạng thái đơn hàng ngoài hai read-only capability |
| NEW-X05 | OUT_OF_SCOPE + UNSUPPORTED | Giờ phục vụ ngoài capability |
| NEW-X06 | OUT_OF_SCOPE + UNSUPPORTED | Loyalty points ngoài capability |
| NEW-A03 | BUSINESS_SUMMARY + ACCEPT + SINGLE_TOOL_MULTI_FIELD | Summary đáp ứng revenue và tổng quan trong một tool |

Các nhãn này chỉ là adjudication đề xuất, chưa phải test độc lập và chưa được đưa vào calibration.

## 5. Đánh giá riêng từng intent

### REVENUE_QUERY

- **Paraphrase:** Có bằng chứng tốt trên câu revenue rõ ràng; NEW-R03 nhận đúng candidate.
- **Period support:** chỉ today/month/year. Hai historical ACCEPT với yesterday/week là nhãn không phù hợp capability.
- **False routing sang summary:** vẫn có overlap; NEW-R04 margin thấp và NEW-R01 trong các artifact trước cho thấy catalog cần boundary rõ hơn.
- **Safe abstention:** đang hoạt động khi period không hợp lệ; không nên đổi thành ACCEPT chỉ để tăng recall.

Nghiên cứu: có thể tiếp tục xem xét sau khi chốt week/yesterday boundary. Runtime: chưa đủ điều kiện tích hợp vì còn overlap và historical adjudication chưa chốt.

### BUSINESS_SUMMARY

- **Summary recognition:** candidate tốt trên NEW-S03 và NEW-A03.
- **Single-tool multi-field:** NEW-A03 cho thấy summary có thể đáp ứng revenue + tổng quan mà không cần hai tool.
- **Composition:** policy đã nhận diện single-tool, nhưng các nhãn lịch sử chưa nhất quán.
- **False routing sang revenue:** cần test riêng các câu revenue-only và summary có revenue; không kết luận từ bảy dòng này rằng đã giải quyết triệt để.

Nghiên cứu: có thể tiếp tục với capability contract rõ hơn. Runtime: chưa đủ điều kiện tích hợp; authorization/branch runtime chưa được kiểm chứng trong spike.

## 6. Điều kiện còn thiếu trước GO

1. Người dùng/nghiệp vụ phải chốt mapping cho revenue theo yesterday/week.
2. Chốt định nghĩa `MULTI_INTENT` so với `SINGLE_TOOL_MULTI_FIELD`.
3. Tách policy OOD khỏi kiểm tra thiếu period để câu ngoài phạm vi không bị diễn giải như câu trong phạm vi thiếu tham số.
4. Chứng minh boundary bằng holdout mới sau khi adjudication, không dùng lại historical labels để calibration.
5. Giữ historical dataset bất biến và lưu adjudication như artifact riêng.
6. Kiểm tra runtime permission/branch bằng integration evidence trước khi tích hợp.

## 7. Kết luận

Bảy dòng không PASS không phải cùng một loại lỗi:

- NEW-R03, NEW-R04 và NEW-A03 chủ yếu là **dataset/capability boundary conflict**.
- NEW-S03 là **thiếu thông tin nghiệp vụ**, policy CLARIFY có cơ sở.
- NEW-X03, NEW-X05 và NEW-X06 là **OOD semantic/policy issues**, dù không có false tool execution.

Do còn lỗi semantic OOD, một false acceptance theo nhãn lịch sử và chưa chốt các boundary nghiệp vụ, **chưa chuyển sang nghiệm thu giới hạn và chưa APPROVED intent**. Bước phù hợp là adjudicate capability/labels trước; chỉ sau khi chốt mới quyết định tiếp tục tối ưu hay thiết kế một holdout độc lập mới.



## Current status / Superseded by

Historical adjudication; current: retained for label history, superseded by v3.3 baseline evidence.
