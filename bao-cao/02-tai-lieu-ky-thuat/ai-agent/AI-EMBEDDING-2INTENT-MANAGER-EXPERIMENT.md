# Thực nghiệm Sentence Embedding — 2 Intent / 1 Role

**Trạng thái:** NOT READY — không tích hợp và chưa APPROVED
**Ngày thực hiện:** 2026-10-17
**Phạm vi:** chỉ chạy spike offline, không gọi API, database hoặc AI runtime chính.

## 1. Role và hai intent

Role được chọn là `manager`. Hai intent đều là thao tác đọc, có ngữ cảnh gần nhau nhưng trả về loại thông tin khác nhau:

| Intent | Tool | Quyền đã xác minh | Phạm vi |
|---|---|---|---|
| `REVENUE_QUERY` | `get_revenue` | `Read`; role `admin`, `manager` | `userContext.BranchId` |
| `BUSINESS_SUMMARY` | `get_business_summary` | `Read`; role `admin`, `manager` | `userContext.BranchId` |

Đối chiếu source: `services/api/src/AI/Tools/Admin/GetRevenueTool.cs:15-18,25-56`, `GetBusinessSummaryTool.cs:14-17,24-81`, đăng ký tại `services/api/Program.cs:332-349`, kiểm tra quyền tại `services/api/src/AI/Services/AiPermissionService.cs:30-69`. Kiểm tra branch của `manager` nằm trong `AiOrchestrator.cs:100-110`; việc thực thi tool qua permission gate nằm tại `AiOrchestrator.cs:226-269`. Không gọi các thành phần backend trong thí nghiệm.

`REVENUE_QUERY` yêu cầu số tiền/doanh thu theo kỳ. `BUSINESS_SUMMARY` yêu cầu tổng quan/KPI hoạt động. Đây là cặp phù hợp để kiểm tra phân biệt mục tiêu trả về, nhưng không đại diện cho toàn bộ nghiệp vụ POS.

## 2. Dữ liệu và cách tách tập

Các file nằm trong `.tools/mock-ai-embedding-spike/experiments/manager-revenue-business-summary-v1/`:

| Tập | Số câu | SHA-256 |
|---|---:|---|
| Reference | 12 (6 mỗi intent) | `E4204386F7049B867776072736888D85F63DB58A4767156357689E9EF5375D38` |
| Development | 14 | `A0CC034A6A03A3A209AC8421ABD9CB53043770BA6DB8A6446B3789F0233A9722` |
| Test | 16 | `7AA845CC1AC691013665BCC89F5AFAC98E6774A1D3FBD909E9D8EB7A68BE3422` |

Reference chứa câu mẫu; Development gồm câu trong phạm vi, ngoài phạm vi và trường hợp cần làm rõ để chọn scorer; Test được giữ độc lập và không được đưa ngược vào Reference hoặc dùng để hiệu chỉnh. Các nhóm có paraphrase, cùng từ khóa nhưng khác ý, câu phủ định, mơ hồ và ngoài phạm vi. Bộ câu này là dữ liệu nghiên cứu nhỏ được tạo cho spike, chưa phải dataset chuẩn hóa hay bộ phê duyệt intent.

## 3. Phương pháp

- Model ONNX: `intfloat/multilingual-e5-small`, dùng artifact đã kiểm chứng trong spike (`artifacts/onnx/model_O4.onnx` và `tokenizer.json`).
- Prefix: `query: `.
- Pooling: mean có attention-mask trên `last_hidden_state`.
- Chuẩn hóa: L2 sau pooling.
- Scorer: cosine similarity tối đa giữa câu truy vấn và các câu Reference của mỗi intent.
- Quyết định: `ACCEPT` khi điểm top-1 đạt `MinimumScore` và margin đạt `MinimumMargin`; nếu không đạt nhưng còn tín hiệu thì `UNSUPPORTED` trong runner hiện tại. Nhãn kỳ vọng `CLARIFY` được dùng để đánh giá an toàn trường hợp mơ hồ; runner chưa có một nhánh clarify độc lập.
- Không dùng keyword matching để quyết định nhãn, không fine-tune trọng số E5, không gọi tool và không sửa `MockAiProvider`/`AiOrchestrator`.

Runner và dữ liệu: `.tools/mock-ai-embedding-spike/TwoIntentExperimentRunner.cs`, `experiments/manager-revenue-business-summary-v1/*.json`. Build sử dụng output riêng `artifacts/experiment-build`.

## 4. Hiệu chỉnh trên Development

Runner quét `MinimumScore` từ 0.70 đến 0.95 (bước 0.01) và margin từ 0 đến 0.06 (bước 0.005), chỉ trên Development. Cấu hình được chọn là:

