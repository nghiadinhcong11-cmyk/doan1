# Đề xuất nâng cấp nhận diện Mock AI bằng sentence embedding

## 1. Kết luận điều hành

Sentence embedding + so khớp câu mẫu là hướng đã chọn cho bước nhận diện intent. Báo cáo này mới là thiết kế/audit; chưa tích hợp model, chưa thêm dependency và chưa thay đổi code ứng dụng.

Đề xuất chính là chạy embedding ngay trong backend .NET bằng ONNX Runtime, dùng một model multilingual nhỏ đã được chuẩn bị và đóng gói cùng ứng dụng. Không thêm microservice, vector database hay agent framework. Tập câu mẫu chỉ có vài chục đến vài trăm câu nên có thể tính cosine similarity tuyến tính trong bộ nhớ.

Embedding chỉ thay thế lớp nhận diện câu gần nghĩa. Nó không thay thế parser tham số, history policy, permission, branch/customer ownership, confirmation hay application service. Pipeline an toàn vẫn là:

```text
Câu nhập
  → chuẩn hóa tối thiểu + giữ bản gốc
  → embedding câu nhập
  → cosine với câu mẫu
  → top-1, margin và policy confidence
  → intent hoặc hỏi lại/không hỗ trợ
  → parser tham số riêng
  → AiToolRegistry/AiPermissionService
  → tool → application service → kết quả
```

Không được lọc theo role trước rồi ép câu hỏi vào intent còn lại. Nhận diện ý định trước; kiểm tra quyền sau.

## 2. Hiện trạng đã xác minh từ source

