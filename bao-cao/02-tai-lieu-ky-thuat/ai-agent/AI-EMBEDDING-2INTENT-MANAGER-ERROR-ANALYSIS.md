# Sentence Embedding — Phân tích lỗi và cải thiện OOD/Abstention

**Trạng thái:** NO-GO / NOT READY
**Phạm vi:** chỉ spike offline cho role `manager`, giữ nguyên `REVENUE_QUERY` và `BUSINESS_SUMMARY`. Không sửa runtime AI, backend, frontend hoặc database.

## 1. Tóm tắt

Lỗi của thí nghiệm v1 không chỉ là ngưỡng thấp. Runner v1 gọi trực tiếp `IntentScorer.Score` và chuyển trạng thái thành `ACCEPT` khi điểm top vượt `0.91`; runner không gọi `IntentDecisionPipeline`. Vì vậy các lớp bảo vệ đã có trong pipeline không tham gia vào bốn câu bị nhận nhầm.

Runner v2 bổ sung trong spike:

- Boundary anchors cho các capability ngoài hai intent.
- Boundary competition: điểm intent phải tách khỏi điểm boundary.
- Kiểm tra phủ định ở mức clause.
- Kiểm tra câu mơ hồ và thiếu kỳ.
- Tách clause và yêu cầu làm rõ khi có nhiều yêu cầu rõ ràng.
- Hiệu chỉnh `score`, `boundary gap` và `intent margin` chỉ trên Development.
- Chạy Test mới độc lập và giữ Test v1 làm historical regression.

Kết quả mới: từ chối ngoài phạm vi đạt **8/8 (100%)**, phủ định/đa ý định/mơ hồ trên Test mới đạt **4/4**, nhưng hai câu trong phạm vi bị chuyển sang `CLARIFY` vì margin thấp và một lỗi nhầm intent còn tồn tại ở `NEW-R01`. Do đó vẫn **NO-GO**.

## 2. Phân tích bốn lỗi của v1

Ngưỡng v1 là `MinimumScore=0.91`, `MinimumMargin=0`. Bảng dưới đây lấy trực tiếp từ `artifacts/manager-revenue-business-summary-v1-result.json`.

| ID | Câu | Expected | Predicted | Top | Second | Margin | Rule v1 | Phân loại và nguyên nhân |
|---|---|---|---|---:|---:|---:|---|---|
| TEST-X02 | Xem danh sách món ăn và giá hiện tại. | UNSUPPORTED | REVENUE_QUERY | .9123 | .9007 | .0116 | `top >= .91`, `margin >= 0` ⇒ ACCEPT | Out-of-domain `MENU_QUERY`; similarity bề mặt cao, không có capability/boundary veto trong runner v1. |
| TEST-A01 | Tình hình hôm nay ra sao? | CLARIFY | BUSINESS_SUMMARY | .9178 | .8979 | .0198 | `top >= .91` ⇒ ACCEPT | Ambiguous request; thiếu subject/kết quả cụ thể và thiếu policy yêu cầu làm rõ. |
| TEST-A02 | Tôi không cần xem doanh thu hôm nay. | CLARIFY | BUSINESS_SUMMARY | .9220 | .9137 | .0083 | `top >= .91` ⇒ ACCEPT | Negated request; embedding vẫn gần câu doanh thu, nhưng v1 không chạy negation detection. |
| TEST-A03 | Doanh thu hôm nay và tổng quan kinh doanh thế nào? | CLARIFY | BUSINESS_SUMMARY | .9243 | .9144 | .0099 | `top >= .91` ⇒ ACCEPT | Multi-intent/compound request; v1 không tách clause và không kiểm tra từng intent độc lập. |

Đây là bốn loại lỗi khác nhau: OOD, mơ hồ, phủ định và đa ý định. Không gộp chúng thành một nhãn OOD.

## 3. Audit scorer và pipeline hiện có

### `IntentScorer`