```text
MinimumScore = 0.91
MinimumMargin = 0.00
aggregation = max cosine
```

Development đạt 8/8 câu trong phạm vi và 4/4 câu ngoài phạm vi bị từ chối; tuy nhiên một câu mơ hồ (`Doanh thu thế nào?`) bị nhận `ACCEPT`. Điều này đã được giữ nguyên khi chạy Test; không điều chỉnh theo các lỗi Test.

## 5. Kiểm thử Test độc lập

Lệnh đã chạy:

```text
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore --output artifacts/experiment-build
dotnet artifacts/experiment-build/MockAiEmbeddingSpike.dll --two-intent-experiment --artifact-root artifacts --experiment-root experiments/manager-revenue-business-summary-v1 --output artifacts/manager-revenue-business-summary-v1-result.json
```

Build: **PASS**, 0 warning, 0 error. Runner: **PASS về thực thi**, exit code 0. Bảng dưới đây là toàn bộ 16 câu Test; điểm được làm tròn 4 chữ số để đọc, kết quả gốc nằm trong JSON artifact.

Kiểm tra spike nền cũng chạy được với Unicode tiếng Việt và câu dài: `TRUNCATION_TEST|original=605|used=512|truncated=True`, vector 384 chiều, norm đầu ra `0.999999970`, sai khác khi chạy lặp `0`. Lệnh `--adapter-checks` trả về toàn bộ cờ lifecycle là `true` (bao gồm phục hồi slot sau hủy và từ chối sau dispose). Đây là kiểm tra adapter, không phải kiểm thử semantic.

| ID | Input | Expected | Predicted | Top | Second | Margin | Decision | KQ |
|---|---|---|---|---:|---:|---:|---|---|
| TEST-R01 | Tôi cần biết hôm nay bán hàng thu về bao nhiêu. | REVENUE_QUERY | REVENUE_QUERY | .9380 | .9105 | .0275 | ACCEPT | PASS |
| TEST-R02 | Trong tháng này nhà hàng đạt doanh thu bao nhiêu? | REVENUE_QUERY | REVENUE_QUERY | .9347 | .8903 | .0444 | ACCEPT | PASS |
| TEST-R03 | Cho xem số tiền bán hàng của ngày hôm qua. | REVENUE_QUERY | REVENUE_QUERY | .9622 | .9207 | .0416 | ACCEPT | PASS |
| TEST-R04 | Cửa hàng kiếm được bao nhiêu trong tuần này? | REVENUE_QUERY | REVENUE_QUERY | .9172 | .9011 | .0161 | ACCEPT | PASS |
| TEST-S01 | Tôi muốn xem bức tranh tổng thể về kinh doanh hôm nay. | BUSINESS_SUMMARY | BUSINESS_SUMMARY | .9562 | .9233 | .0329 | ACCEPT | PASS |
| TEST-S02 | Hãy tổng hợp các chỉ số hoạt động của tháng này. | BUSINESS_SUMMARY | BUSINESS_SUMMARY | .9279 | .8965 | .0314 | ACCEPT | PASS |
| TEST-S03 | Báo cáo nhanh tình hình cửa hàng trong tuần này. | BUSINESS_SUMMARY | BUSINESS_SUMMARY | .9452 | .9281 | .0171 | ACCEPT | PASS |
| TEST-S04 | Tổng hợp doanh thu cùng số lượng đơn của hôm qua. | BUSINESS_SUMMARY | BUSINESS_SUMMARY | .9342 | .9219 | .0123 | ACCEPT | PASS |
| TEST-X01 | Cho tôi biết bàn nào đang còn trống. | UNSUPPORTED | REVENUE_QUERY | .8967 | .8817 | .0150 | UNSUPPORTED | PASS |
| TEST-X02 | Xem danh sách món ăn và giá hiện tại. | UNSUPPORTED | REVENUE_QUERY | .9123 | .9007 | .0116 | ACCEPT | **FAIL** |
| TEST-X03 | Tôi muốn xem lịch làm việc của mình. | UNSUPPORTED | BUSINESS_SUMMARY | .8983 | .8670 | .0313 | UNSUPPORTED | PASS |
| TEST-X04 | Đổi trạng thái đơn hàng thành hoàn thành. | UNSUPPORTED | REVENUE_QUERY | .8738 | .8673 | .0065 | UNSUPPORTED | PASS |
| TEST-X05 | Bao giờ nhà hàng đóng cửa tối nay? | UNSUPPORTED | BUSINESS_SUMMARY | .8634 | .8542 | .0092 | UNSUPPORTED | PASS |
| TEST-A01 | Tình hình hôm nay ra sao? | CLARIFY | BUSINESS_SUMMARY | .9178 | .8979 | .0198 | ACCEPT | **FAIL** |
| TEST-A02 | Tôi không cần xem doanh thu hôm nay. | CLARIFY | BUSINESS_SUMMARY | .9220 | .9137 | .0083 | ACCEPT | **FAIL** |
| TEST-A03 | Doanh thu hôm nay và tổng quan kinh doanh thế nào? | CLARIFY | BUSINESS_SUMMARY | .9243 | .9144 | .0099 | ACCEPT | **FAIL** |