Mốc khảo sát là HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`. Working tree đang dirty với thay đổi source AI/frontend và tài liệu từ các lượt trước; báo cáo này bảo toàn chúng và đối chiếu source thực tế, không coi handoff cũ là bằng chứng runtime.

### 2.1. Luồng hiện tại

- Admin/staff dùng `apps/admin-web/src/components/ChatBot.tsx`; customer dùng `apps/customer-web/src/components/ChatBot.tsx`.
- Frontend gửi tối đa 6 message history tới `POST /api/Ai/chat`, hoặc guest không có token tới `POST /api/Ai/guest-chat`.
- `services/api/src/AI/Controllers/AiController.cs`, `Chat()` và `GuestChat()` lấy role/user/branch từ JWT hoặc tạo guest context; không tin các giá trị tương tự trong câu chat.
- `AiOrchestrator.ProcessInternalAsync()` lấy context, prompt và tool definition, gọi provider tối đa 5 vòng, rồi kiểm tra registry, read-only, allowlist và permission trước khi gọi tool.
- `Program.cs` đăng ký `IAiProvider` bằng `MockAiProvider`; không có Gemini/OpenAI provider active.
- `MockAiProvider.GenerateContentAsync()` đọc message cuối, history và tool schema. Nó gọi `MockAiIntentRecognizer.Recognize()` để trả function call hoặc text.
- Kết quả function call được gửi lại cho provider để format; tool failure hiện được `AiOrchestrator` trả `Success=false` theo source đang có.

### 2.2. Nhận diện hiện tại

`MockAiIntentRecognizer.Recognize()` trong `services/api/src/AI/Services/MockAiIntentRecognizer.cs` dùng một chuỗi `if` tuần tự. Các helper `IsRevenue`, `IsRevenueComparison`, `IsProductRanking`, `IsOrderStatus`, `IsMultiIntent` và các parser `Resolve...` đều dùng `Contains`/`ContainsAny`, không dùng regex, scoring hoặc model.

`MockAiTextNormalizer.Normalize()`:

- chuyển `đ/Đ` thành `d`;
- bỏ dấu Unicode tiếng Việt;
- chuyển lowercase;
- thay ký tự không phải chữ/số/khoảng trắng bằng khoảng trắng;
- gộp khoảng trắng.

Nó không sửa lỗi chính tả, không hiểu đồng nghĩa ngoài danh sách hard-code và không trích xuất tổng quát tên món, số lượng, ID hoặc branch.

Các intent hiện có gồm introduction, inventory unsupported, write unsupported, ambiguous multi-intent, revenue comparison, product ranking, my orders, order status, order count, revenue, business summary, table status, menu, my shift và unsupported intent. Tool tương ứng chỉ được gọi nếu tên tool có trong schema role hiện tại.

### 2.3. Quyền và phạm vi không thay đổi

`AiToolRegistry.GetToolsDefinition()` lọc theo role, risk và authenticated-customer allowlist. `AiPermissionService.CanExecuteAsync()` kiểm tra guest menu-only, customer allowlist, branch context, role và risk. `AiOrchestrator` còn có read-only/guest allowlist defense-in-depth.

Embedding không được đưa role, branch ID, customer ID hay quyền vào vector để “học” quyền. Identity vẫn lấy từ server-side context. Write intent/confirmation vẫn chưa mở; các write tool đã đăng ký không phải bằng chứng rằng chat có thể gọi chúng.

## 3. Hạn chế hiện tại và vai trò của embedding

| Hạn chế | Bằng chứng source | Ví dụ | Hiện tại | Embedding cải thiện | Embedding không giải quyết |
|---|---|---|---|---|---|
| Khác cách diễn đạt | `IsRevenue`, `IsBusinessSummary`, `IsMenu` dùng danh sách substring | `Hôm nay bán được bao nhiêu tiền?` khác `doanh thu hôm nay` | Chỉ khớp nếu có cụm đã liệt kê | Gom câu gần nghĩa vào cùng intent | Không chứng minh câu đó có đủ kỳ/tham số |
| Phủ định | Không có lớp negation | `Tôi không muốn xem doanh thu hôm nay` vẫn chứa `doanh thu` | Có thể gọi revenue | Có thể giúp phân biệt mẫu phủ định nếu có mẫu/feature riêng | Vector similarity tự nó không bảo đảm hiểu phủ định; vẫn cần policy/parser |
| Nhiều intent | `IsMultiIntent` chỉ đếm nhóm từ khóa | `Xem doanh thu và món bán chạy` | Trả ambiguous nếu đếm được hai nhóm | Có thể phát hiện hai ý khi so top intent | Không tự tách câu an toàn; cần multi-intent policy |
| Intent gần nhau | Revenue, business summary, order count dùng từ chung | `Báo cáo bán hàng hôm nay` | Thứ tự `if` quyết định | Top-1 + margin cho phép hỏi lại khi sát điểm | Không tự quyết định product policy giữa hai intent gần nhau |
| Thiếu tham số | `ResolvePeriod`, `ResolveComparison`, `ResolveBestSellerPeriod` chỉ tìm token cố định | `Doanh thu thế nào?` | Hỏi lại | Vẫn nhận đúng intent dù câu tự nhiên hơn | Không tự tạo kỳ mặc định tùy ý |
| Ngoài phạm vi | Fallback `UNSUPPORTED_INTENT` | `Tính lương nhân viên tháng này` | Không hỗ trợ | Mẫu negative/out-of-scope giúp tránh kéo vào intent gần | Không biết toàn bộ nghiệp vụ mới; cần negative samples và allowlist |
| Câu nối tiếp | Context chỉ được dùng đặc biệt trong `IsRevenue` | `Còn hôm qua thì sao?` | Chỉ có thể nối với revenue/history | Có thể nhận câu rút gọn gần mẫu | Không thay thế conversation state hoặc entity binding |
| Chính tả/telex | Normalizer chỉ bỏ dấu/ký tự | `doanh thuu hom nay`, `dt hom nay` | Có thể fallback | Có thể chịu paraphrase một phần | Không nên kỳ vọng sửa mọi typo; cần test thực tế |
| Tham số | `ExtractCategory` mới xử lý một số marker | `Cho tôi 3 món phở bán chạy` | Không có parser quantity/product | Không nên dùng embedding để lấy số lượng | Cần parser/validator riêng và không được đoán ID |

Một điểm nguy hiểm là embedding có thể làm câu ngoài phạm vi gần một intent hợp lệ. Vì vậy phải có threshold, margin, negative mẫu và trạng thái `UNKNOWN/UNSUPPORTED`; không được luôn chọn top-1.

## 4. So sánh hai cách triển khai local

### Phương án A — In-process .NET bằng ONNX Runtime

Đặt một `ISentenceEmbeddingService` trong Application/AI boundary, triển khai bằng `Microsoft.ML.OnnxRuntime`. Load `InferenceSession` một lần qua DI singleton; cache vector câu mẫu; mỗi request tokenize, chạy model, mean-pool theo attention mask, L2-normalize rồi cosine.

ONNX Runtime có C# binding cho .NET Standard và gói CPU hỗ trợ Windows/Linux/macOS theo tài liệu chính thức. Tài liệu cũng lưu ý tạo session là thao tác đắt và nên cache session. Xem [ONNX Runtime C# API](https://onnxruntime.ai/docs/get-started/with-csharp) và [C# tutorial](https://onnxruntime.ai/docs/tutorials/csharp/basic_csharp.html).

Model ứng viên để thử nghiệm đầu tiên là [intfloat/multilingual-e5-small](https://huggingface.co/intfloat/multilingual-e5-small): model card công bố hỗ trợ đa ngôn ngữ, embedding size 384, 12 layers, model size khoảng 0.1B; repository gắn license MIT và tokenizer có `model_max_length=512`. Model card cũng nêu văn bản dài hơn bị truncate và low-resource language có thể giảm chất lượng. Đây là khả năng được model card xác nhận, không phải SLA cho tiếng Việt của dự án.

Quan trọng: model card yêu cầu quy ước prefix theo tác vụ (`query:`/`passage:` trong hướng dẫn của E5). Nếu xuất ONNX, phải xác minh tokenizer, input names, attention mask, pooling và normalization đúng với model card. Có file ONNX hoặc hidden states không đồng nghĩa đã có sentence embedding tương đương model gốc.

Ưu điểm:

- ít thay đổi kiến trúc và giữ request path hiện tại;
- không có network hop/service discovery;
- chạy offline sau khi model/tokenizer được chuẩn bị;
- tập mẫu nhỏ nên cosine tuyến tính đủ dùng;
- dễ giữ `MockAiProvider` và `IAiProvider` làm boundary.

Chi phí/rủi ro:

- thêm dependency native/managed, model artifact và quy trình license/provenance;
- phải tự bảo đảm tokenizer/pooling đúng;
- RAM/CPU/latency chưa được biết trong server hiện tại;
- ASP.NET request path cần cancellation, session thread-safety và giới hạn input;
- model tải lỗi phải có trạng thái rõ ràng, không âm thầm dùng embedding giả.

### Phương án B — Service embedding local riêng

Chạy một process nội bộ Python/ONNX Runtime hoặc service local tương đương, backend gọi HTTP/gRPC để lấy vector.

Ưu điểm là cô lập dependency ML, dễ đổi runtime/model và có thể scale độc lập. Nhưng với tập mẫu nhỏ, phương án này thêm process, health check, timeout, deployment, versioning, network failure và logging. Nó không cải thiện permission hay business correctness.

Repository hiện không có embedding service, service discovery hay deployment contract cho service này. Vì vậy không nên thêm microservice ở giai đoạn đầu. Chỉ xem xét phương án B nếu đo được model ảnh hưởng đáng kể tới API process hoặc cần chia sẻ model cho nhiều ứng dụng.

### Lựa chọn

Chọn **Phương án A — in-process ONNX Runtime**, nhưng chỉ sau một spike cô lập chứng minh tokenizer/pooling/model artifact và đo tài nguyên. Không coi kích thước model trên model card là tài nguyên server đã được chấp nhận; cần đo trên môi trường triển khai thật.

## 5. Thiết kế nhận diện đề xuất

### 5.1. Cấu trúc dữ liệu câu mẫu

Không lấy trực tiếp Markdown runtime làm registry. Tạo dữ liệu versioned trong source, ví dụ:

```csharp
sealed record MockIntentExample(
    string Id,
    string IntentCode,
    string Text,
    IReadOnlyDictionary<string, string>? ExpectedParameters,
    bool IsNegative,
    string ActorScope);