`IntentScorer.Score` nhóm theo intent, lấy cosine cao nhất (hoặc TopTwoMean), xếp top/second và tính margin tại `IntentScorer.cs:26-56`. Điều kiện hiện tại:

1. `top < minimumScore` → `Unsupported` (`:49-50`).
2. `margin < minimumMargin` → `ClarificationRequired` (`:51-52`).
3. Intent không có capability hoặc intent write unsupported → `Unsupported` (`:53-54`).
4. Còn lại → `Accepted` (`:55`).

Scorer không biết phủ định, subject, tham số, role hoặc branch.

### `IntentDecisionPipeline`

Pipeline offline tại `IntentDecisionPipeline.cs:38-143` có các nhánh:

- Chuẩn hóa và tách clause (`:43-66`).
- Negation → `NegatedRequest` (`:64-68`).
- Re-embed phần khẳng định còn lại (`:70-76`).
- Chấm từng clause; nhiều intent → `MultipleIntents` (`:78-85`).
- Capability veto cho loyalty (`:89-92`) và write intent chưa xác nhận (`:94-98`).
- Candidate unsupported → `LowSimilarity`/`CapabilityNotSupported` (`:100-103`).
- Kiểm tra subject scope (`:106-107`).
- Kiểm tra kỳ/tham số và chuyển `NeedsClarification` khi thiếu hoặc không hợp lệ (`:109-134`).

Các nhánh này đã tồn tại nhưng không cứu được v1 vì `TwoIntentExperimentRunner.Evaluate` chỉ gọi `scorer.Score` tại `TwoIntentExperimentRunner.cs:120-138`. Đây là lỗi policy/integration của harness, không phải bằng chứng rằng các nhánh pipeline đã thất bại khi được gọi.

## 4. Thay đổi trong spike v2

Chỉ các file sau được thêm/sửa trong `.tools/mock-ai-embedding-spike/`:

- `OodAbstentionExperimentRunner.cs`: runner v2, boundary score, negation, ambiguity, clause policy, calibration và metrics.
- `Program.cs`: thêm cờ `--ood-abstention-experiment`.
- `experiments/manager-revenue-business-summary-v2/boundary.json`: 10 anchor ngoài capability.
- `experiments/...-v2/development.json`: 19 câu dùng để hiệu chỉnh.
- `experiments/...-v2/test.json`: 20 câu Test mới, giữ độc lập.
- `experiments/...-v2/reference.json`: copy Reference v1, không đưa Test vào.

Không có quy tắc khớp nguyên văn bốn câu lỗi v1. Boundary anchors mô tả capability tổng quát như menu, ca làm, tồn kho, trạng thái đơn, loyalty, xếp hạng món và đếm đơn.

Policy v2 được chọn trên Development:

```text
MinimumScore       = 0.800
MinimumBoundaryGap = 0.000
MinimumIntentMargin= 0.005
```

`ACCEPT` cần vượt score, không bị boundary cạnh tranh, đủ margin, có kỳ và không mơ hồ. Phủ định → `CLARIFY`; câu thiếu subject/kỳ → `CLARIFY`; câu có nhiều mệnh đề yêu cầu rõ → `CLARIFY`; capability boundary → `UNSUPPORTED`.

## 5. Kết quả Development

Development v2 có 19 câu:

- In-scope đúng: **8/8**.
- Unsupported bị từ chối: **6/6**.
- Clarify đúng: **4/5**.
- Một câu multi-intent còn nhận `ACCEPT`: `DEV2-A03` có cấu trúc phối hợp nhưng hai clause cùng nghiêng về `BUSINESS_SUMMARY`, nên policy hiện tại chưa đủ mạnh để nhận ra mọi compound request.

Development chỉ dùng cho hiệu chỉnh. Không dùng Test mới hoặc historical Test để chọn ngưỡng.

## 6. Test mới độc lập

Test v2 có 20 câu, không trùng ID với Reference/Development và chưa được dùng để hiệu chỉnh.