### Tổng hợp

| Chỉ số | Kết quả |
|---|---:|
| In-scope accuracy | 8/8 = **100%** |
| Nhầm giữa hai intent trong Test | 0/8 |
| Ngoài phạm vi bị từ chối | 1/5 = **20%** |
| False acceptance ngoài phạm vi | 4/5 |
| Safe non-accept trong nhóm ngoài phạm vi + mơ hồ | 4/8 |
| Confusion | REVENUE_QUERY→REVENUE_QUERY: 4; BUSINESS_SUMMARY→BUSINESS_SUMMARY: 4 |

Các lỗi chính là `TEST-X02` (danh sách món/giá bị nhận thành doanh thu), `TEST-A01` (mơ hồ), `TEST-A02` (phủ định) và `TEST-A03` (gộp hai mục tiêu). Đây là lỗi quyết định `ACCEPT`, có khả năng dẫn đến gọi nhầm tool nếu scorer được nối vào runtime.

## 6. Chi phí inference và bộ nhớ

Đo trong cùng process trên môi trường hiện tại, không tải model mới:

| Đo lường | Kết quả |
|---|---:|
| Khởi tạo adapter | 1,991.18 ms |
| Encode toàn bộ Reference + Development + Test | 1,364.42 ms |
| Working set sau encode | 625,635,328 bytes (~596.7 MiB) |

Đây là một lần đo của spike, không phải benchmark ổn định; chưa có phân phối p50/p95, chưa kiểm tra tải đồng thời và chưa tách chi phí warm/cold ngoài hai mốc trên.

## 7. Giới hạn và trạng thái

- Test nhỏ, chỉ có hai intent và một role; không suy ra khả năng hiểu tiếng Việt tổng quát.
- Scorer hiện dùng `UNSUPPORTED` khi không đạt ngưỡng; chưa có policy clarify độc lập đủ an toàn cho mơ hồ/phủ định.
- False acceptance ngoài phạm vi còn cao; margin bằng 0 không bảo vệ được các câu gần biên.
- Câu dài và Unicode đã được kiểm tra ở spike nền; câu dài bị truncate về 512 token như cấu hình model. Chưa chạy một lượt encode trực tiếp với input rỗng trên adapter đang sống; validation trong source từ chối chuỗi rỗng, nên mục này là **INSPECTED/NOT RUN**. Chưa tạo lỗi inference nhân tạo; lifecycle/error-path check chỉ xác minh hủy và dispose an toàn.
- Chưa đo parity Python/.NET trong lần thí nghiệm này; parity trước đó của spike được ghi là PASS trong `AI-EMBEDDING-SPIKE-VERIFICATION.md`, nhưng không thay thế kiểm thử intent.
- Chưa có đánh giá tham số thời gian/chi nhánh độc lập; thí nghiệm chỉ đánh giá intent.

## 8. Kết luận GO/NO-GO

**NO-GO cho tích hợp vào AI Chat ở trạng thái hiện tại.** Có bằng chứng kỹ thuật rằng E5/ONNX phân biệt được 8/8 câu thuộc hai intent trong bộ Test này, nhưng chỉ từ chối đúng 1/5 câu ngoài phạm vi và chấp nhận 3/3 câu mơ hồ/ phủ định. Vì vậy chưa đạt mục tiêu thí điểm 90% nhận diện trong phạm vi và 90% từ chối ngoài phạm vi đồng thời với xử lý mơ hồ an toàn.

Spike có thể tái sử dụng adapter ONNX, tokenizer, pooling, normalization, scorer và runner tách tập. Chưa được phép tích hợp vào `MockAiProvider`, `AiOrchestrator`, Tool Registry, API hoặc giao diện. Hai intent cũng chưa ở trạng thái `APPROVED`; cần người dùng xem xét kết quả trước khi quyết định bước nghiên cứu tiếp theo.


## Current status / Superseded by

Historical v1 experiment; current: retained as historical evidence, not a current metric.