```

Mỗi intent có câu mẫu dương, negative gần nghĩa, câu thiếu tham số, actor metadata, model revision và prefix convention. Actor scope chỉ để test/metadata, không dùng để bypass permission.

Tập mẫu nên bắt đầu từ intent thật sự hỗ trợ: revenue, revenue comparison, business summary, product ranking, order status/count, menu, my orders, my shift và unsupported/negative. Không thêm write intent.

Vector câu mẫu có thể cache trong memory sau khi model load. Nếu lưu file vector đã tính trước, phải lưu model revision, tokenizer revision, prefix convention, dimension và normalization mode; model/config mismatch phải làm startup/preflight fail rõ ràng.

### 5.2. Điểm và quyết định

Với mỗi câu nhập:

1. Giữ raw text để log/audit phù hợp, không log secret.
2. Chuẩn hóa nhẹ cho model theo đúng tokenizer; không dùng normalizer bỏ dấu cũ làm mất thông tin trước khi embedding nếu model phân biệt được dấu.
3. Tính vector `v` và cosine với từng example vector `e`.
4. Gom điểm theo intent, ví dụ lấy top-k hoặc max có trọng số; không lấy điểm trung bình mù.
5. Chọn intent top-1 `p1`; tìm điểm cao nhất của intent khác `p2`.
6. Chỉ nhận nếu dữ liệu calibration cho thấy `p1 >= threshold` và `p1 - p2 >= margin`.
7. Nếu thiếu điều kiện, trả `CLARIFICATION_REQUIRED` hoặc `UNSUPPORTED_INTENT`.

Không chốt threshold tùy ý. Dùng tập calibration riêng để đo score dương, negative/out-of-scope, false-positive budget, cặp intent gần nhau và câu có dấu/không dấu. Model revision và kết quả calibration phải được cố định.

### 5.3. Phủ định, nhiều ý và ngoài phạm vi

- Phủ định cần rule trước/sau embedding: `không`, `đừng`, `không muốn`, `không cần`. Nếu có động từ phủ định bao quanh intent, không tự gọi read tool. Embedding chỉ cung cấp candidate.
- Nhiều ý: chạy candidate trên các mệnh đề; nếu có từ hai intent hợp lệ thì hỏi người dùng tách câu.
- Ngoài phạm vi: duy trì negative examples và unknown threshold. Guest/admin/write phải có policy rõ, không dựa vào khoảng cách vector đơn thuần.

### 5.4. Parser và history

Giữ parser thời gian hiện có như fallback ban đầu, nhưng tách thành `IIntentParameterParser` theo intent. Embedding không lấy `period`, category, status hay quantity.

- `period`: giữ allowlist hôm nay/hôm qua/tuần/tháng; thiếu thì hỏi lại.
- `category`: giữ marker/category allowlist.
- status: map từ allowlist hiện có.
- tên món, quantity, order/product ID: chưa mở rộng phase đầu; không đoán ID.

History phase đầu chỉ hỗ trợ continuation có policy rõ, tốt nhất là revenue period continuation. Cần server-trusted conversation state hoặc history đã được phân loại; nếu không đủ state, hỏi lại.

### 5.5. Model không tải được

Provider phải có trạng thái `EmbeddingUnavailable` ở health/preflight/log an toàn. Policy phải chọn rõ:

- fail closed: không gọi tool;
- fallback deterministic rule: ghi rõ response mode là `RULE_FALLBACK`, chỉ dùng allowlist hiện có.

Không được trả kết quả như thể embedding đang hoạt động khi model chưa tải.

## 6. File/component dự kiến

### Giữ nguyên

- `AiController`, `AiOrchestrator`.
- `AiToolRegistry`, `AiPermissionService`, `AiAuthorization`.
- `IAiProvider` và `MockAiProvider` boundary.
- AI tools, application services, branch/customer ownership và confirmation endpoint.
- Frontend ChatBot và bộ 104 case Markdown.

### Có thể sửa sau spike

- `MockAiIntentRecognizer`: tách `RuleIntentRecognizer` và `EmbeddingIntentRecognizer`, hoặc strategy interface.
- `MockAiProvider`: gọi recognizer strategy, ghi mode/model revision.
- `Program.cs`: đăng ký embedding service singleton, model path/config.
- `MockAiTextNormalizer`: giữ cho rule fallback/parser; không áp dụng mù cho embedding.
- DTO nội bộ intent/result: thêm score/margin/model revision, không để client quyết định permission.

### File/dependency mới dự kiến

- `ISentenceEmbeddingService` và ONNX implementation.
- tokenizer adapter tương ứng model.
- catalog mẫu intent versioned trong `services/api/src/AI/`.
- model artifact hoặc deployment manifest/checksum/license notice.
- config mode, model path, revision, timeout, thresholds.

Chưa tạo các file/dependency này trong lượt audit.

## 7. Bộ minh chứng thủ công khoảng 30 case

Đây là bộ đề xuất, chưa chạy. Tách ba tập: mẫu dùng tạo catalog, calibration dùng chọn threshold/margin, evaluation khóa lại để đo. Không dùng nguyên câu mẫu làm evidence.

### 7.1. Mười tám câu diễn đạt khác cho sáu intent

| ID | Actor | Câu nhập | Intent | Tham số | Tool/hành vi |
|---|---|---|---|---|---|
| EMB-E01 | Owner | Hôm nay doanh thu được bao nhiêu? | REVENUE_QUERY | today | get_revenue |
| EMB-E02 | Owner | Tiền bán hàng trong ngày hiện tại là bao nhiêu? | REVENUE_QUERY | today | get_revenue |
| EMB-E03 | Owner | Cho tôi biết doanh số của hôm nay. | REVENUE_QUERY | today | get_revenue |
| EMB-E04 | Owner | So sánh doanh thu hôm nay với hôm qua. | REVENUE_COMPARISON | today/yesterday | get_revenue_comparison |
| EMB-E05 | Owner | Hôm nay bán được nhiều tiền hơn hôm qua không? | REVENUE_COMPARISON | today/yesterday | get_revenue_comparison |
| EMB-E06 | Owner | Đối chiếu doanh thu giữa hôm nay và ngày trước. | REVENUE_COMPARISON | cần xác nhận | hỏi lại nếu parser chưa hiểu |
| EMB-E07 | Owner | Món nào đang bán chạy nhất tuần này? | PRODUCT_RANKING | week | get_best_sellers |
| EMB-E08 | Owner | Xếp hạng các món bán nhiều trong tuần. | PRODUCT_RANKING | week | get_best_sellers |
| EMB-E09 | Owner | Tuần này khách gọi món nào nhiều nhất? | PRODUCT_RANKING | week | get_best_sellers |
| EMB-E10 | Owner | Tóm tắt tình hình kinh doanh tháng này. | BUSINESS_SUMMARY | this_month | get_business_summary |
| EMB-E11 | Owner | Cho tôi bức tranh tổng quan hoạt động tháng hiện tại. | BUSINESS_SUMMARY | this_month | get_business_summary |
| EMB-E12 | Owner | Báo cáo KPI của tháng này. | BUSINESS_SUMMARY | this_month | get_business_summary |
| EMB-E13 | Customer | Tôi muốn xem thực đơn hôm nay. | MENU_QUERY | none | customer_get_menu |
| EMB-E14 | Guest | Nhà hàng hiện có những món nào? | MENU_QUERY | none | guest menu only |
| EMB-E15 | Customer | Cho tôi danh sách các món ăn và giá. | MENU_QUERY | none | customer_get_menu |
| EMB-E16 | Customer | Xem lại những đơn tôi đã đặt. | MY_ORDERS | none | get_my_orders |
| EMB-E17 | Customer | Các đơn hàng của tài khoản này là gì? | MY_ORDERS | none | get_my_orders, identity từ JWT |
| EMB-E18 | Employee | Hôm nay tôi làm ca nào? | MY_SHIFT | none | get_my_shift, identity từ JWT |

### 7.2. Sáu case nhầm/thiếu tham số/nhiều ý

| ID | Actor | Câu nhập | Intent mong đợi | Tham số | Kết quả cần ghi |
|---|---|---|---|---|---|
| EMB-E19 | Owner | Doanh thu thế nào? | REVENUE_QUERY | thiếu period | Hỏi lại; không chọn today mặc định |
| EMB-E20 | Owner | So sánh doanh thu giúp tôi. | REVENUE_COMPARISON | thiếu hai kỳ | Hỏi lại; không gọi tool |
| EMB-E21 | Owner | Xem doanh thu hôm nay và món bán chạy tuần này. | ambiguous | hai intent | Hỏi tách câu |
| EMB-E22 | Owner | Báo cáo bán hàng hôm nay. | policy-dependent | today | Đo top-1/margin; có thể hỏi lại revenue/summary |
| EMB-E23 | Customer | Còn hôm qua thì sao? | continuation | phụ thuộc state | Chỉ xử lý nếu state trước là revenue |
| EMB-E24 | Customer | Cho tôi 3 món phở bán chạy. | ranking/menu | quantity/product filter | Không đoán quantity/filter nếu parser chưa hỗ trợ |

### 7.3. Sáu case ngoài phạm vi/phủ định/sai quyền

| ID | Actor | Câu nhập | Intent mong đợi | Tham số | Kết quả cần ghi |
|---|---|---|---|---|---|
| EMB-E25 | Owner | Tôi không muốn xem doanh thu hôm nay. | negative revenue | today | Không gọi get_revenue |
| EMB-E26 | Owner | Đừng cập nhật trạng thái đơn này. | write/negative | none | Không mutation |
| EMB-E27 | Owner | Kiểm tra tồn kho nguyên liệu. | unsupported inventory | none | Từ chối capability |
| EMB-E28 | Customer | Cho tôi doanh thu toàn chuỗi. | revenue + permission denial | thiếu period | Nhận diện revenue rồi kiểm tra quyền, không đổi thành menu |
| EMB-E29 | Guest | Cho tôi xem đơn của khách hàng khác. | ownership denial | customer absent | Từ chối/yêu cầu đăng nhập |
| EMB-E30 | Guest | Đổi giá món Phở thành 50.000 đồng. | write unsupported | product/price | Không gọi write/confirmation |

Mỗi case phải ghi riêng: nhận diện intent, parser tham số, permission decision và tool/business response. Nhận diện đúng nhưng thiếu period không được tính là hoàn thành nghiệp vụ.

Tổng hợp sau khi chạy gồm: intent đúng/tổng, parser đúng/tham số có mặt, permission đúng, tool result đúng, clarification đúng, refusal đúng, confusion pairs và false positive. Không báo accuracy trước.

## 8. Lộ trình tích hợp nhỏ

1. **Chốt artifact/policy:** model revision, license, prefix/pooling/normalization, lưu model, mode unavailable, false-positive budget.
2. **Spike cô lập:** xác minh tokenizer → ONNX inputs → hidden output → attention-mask mean pooling → L2 normalization; đo CPU/RAM/latency.
3. **Catalog/calibration:** tách mẫu, negative, calibration, evaluation; chốt threshold/margin bằng dữ liệu.
4. **Adapter sau provider boundary:** thêm recognizer strategy; giữ rule fallback; ghi mode/model revision; không đổi permission/tool flow.
5. **Parser/policy:** tách parser allowlist, phủ định, multi-intent và continuation; không thêm write intent.
6. **Manual evidence:** chạy 30 case evaluation và đối chiếu bộ 104 case theo role.
7. **Rollout:** chỉ bật khi preflight model/config hợp lệ; fallback phải ghi rõ.

### Điều kiện hoàn thành

- Model/tokenizer/pooling đã được xác minh cùng nhau.
- Có calibration/evaluation tách biệt.
- Có threshold/margin và false-positive budget được ghi nhận.
- Model unavailable không âm thầm giả vờ hoạt động.
- Permission, branch, customer ownership, confirmation và service path không bị bypass.
- Chưa claim chất lượng production nếu chưa có runtime evidence.

## 9. Rủi ro và giới hạn

- Chất lượng tiếng Việt, tên món, telex và typo phải đo; không suy ra chỉ từ multilingual claim.
- License MIT không loại bỏ nghĩa vụ provenance/checksum và kiểm tra license artifact đi kèm.
- ONNX Runtime hỗ trợ C# nhưng model export/tokenizer/pooling cần xác minh riêng.
- CPU/RAM/latency, cold start và concurrent request chưa được đo; chưa kết luận server đủ tài nguyên.
- Threshold thấp có thể làm tăng false positive ngoài phạm vi.
- Embedding không bảo đảm hiểu phủ định, không cấp quyền, không xác minh ownership và không thay thế application service.
- Câu mẫu lệch actor hoặc quá ít sẽ làm calibration sai.
- History client vẫn là dữ liệu không tin cậy; không dùng embedding để biến history thành authoritative state.

## 10. Tài liệu tham chiếu

- Source: `services/api/src/AI/Services/MockAiProvider.cs`, `MockAiIntentRecognizer.cs`, `AiOrchestrator.cs`, `AiController.cs`, `AiToolRegistry.cs`, `AiPermissionService.cs`.
- Audit: [MOCK-AI-REVIEW.md](MOCK-AI-REVIEW.md), [AI-QUESTION-SETS-COMPARISON.md](AI-QUESTION-SETS-COMPARISON.md).
- Test chính: `ba-test-frontend/ai-chat/` và `ba-test-frontend/ky-thuat-runtime/README.md`.
- Runtime: [ONNX Runtime C#](https://onnxruntime.ai/docs/get-started/with-csharp), [C# tutorial](https://onnxruntime.ai/docs/tutorials/csharp/basic_csharp.html).
- Model: [intfloat/multilingual-e5-small](https://huggingface.co/intfloat/multilingual-e5-small) và [tokenizer config](https://huggingface.co/intfloat/multilingual-e5-small/blob/main/tokenizer_config.json).

Trạng thái: **Sentence embedding là hướng đã chọn; thiết kế đang được đánh giá, chưa tích hợp.**