| Chỉ số | Kết quả |
|---|---:|
| In-scope intent đúng và ACCEPT | 6/8 |
| False acceptance | 0/12 |
| Unsupported bị từ chối | 8/8 = **100%** |
| Clarify đúng (mơ hồ, phủ định, multi-intent) | 4/4 = **100%** |
| Confusion trong các câu ACCEPT | `REVENUE_QUERY→REVENUE_QUERY`: 3; `BUSINESS_SUMMARY→BUSINESS_SUMMARY`: 4; 1 câu revenue bị nhầm thành summary |

Hai câu trong phạm vi bị hạ an toàn xuống `CLARIFY` do margin nhỏ. `NEW-R01` vẫn là lỗi nhầm intent ở tầng semantic: top `BUSINESS_SUMMARY` `.9370`, second `REVENUE_QUERY` `.9343`, margin `.0027`; policy không tự chọn runner-up và không gọi tool, nhưng nếu chấp nhận sẽ là sai tool. `NEW-R04` bị `CLARIFY` vì margin `.0044` thấp hơn `.005`.

Các nhóm ngoài phạm vi đều bị từ chối nhờ boundary competition, kể cả menu, ca làm, order status, tồn kho, giờ đóng cửa, loyalty, ranking món và đếm đơn.

## 7. Historical regression trên Test v1

Test v1 được chạy lại sau khi ngưỡng/policy đã khóa, chỉ với mục đích regression; không còn là đánh giá độc lập mới.

- In-scope đúng: **8/8**.
- Out-of-scope bị từ chối: **5/5**.
- False acceptance: **1**, là `TEST-A03` (multi-intent cũ) vẫn bị `ACCEPT` vì hai clause không có hai động từ yêu cầu rõ.
- Negation `TEST-A02` và ambiguity `TEST-A01` đã chuyển đúng sang `CLARIFY`.

## 8. Bằng chứng thực thi

Lệnh build và chạy:

```text
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore --output artifacts/experiment-build-v2
dotnet artifacts/experiment-build-v2/MockAiEmbeddingSpike.dll --ood-abstention-experiment --artifact-root artifacts --experiment-root experiments/manager-revenue-business-summary-v2 --historical-root experiments/manager-revenue-business-summary-v1 --output artifacts/manager-revenue-business-summary-v2-result.json
```

Build: **PASS**, 0 warning, 0 error. Inference runner: exit code 0, model/tokenizer local, không tải tài nguyên mới và không truy cập database/API. Kết quả chi tiết lưu tại `.tools/mock-ai-embedding-spike/artifacts/manager-revenue-business-summary-v2-result.json`.

## 9. Giới hạn còn lại

- Boundary anchors là dữ liệu nghiên cứu nhỏ; chúng không thay thế capability registry hoặc authorization backend.
- Margin policy làm giảm false acceptance nhưng giảm recall: 2/8 câu trong phạm vi bị yêu cầu làm rõ.
- Một compound request diễn đạt bằng một động từ và nhiều đối tượng vẫn có thể lọt như `TEST-A03`.
- Chưa tích hợp pipeline vào `MockAiProvider` hoặc `AiOrchestrator`; chưa có bằng chứng an toàn ở runtime chính.
- Chưa đánh giá entity/period extraction đầy đủ, tải đồng thời hoặc dataset lớn.

## 10. Kết luận GO/NO-GO

**NO-GO.** v2 đã cải thiện rõ OOD/abstention: Test mới từ chối đúng 100% câu ngoài phạm vi và không false-accept nhóm mơ hồ/phủ định/đa ý định. Tuy nhiên còn một lỗi nhầm `REVENUE_QUERY` thành `BUSINESS_SUMMARY`, hai câu hợp lệ bị hạ thành `CLARIFY`, và historical compound request vẫn lọt. Chưa đủ cơ sở để tích hợp giới hạn vào AI Chat hoặc chốt hai intent ở trạng thái `APPROVED`.


## Current status / Superseded by

Historical v2 error analysis; current: NO-GO result retained as historical evidence. Superseded by later v3.3 and final acceptance evidence.
