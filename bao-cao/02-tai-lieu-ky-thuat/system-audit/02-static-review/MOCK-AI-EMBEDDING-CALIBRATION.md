# Mock AI embedding adapter và calibration

## TẠM DỪNG — kiểm kê artifact và bàn giao 2026-10-15

Theo yêu cầu người dùng, dừng phép so sánh model ở bước chuẩn bị. **Không có kết quả E5–MiniLM. Không thực hiện tải mới/inference/benchmark hoặc thay đổi kỹ thuật khi bàn giao.** Gửi Ctrl+C đúng session tải71907 của phiên; tool trả exit0 cùng đủ8 file/checksum: download đã hoàn tất trước tín hiệu, không còn session đó chạy. Không dừng API/process khác, không tự xóa model/cache/file tải dở.

### Kiểm kê thực tế khi tạm dừng

Base path mọi file trong bảng: `.tools/mock-ai-embedding-spike/artifacts/`; dùng Get-ChildItem/Get-FileHash SHA256, không mở model bằng inference.

- **E5** `intfloat/multilingual-e5-small`, revision `614241f622f53c4eeff9890bdc4f31cfecc418b3`. O4/tokenizer khớp manifest v2. `onnx/model.onnx` chuẩn có file370,938,763bytes nhưng báo cáo spike từ trước xác định **tải dở**; không coi có hash là hoàn chỉnh, không sử dụng hoặc tự xóa. O4 mới là artifact đã được kiểm chứng trước.
- **Ứng viên duy nhất MiniLM** `sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2`, revision `e8f8c211226b894fcb81acc59f3b34ba3efd5f42`:8file tải xong từ resolve URL pinned revision. O4 size235,166,264/SHA307BBA… khớp metadata LFS chính thức đã tra. Không phát hiện file có tên partial/tmp/incomplete trong thư mục artifact kiểm kê; không khẳng định cache toàn máy sạch.
- [Model card chính thức](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/blob/e8f8c211226b894fcb81acc59f3b34ba3efd5f42/README.md), [pooling config](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/blob/e8f8c211226b894fcb81acc59f3b34ba3efd5f42/1_Pooling/config.json) và API metadata khai Apache-2.0, vi, vector384, mean pooling, max_seq_length128, do_lower_case=false. Usage không thêm query prefix của E5. Đây là **metadata/thiết kế cần xác minh inference**, chưa chứng minh .NET tokenizer/output/pooling tương đương.
- Cũng chỉ xem metadata `onnx/model_quint8_avx2.onnx` của cùng MiniLM:118,453,870bytes; advertisedSHA `98A01D88B7DE996CDEA58C32CA71208C09968D143798814B2EA09D3439DC334F`. **Không tải/chạy int8**, không coi đã so nhiều model/biến thể.
- Chưa tạo project/harness so sánh mới; chỉ có project spike E5 hiện hữu. Chưa load MiniLM/tokenizer vào ONNX, chưa build mới/scoring/benchmark MiniLM. Adapter E5 hiện đòi prefix không rỗng nên không được thêm query cho MiniLM chỉ để qua validation.

| File relative artifacts/ | Bytes thực tế | SHA256 thực tế | Trạng thái |
|---|---:|---|---|
| `onnx/model_O4.onnx` | 235,052,531 | `4654C156F3E4171ABC9C716CDB771BF9116455D15AC1AAB364AEEEDE0E3205B0` | Artifact E5/metadata đã dùng ở spike |
| `onnx/model.onnx` | 370,938,763 | `BF8ADC716255C5ECDFCD7A10039FCCC11EB1F98BCFEE0F9AB6007F99C24702C3` | Tải dở cũ; KHÔNG dùng |
| `onnx/tokenizer.json` | 17,082,730 | `0B44A9D7B51C3C62626640CDA0E2C2F70FDACDC25BBBD68038369D14EBDF4C39` | Artifact E5/metadata đã dùng ở spike |
| `onnx/tokenizer_config.json` | 443 | `A1D6BC8734A6F635DC158508BEF000F8E2E5A759C7D92F984B2C86E5FF53425B` | Artifact E5/metadata đã dùng ở spike |
| `onnx/config.json` | 653 | `BBB7C1333FC4B3E27FBC9CD5D2070AABCC1D4DFB99917C3633E772F97545A6B6` | Artifact E5/metadata đã dùng ở spike |
| `1_Pooling/config.json` | 200 | `987F7A67A38FA564C849BB5D277C52AB9088A84368FC0BE31A354125AEBB12A0` | Artifact E5/metadata đã dùng ở spike |
| `modules.json` | 387 | `C6E29747481E8B5DD2B58401966AEAC910DE39092F90CDA9A704B1545F902B04` | Artifact E5/metadata đã dùng ở spike |
| `minilm/onnx__model_O4.onnx` | 235,166,264 | `307BBA13F9F5708461169C9F2D633C76C5572919BCC998606B6E5AEA46F05DB4` | Tải hoàn tất; chưa chạy inference |
| `minilm/tokenizer.json` | 9,081,518 | `2C3387BE76557BD40970CEC13153B3BBF80407865484B209E655E5E4729076B8` | Tải hoàn tất; chưa chạy inference |
| `minilm/tokenizer_config.json` | 526 | `5036EA374FFEDD706E3BEF33E2E0D6953CB868EF8A490E76E32BA0FAA37A6B9B` | Tải hoàn tất; chưa chạy inference |
| `minilm/config.json` | 645 | `6300193CB75E01CF80C96DECEF7187DFB33094D97CC1490B7EAD6FF134476E4E` | Tải hoàn tất; chưa chạy inference |
| `minilm/sentence_bert_config.json` | 53 | `70F4448F31320443FE3557CACEA5ABF2DCC4915DDA8C80646BEC9F3BB0AA5A1F` | Tải hoàn tất; chưa chạy inference |
| `minilm/1_Pooling__config.json` | 190 | `4BE450DDE3B0273BB9787637CFBD28FE04A7BA6AB9D36AC48E92B11E350FFC23` | Tải hoàn tất; chưa chạy inference |
| `minilm/modules.json` | 229 | `8F4B264B80206C830BEBBDCAE377E137925650A433B689343A63BDC9B3145460` | Tải hoàn tất; chưa chạy inference |
| `minilm/README.md` | 3,888 | `1E98EA05B0DE579FCAAD3D625B62EA55647142ED674D5F5EBF1440E4BBBB6F23` | Tải hoàn tất; chưa chạy inference |

### Tài nguyên và môi trường quan sát

Máy local Windows11 Home Single Language10.0.26200, Intel i3-1125G4 4core/8logical, RAM8,268,136,448bytes (~7.70GiB). Get-CimInstance trước download ghi freePhysical574,308 rồi460,548KiB (~561→450MiB), freeVirtual3,261,072KiB (~3.11GiB) ở snapshot thứ hai. **Áp lực RAM rõ, không phải số đo model hoặc kết luận có thể deploy.** Không load hai model đồng thời, không tự kill process giải phóng bộ nhớ. HTTP/CIM sandbox ban đầu lỗi; lệnh được cấp quyền đã đọc được metadata/CIM và tải file, không dùng output lỗi làm evidence.

E5 warm median5.422ms và working set~616.8MB là số **lịch sử** của spike trước, không đo lại trong bước chuẩn bị/tạm dừng và không đem so với MiniLM chưa chạy. Nếu phiên sau tiếp tục, cần kiểm tra memory budget mới; thiếu môi trường an toàn thì ghi **BLOCKED** và hướng dẫn làm sau, không giả lập số.

### Mốc thử nghiệm giữ nguyên và công việc chưa làm

- `max-targeted-revision-v2.json` giữ nguyên, SHA256 `4BE05424E8DAC532BAF6E9CD74443B4BFD6D12E586CE1E6392AA93426CD5E8C2`, max/.85/.01; 9 hash source/data trong manifest còn khớp. Model/scorer/parser/guard/catalog không sửa trong lượt chuẩn bị hoặc bàn giao này.
- Kết quả gần nhất: CAL9/11ready (24tổng), SUP6/8 (18), EMB12/17 (30); accepted đúng27/36ready, 0 sai đã chốt quan sát được. E22 unknown. Tập15mới: candidate9/14single-target, ready candidate4/6; accepted3/6ready, sai0; valid blocked3, non-accept8/9. Chi tiết và mọi baseline/profile/bảng trước-sau nằm ở phần lịch sử dưới.
- CAL/SUP/EMB/NEW đã quan sát, **không independent clean**. E22 “Báo cáo bán hàng hôm nay” vẫn chưa chốt KPI summary/revenue-only/hỏi lại; không tự gán nhãn chính thức hoặc lấy counter thô làm điểm.
- Còn routing revenue/menu/orders, CAL23 semantic orders dù loyalty veto, E24 write candidate dù hỏi lại; N03 false multi do tiền cảnh/dấu phẩy, N02/05/14 sai routing; margin chặn E03/05/11/13/15, biên parser tuần chưa kín. Chưa đủ cơ sở tích hợp selector runtime.
- Runtime vẫn MockAiProvider/recognizer hiện tại; không write intent/quyền mới. Source-fix AI/kitchen và bộ104manualcase đã bàn giao riêng; technical/cross-role không cộng vào104. Không chuyển evidence embedding thành PASS API/permission/UI.
- HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`; status31tracked modified+7untracked entries mặc định (directory tính một entry, không tổng -uall). Dirty thuộc AI backend/frontend, role/checklist/coverage/audit/findings/context, .gitignore, spike và báo cáo mới; bảo toàn toàn bộ. Artifact/output/cache/venv/bin/obj ignored; git check-ignore xác nhận2O4/output/binary.
- Build spike Release đã pass0warning/error và scoring/lifecycle checks đã có trước đó. Trong bước chuẩn bị/tạm dừng **không build mới/EF inspection/API/tool/browser/DB/migration/seed**; không runtime PASS, không xác nhận đã apply migration.

### Phiên sau — quyền kiểm chứng và thứ tự

Đọc đầu [WORK-HANDOFF](../../../../chatgpt-context/04-WORK-HANDOFF.md) và [CONTEXT-PACK](../../../../chatgpt-context/CHATGPT-CONTEXT-PACK.md), kiểm tra HEAD/status/hash/files trước lệnh tải/inference. Quyết định tiếp tục đúng **một MiniLM đã có** khi tài nguyên phù hợp; không tải nhiều biến thể hoặc load2model đồng thời. Giữ catalog/data/parser/guard và max cosine; tokenizer/prefix/pooling/length phải đúng từng model. So top-1/second/margin trước guard/threshold, tách parser/guard/tài nguyên; **không áp .85/.01 E5 rồi kết luận MiniLM**, không tự tiếp tục vá catalog/threshold theo từng câu.

UI/chat kiểm tra được bằng tay ghi `ba-test-frontend/` đúng role/chức năng, điều kiện/bước/kỳ vọng, Chưa test; không tạo chat runner. Kỹ thuật được build, EF inspection không đổi DB, inference/scoring/resource/adapter; API/tool/browser chỉ khi cần để xác minh và local/test được xác nhận. Write cần DB/test data an toàn trước, không tự migration/seed/delete hoặc dữ liệu vận hành; thiếu môi trường ghi BLOCKED. Tách embedding component, API/tool/quyền và UI end-to-end, không cross-layer PASS. Lượt bàn giao này **chỉ cập nhật tài liệu**, không runtime/database/migration/AGENTS/AI_RULES/commit/push.

## Đợt sửa có mục tiêu sau EMB — targeted-revision-v2

### Phạm vi, provenance và chẩn đoán trước sửa

EMB-E01..E30 từ đợt này là **dữ liệu phát triển/hồi quy** cùng CAL/SUP, không còn gọi là bộ độc lập. Giữ nguyên byte câu/nhãn/contract và output cũ. HEAD vẫn `38bbd423b963aaf5779d4ae3356f942ff236643b`, working tree dirty đã có nhiều thay đổi ứng dụng/tài liệu; không ghi đè chúng. Các mục phía dưới là lịch sử theo thời điểm, không phải trạng thái mới nhất.

Đã đọc catalog/scorer/pipeline/runner/adapter, registry/permission/recognizer hiện tại, schema tool revenue/summary/ranking và `DashboardService`. Không sửa runtime. Bằng chứng trước sửa lấy từ `evaluation-EMB-E01-E30-frozen-v1.json` và `threshold-T2-cal/sup.json` (.85/.01), không dùng số .85/.02 làm baseline.

| Case và nhãn giữ nguyên | Top-1 trước sửa / mẫu gần nhất / score | Intent khác thứ hai / mẫu / score; margin | Chẩn đoán và xử lý tối thiểu đã chốt trước chạy |
|---|---|---|---|
| E02 revenue: hỏi số tiền một kỳ, không KPI | SUMMARY / CAT-SUM-03 / .912692 | REVENUE / CAT-REV-03 / .911483; .001209 | CAT-SUM-03 “Tình hình bán hàng gần đây ra sao?” quá rộng, không có KPI; revenue thiếu mẫu “doanh số bán hàng” rõ monetary. Là catalog cạnh tranh + embedding gần nhau, không sai nhãn. |
| E03 revenue: “doanh số” là số tiền, không ca làm | MY_SHIFT / CAT-SHIFT-02 / .930056 | REVENUE / CAT-REV-02 / .925908; .004149 | SHIFT-02 “Cho tôi biết lịch ca của tôi hôm nay.” chia sẻ lời dẫn/ngày với câu hỏi; max bị chi phối một mẫu (SHIFT-01 chỉ .817713). Không có bằng chứng bug công thức cosine; làm rõ mẫu shift bằng giờ ca, bổ sung revenue-domain, không luật sửa riêng câu. |
| E05 comparison: hỏi tăng/giảm giữa hai ngày | SUMMARY / CAT-SUM-03 / .909043 | COMPARISON / CAT-CMP-01 / .908886; .000157 | Summary quá rộng; mẫu comparison chủ yếu mệnh lệnh so sánh/đối chiếu, thiếu câu tăng/giảm. “không?” cuối là tiểu từ hỏi, không phủ định. Không đổi taxonomy, chỉ làm rõ catalog. |
| E28 revenue: tổng tiền toàn chuỗi, thiếu kỳ | COMPARISON / CAT-CMP-03 / .924423 | SUMMARY / CAT-SUM-02 / .915348; .009075; REVENUE hạng 3 .903624 | CMP-03 “Đặt doanh thu hai ngày cạnh nhau cho tôi.” nặng lời dẫn; revenue thiếu dạng aggregate. Thay mẫu comparison bằng chênh lệch hai kỳ, thêm mẫu revenue cộng gộp. Nhận diện scope khác permission; không lấy branch từ text hoặc giả lập quyền. |
| E08 ranking/week | RANKING .959422 | MENU .887170; .072252 | Parser `Periods` chỉ có “tuần này”, bỏ sót “trong tuần” vốn đã được catalog CAT-RANK-01 và contract EMB định nghĩa là week hiện tại. Thêm alias có chặn hậu tố trước/sau/tới/vừa/qua/số. Đây chưa là parser mọi định ngữ tuần; xem giới hạn bên dưới. |
| E11 summary/this_month | SUMMARY .918084 | REVENUE .908356; .009728 | “tháng hiện tại” đồng nghĩa this_month, thiếu alias. Sửa parser không thay score: sau sửa vẫn có thể bị margin .01. |
| E24 read ranking/menu có filter, thiếu kỳ | WRITE / CAT-WRITE-01 .918933 | RANKING / CAT-RANK-04 .893072; .025860 | Không có guard Contains(phở) hoặc guard từ đơn gây lỗi: embedding kéo về mẫu đổi giá phở; scorer Unsupported rồi pipeline trả CapabilityNotSupported. Thêm **kiểm tra bằng chứng mutation** chỉ khi candidate WRITE: nếu không có động từ+đối tượng ghi rõ thì hỏi lại; không chọn runner-up, không tuyên bố candidate đã đúng. |
| CAL-23 loyalty người khác, chưa có capability | MY_ORDERS .922414 | UNSUPPORTED .909114; .013300 | Semantic sai chức năng, không phải ownership là nguyên nhân. Không có loyalty tool. Veto capability theo cụm domain loyalty/điểm thưởng/thành viên/tích lũy trước subject guard; giữ nguyên candidate để không che lỗi model. |

**Thay đổi catalog riêng** `IntentCatalog.TargetedRevision`: giữ `Examples` baseline và experimental profile cũ nguyên vẹn; revision thay text CAT-SUM-03 bằng KPI đa chỉ số, CAT-SHIFT-02 bằng lịch ca có giờ bắt đầu/kết thúc, CAT-CMP-03 bằng chênh lệch hai kỳ. Thêm đúng 3 mẫu: CAT-REV-R2-01 doanh số monetary, CAT-REV-R2-02 doanh thu aggregate, CAT-CMP-R2-01 tiền tăng/giảm qua hai kỳ. Không chép câu CAL/SUP/EMB hoặc thêm intent/tool. 37 retained /35 competing /9 label; max cosine không thay. Bản text cũ vẫn trong `Examples`, phiên bản mẫu phân biệt bằng profile.

**Guard** `IntentDecisionPipeline.DecideAsync`: loyalty là capability veto (cụm từ có nghĩa domain, không nguyên câu), `Capability=Unsupported`, tool=null, BackendAuthorizationRequired=false; candidate vẫn raw. Chỉ nhìn mệnh đề dương sau tách phủ định; không lấy quyền/identity từ câu. Write check cần mẫu hành động+đối tượng (đổi/sửa/cập nhật/tăng/giảm giá hoặc trạng thái; xóa/hủy đơn/món/nhân viên), không một từ “không/phở/giá” đơn lẻ. Nếu embedding WRITE nhưng không có bằng chứng thì Clarification/UnconfirmedMutationIntent, không mở quyền ghi. Đây là guard bảo thủ hẹp, **không phải parser mọi lệnh ghi hoặc bộ thay embedding bằng luật**. Những cách viết khác vẫn có thể hỏi lại/thất bại; không tuyên bố đã xử lý tổng quát.

**Thời gian:** parser chỉ trả symbolic period, không tự tính ngày. Ranking week và summary this_month vẫn để tool dùng SE Asia Standard Time và tính ranh giới server; week tool đang bắt đầu Chủ nhật, không tự đổi sang Thứ hai. Revenue this_month ánh xạ `month`. Không tự suy “ngày trước”, kỳ tương lai hay ID. Không sửa service/timezone.

### E22 — contract source và câu hỏi cần quyết định

“Báo cáo bán hàng hôm nay.” không nêu chỉ số. `GetRevenueTool.GetSchema/ExecuteAsync` nhận today/month/year, gọi `DashboardService.GetRevenueByPeriodAsync`, trả một giá trị Revenue. `GetBusinessSummaryTool.GetSchema/ExecuteAsync` nhận period day/week/month, gọi `DashboardService.GetBusinessSummaryAsync`, trả revenue, order count, AOV, expenses, COGS/profit ước tính, top products; ước tính dùng cost hiện tại chứ không tự thành lợi nhuận lịch sử chính xác. Cả hai đủ thời gian today nhưng khác nội dung. Schema không có quy tắc quy mọi “báo cáo bán hàng” về một tool; recognizer hiện tại cũng không là quyết định sản phẩm để đổi label nguồn policy-dependent.

**Cần chủ dự án chốt:** câu không chỉ rõ chỉ số như E22 mặc định tổng quan KPI, chỉ doanh thu, hay phải hỏi lại chọn hai loại? Chưa tự chốt, không đổi nhãn/runtime. Giữ E22 unknown khi tổng hợp. Nếu candidate COMPARISON đòi hai kỳ thì ghi định tuyến bất hợp lý, không dùng ambiguity để tính đúng.

### Đóng băng và tập kiểm tra mới — ghi trước inference tập mới

Bản sửa đã build Release thành công 0 warning/error, đóng băng `profiles/max-targeted-revision-v2.json` trước chạy dữ liệu mới. Giữ model/tokenizer hiện có, scorer/adapter không sửa, .85/.01; không điều chỉnh thêm sau hồi quy. Manifest v1 giữ nguyên byte như bằng chứng lịch sử, nhưng source pipeline hiện đã là v2 nên **không dùng binary hiện tại để tuyên bố tái lập v1**. Muốn tái lập v1 phải phục hồi source snapshot riêng có kiểm soát, không ghi đè working tree này. CLI default catalog full-baseline và .85/.02 không được đổi; profile mới cần explicit flags, runtime không đăng ký.

Tập mới [check-targeted-v2.json](../../../../.tools/mock-ai-embedding-spike/check-targeted-v2.json): **15** câu/nhãn/args/hành vi dưới đây chốt trước chạy; 6 ready (2 mỗi menu/revenue/orders), 9 không ready. Không phải đánh giá độc lập bên ngoài: cùng quy trình biết lỗi cũ chuẩn bị, bộ nhỏ chẩn đoán, không điều chỉnh sau lần chạy.

| ID | Actor | Câu nhập chốt trước chạy | Expected intent | Expected args | Expected decision / lý do |
|---|---|---|---|---|---|
| NEW-V2-01 | Guest | Trước khi chọn bữa, tôi có thể tham khảo bảng món kèm giá ở đâu? | MENU_QUERY | {} | Accepted; menu/giá chung; guest gate do backend kiểm tra |
| NEW-V2-02 | Customer | Liệt kê giúp những đồ ăn quán đang phục vụ để tôi lựa chọn. | MENU_QUERY | {} | Accepted; Menu chung, không phải ranking |
| NEW-V2-03 | Owner | Chốt ngày hôm nay, doanh thu ghi nhận tổng cộng là mấy đồng? | REVENUE_QUERY | {"period":"today"} | Accepted; Doanh thu một kỳ today |
| NEW-V2-04 | Owner | Trong tháng này cửa hàng đạt doanh thu bao nhiêu tiền? | REVENUE_QUERY | {"period":"month"} | Accepted; month của tool revenue, không truyền this_month |
| NEW-V2-05 | Customer | Tôi cần tra cứu lịch sử các lần đặt hàng bằng tài khoản đang đăng nhập. | MY_ORDERS | {} | Accepted; Self orders, identity backend |
| NEW-V2-06 | Customer | Những đơn mua trước của chính tôi có thể xem lại được chứ? | MY_ORDERS | {} | Accepted; Self orders; không suy customer ID |
| NEW-V2-07 | Owner | Nhà bếp còn bao nhiêu ki-lô thịt bò trong kho? | UNSUPPORTED_INTENT | null | Unsupported; Inventory chưa có tool; không khôi phục kitchen AI |
| NEW-V2-08 | Customer | Tài khoản hội viên này có số dư điểm thưởng bao nhiêu? | UNSUPPORTED_INTENT | null | Unsupported; Không có loyalty capability, không dùng orders |
| NEW-V2-09 | Guest | Tra số điểm tích lũy gắn với hồ sơ của một người khác. | UNSUPPORTED_INTENT | null | Unsupported; Loyalty unsupported trước ownership, không self-order |
| NEW-V2-10 | Owner | Chớ lấy số liệu doanh thu tháng này. | REVENUE_QUERY | null | Unsupported; Phủ định hành động; không tool |
| NEW-V2-11 | Customer | Tôi cần danh mục đồ ăn, đồng thời tra lại những đơn đã mua. | AMBIGUOUS_MULTI_INTENT | null | ClarificationRequired; Hai capability menu/orders; không chọn một tool |
| NEW-V2-12 | Owner | Cửa hàng có doanh thu bao nhiêu tiền vậy? | REVENUE_QUERY | null | ClarificationRequired; Thiếu period, không mặc định hôm nay |
| NEW-V2-13 | Owner | Cập nhật giá bán của trà đào sang mức 35 nghìn. | WRITE_INTENT_UNSUPPORTED | null | Unsupported; Write vẫn chưa hỗ trợ; không mutation/confirmation |
| NEW-V2-14 | Customer | Tra cứu các đơn đã mua bằng tài khoản của khách hàng khác. | MY_ORDERS | null | ClarificationRequired; Giữ target other; không chuyển yêu cầu sang self |
| NEW-V2-15 | Owner | Doanh thu của ngày mai sẽ được ghi là bao nhiêu? | REVENUE_QUERY | null | ClarificationRequired; Kỳ tương lai không hỗ trợ; không suy đoán doanh thu |

Đối chiếu trước inference với toàn bộ câu mẫu active/baseline/experimental và CAL/SUP/EMB: NFC/lowercase, bỏ dấu câu/whitespace, Dice token-set ≥.8 để sàng lọc; **không exact/near ≥.8**. Rà nội dung/cấu trúc thủ công: N04/N12 vẫn cùng dạng hỏi số tiền (chung lời hỏi bao nhiêu và slot thời gian), N06 cùng chức năng xem lại đơn nhưng đảo cấu trúc, N09/N14 giống schema action+target-other, N10 cùng họ câu phủ định động từ. Đây là overlap cấu trúc cần công khai, không coi chỉ đổi từ/ngày là bằng chứng độc lập mạnh; không âm thầm thay câu hoặc loại câu để đẹp kết quả. N01 câu tìm nơi tham khảo trước chọn bữa, N03 ngữ cảnh chốt ngày, N05 lịch sử theo phiên đăng nhập, N07 đo lượng nguyên liệu, N11 cần hai capability bằng “đồng thời” là dạng câu khác lời dẫn cũ nhưng vẫn chỉ kiểm tra các capability đã biết. Chưa sử dụng embedding score để chọn/loại dữ liệu mới.

| ID | Câu nguồn gần nhất theo Dice (kể cả profile không active) | Dice lớn nhất |
|---|---|---:|
| NEW-V2-01 | CAT-MENU-EXP-02 | 0.3200 |
| NEW-V2-02 | CAT-MENU-EXP-02 | 0.4167 |
| NEW-V2-03 | CAT-REV-01 | 0.4348 |
| NEW-V2-04 | CAT-REV-03 | 0.6000 |
| NEW-V2-05 | EMB-E17 | 0.3200 |
| NEW-V2-06 | EMB-E16 | 0.5000 |
| NEW-V2-07 | EMB-E02 | 0.2857 |
| NEW-V2-08 | CAT-UNSUP-05 | 0.3636 |
| NEW-V2-09 | CAT-UNSUP-03 | 0.5217 |
| NEW-V2-10 | CAL-22 | 0.4286 |
| NEW-V2-11 | EMB-E16 | 0.4762 |
| NEW-V2-12 | CAT-REV-03 | 0.6667 |
| NEW-V2-13 | CAL-10 | 0.2857 |
| NEW-V2-14 | EMB-E17 | 0.5455 |
| NEW-V2-15 | CAT-REV-01 | 0.5714 |

Kết quả hồi quy và tập mới sẽ ghi tiếp bên dưới; không sửa tiếp catalog/parser/guard sau khi xem tập mới.

### Kết quả hồi quy sau đúng một bản sửa

Giữ fixed .85/.01 và max. Baseline là output T2 cũ và EMB v1; output sau sửa riêng `artifacts/targeted-v2-cal.json`, `targeted-v2-sup.json`, `targeted-v2-emb.json`; không ghi đè output cũ. Cả ba process exit 0. Tất cả đều dữ liệu đã quan sát/dùng phát triển.

| Tập | Ready / tổng | Candidate đúng trên ready trước → sau | Candidate đúng / có một target trước → sau | Accepted đúng trước → sau | Accepted sai trước → sau | Ready bị chặn trước → sau | Non-accept đúng trước → sau | Unknown |
|---|---|---|---|---|---|---|---|---|
| CAL | 11/24 | 10/11 → 10/11 | 18/21 → 18/21 | 8 → 9 | 0 → 0 | 3 → 2 | 12 → 13 | 0 |
| SUP | 8/18 | 7/8 → 7/8 | 13/16 → 13/16 | 6 → 6 | 0 → 0 | 2 → 2 | 10 → 10 | 0 |
| EMB | 17/30 | 14/17 → 17/17 | 21/26 → 24/26 | 10 → 12 | 0 → 0 | 7 → 5 | 11 → 12 | 1 (E22) |

Mẫu số semantic: CAL 21 (trừ multi 15/22 và context17), SUP16 (trừ context09/multi12), EMB26 theo contract cũ. Không coi các nhãn ambiguous/context là phải chọn một tool. E22 unknown giữ nguyên; raw runner thay FalseAccept=1 v1 thành DecisionMismatch=1 v2 vì `PolicyUnresolved` không phải enum, không phải lỗi nghiệp vụ mới được chốt.

- Tổng ready 36: accepted đúng **24→27/36**, coverage 66.7%→75.0%; trên 72 câu là 24→27/72. Quan sát 0 accepted sai ở hai phiên bản; trước có 1 accepted unknown E22, sau E22 không accepted. Không coi 27/27 accepted assessable là accuracy ngoài mẫu hoặc chứng minh an toàn production.
- Candidate ready đúng **31→34/36**. Ba cải thiện E02/E03/E05; E28 vẫn sai (comparison→summary, không phải đã sửa). CAL-21 và SUP-07 vẫn sai revenue; CAL-10 menu→ranking, SUP-02 comparison→revenue vẫn chưa hết.
- Không câu đã accepted đúng trước bị mất accepted đúng sau trên ba tập. Thay đổi quyết định hữu ích: CAL-02 hỏi lại→accepted; E02/E08→accepted; CAL-23 scope clarification sai→loyalty unsupported đúng; E24 từ chối write sai→hỏi lại. Đây là **cải thiện trên dữ liệu phát triển**, không bằng chứng tổng quát hóa.
- Parser args trên 11 EMB ready có kỳ rõ: **7/11→11/11**. Conditional khi candidate đúng trước 6/8, sau11/11; phần tăng gồm routing đúng chứ không chỉ parser. E08 week đã được nhận; E11 this_month đã lấy đúng nhưng còn margin .009728 nên hỏi lại. E03 today/E05 cặp kỳ đã lấy được nhưng margin .002334/.008776 chưa đủ.
- CAL-23 candidate vẫn **MY_ORDERS .922414**, UNSUPPORTED .909114, margin .013300; tool=null, Capability Unsupported, BackendAuthorizationRequired=false, reason LoyaltyCapabilityNotSupported. **Đã sửa quyết định capability, chưa sửa semantic.** SUP13/14 cũng từ chối loyalty 2/2; không dùng ownership để che lỗi.
- Write CAL19/20, SUP16, EMB26/30 giữ không tool/mutation đúng 5/5; trong đó EMB26 là phủ định. E24 vẫn candidate WRITE .918933, nhưng thiếu evidence mutation nên hỏi lại; không tuyên bố chuyển được ranking/filter. Không mở intent/tool ghi.
- E22 mới chọn COMPARISON .924386, SUMMARY .908652, margin .015733; hỏi MissingOrUnsupportedComparisonPeriods dù today đã rõ. Nghiệp vụ chưa chốt revenue/summary, nhưng **comparison là ứng viên bất hợp lý** vì không yêu cầu hai kỳ; ghi rõ không tính vào đúng.
- Ready còn bị chặn: CAL10/11; SUP02/18; EMB03/05/11/13/15. E13/E15 menu margin vẫn thấp. Không sửa tiếp để đẩy coverage.

#### Hồi quy CAL — đủ 24 ID

| ID | Candidate/score/margin trước | Quyết định/reason trước | Candidate/score/margin sau | Quyết định/reason sau | Arguments sau |
|---|---|---|---|---|---|
| CAL-01 | REVENUE_QUERY / 0.894166 / 0.033416 | Accepted; ReadyForBackendChecks | REVENUE_QUERY / 0.894166 / 0.033416 | Accepted; ReadyForBackendChecks | `{"period":"today"}` |
| CAL-02 | REVENUE_QUERY / 0.915022 / 0.005386 | ClarificationRequired; CloseCandidates | REVENUE_QUERY / 0.930460 / 0.035092 | Accepted; ReadyForBackendChecks | `{"period":"today"}` |
| CAL-03 | REVENUE_COMPARISON / 0.936564 / 0.041123 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | REVENUE_COMPARISON / 0.936564 / 0.034792 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | `{}` |
| CAL-04 | REVENUE_COMPARISON / 0.938221 / 0.033820 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | REVENUE_COMPARISON / 0.938221 / 0.033820 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | `{}` |
| CAL-05 | PRODUCT_RANKING / 0.962126 / 0.070923 | Accepted; ReadyForBackendChecks | PRODUCT_RANKING / 0.962126 / 0.070923 | Accepted; ReadyForBackendChecks | `{"period":"week"}` |
| CAL-06 | PRODUCT_RANKING / 0.937267 / 0.028773 | ClarificationRequired; MissingOrAmbiguousPeriod | PRODUCT_RANKING / 0.937267 / 0.028773 | ClarificationRequired; MissingOrAmbiguousPeriod | `{}` |
| CAL-07 | BUSINESS_SUMMARY / 0.918458 / 0.044481 | Accepted; ReadyForBackendChecks | BUSINESS_SUMMARY / 0.918458 / 0.044481 | Accepted; ReadyForBackendChecks | `{"period":"this_month"}` |
| CAL-08 | BUSINESS_SUMMARY / 0.932428 / 0.036599 | ClarificationRequired; MissingOrAmbiguousPeriod | BUSINESS_SUMMARY / 0.932428 / 0.026149 | ClarificationRequired; MissingOrAmbiguousPeriod | `{}` |
| CAL-09 | MENU_QUERY / 0.913195 / 0.048679 | Accepted; ReadyForBackendChecks | MENU_QUERY / 0.913195 / 0.048679 | Accepted; ReadyForBackendChecks | `{}` |
| CAL-10 | PRODUCT_RANKING / 0.924186 / 0.003119 | ClarificationRequired; MissingOrAmbiguousPeriod | PRODUCT_RANKING / 0.924186 / 0.003119 | ClarificationRequired; MissingOrAmbiguousPeriod | `{}` |
| CAL-11 | MY_ORDERS / 0.915194 / 0.008524 | ClarificationRequired; CloseCandidates | MY_ORDERS / 0.915194 / 0.008524 | ClarificationRequired; CloseCandidates | `{}` |
| CAL-12 | MY_ORDERS / 0.895600 / 0.038137 | Accepted; ReadyForBackendChecks | MY_ORDERS / 0.895600 / 0.038137 | Accepted; ReadyForBackendChecks | `{}` |
| CAL-13 | MY_SHIFT / 0.877347 / 0.015414 | Accepted; ReadyForBackendChecks | MY_SHIFT / 0.898809 / 0.036877 | Accepted; ReadyForBackendChecks | `{}` |
| CAL-14 | MY_SHIFT / 0.887396 / 0.071103 | Accepted; ReadyForBackendChecks | MY_SHIFT / 0.887396 / 0.071103 | Accepted; ReadyForBackendChecks | `{}` |
| CAL-15 | REVENUE_QUERY / 0.927013 / 0.012196 | ClarificationRequired; MultipleIntents | REVENUE_QUERY / 0.927013 / 0.012196 | ClarificationRequired; MultipleIntents | `{}` |
| CAL-16 | MENU_QUERY / 0.912702 / 0.052874 | Accepted; ReadyForBackendChecks | MENU_QUERY / 0.912702 / 0.052874 | Accepted; ReadyForBackendChecks | `{}` |
| CAL-17 | MY_SHIFT / 0.886462 / 0.002481 | ClarificationRequired; MissingHistory | REVENUE_COMPARISON / 0.883981 / 0.003644 | ClarificationRequired; MissingHistory | `{}` |
| CAL-18 | UNSUPPORTED_INTENT / 0.949023 / 0.065213 | Unsupported; CapabilityNotSupported | UNSUPPORTED_INTENT / 0.949023 / 0.065213 | Unsupported; CapabilityNotSupported | `{}` |
| CAL-19 | WRITE_INTENT_UNSUPPORTED / 0.930183 / 0.032463 | Unsupported; CapabilityNotSupported | WRITE_INTENT_UNSUPPORTED / 0.930183 / 0.032463 | Unsupported; CapabilityNotSupported | `{}` |
| CAL-20 | WRITE_INTENT_UNSUPPORTED / 0.968191 / 0.059149 | Unsupported; CapabilityNotSupported | WRITE_INTENT_UNSUPPORTED / 0.968191 / 0.059149 | Unsupported; CapabilityNotSupported | `{}` |
| CAL-21 | REVENUE_COMPARISON / 0.910197 / 0.020191 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | REVENUE_COMPARISON / 0.910197 / 0.020191 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | `{}` |
| CAL-22 | REVENUE_COMPARISON / 0.903261 / 0.011722 | ClarificationRequired; MultipleIntents | REVENUE_COMPARISON / 0.903261 / 0.011722 | ClarificationRequired; MultipleIntents | `{}` |
| CAL-23 | MY_ORDERS / 0.922414 / 0.013300 | ClarificationRequired; SubjectScopeMismatch | MY_ORDERS / 0.922414 / 0.013300 | Unsupported; LoyaltyCapabilityNotSupported | `{}` |
| CAL-24 | UNSUPPORTED_INTENT / 0.922288 / 0.036978 | Unsupported; CapabilityNotSupported | UNSUPPORTED_INTENT / 0.922288 / 0.036978 | Unsupported; CapabilityNotSupported | `{}` |

#### Hồi quy SUP — đủ 18 ID

| ID | Candidate/score/margin trước | Quyết định/reason trước | Candidate/score/margin sau | Quyết định/reason sau | Arguments sau |
|---|---|---|---|---|---|
| SUP-01 | REVENUE_QUERY / 0.918053 / 0.013496 | Accepted; ReadyForBackendChecks | REVENUE_QUERY / 0.918053 / 0.013496 | Accepted; ReadyForBackendChecks | `{"period":"today"}` |
| SUP-02 | REVENUE_QUERY / 0.912194 / 0.015595 | ClarificationRequired; MissingOrAmbiguousPeriod | REVENUE_QUERY / 0.912194 / 0.013478 | ClarificationRequired; MissingOrAmbiguousPeriod | `{}` |
| SUP-03 | MENU_QUERY / 0.933862 / 0.018300 | Accepted; ReadyForBackendChecks | MENU_QUERY / 0.933862 / 0.018300 | Accepted; ReadyForBackendChecks | `{}` |
| SUP-04 | PRODUCT_RANKING / 0.900206 / 0.027394 | Accepted; ReadyForBackendChecks | PRODUCT_RANKING / 0.900206 / 0.027394 | Accepted; ReadyForBackendChecks | `{"period":"month"}` |
| SUP-05 | BUSINESS_SUMMARY / 0.922407 / 0.018071 | Accepted; ReadyForBackendChecks | BUSINESS_SUMMARY / 0.922407 / 0.021176 | Accepted; ReadyForBackendChecks | `{"period":"this_week"}` |
| SUP-06 | MY_ORDERS / 0.904811 / 0.015332 | Accepted; ReadyForBackendChecks | MY_ORDERS / 0.904811 / 0.015332 | Accepted; ReadyForBackendChecks | `{}` |
| SUP-07 | UNSUPPORTED_INTENT / 0.899475 / 0.003330 | ClarificationRequired; CloseCapabilityCandidates | BUSINESS_SUMMARY / 0.901015 / 0.001541 | ClarificationRequired; MissingOrAmbiguousPeriod | `{}` |
| SUP-08 | REVENUE_COMPARISON / 0.931228 / 0.026134 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | REVENUE_COMPARISON / 0.931228 / 0.026134 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | `{}` |
| SUP-09 | REVENUE_COMPARISON / 0.879400 / 0.008798 | ClarificationRequired; MissingHistory | REVENUE_COMPARISON / 0.879400 / 0.008798 | ClarificationRequired; MissingHistory | `{}` |
| SUP-10 | REVENUE_QUERY / 0.917928 / 0.021411 | Unsupported; NegatedRequest | REVENUE_QUERY / 0.917928 / 0.021411 | Unsupported; NegatedRequest | `{}` |
| SUP-11 | MENU_QUERY / 0.904556 / 0.014507 | Accepted; ReadyForBackendChecks | MENU_QUERY / 0.904556 / 0.014507 | Accepted; ReadyForBackendChecks | `{}` |
| SUP-12 | MENU_QUERY / 0.915441 / 0.004939 | ClarificationRequired; CloseCandidates | MENU_QUERY / 0.915441 / 0.004939 | ClarificationRequired; CloseCandidates | `{}` |
| SUP-13 | UNSUPPORTED_INTENT / 0.887561 / 0.035004 | Unsupported; CapabilityNotSupported | UNSUPPORTED_INTENT / 0.887561 / 0.035004 | Unsupported; LoyaltyCapabilityNotSupported | `{}` |
| SUP-14 | UNSUPPORTED_INTENT / 0.927233 / 0.026037 | Unsupported; CapabilityNotSupported | UNSUPPORTED_INTENT / 0.927233 / 0.026037 | Unsupported; LoyaltyCapabilityNotSupported | `{}` |
| SUP-15 | MY_ORDERS / 0.932828 / 0.028516 | ClarificationRequired; SubjectScopeMismatch | MY_ORDERS / 0.932828 / 0.028516 | ClarificationRequired; SubjectScopeMismatch | `{}` |
| SUP-16 | WRITE_INTENT_UNSUPPORTED / 0.926427 / 0.053201 | Unsupported; CapabilityNotSupported | WRITE_INTENT_UNSUPPORTED / 0.926427 / 0.053201 | Unsupported; CapabilityNotSupported | `{}` |
| SUP-17 | REVENUE_COMPARISON / 0.819668 / 0.011154 | Unsupported; LowSimilarity | REVENUE_COMPARISON / 0.819668 / 0.012495 | Unsupported; LowSimilarity | `{}` |
| SUP-18 | MENU_QUERY / 0.908735 / 0.008525 | ClarificationRequired; CloseCandidates | MENU_QUERY / 0.908735 / 0.008525 | ClarificationRequired; CloseCandidates | `{}` |

#### Hồi quy EMB — đủ 30 ID

| ID | Candidate/score/margin trước | Quyết định/reason trước | Candidate/score/margin sau | Quyết định/reason sau | Arguments sau |
|---|---|---|---|---|---|
| EMB-E01 | REVENUE_QUERY / 0.947170 / 0.020791 | Accepted; ReadyForBackendChecks | REVENUE_QUERY / 0.947170 / 0.020791 | Accepted; ReadyForBackendChecks | `{"period":"today"}` |
| EMB-E02 | BUSINESS_SUMMARY / 0.912692 / 0.001209 | ClarificationRequired; CloseCandidates | REVENUE_QUERY / 0.927486 / 0.028517 | Accepted; ReadyForBackendChecks | `{"period":"today"}` |
| EMB-E03 | MY_SHIFT / 0.930056 / 0.004149 | ClarificationRequired; CloseCandidates | REVENUE_QUERY / 0.925908 / 0.002334 | ClarificationRequired; CloseCandidates | `{"period":"today"}` |
| EMB-E04 | REVENUE_COMPARISON / 1.000000 / 0.097490 | Accepted; ReadyForBackendChecks | REVENUE_COMPARISON / 1.000000 / 0.097490 | Accepted; ReadyForBackendChecks | `{"currentPeriod":"today","comparisonPeriod":"yesterday"}` |
| EMB-E05 | BUSINESS_SUMMARY / 0.909043 / 0.000157 | ClarificationRequired; MissingOrAmbiguousPeriod | REVENUE_COMPARISON / 0.908886 / 0.008776 | ClarificationRequired; CloseCandidates | `{"currentPeriod":"today","comparisonPeriod":"yesterday"}` |
| EMB-E06 | REVENUE_COMPARISON / 0.952328 / 0.054763 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | REVENUE_COMPARISON / 0.952328 / 0.054763 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | `{}` |
| EMB-E07 | PRODUCT_RANKING / 0.968096 / 0.064134 | Accepted; ReadyForBackendChecks | PRODUCT_RANKING / 0.968096 / 0.064134 | Accepted; ReadyForBackendChecks | `{"period":"week"}` |
| EMB-E08 | PRODUCT_RANKING / 0.959422 / 0.072252 | ClarificationRequired; MissingOrAmbiguousPeriod | PRODUCT_RANKING / 0.959422 / 0.072252 | Accepted; ReadyForBackendChecks | `{"period":"week"}` |
| EMB-E09 | PRODUCT_RANKING / 0.909003 / 0.028936 | Accepted; ReadyForBackendChecks | PRODUCT_RANKING / 0.909003 / 0.028936 | Accepted; ReadyForBackendChecks | `{"period":"week"}` |
| EMB-E10 | BUSINESS_SUMMARY / 0.996265 / 0.100365 | Accepted; ReadyForBackendChecks | BUSINESS_SUMMARY / 0.996265 / 0.100365 | Accepted; ReadyForBackendChecks | `{"period":"this_month"}` |
| EMB-E11 | BUSINESS_SUMMARY / 0.918084 / 0.009728 | ClarificationRequired; MissingOrAmbiguousPeriod | BUSINESS_SUMMARY / 0.918084 / 0.009728 | ClarificationRequired; CloseCandidates | `{"period":"this_month"}` |
| EMB-E12 | BUSINESS_SUMMARY / 0.922660 / 0.019421 | Accepted; ReadyForBackendChecks | BUSINESS_SUMMARY / 0.922660 / 0.019421 | Accepted; ReadyForBackendChecks | `{"period":"this_month"}` |
| EMB-E13 | MENU_QUERY / 0.927090 / 0.001999 | ClarificationRequired; CloseCandidates | MENU_QUERY / 0.927090 / 0.001999 | ClarificationRequired; CloseCandidates | `{}` |
| EMB-E14 | MENU_QUERY / 0.914960 / 0.035285 | Accepted; ReadyForBackendChecks | MENU_QUERY / 0.914960 / 0.035285 | Accepted; ReadyForBackendChecks | `{}` |
| EMB-E15 | MENU_QUERY / 0.929125 / 0.006916 | ClarificationRequired; CloseCandidates | MENU_QUERY / 0.929125 / 0.006916 | ClarificationRequired; CloseCandidates | `{}` |
| EMB-E16 | MY_ORDERS / 0.930285 / 0.024532 | Accepted; ReadyForBackendChecks | MY_ORDERS / 0.930285 / 0.024532 | Accepted; ReadyForBackendChecks | `{}` |
| EMB-E17 | MY_ORDERS / 0.923819 / 0.046037 | Accepted; ReadyForBackendChecks | MY_ORDERS / 0.923819 / 0.046037 | Accepted; ReadyForBackendChecks | `{}` |
| EMB-E18 | MY_SHIFT / 0.935931 / 0.092016 | Accepted; ReadyForBackendChecks | MY_SHIFT / 0.935927 / 0.092012 | Accepted; ReadyForBackendChecks | `{}` |
| EMB-E19 | REVENUE_QUERY / 0.905412 / 0.003865 | ClarificationRequired; MissingOrAmbiguousPeriod | REVENUE_QUERY / 0.905412 / 0.003865 | ClarificationRequired; MissingOrAmbiguousPeriod | `{}` |
| EMB-E20 | REVENUE_COMPARISON / 0.940884 / 0.036740 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | REVENUE_COMPARISON / 0.940884 / 0.036740 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | `{}` |
| EMB-E21 | REVENUE_COMPARISON / 0.936042 / 0.021812 | ClarificationRequired; MultipleIntents | REVENUE_COMPARISON / 0.936042 / 0.021812 | ClarificationRequired; MultipleIntents | `{}` |
| EMB-E22 | BUSINESS_SUMMARY / 0.936581 / 0.012196 | Accepted; ReadyForBackendChecks | REVENUE_COMPARISON / 0.924386 / 0.015733 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | `{}` |
| EMB-E23 | REVENUE_COMPARISON / 0.894969 / 0.006145 | ClarificationRequired; MissingHistory | REVENUE_COMPARISON / 0.894969 / 0.006145 | ClarificationRequired; MissingHistory | `{}` |
| EMB-E24 | WRITE_INTENT_UNSUPPORTED / 0.918933 / 0.025860 | Unsupported; CapabilityNotSupported | WRITE_INTENT_UNSUPPORTED / 0.918933 / 0.025860 | ClarificationRequired; UnconfirmedMutationIntent | `{}` |
| EMB-E25 | REVENUE_COMPARISON / 0.915947 / 0.006585 | Unsupported; NegatedRequest | REVENUE_COMPARISON / 0.915173 / 0.005812 | Unsupported; NegatedRequest | `{}` |
| EMB-E26 | WRITE_INTENT_UNSUPPORTED / 0.895700 / 0.015844 | Unsupported; NegatedRequest | WRITE_INTENT_UNSUPPORTED / 0.895700 / 0.015844 | Unsupported; NegatedRequest | `{}` |
| EMB-E27 | UNSUPPORTED_INTENT / 0.970954 / 0.082549 | Unsupported; CapabilityNotSupported | UNSUPPORTED_INTENT / 0.970954 / 0.082549 | Unsupported; CapabilityNotSupported | `{}` |
| EMB-E28 | REVENUE_COMPARISON / 0.924423 / 0.009075 | ClarificationRequired; MissingOrUnsupportedComparisonPeriods | BUSINESS_SUMMARY / 0.915348 / 0.005844 | ClarificationRequired; MissingOrAmbiguousPeriod | `{}` |
| EMB-E29 | MY_ORDERS / 0.952013 / 0.030672 | ClarificationRequired; SubjectScopeMismatch | MY_ORDERS / 0.952013 / 0.030672 | ClarificationRequired; SubjectScopeMismatch | `{}` |
| EMB-E30 | WRITE_INTENT_UNSUPPORTED / 0.967748 / 0.089899 | Unsupported; CapabilityNotSupported | WRITE_INTENT_UNSUPPORTED / 0.967748 / 0.089899 | Unsupported; CapabilityNotSupported | `{}` |

### Kết quả tập mới — chạy đúng một lượt, không chỉnh tiếp

Input SHA256 `FF01913D75125A1588A0761BDB0CD6A636A5DEEDDC540B57A8E93546A13245A2`. Source manifest SHA256 `4BE05424E8DAC532BAF6E9CD74443B4BFD6D12E586CE1E6392AA93426CD5E8C2`. Chạy sau đóng băng, metadata xác nhận targeted-revision-v2/max/.85/.01, 37 samples; exit0. Raw `artifacts/targeted-v2-new-once.json` ignored; không chạy lại hoặc điều chỉnh dựa trên output.

```powershell
# Từ .tools/mock-ai-embedding-spike/
dotnet build -c Release --no-restore
dotnet run --no-build -c Release -- --decision-review --catalog-profile targeted-revision-v2 --scoring max --minimum-score 0.85 --minimum-margin 0.01 --calibration check-targeted-v2.json --review-output artifacts/targeted-v2-new-once.json
# Hồi quy dùng cùng cờ, thay input bằng calibration.json,
# calibration-supplement.json hoặc evaluation-EMB-E01-E30.json; dùng output riêng.
```

Câu/actor/nhãn/args kỳ vọng giữ nguyên ở bảng chốt trước chạy phía trên. Bảng kết quả không sửa label để làm đẹp counter:

| ID | Candidate / mẫu gần nhất | Score | Intent thứ hai / score | Margin | Args thực tế | Quyết định / reason / tool | Đánh giá tách tầng |
|---|---|---:|---|---:|---|---|---|
| NEW-V2-01 | MENU_QUERY / CAT-MENU-05 | 0.920526 | PRODUCT_RANKING / 0.885892 | 0.034634 | `{}` | Accepted; ReadyForBackendChecks; customer_get_menu | Đúng menu, args rỗng; guest gate chưa chạy. |
| NEW-V2-02 | PRODUCT_RANKING / CAT-RANK-04 | 0.910055 | MENU_QUERY / 0.907141 | 0.002914 | `{}` | ClarificationRequired; MissingOrAmbiguousPeriod; không tool | Sai menu→ranking; bị parser đòi period của sai intent. Câu hợp lệ bị chặn. |
| NEW-V2-03 | REVENUE_QUERY / CAT-REV-01 | 0.905020 | REVENUE_COMPARISON / 0.893778 | 0.011242 | `{}` | ClarificationRequired; MultipleIntents; không tool | Candidate đúng, score/margin đủ; guard nhiều ý chặn nhầm tiền cảnh 'Chốt ngày hôm nay' qua dấu phẩy. Parser chưa chạy. |
| NEW-V2-04 | REVENUE_QUERY / CAT-REV-03 | 0.917211 | BUSINESS_SUMMARY / 0.904288 | 0.012922 | `{"period":"month"}` | Accepted; ReadyForBackendChecks; get_revenue | Đúng revenue, period month đúng schema. |
| NEW-V2-05 | UNSUPPORTED_INTENT / CAT-UNSUP-01 | 0.911897 | MY_ORDERS / 0.902529 | 0.009368 | `{}` | ClarificationRequired; CloseCapabilityCandidates; không tool | Sai self-orders→unsupported (CAT-UNSUP-01 inventory); margin/capability hỏi lại, chưa tới ownership. |
| NEW-V2-06 | MY_ORDERS / CAT-ORDER-01 | 0.904802 | WRITE_INTENT_UNSUPPORTED / 0.877515 | 0.027287 | `{}` | Accepted; ReadyForBackendChecks; get_my_orders | Đúng self-orders, identity để backend. |
| NEW-V2-07 | PRODUCT_RANKING / CAT-RANK-04 | 0.845786 | MENU_QUERY / 0.843042 | 0.002743 | `{}` | Unsupported; LowSimilarity; không tool | Candidate sai inventory→ranking nhưng score <.85, từ chối đúng. Không coi intent đúng. |
| NEW-V2-08 | UNSUPPORTED_INTENT / CAT-UNSUP-05 | 0.899301 | MY_ORDERS / 0.863906 | 0.035394 | `{}` | Unsupported; LoyaltyCapabilityNotSupported; không tool | Đúng candidate unsupported, veto loyalty rõ, không orders. |
| NEW-V2-09 | UNSUPPORTED_INTENT / CAT-UNSUP-03 | 0.921176 | REVENUE_COMPARISON / 0.872131 | 0.049045 | `{}` | Unsupported; LoyaltyCapabilityNotSupported; không tool | Đúng unsupported loyalty, giữ other, không orders/ownership excuse. |
| NEW-V2-10 | BUSINESS_SUMMARY / CAT-SUM-01 | 0.911141 | REVENUE_COMPARISON / 0.910291 | 0.000850 | `{}` | Unsupported; NegatedRequest; không tool | Semantic sai revenue→summary nhưng phủ định chặn đúng. |
| NEW-V2-11 | MY_ORDERS / CAT-ORDER-01 | 0.924291 | UNSUPPORTED_INTENT / 0.914913 | 0.009378 | `{}` | ClarificationRequired; MultipleIntents; không tool | Không một gold intent; hỏi tách menu/orders đúng. |
| NEW-V2-12 | REVENUE_QUERY / CAT-REV-03 | 0.913711 | MENU_QUERY / 0.876592 | 0.037119 | `{}` | ClarificationRequired; MissingOrAmbiguousPeriod; không tool | Đúng revenue, thiếu kỳ nên hỏi lại; không default. |
| NEW-V2-13 | WRITE_INTENT_UNSUPPORTED / CAT-WRITE-01 | 0.876248 | PRODUCT_RANKING / 0.856680 | 0.019569 | `{}` | Unsupported; CapabilityNotSupported; không tool | Đúng write unsupported, có bằng chứng cập nhật giá; không ghi. |
| NEW-V2-14 | UNSUPPORTED_INTENT / CAT-UNSUP-03 | 0.925544 | MY_ORDERS / 0.897490 | 0.028054 | `{}` | Unsupported; CapabilityNotSupported; không tool | Sai order-domain→unsupported (CAT-UNSUP-03 ưu đãi khách khác); từ chối sai capability thay vì hỏi scope/đăng nhập; giữ other=true không chữa semantic. |
| NEW-V2-15 | REVENUE_QUERY / CAT-REV-R2-01 | 0.888017 | REVENUE_COMPARISON / 0.885478 | 0.002539 | `{}` | ClarificationRequired; UnsupportedPeriod; không tool | Đúng revenue; UnsupportedPeriod cho ngày mai, không suy số tiền tương lai. |

**Tổng hợp tập mới:**
- Candidate đúng **9/14**, sai **5/14** N02/05/07/10/14; N11 multi-intent không một semantic gold. Trên 6 ready, top-1 đúng **4/6**; không dùng nhãn multi không khớp chuỗi để cộng thêm lỗi model.
- Accepted đúng **3**, sai **0 quan sát được**; accepted correctness **3/3**, ready coverage **3/6 (50%)**, toàn tập **3/15 (20%)**. Ready bị chặn **3/6** N02 (routing), N03 (multi guard false positive), N05 (routing/capability). Không gọi mức này là đạt.
- Non-accept đúng **8/9**; mismatch **1/9 N14**, dù an toàn không gọi tool vẫn sai chức năng/lý do. Không có policy unknown mới; E22 chỉ ở hồi quy.
- Parser kỳ rõ trên ready có 2 câu N03(today)/N04(month): args đúng **1/2** xét cả pipeline; parser **chưa chạy** ở N03 do guard, không ghi thành lỗi regex date. Khi parser thật sự chạy trên ready có kỳ: **1/1** đúng. N12 hỏi thiếu kỳ đúng, N15 báo kỳ không hỗ trợ đúng; không giả định hiểu tất cả thời gian. N01/N02/N05/N06 không cần params, args rỗng; đây không chứng minh đúng intent/permission.
- Menu ready: top-1 1/2, accepted1/2; revenue ready: top-1 2/2, accepted1/2 (guard chặn nhầm); orders ready: top-1 1/2, accepted1/2.
- Actor: Owner 7 câu, ready2 accepted1; Customer6 câu, ready3 accepted1; Guest2 câu, ready1 accepted1. Permission/backend identity chưa thực thi cho mọi actor.
- Loyalty mới N08/N09: candidate unsupported và quyết định từ chối đúng **2/2**, không orders. Read/write: N13 write từ chối đúng, N10 phủ định không gọi tool; vẫn không chứng minh mọi biến thể mutation đều được hiểu.
- Sai routing N05 gần **CAT-UNSUP-01 inventory** .911897 (orders .902529); N14 gần **CAT-UNSUP-03 ưu đãi khách khác** .925544 (orders .897490). Cho thấy template action/subject còn lấn domain, không giải quyết chỉ bằng thêm mẫu cho revenue.
- N03 có toàn câu revenue đúng .905020/margin .011242 nhưng `Regex.Split` dấu phẩy tách tiền cảnh “Chốt ngày hôm nay”; detector clause chỉ cần score floor/tool, không yêu cầu clause là yêu cầu độc lập/margin. Lý do MultipleIntents xác nhận guard false positive; output chưa ghi riêng từng clause score nên không bịa intent của từng clause. N11 thật nhiều ý được hỏi lại đúng. **Chưa sửa cả hai tình huống theo tập mới trong lượt này.**

### Kết luận, rủi ro và bàn giao đợt này

**Chưa đủ cơ sở tích hợp thử embedding làm bộ chọn tool chat, dù có cấu hình bật/tắt.** Bản sửa có lợi có mục tiêu: E02 nhận đúng, E03/E05 candidate đúng, E08/E11 parser đúng alias, E24 hỏi lại thay vì kết luận write, loyalty không chuyển tool orders. Nhưng điểm nghẽn còn:
1. Sai semantic còn ở E28/CAL21/SUP07 và các câu menu/order mới; CAL23 vẫn loyalty→orders raw. Model/catalog/template cạnh tranh chưa phân biệt ổn định; max cosine được giữ, không có bằng chứng lỗi toán scoring.
2. Guard nhiều ý chặn nhầm N03 dù whole-text candidate/margin đủ. Đây là blocker mới có bằng chứng, không thể đổ hết cho threshold.
3. Tập mới ready coverage chỉ3/6; menu và orders chưa bền, E03/E05/E11/E13/E15 còn hỏi lại do margin. Không giảm ngưỡng trong đợt này.
4. E22 contract cần quyết định từ chủ dự án; candidate comparison không hợp lý dù tool không chạy. Veto loyalty và kiểm tra mutation có phạm vi hẹp, không là bộ hiểu ngôn ngữ/phân quyền tổng quát.
5. Ownership/branch/guest gate/tool errors/confirmation/UI/end-to-end chưa kiểm chứng; O4-vs-original-model và hosting RAM vẫn chưa được kết luận từ các kết quả này.
6. Rà source sau đóng băng còn thấy alias “trong tuần” chỉ chặn một danh sách hậu tố: dạng chưa hỗ trợ như “trong tuần thứ 3” có thể khớp tiền tố this_week vì “thứ” chưa bị chặn. Không tuyên bố parser từ chối mọi định ngữ thời gian lạ; cần giới hạn biên alias trước tích hợp. Chưa chạy thêm case hoặc sửa tiếp sau tập mới.

**Bước tiếp theo đề xuất, chưa làm:** chốt E22; review điều kiện “mệnh đề yêu cầu độc lập” của multi guard bằng N03/N11 và review mẫu unsupported bị hút bởi lời dẫn/target trong N05/N14, không bắt đầu bằng tăng mẫu/hạ threshold. Nếu được giao sửa dựa trên tập mới, phải ghi nó trở thành development và đóng băng revision khác trước phép kiểm tra mới. Không tiếp tục vá nhiều vòng trong lượt này.

Kiểm chứng đã làm: build component Release (0 warning/error); scoring CAL24/SUP18/EMB30 và tập mới15 đúng một lượt mỗi tập; hash toàn source/data trong manifest v2 giữ nguyên sau tập mới, baseline data/scorer/adapter/model/tokenizer/profile v1 không đổi. Chỉ sửa C# spike 3 file, thêm manifest v2/input15 và cập nhật README/báo cáo/context; không đổi dependency/cấu hình ứng dụng/API/DB/migration/frontend. Không chat runner, không chạy bộ test tay, không PASS end-to-end, không commit/push/dừng process. EmployeeType/BasicSalary/MockAiProvider không đổi.

## Đánh giá bộ EMB-E01..E30 với ứng viên đóng băng

### Kiểm tra phiên bản và tính độc lập trước chạy

Đã đọc profile thực tế `profiles/max-full-baseline-candidate-v1.json` (SHA256 `F598F036DAEBC6299BD04C155C55E35A90A41FFBD3B4460BD8046DAB86FDE861`). Cả 8 checksum source/data trong manifest, model O4 và tokenizer đều khớp. Sử dụng max/full-baseline/.85/.01 bằng cờ explicit vì manifest không tự load. Không sửa mẫu/nhãn/source quyết định/ngưỡng hoặc baseline mặc định. HEAD vẫn `38bbd423b963aaf5779d4ae3356f942ff236643b`, working tree có thay đổi cũ được bảo toàn.

**Bộ 30 câu không hoàn toàn độc lập.** Kiểm tra trước inference đối chiếu 34 mẫu baseline, 6 mẫu thử nghiệm không được chọn nhưng đã dùng phát triển, CAL 24, SUP 18 và 9 câu smoke/reference của spike:
- 1 câu trùng nguyên văn mẫu active: E04 = CAT-CMP-01.
- 0 câu trùng nguyên văn CAL/SUP.
- 9 câu đã có nguyên văn trong smoke/reference spike: E01/02/04/07/10/13/23/25/27.
- Các near-duplicate rõ được giữ ở bảng dưới; không thay câu hoặc loại lặng khỏi lượt chạy.
- Tổng 12 ID có exact/near overlap theo tiêu chí kiểm tra này; 18 ID chưa phát hiện overlap. Không tự khẳng định 18 câu đó là một bộ độc lập đã được kiểm chứng toàn lịch sử.

Screening từ vựng giữ dấu, lowercase/NFC, bỏ dấu câu/chuẩn hóa whitespace, dùng Dice trên tập token ≥.80 để tìm ứng viên rà thủ công. Đây chỉ là phép kiểm tra provenance, không thêm thuật toán nhận diện. Không coi mọi paraphrase cùng intent là leak. Bổ sung rà thủ công E19 (chỉ thêm từ đệm so với CAL-21) và E30 (cùng món/giá mẫu write, khác cách viết số). Không dựa score embedding của kết quả đánh giá để quyết định case có leak hay không.

| ID | Trùng nguyên văn/chuẩn hóa | Gần như trùng cần tách khi diễn giải |
|---|---|---|
| EMB-E01 | SPIKE-1 (spike-smoke) | Không |
| EMB-E02 | SPIKE-2 (spike-smoke) | CAL-02: gần từ vựng Dice ≥ .80 |
| EMB-E04 | CAT-CMP-01 (baseline); SPIKE-3 (spike-smoke) | Không |
| EMB-E07 | SPIKE-4 (spike-smoke) | Không |
| EMB-E09 | Không | CAL-05: gần từ vựng Dice ≥ .80 |
| EMB-E10 | SPIKE-5 (spike-smoke) | CAT-SUM-01: gần từ vựng Dice ≥ .80 |
| EMB-E13 | SPIKE-6 (spike-smoke) | Không |
| EMB-E19 | Không | CAL-21: Chỉ thêm câu đệm 'thế nào'; cùng thiếu kỳ. |
| EMB-E23 | SPIKE-7 (spike-smoke) | CAL-17: gần từ vựng Dice ≥ .80 |
| EMB-E25 | SPIKE-9 (spike-smoke) | Không |
| EMB-E27 | SPIKE-8 (spike-smoke) | CAT-UNSUP-01: gần từ vựng Dice ≥ .80 |
| EMB-E30 | Không | CAT-WRITE-01: Cùng món phở và giá 50 nghìn; đổi cách viết số/loại câu. |

Nhóm overlap: EMB-E01, EMB-E02, EMB-E04, EMB-E07, EMB-E09, EMB-E10, EMB-E13, EMB-E19, EMB-E23, EMB-E25, EMB-E27, EMB-E30. Nhóm chưa phát hiện trùng: EMB-E03, EMB-E05, EMB-E06, EMB-E08, EMB-E11, EMB-E12, EMB-E14, EMB-E15, EMB-E16, EMB-E17, EMB-E18, EMB-E20, EMB-E21, EMB-E22, EMB-E24, EMB-E26, EMB-E28, EMB-E29. Báo kết quả đủ 30 và phần chưa phát hiện trùng riêng; không gọi toàn bộ kết quả là accuracy độc lập.

### Contract chấm đã ghi trước khi chạy

Nguồn 30 câu: bảng §7 của `MOCK-AI-EMBEDDING-PROPOSAL.md`. File `evaluation-EMB-E01-E30.json` chép nguyên ID/actor/câu/nhãn intent/params/hành vi nguồn; `expectedIntent` giữ nguyên cả nhãn mô tả như “negative revenue”, không thay thành label dễ chấm hơn. Metadata `semanticExpected`, `expectedArguments`, `allowedStatuses` chỉ giải thích contract để tách semantic/decision/permission và được ghi **trước output**:
- 26 câu có một semantic target kiểm được. E21/E23 là multi/context, E22 policy-dependent, E24 ranking/menu: không ép một label để chấm đúng/sai.
- 17 câu E01..E18 trừ E06 kỳ vọng đủ cho offline ready (không đồng nghĩa quyền backend). E06 “ngày trước” cần xác nhận, không đoán yesterday.
- E08 “trong tuần” và E11 “tháng hiện tại” giữ period nguồn là week/this_month; nếu parser hiện tại không lấy được thì ghi lỗi, không đổi kỳ vọng thành thiếu dữ liệu.
- E19/20 thiếu kỳ; E21 nhiều ý; E23 không có state trước; E24 thiếu kỳ và có quantity/product filter chưa được parser hỗ trợ: không được bỏ filter rồi nhận tool.
- E25/26 là phủ định yêu cầu, cần không chuyển tool; giữ semantic domain và chấm riêng quyết định không thực hiện.
- E27/30 capability chưa hỗ trợ; E28 revenue cần backend từ chối quyền, nhưng ở offline còn thiếu period nên không mô phỏng permission; E29 domain order/other target cần không chuyển thành self orders. Permission/ownership thực thi vẫn Chưa kiểm tra.
- E22 nhãn/chính sách nguồn chưa chốt: mọi kết quả được ghi, không ép vào correct/false accept. E24 semantic hai lựa chọn chưa chốt nhưng quyết định an toàn cần hỏi lại, có thể chấm riêng.
- Bộ 30 **không có câu loyalty**. E29 là đơn của khách khác, không phải loyalty; kết quả không chứng minh CAL-23 đã được sửa. Không thêm câu 31 hoặc chạy lại CAL-23 trong đợt này.

Runner có counter khớp chuỗi nhãn; với các nhãn mô tả/ambiguous, counter đó không phản ánh semantic target và không được dùng như báo cáo cuối. Tổng hợp review sẽ dùng contract ở trên, giữ provenance nhãn nguồn và tách unknowns. Lượt này chỉ chạy scoring thành phần đúng 30 câu, không tool/API/database/browser hoặc chat runner.



### Kết quả chạy đóng băng — không hiệu chỉnh sau quan sát

Đã chạy **một lượt đúng 30 case** bằng `DecisionReviewRunner.RunAsync` hiện có, exit 0; không chạy CAL/SUP hoặc thêm case loyalty. Input [evaluation-EMB-E01-E30.json](../../../../.tools/mock-ai-embedding-spike/evaluation-EMB-E01-E30.json), SHA256 `2F634D54F2B7A5666A350A62043632E3810566000865B9EF0A688C01AB5DC08E`. Raw output local ignored: `.tools/mock-ai-embedding-spike/artifacts/evaluation-EMB-E01-E30-frozen-v1.json`. Bảng dưới lưu đủ kết quả ngay trong tài liệu, không phụ thuộc output ignored để xem bằng chứng.

Chạy lại từ thư mục spike (manifest không tự load):

```powershell
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --minimum-score 0.85 --minimum-margin 0.01 --calibration evaluation-EMB-E01-E30.json --review-output artifacts/evaluation-EMB-E01-E30-frozen-v1.json
```

Giữ nguyên bản binary Release hiện có, không thay source hoặc build ở lượt này. Metadata output xác nhận catalogCount=34, max, .85/.01; cache vector 52,224 bytes. Max trên từng intent, second là **intent khác**. Score/margin dưới đây làm tròn 6 chữ số, không phải xác suất. “Trước → cuối” là status của scorer rồi pipeline; scorer Unsupported còn bao gồm nhãn capability không có tool, không chỉ score floor. Candidate và semantic toàn câu không đổi trong 30 case này (không có câu pha mệnh đề phủ định cần re-embed). Accepted nghĩa là đề xuất chuyển backend kiểm tra, **không gọi tool**.

#### Bảng từng case

| ID | Actor | Câu hỏi | Intent mong đợi nguyên nguồn | Candidate | Score | Intent thứ hai / score | Margin | Tham số thực tế | Trước → quyết định cuối / reason / tool dự kiến | Đúng/sai và lý do |
|---|---|---|---|---|---:|---|---:|---|---|---|
| EMB-E01 | Owner | Hôm nay doanh thu được bao nhiêu? | REVENUE_QUERY | REVENUE_QUERY | 0.947170 | REVENUE_COMPARISON / 0.926378 | 0.020791 | `{"period":"today"}` | Accepted → Accepted; ReadyForBackendChecks; get_revenue | Đúng intent/args; chỉ ready, chưa thực thi. |
| EMB-E02 | Owner | Tiền bán hàng trong ngày hiện tại là bao nhiêu? | REVENUE_QUERY | BUSINESS_SUMMARY | 0.912692 | REVENUE_QUERY / 0.911483 | 0.001209 | `{"period":"today"}` | ClarificationRequired → ClarificationRequired; CloseCandidates; không tool | Sai intent revenue→summary; period today lấy được, margin chặn. |
| EMB-E03 | Owner | Cho tôi biết doanh số của hôm nay. | REVENUE_QUERY | MY_SHIFT | 0.930056 | REVENUE_QUERY / 0.925908 | 0.004149 | `{}` | ClarificationRequired → ClarificationRequired; CloseCandidates; không tool | Sai revenue→shift; margin chặn; parser revenue không được chạy. |
| EMB-E04 | Owner | So sánh doanh thu hôm nay với hôm qua. | REVENUE_COMPARISON | REVENUE_COMPARISON | 1.000000 | REVENUE_QUERY / 0.902510 | 0.097490 | `{"currentPeriod":"today","comparisonPeriod":"yesterday"}` | Accepted → Accepted; ReadyForBackendChecks; get_revenue_comparison | Đúng; trùng mẫu active, không chứng minh paraphrase. |
| EMB-E05 | Owner | Hôm nay bán được nhiều tiền hơn hôm qua không? | REVENUE_COMPARISON | BUSINESS_SUMMARY | 0.909043 | REVENUE_COMPARISON / 0.908886 | 0.000157 | `{}` | ClarificationRequired → ClarificationRequired; MissingOrAmbiguousPeriod; không tool | Sai comparison→summary; hai kỳ khiến parser summary hỏi lại; không phải phủ định. |
| EMB-E06 | Owner | Đối chiếu doanh thu giữa hôm nay và ngày trước. | REVENUE_COMPARISON | REVENUE_COMPARISON | 0.952328 | REVENUE_QUERY / 0.897565 | 0.054763 | `{}` | Accepted → ClarificationRequired; MissingOrUnsupportedComparisonPeriods; không tool | Đúng intent và hỏi lại ngày trước, không tự đoán yesterday. |
| EMB-E07 | Owner | Món nào đang bán chạy nhất tuần này? | PRODUCT_RANKING | PRODUCT_RANKING | 0.968096 | MENU_QUERY / 0.903961 | 0.064134 | `{"period":"week"}` | Accepted → Accepted; ReadyForBackendChecks; get_best_sellers | Đúng intent/period week. |
| EMB-E08 | Owner | Xếp hạng các món bán nhiều trong tuần. | PRODUCT_RANKING | PRODUCT_RANKING | 0.959422 | MENU_QUERY / 0.887170 | 0.072252 | `{}` | Accepted → ClarificationRequired; MissingOrAmbiguousPeriod; không tool | Đúng intent; sai parser: bỏ sót trong tuần; hợp lệ bị chặn. |
| EMB-E09 | Owner | Tuần này khách gọi món nào nhiều nhất? | PRODUCT_RANKING | PRODUCT_RANKING | 0.909003 | MENU_QUERY / 0.880067 | 0.028936 | `{"period":"week"}` | Accepted → Accepted; ReadyForBackendChecks; get_best_sellers | Đúng intent/period week. |
| EMB-E10 | Owner | Tóm tắt tình hình kinh doanh tháng này. | BUSINESS_SUMMARY | BUSINESS_SUMMARY | 0.996265 | PRODUCT_RANKING / 0.895900 | 0.100365 | `{"period":"this_month"}` | Accepted → Accepted; ReadyForBackendChecks; get_business_summary | Đúng intent/period this_month. |
| EMB-E11 | Owner | Cho tôi bức tranh tổng quan hoạt động tháng hiện tại. | BUSINESS_SUMMARY | BUSINESS_SUMMARY | 0.918084 | REVENUE_QUERY / 0.908356 | 0.009728 | `{}` | ClarificationRequired → ClarificationRequired; MissingOrAmbiguousPeriod; không tool | Đúng intent; bỏ sót tháng hiện tại; margin cũng dưới .01. |
| EMB-E12 | Owner | Báo cáo KPI của tháng này. | BUSINESS_SUMMARY | BUSINESS_SUMMARY | 0.922660 | PRODUCT_RANKING / 0.903239 | 0.019421 | `{"period":"this_month"}` | Accepted → Accepted; ReadyForBackendChecks; get_business_summary | Đúng intent/period this_month. |
| EMB-E13 | Customer | Tôi muốn xem thực đơn hôm nay. | MENU_QUERY | MENU_QUERY | 0.927090 | MY_ORDERS / 0.925091 | 0.001999 | `{}` | ClarificationRequired → ClarificationRequired; CloseCandidates; không tool | Đúng menu, không cần category; margin chặn câu hợp lệ. |
| EMB-E14 | Guest | Nhà hàng hiện có những món nào? | MENU_QUERY | MENU_QUERY | 0.914960 | PRODUCT_RANKING / 0.879674 | 0.035285 | `{}` | Accepted → Accepted; ReadyForBackendChecks; customer_get_menu | Đúng menu; guest environment/QR/branch gate chưa chạy. |
| EMB-E15 | Customer | Cho tôi danh sách các món ăn và giá. | MENU_QUERY | MENU_QUERY | 0.929125 | PRODUCT_RANKING / 0.922210 | 0.006916 | `{}` | ClarificationRequired → ClarificationRequired; CloseCandidates; không tool | Đúng menu; margin chặn câu hợp lệ. |
| EMB-E16 | Customer | Xem lại những đơn tôi đã đặt. | MY_ORDERS | MY_ORDERS | 0.930285 | WRITE_INTENT_UNSUPPORTED / 0.905753 | 0.024532 | `{}` | Accepted → Accepted; ReadyForBackendChecks; get_my_orders | Đúng self-orders; ownership backend chưa kiểm chứng. |
| EMB-E17 | Customer | Các đơn hàng của tài khoản này là gì? | MY_ORDERS | MY_ORDERS | 0.923819 | WRITE_INTENT_UNSUPPORTED / 0.877783 | 0.046037 | `{}` | Accepted → Accepted; ReadyForBackendChecks; get_my_orders | Đúng self-orders; không lấy customer ID từ text. |
| EMB-E18 | Employee | Hôm nay tôi làm ca nào? | MY_SHIFT | MY_SHIFT | 0.935931 | WRITE_INTENT_UNSUPPORTED / 0.843915 | 0.092016 | `{}` | Accepted → Accepted; ReadyForBackendChecks; get_my_shift | Đúng shift; quyền employee/branch/backend chưa chạy. |
| EMB-E19 | Owner | Doanh thu thế nào? | REVENUE_QUERY | REVENUE_QUERY | 0.905412 | REVENUE_COMPARISON / 0.901547 | 0.003865 | `{}` | ClarificationRequired → ClarificationRequired; MissingOrAmbiguousPeriod; không tool | Đúng revenue và hỏi thiếu kỳ; không default today. |
| EMB-E20 | Owner | So sánh doanh thu giúp tôi. | REVENUE_COMPARISON | REVENUE_COMPARISON | 0.940884 | WRITE_INTENT_UNSUPPORTED / 0.904143 | 0.036740 | `{}` | Accepted → ClarificationRequired; MissingOrUnsupportedComparisonPeriods; không tool | Đúng comparison và hỏi thiếu hai kỳ. |
| EMB-E21 | Owner | Xem doanh thu hôm nay và món bán chạy tuần này. | ambiguous | REVENUE_COMPARISON | 0.936042 | PRODUCT_RANKING / 0.914230 | 0.021812 | `{}` | Accepted → ClarificationRequired; MultipleIntents; không tool | Đúng quyết định hỏi tách câu; không có một top-1 gold. |
| EMB-E22 | Owner | Báo cáo bán hàng hôm nay. | policy-dependent | BUSINESS_SUMMARY | 0.936581 | REVENUE_COMPARISON / 0.924386 | 0.012196 | `{"period":"today"}` | Accepted → Accepted; ReadyForBackendChecks; get_business_summary | Chưa kết luận: source policy-dependent; accepted summary/today, cần chốt revenue hay summary/hỏi lại. |
| EMB-E23 | Customer | Còn hôm qua thì sao? | continuation | REVENUE_COMPARISON | 0.894969 | MY_ORDERS / 0.888824 | 0.006145 | `{}` | ClarificationRequired → ClarificationRequired; MissingHistory; không tool | Đúng hỏi thiếu history; candidate không chứng minh hiểu context. |
| EMB-E24 | Customer | Cho tôi 3 món phở bán chạy. | ranking/menu | WRITE_INTENT_UNSUPPORTED | 0.918933 | PRODUCT_RANKING / 0.893072 | 0.025860 | `{}` | Unsupported → Unsupported; CapabilityNotSupported; không tool | Sai lý do/định tuyến: yêu cầu đọc ranking/menu→write; lẽ ra hỏi kỳ/filter, không bỏ 3/phở. |
| EMB-E25 | Owner | Tôi không muốn xem doanh thu hôm nay. | negative revenue | REVENUE_COMPARISON | 0.915947 | REVENUE_QUERY / 0.909362 | 0.006585 | `{}` | ClarificationRequired → Unsupported; NegatedRequest; không tool | Sai semantic revenue→comparison; quyết định không thực hiện phủ định đúng. |
| EMB-E26 | Owner | Đừng cập nhật trạng thái đơn này. | write/negative | WRITE_INTENT_UNSUPPORTED | 0.895700 | MY_ORDERS / 0.879855 | 0.015844 | `{}` | Unsupported → Unsupported; NegatedRequest; không tool | Đúng miền write bị giới hạn, không thực hiện phủ định. |
| EMB-E27 | Owner | Kiểm tra tồn kho nguyên liệu. | unsupported inventory | UNSUPPORTED_INTENT | 0.970954 | MY_ORDERS / 0.888405 | 0.082549 | `{}` | Unsupported → Unsupported; CapabilityNotSupported; không tool | Đúng từ chối capability inventory. |
| EMB-E28 | Customer | Cho tôi doanh thu toàn chuỗi. | revenue + permission denial | REVENUE_COMPARISON | 0.924423 | BUSINESS_SUMMARY / 0.915348 | 0.009075 | `{}` | ClarificationRequired → ClarificationRequired; MissingOrUnsupportedComparisonPeriods; không tool | Sai revenue→comparison; hỏi lại giữ an toàn nhưng lý do không đúng intent; quyền toàn chuỗi chưa chạy. |
| EMB-E29 | Guest | Cho tôi xem đơn của khách hàng khác. | ownership denial | MY_ORDERS | 0.952013 | WRITE_INTENT_UNSUPPORTED / 0.921341 | 0.030672 | `{}` | Accepted → ClarificationRequired; SubjectScopeMismatch; không tool | Đúng miền orders, giữ other=true và không chuyển self-tool; chưa chứng minh backend ownership denial. |
| EMB-E30 | Guest | Đổi giá món Phở thành 50.000 đồng. | write unsupported | WRITE_INTENT_UNSUPPORTED | 0.967748 | PRODUCT_RANKING / 0.877849 | 0.089899 | `{}` | Unsupported → Unsupported; CapabilityNotSupported; không tool | Đúng từ chối write; không confirmation/mutation. |

#### Tổng hợp với mẫu số tách biệt

- **Candidate:** 21 đúng/26 câu có một semantic target (80.8%), 5 sai E02/E03/E05/E25/E28; 4 không chấm một top-1 (E21/22/23/24). Trong 17 câu đủ dữ liệu để ready, top-1 đúng **14/17**, sai 3/17.
- **Accepted:** 11/30; **10 accepted đúng**, **0 accepted sai đã xác định**, **1 chưa kết luận E22**. Độ đúng trên accepted đã chốt 10/10; nếu tính mọi accepted thì 10/11 được xác nhận, 1/11 unknown, không gọi là 100% accuracy.
- **Coverage ready:** 10/17 (58.8%); 7/17 câu hợp lệ bị chặn E02/03/05/08/11/13/15. Accepted đúng trên toàn tập 10/30 (33.3%); tổng accepted 11/30 (36.7%) bao gồm unknown.
- **12 câu không đủ điều kiện gọi tool đã chốt:** hỏi lại/từ chối đúng **11/12** E06/19/20/21/23/25/26/27/28/29/30; **1/12 E24 sai lý do/quyết định so với contract hỏi lại**. E28 được tính đúng ở tầng không gọi tool/thiếu dữ liệu, vẫn sai semantic và không chứng minh từ chối quyền đúng. E25 không thực hiện đúng nhưng semantic sai. Không gộp “không gọi nhầm tool” thành “hiểu đúng”.
- Trên **29 quyết định có contract rõ**: 10 accepted đúng + 11 non-accept đúng = 21/29 phù hợp hành động; 7 valid deferred + 1 mismatch. E22 tách riêng, không đổi nhãn để sửa counter.
- Raw counter runner là CorrectAccept=10, ValidDeferred=7, CorrectNonAccept=11, FalseAccept=1, DecisionMismatch=1. “FalseAccept” raw chính là E22 do runner so chuỗi `PolicyUnresolved`; báo cáo giữ output đó nhưng không đánh đồng với false accept nghiệp vụ đã chứng minh.

**Nguyên nhân 7 câu hợp lệ bị chặn:** 2 sai top-1 rồi margin chặn (E02/03); 1 sai top-1 rồi parser candidate chặn (E05); 1 đúng top-1 nhưng parser bỏ sót (E08); 1 parser bỏ sót **và** margin dưới ngưỡng (E11, reason trả parser trước); 2 đúng menu nhưng margin chặn (E13/15). Không câu nào trong nhóm này bị score floor .85. Không hạ ngưỡng hoặc bỏ guard trong lượt đánh giá.

**Parser:** 11 câu ready có period/pair đã xác định theo nguồn (E01/02/03/04/05/07/08/09/10/11/12). Arguments đúng **7/11** (E01/02/04/07/09/10/12); 4/11 không đáp ứng, nhưng **không quy cả 4 là lỗi parser độc lập**:
- Với candidate đúng (8 câu), args đúng **6/8**, sai **2/8** E08 “trong tuần”, E11 “tháng hiện tại”.
- E02 candidate sai nhưng vẫn lấy today; E03 không vào parser revenue; E05 parser summary không nhận một period vì có today/yesterday. Hai lỗi sau phụ thuộc routing sai, không chứng minh parser comparison không lấy được kỳ.
- E06/19/20 hỏi thiếu/không xác định kỳ đúng 3/3; E23 history absent và E21 nhiều intent đúng ở guard, không tính thành parser date thành công. E28 có thiếu kỳ nhưng bị route sai.
- 6 câu ready không cần args (E13..18) giữ args rỗng 6/6; branch/customer identity không được đoán. E24 không trích 3/phở, bị chặn ở capability, **không** là bằng chứng parser quantity/filter đúng.
- Parser tổng quát product/category/quantity, lịch sử có state và resolution branch không được bài offline này kiểm chứng.

#### Theo intent và actor

Các hàng intent chỉ gồm 26 semantic target đã chốt; loại E21/22/23/24 khỏi mẫu số top-1. “Accepted đúng / ready” không tính câu cố ý cần hỏi lại hoặc từ chối.

| Intent | Candidate đúng / semantic target | Accepted đúng / ready | Ready bị chặn |
|---|---:|---:|---:|
| REVENUE_QUERY | 2/6 | 1/3 | 2 |
| REVENUE_COMPARISON | 3/4 | 1/2 | 1 |
| PRODUCT_RANKING | 3/3 | 2/3 | 1 |
| BUSINESS_SUMMARY | 3/3 | 2/3 | 1 |
| MENU_QUERY | 3/3 | 1/3 | 2 |
| MY_ORDERS | 3/3 | 2/2 | 0 |
| MY_SHIFT | 1/1 | 1/1 | 0 |
| WRITE_INTENT_UNSUPPORTED | 2/2 | 0/0 | 0 |
| UNSUPPORTED_INTENT | 1/1 | 0/0 | 0 |

| Actor | Tổng case | Candidate đúng / có target | Accepted đúng / ready | Ready bị chặn | Accepted chưa rõ chính sách |
|---|---:|---:|---:|---:|---|
| Owner | 19 | 13/17 | 6/11 | 5 | EMB-E22 |
| Customer | 7 | 4/5 | 2/4 | 2 | Không |
| Guest | 3 | 3/3 | 1/1 | 0 | Không |
| Employee | 1 | 1/1 | 1/1 | 0 | Không |

Theo provenance: nhóm overlap 12 có candidate đúng 9/11 target, accepted đúng 5/7 ready, non-accept đúng 5/5. Nhóm 18 **chưa phát hiện trùng** có candidate đúng 12/15 target, accepted đúng **5/10 ready**, 5 ready bị chặn, 6/7 non-accept đúng, 1 mismatch E24, 1 accepted unknown E22. Ở nhóm này accepted là 6/18, được xác nhận đúng 5/6 và 1 unknown. Tỷ lệ trên nhóm ít overlap không cải thiện đủ để dùng các câu trùng làm bằng chứng tổng quát hóa.

### Giới hạn runtime và quyết định sau đánh giá

**Chưa đủ cơ sở nối thử embedding làm bộ chọn tool trong chat, kể cả có cấu hình bật/tắt.** Các lỗi cản trở cụ thể:
1. Câu revenue/comparison phổ biến E02/03/05/28 vẫn bị định tuyến sai; E03 còn sang shift. Parser/guard chặn được lần này nhưng không chứng minh sai semantic luôn an toàn.
2. Chỉ 10/17 câu ready được nhận, phần ít overlap 5/10; menu customer E13/15 đều hỏi lại dù đúng intent, date parser E08/11 bỏ sót thông tin sẵn có.
3. E24 phân loại yêu cầu đọc thành write rồi từ chối sai lý do; E22 được accepted dù contract revenue/summary chưa chốt. Không tính quyền backend như cách chữa lỗi nhận diện này.
4. CAL-23 loyalty→orders vẫn là vấn đề đã biết theo lần phát triển trước; bộ 30 không có loyalty nên **không có bằng chứng mới để kết luận hết lỗi**. E29 chỉ xác nhận subject guard cho other-orders giữ `RequestedOtherSubject=true`, tool=null.
5. Bộ cuối có overlap đáng kể; không còn cơ sở gọi toàn bộ là bộ độc lập chưa tiếp xúc. Việc quan sát hôm nay không tự biến thành tuning: chưa chỉnh gì. Nếu thiết kế sửa theo các lỗi này, phải ghi EMB là dữ liệu phát triển và chuẩn bị đánh giá độc lập khác trước khi tuyên bố tổng quát hóa.

Bước nhỏ tiếp theo **đề xuất, chưa thực hiện**: chốt contract E22, review routing ba lỗi revenue/comparison và parser E08/11 dựa trên bảng này; tạo phiên bản ứng viên mới nếu được giao sửa, không sửa manifest v1. Sau đó đánh giá bằng dữ liệu mới chưa dùng phát triển. Có thể tiếp tục chẩn đoán offline; không yêu cầu test UI khi embedding chưa tích hợp.

Backend vẫn phải kiểm tra role allowlist, read-only, branch, guest environment/QR/session, customer ownership, service validation và kết quả nghiệp vụ. `BackendAuthorizationRequired=true` là cờ nhắc kiểm tra, **không phải permission passed**; runner không sử dụng actor để xác thực. E28 thiếu period nên chưa đi tới quyền; E29 bị subject guard trước backend. Không kiểm chứng kitchen endpoint rejection, confirmation expiry/replay, lỗi tool, loading/history/concurrency UI hay scope nhiều phiên. Các case test tay end-to-end vẫn **Chưa test**, không ghi PASS. O4 consistency .NET/Python từ spike trước không chứng minh O4 tương đương model gốc hoặc chất lượng tiếng Việt.

Không sửa C#/catalog/parser/guard/profile/CAL/SUP hoặc runtime, không tải model, không API/tool/database/browser/migration/seed, không commit/push/dừng process. Không chạy build vì không đổi code. Chỉ thêm bản chép dữ liệu đánh giá để dùng công cụ offline hiện có và cập nhật báo cáo/bàn giao.

## Đợt hiệu chỉnh giới hạn và cấu hình ứng viên — max/full-baseline

### Xác minh mẫu số, phân bố và kế hoạch trước khi chạy

Đối chiếu source hiện tại và `scoring-max-cal.json`/`scoring-max-sup.json`: baseline max/.85/.02 có 7 CAL và 1 SUP accepted đúng, 0 accepted sai. HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`, working tree dirty được bảo toàn.

“Câu hợp lệ” trong đợt này là câu có `expectedStatus=Accepted` theo kỳ vọng nghiệp vụ đã chốt: đủ thông tin cho capability đọc hiện có để chuyển sang **backend kiểm tra quyền/scope**, không phải đã có quyền hoặc đã thành công nghiệp vụ. CAL có **11** câu (01,02,05,07,09,10,11,12,13,14,16); SUP có **8** câu (01,02,03,04,05,06,11,18), tổng **19**. Các câu thiếu kỳ, thiếu history, multi-intent, phủ định, ngoài capability và target người khác cho self-only tool không nằm trong mẫu số này.

Số **17/19** đúng khi xét `Candidate.Top` của câu/mệnh đề dương: CAL 10/11, SUP 7/8. Hai câu sai là CAL-10 (menu → ranking) và SUP-02 (comparison → revenue). `Semantic.Top` toàn câu trước phủ định chỉ đúng **16/19**, vì SUP-18 raw là MY_ORDERS nhưng mệnh đề dương là menu. Đây là thống kê trên dữ liệu phát triển, không phải accuracy độc lập.

Các candidate hợp lệ đúng có score .877347–.962126 và margin .005386–.071103. Có **9** câu đúng nhưng bị margin .02 chặn: CAL-02/11/13, SUP-01/03/05/06/11/18. Không câu hợp lệ nào bị score floor .85 loại. CAL-10/SUP-02 bị định tuyến sai rồi parser chặn; hạ margin không biến candidate đó thành đúng hoặc bỏ parser.

Các bảng dưới được lập **trước chạy cấu hình bổ sung**. “Trước guard” là status sau score/margin; final gồm mọi parser/guard giữ nguyên. Case phân nhóm theo kỳ vọng, không đổi nhãn cũ.

#### Câu hợp lệ, top-1 đúng (17)

| ID | Expected / candidate | Score | Margin | Baseline trước guard → final | Reason / arguments |
|---|---|---:|---:|---|---|
| CAL-01 | REVENUE_QUERY / REVENUE_QUERY | 0.894166 | 0.033416 | Accepted → Accepted | ReadyForBackendChecks; {"period":"today"} |
| CAL-02 | REVENUE_QUERY / REVENUE_QUERY | 0.915022 | 0.005386 | ClarificationRequired → ClarificationRequired | CloseCandidates; {"period":"today"} |
| CAL-05 | PRODUCT_RANKING / PRODUCT_RANKING | 0.962126 | 0.070923 | Accepted → Accepted | ReadyForBackendChecks; {"period":"week"} |
| CAL-07 | BUSINESS_SUMMARY / BUSINESS_SUMMARY | 0.918458 | 0.044481 | Accepted → Accepted | ReadyForBackendChecks; {"period":"this_month"} |
| CAL-09 | MENU_QUERY / MENU_QUERY | 0.913195 | 0.048679 | Accepted → Accepted | ReadyForBackendChecks; {} |
| CAL-11 | MY_ORDERS / MY_ORDERS | 0.915194 | 0.008524 | ClarificationRequired → ClarificationRequired | CloseCandidates; {} |
| CAL-12 | MY_ORDERS / MY_ORDERS | 0.895600 | 0.038137 | Accepted → Accepted | ReadyForBackendChecks; {} |
| CAL-13 | MY_SHIFT / MY_SHIFT | 0.877347 | 0.015414 | ClarificationRequired → ClarificationRequired | CloseCandidates; {} |
| CAL-14 | MY_SHIFT / MY_SHIFT | 0.887396 | 0.071103 | Accepted → Accepted | ReadyForBackendChecks; {} |
| CAL-16 | MENU_QUERY / MENU_QUERY | 0.912702 | 0.052874 | Accepted → Accepted | ReadyForBackendChecks; {} |
| SUP-01 | REVENUE_QUERY / REVENUE_QUERY | 0.918053 | 0.013496 | ClarificationRequired → ClarificationRequired | CloseCandidates; {"period":"today"} |
| SUP-03 | MENU_QUERY / MENU_QUERY | 0.933862 | 0.018300 | ClarificationRequired → ClarificationRequired | CloseCandidates; {} |
| SUP-04 | PRODUCT_RANKING / PRODUCT_RANKING | 0.900206 | 0.027394 | Accepted → Accepted | ReadyForBackendChecks; {"period":"month"} |
| SUP-05 | BUSINESS_SUMMARY / BUSINESS_SUMMARY | 0.922407 | 0.018071 | ClarificationRequired → ClarificationRequired | CloseCandidates; {"period":"this_week"} |
| SUP-06 | MY_ORDERS / MY_ORDERS | 0.904811 | 0.015332 | ClarificationRequired → ClarificationRequired | CloseCandidates; {} |
| SUP-11 | MENU_QUERY / MENU_QUERY | 0.904556 | 0.014507 | ClarificationRequired → ClarificationRequired | CloseCandidates; {} |
| SUP-18 | MENU_QUERY / MENU_QUERY | 0.908735 | 0.008525 | ClarificationRequired → ClarificationRequired | CloseCandidates; {} |

#### Câu hợp lệ, top-1 sai (2)

| ID | Expected / candidate | Score | Margin | Baseline trước guard → final | Reason / arguments |
|---|---|---:|---:|---|---|
| CAL-10 | MENU_QUERY / PRODUCT_RANKING | 0.924186 | 0.003119 | ClarificationRequired → ClarificationRequired | MissingOrAmbiguousPeriod; {} |
| SUP-02 | REVENUE_COMPARISON / REVENUE_QUERY | 0.912194 | 0.015595 | ClarificationRequired → ClarificationRequired | MissingOrAmbiguousPeriod; {} |

#### Ngoài capability hoặc không đủ điều kiện gọi tool (11)

| ID | Expected / candidate | Score | Margin | Baseline trước guard → final | Reason / arguments |
|---|---|---:|---:|---|---|
| CAL-18 | UNSUPPORTED_INTENT / UNSUPPORTED_INTENT | 0.949023 | 0.065213 | Unsupported → Unsupported | CapabilityNotSupported; {} |
| CAL-19 | WRITE_INTENT_UNSUPPORTED / WRITE_INTENT_UNSUPPORTED | 0.930183 | 0.032463 | Unsupported → Unsupported | CapabilityNotSupported; {} |
| CAL-20 | WRITE_INTENT_UNSUPPORTED / WRITE_INTENT_UNSUPPORTED | 0.968191 | 0.059149 | Unsupported → Unsupported | CapabilityNotSupported; {} |
| CAL-23 | UNSUPPORTED_INTENT / MY_ORDERS | 0.922414 | 0.013300 | ClarificationRequired → ClarificationRequired | SubjectScopeMismatch; {} |
| CAL-24 | UNSUPPORTED_INTENT / UNSUPPORTED_INTENT | 0.922288 | 0.036978 | Unsupported → Unsupported | CapabilityNotSupported; {} |
| SUP-10 | REVENUE_QUERY / REVENUE_QUERY | 0.917928 | 0.021411 | Accepted → Unsupported | NegatedRequest; {} |
| SUP-13 | UNSUPPORTED_INTENT / UNSUPPORTED_INTENT | 0.887561 | 0.035004 | Unsupported → Unsupported | CapabilityNotSupported; {} |
| SUP-14 | UNSUPPORTED_INTENT / UNSUPPORTED_INTENT | 0.927233 | 0.026037 | Unsupported → Unsupported | CapabilityNotSupported; {} |
| SUP-15 | MY_ORDERS / MY_ORDERS | 0.932828 | 0.028516 | Accepted → ClarificationRequired | SubjectScopeMismatch; {} |
| SUP-16 | WRITE_INTENT_UNSUPPORTED / WRITE_INTENT_UNSUPPORTED | 0.926427 | 0.053201 | Unsupported → Unsupported | CapabilityNotSupported; {} |
| SUP-17 | UNSUPPORTED_INTENT / REVENUE_COMPARISON | 0.819668 | 0.011154 | Unsupported → Unsupported | LowSimilarity; {} |

#### Thiếu tham số/history hoặc nhiều intent (12)

| ID | Expected / candidate | Score | Margin | Baseline trước guard → final | Reason / arguments |
|---|---|---:|---:|---|---|
| CAL-03 | REVENUE_COMPARISON / REVENUE_COMPARISON | 0.936564 | 0.041123 | Accepted → ClarificationRequired | MissingOrUnsupportedComparisonPeriods; {} |
| CAL-04 | REVENUE_COMPARISON / REVENUE_COMPARISON | 0.938221 | 0.033820 | Accepted → ClarificationRequired | MissingOrUnsupportedComparisonPeriods; {} |
| CAL-06 | PRODUCT_RANKING / PRODUCT_RANKING | 0.937267 | 0.028773 | Accepted → ClarificationRequired | MissingOrAmbiguousPeriod; {} |
| CAL-08 | BUSINESS_SUMMARY / BUSINESS_SUMMARY | 0.932428 | 0.036599 | Accepted → ClarificationRequired | MissingOrAmbiguousPeriod; {} |
| CAL-15 | AMBIGUOUS_MULTI_INTENT / REVENUE_QUERY | 0.927013 | 0.012196 | ClarificationRequired → ClarificationRequired | MultipleIntents; {} |
| CAL-17 | UNSUPPORTED_INTENT / MY_SHIFT | 0.886462 | 0.002481 | ClarificationRequired → ClarificationRequired | MissingHistory; {} |
| CAL-21 | REVENUE_QUERY / REVENUE_COMPARISON | 0.910197 | 0.020191 | Accepted → ClarificationRequired | MissingOrUnsupportedComparisonPeriods; {} |
| CAL-22 | AMBIGUOUS_MULTI_INTENT / REVENUE_COMPARISON | 0.903261 | 0.011722 | ClarificationRequired → ClarificationRequired | MultipleIntents; {} |
| SUP-07 | REVENUE_QUERY / UNSUPPORTED_INTENT | 0.899475 | 0.003330 | ClarificationRequired → ClarificationRequired | CloseCapabilityCandidates; {} |
| SUP-08 | REVENUE_COMPARISON / REVENUE_COMPARISON | 0.931228 | 0.026134 | Accepted → ClarificationRequired | MissingOrUnsupportedComparisonPeriods; {} |
| SUP-09 | CONTEXT_UNRESOLVED / REVENUE_COMPARISON | 0.879400 | 0.008798 | ClarificationRequired → ClarificationRequired | MissingHistory; {} |
| SUP-12 | AMBIGUOUS_MULTI_INTENT / MENU_QUERY | 0.915441 | 0.004939 | ClarificationRequired → ClarificationRequired | CloseCandidates; {} |

CAL-17 và SUP-09 dùng nhãn xử lý history chứ không có một tool intent xác định; CAL-15/22/SUP-12 là multi-intent. Không ép các nhãn này thành “đúng model” chỉ vì hỏi lại. CAL-23 cần báo capability loyalty chưa có nhưng đang hỏi scope của MY_ORDERS, vẫn là decision mismatch. SUP-15 là domain orders nhưng target người khác; ownership phải kiểm tra riêng, không tự đổi thành self orders.

### Bốn cấu hình được khai báo trước khi chạy

File `profiles/threshold-trials-v1.json` ghi cố định kế hoạch. Giữ model/tokenizer, full-baseline 34 mẫu, max cosine, parser, guard, mọi câu/nhãn và kỳ vọng hành vi.

| ID | Score / margin | Lý do chọn trước khi chạy |
|---|---|---|
| T0 | .85/.020 | Baseline để tái lập |
| T1 | .85/.015 | Giảm gap 25%, kiểm tra cụm candidate đúng .015–.02 |
| T2 | .85/.010 | Giảm gap một nửa, kiểm tra cụm .01–.015 |
| T3 | .85/.005 | Mức sensitivity một phần tư gap, không tự chọn chỉ vì coverage cao |

Chỉ đúng ba cấu hình bổ sung; không dò từng câu hoặc thêm cấu hình thứ năm. Score floor giữ .85 vì các câu hợp lệ đều vượt và weather SUP-17 chỉ .819668. Sẽ báo cả các chặn parser/guard và case đổi decision, không chỉ những câu được nhận thêm. Bộ EMB cuối không được mở/chạy/dùng; không thêm dataset hoặc thuật toán.




### Kết quả đã chạy — hoàn tất đúng bốn cấu hình

Chạy cùng binary/source hiện có với `--catalog-profile full-baseline --scoring max`, đúng 4 cấu hình × 2 tập. T0 tái lập baseline; không thay parser/guard sau khi xem kết quả, không thêm cấu hình thứ năm. Tất cả lệnh scoring local exit 0. Chỉ tạo/cập nhật manifest và tài liệu, không đổi C# trong đợt này nên không build lại; build thành công của đợt scoring trước vẫn chỉ là bằng chứng lịch sử.

| Cấu hình score/margin | Accepted đúng | Accepted sai | Hợp lệ bị hỏi lại/từ chối | Hỏi lại/từ chối đúng | Decision mismatch | Coverage đúng / hợp lệ | Coverage accepted / toàn tập | Đúng / accepted |
|---|---:|---:|---:|---:|---:|---|---|---|
| T0: 0.850000/0.020000 | 8 | 0 | 11 | 22 | 1 | 8/19 (42.1%) | 8/42 (19.0%) | 8/8 |
| T1: 0.850000/0.015000 | 12 | 0 | 7 | 22 | 1 | 12/19 (63.2%) | 12/42 (28.6%) | 12/12 |
| T2: 0.850000/0.010000 | 14 | 0 | 5 | 22 | 1 | 14/19 (73.7%) | 14/42 (33.3%) | 14/14 |
| T3: 0.850000/0.005000 | 17 | 0 | 2 | 22 | 1 | 17/19 (89.5%) | 17/42 (40.5%) | 17/17 |

| Cấu hình | CAL accepted / toàn tập; / hợp lệ | SUP accepted / toàn tập; / hợp lệ | Hợp lệ bị chặn |
|---|---|---|---|
| T0 | 7/24; hợp lệ 7/11 | 1/18; hợp lệ 1/8 | 4 CAL + 7 SUP |
| T1 | 8/24; hợp lệ 8/11 | 4/18; hợp lệ 4/8 | 3 CAL + 4 SUP |
| T2 | 8/24; hợp lệ 8/11 | 6/18; hợp lệ 6/8 | 3 CAL + 2 SUP |
| T3 | 10/24; hợp lệ 10/11 | 7/18; hợp lệ 7/8 | 1 CAL + 1 SUP |

Cả 4 cấu hình vẫn có 17/19 top-1 effective đúng và cùng hai câu hợp lệ xếp sai. Ngưỡng không đổi thứ hạng. Cột “đúng / accepted” chỉ là đối chiếu câu và kỳ vọng trên 42 case phát triển, không phải xác suất, không phải accuracy độc lập hay chứng nhận backend tool/business result. Cả 22 final hỏi lại/từ chối được kỳ vọng và 1 decision mismatch còn lại đều không đổi qua bốn cấu hình.

#### Những câu đổi final so với baseline

| Cấu hình | ID đổi quyết định so với T0 | Ý nghĩa |
|---|---|---|
| T0 | Không có | Tất cả thay đổi là ClarificationRequired → Accepted đúng; không đổi final của negative |
| T1 | CAL-13, SUP-03, SUP-05, SUP-06 | Tất cả thay đổi là ClarificationRequired → Accepted đúng; không đổi final của negative |
| T2 | CAL-13, SUP-01, SUP-03, SUP-05, SUP-06, SUP-11 | Tất cả thay đổi là ClarificationRequired → Accepted đúng; không đổi final của negative |
| T3 | CAL-02, CAL-11, CAL-13, SUP-01, SUP-03, SUP-05, SUP-06, SUP-11, SUP-18 | Tất cả thay đổi là ClarificationRequired → Accepted đúng; không đổi final của negative |

Hạ margin không bỏ qua các tầng còn lại:
- CAL-03/04/06/08/21, SUP-08 vẫn yêu cầu kỳ hợp lệ/đủ cặp. CAL-10 và SUP-02 vẫn bị parser chặn do chọn sai intent rồi không khớp tham số.
- CAL-15/22 vẫn MultipleIntents; CAL-17/SUP-09 vẫn MissingHistory.
- SUP-10 vẫn NegatedRequest. SUP-15 vẫn SubjectScopeMismatch, không đổi đơn người khác thành đơn của tôi.
- CAL-18/24, SUP-13/14/16 vẫn CapabilityNotSupported; SUP-17 vẫn LowSimilarity, score .819668 < .85.
- CAL-23 vẫn candidate MY_ORDERS sai cho loyalty và bị SubjectScopeMismatch. Ở T2/T3 candidate đã vượt score/margin nhưng tool vẫn không được chuyển; final vẫn là mismatch về loại phản hồi (cần unsupported capability, đang hỏi lại scope).
- SUP-07 vẫn candidate unsupported thay vì revenue thiếu kỳ; hỏi lại đúng status nhưng chưa đúng lý do. SUP-12 vẫn chỉ được margin chặn (guard chưa nhận ra đủ hai ý), không tính là multi-intent recognition đúng.

Các nhãn/context cần đọc thận trọng được giữ riêng: CAL-17/SUP-09 không có single tool intent từ câu nối tiếp; CAL-15/22/SUP-12 là trạng thái multi-intent. Chúng không được đưa vào 19 câu ready hoặc được đổi nhãn để tăng số đúng. SUP-15 là semantic order lookup kèm target không phù hợp self-only capability, không phải một phép kiểm tra quyền backend đã chạy. Không ép các trường hợp này vào “đúng nghiệp vụ”.

### Chốt ứng viên và đóng băng

**Chọn T2 .85/.010**, tên profile **`max-full-baseline-candidate-v1`**, trạng thái **“ứng viên thử nghiệm, chưa triển khai”**. Manifest: `.tools/mock-ai-embedding-spike/profiles/max-full-baseline-candidate-v1.json`. SHA256 manifest: `F598F036DAEBC6299BD04C155C55E35A90A41FFBD3B4460BD8046DAB86FDE861`.

Lý do:
- So với T0, số câu hợp lệ được nhận tăng 8/19 → 14/19 (42.1% → 73.7%), không phải chọn cấu hình gần như hỏi lại hết chỉ để có false accept bằng 0.
- So với T1, T2 nhận thêm SUP-01/SUP-11 mà không đổi final của negative trong dữ liệu đã quan sát.
- T3 đạt 17/19 nhưng margin .005 chỉ cao hơn SUP-12 (.004938960) khoảng **.000061040**. Câu này chứa hai yêu cầu, guard hiện chưa nhận ra, nên lựa chọn T3 phụ thuộc quá sát một ranh giới của dữ liệu phát triển. Không có cơ sở xem khoảng cách đó bền vững. T2 vẫn có khoảng cách .005061040 so với điểm này, nhưng đó cũng **không phải bảo đảm an toàn ngoài tập đã biết**.
- Không hạ score vì không có candidate hợp lệ bị score floor hiện tại loại; không nới parser/guard để nhận hai câu semantic sai.

Không có false accept quan sát trong bốn cấu hình **không đồng nghĩa** được hứa 0 false accept trên dữ liệu mới. Đây là lựa chọn ứng viên có cân nhắc để đánh giá sau, không phải threshold tối ưu hoặc quyết định rollout.

Manifest ghi model `intfloat/multilingual-e5-small`, revision artifact `614241f622f53c4eeff9890bdc4f31cfecc418b3`, checksum O4/tokenizer, prefix/query/pooling/normalization; catalog `full-baseline` 34 retained/32 competing samples với hash source; hash scorer, parser/guard, adapter, runner và các tập phát triển. HEAD một mình không đại diện tree dirty nên dùng thêm checksum.

Profile là **manifest đóng băng**, chưa có loader runtime. Runner vẫn mặc định max/full-baseline/.85/.02; khi muốn tái tạo ứng viên phải truyền rõ .85/.01 bằng các cờ hiện có. Không tự đổi baseline mặc định, không thêm DI hoặc feature flag. Không chỉnh manifest v1 sau khi đánh giá độc lập; nếu phải sửa thì tạo phiên bản mới và ghi lý do ở một nhiệm vụ sau.

#### Những câu còn lại ở ứng viên T2

| Nhóm | ID | Điểm nghẽn còn lại |
|---|---|---|
| Hợp lệ, candidate đúng nhưng gap hẹp | CAL-02 | score .915022, margin .005386; hỏi lại |
| Hợp lệ, candidate đúng nhưng gap hẹp | CAL-11 | score .915194, margin .008524; hỏi lại |
| Hợp lệ, candidate đúng nhưng gap hẹp | SUP-18 | menu mệnh đề dương .908735, margin .008525; hỏi lại |
| Hợp lệ, candidate sai | CAL-10 | menu → ranking; parser yêu cầu kỳ sai lĩnh vực; không gọi tool |
| Hợp lệ, candidate sai | SUP-02 | comparison → revenue; hai kỳ gây ambiguous period trong nhánh sai |
| Sai capability còn bị chặn | CAL-23 | loyalty → MY_ORDERS; final mismatch 1/42 chưa sửa |
| Nhận diện nội dung còn thiếu | SUP-07, SUP-12, CAL-21 | Candidate hoặc lý do hỏi lại vẫn chưa đúng dù không nhận tool |

Các lỗi còn lại được giữ nguyên; không vá catalog/parser/guard trong lượt chốt này. Theo kỳ vọng hiện có, 5 câu hợp lệ còn bị chặn đều là hỏi lại, không có hard refusal trong nhóm này.

### Hướng dẫn bước đánh giá độc lập sau — chưa thực hiện

1. Bắt đầu một đợt đánh giá riêng với manifest v1 đã đóng băng. Xác minh checksum source/model/tokenizer/catalog, chọn max/full-baseline/.85/.01; mọi thay đổi version làm mất tính so sánh. Không đọc/chạy bộ EMB cuối trong đợt hiệu chỉnh này; cũng chưa xác nhận provenance/trùng mẫu của bộ đó trên toàn lịch sử.
2. Ở đợt đánh giá sau, kiểm tra provenance/trùng với câu mẫu và dữ liệu phát triển trước khi công bố tính độc lập. Giữ nguyên case/nhãn/kết quả gốc; nếu có trùng thì ghi riêng, không dùng làm bằng chứng hiểu cách diễn đạt mới.
3. Đánh giá **thành phần offline** bằng adapter/scorer/pipeline đã đóng băng. Mỗi case ghi: ID, actor, nguyên văn, profile/hash, raw top-1 và second/score/margin, effective candidate sau mệnh đề, arguments/thiếu gì, capability, status/reason, tool eligible cho backend check, expected và quan sát. Không gọi tool nghiệp vụ. Các bước permission/business phải ghi **Chưa kiểm tra**.
4. Dùng mẫu ghi kết quả sau (hướng dẫn, chưa tạo dataset hoặc chạy case):

| ID có sẵn | Profile/hash | Raw top/second/score/margin | Effective candidate | Args/missing | Capability | Status/reason | Backend permission | Business result | Expected vs actual / evidence |
|---|---|---|---|---|---|---|---|---|---|
| Ghi khi bắt đầu đợt đánh giá riêng | manifest v1 | Chưa kiểm tra | Chưa kiểm tra | Chưa kiểm tra | Chưa kiểm tra | Chưa kiểm tra | Chưa kiểm tra | Chưa kiểm tra | Chưa kiểm tra |

5. Tách kết quả semantic, parser, decision, quyền và nghiệp vụ. Báo correct/false accept, valid deferred, correct clarify/refusal và mismatch có mẫu số. Xác định kỳ vọng trước khi xem candidate; trường hợp chưa rõ để riêng thay vì sửa nhãn theo output.
6. **End-to-end chat** chỉ kiểm tra ở một nhiệm vụ sau khi có integration được cho phép: UI → API → identity/scope → permission → tool/service → response. Hiện chưa có tính năng embedding trong UI, không yêu cầu người dùng test qua chat và không tạo runner chat.
7. Nếu sử dụng lỗi của bộ đánh giá để sửa mẫu/threshold/parser/scoring, ghi bộ đó đã trở thành dữ liệu phát triển; đánh giá lại cùng bộ không còn là bằng chứng độc lập. Không đổi cấu hình sau từng câu trong cùng đợt.

### Tái tạo cấu hình đã thử và kiểm chứng

Kế hoạch cố định nằm trong `profiles/threshold-trials-v1.json`; output ignored là `artifacts/threshold-T0-cal.json` … `threshold-T3-sup.json`. Snapshot bảng ở trên được giữ trong báo cáo. Các command sau dùng binary spike hiện có, không chạy ứng dụng API:

```powershell
# T0 baseline; đổi minimum-margin đúng theo kế hoạch T1 .015, T2 .01, T3 .005.
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --minimum-score 0.85 --minimum-margin 0.02 --review-output artifacts/threshold-T0-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --minimum-score 0.85 --minimum-margin 0.02 --calibration calibration-supplement.json --review-output artifacts/threshold-T0-sup.json
# T2 ứng viên đã đóng băng:
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --minimum-score 0.85 --minimum-margin 0.01 --review-output artifacts/threshold-T2-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --minimum-score 0.85 --minimum-margin 0.01 --calibration calibration-supplement.json --review-output artifacts/threshold-T2-sup.json
```

Không có code ứng dụng/C#/dependency/model/dataset mới; chỉ cấu hình thí nghiệm và tài liệu. Toàn bộ hash source/dữ liệu cố định được đối chiếu lại sau chạy. Không build lại khi source không đổi, không gọi API/browser/database/tool, không migration/seed, không commit/push hoặc dừng process. Đợt hiệu chỉnh kết thúc tại đây.

---


## Thử nghiệm scoring trước: max và top-2 mean, full-baseline cố định

### Phạm vi và cách tính

Đã kiểm tra HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b` và working tree dirty, bảo toàn thay đổi trước đó. Chỉ sửa `IntentScorer.cs`, `DecisionReviewRunner.cs`, hướng dẫn spike và báo cáo/context. Không thêm câu mẫu, không dùng profile `three-intent-examples`, không đổi model/parser/guard hoặc các câu/nhãn CAL/SUP. **Cả hai tập đều là dữ liệu phát triển đã quan sát, không phải đánh giá độc lập.** Không dùng tập EMB cuối.

Đúng hai phép tổng hợp:

- `max`: điểm intent bằng cosine cao nhất của các mẫu thuộc intent.
- `top2-mean`: sắp cosine giảm dần trong từng intent, lấy trung bình hai mẫu **khác ID** cao nhất. Không average hai intent đứng đầu.
- Nếu intent có đúng một mẫu, dùng chính cosine đó (mean trên một mẫu); ghi số mẫu trong `samplesPerIntent`, không thêm/nhân đôi mẫu. Intent không có mẫu không cạnh tranh. Catalog hiện tại không có singleton nên nhánh đó chỉ được xác định từ source, chưa được CAL/SUP thực thi.
- Cả hai giữ cách loại CAT-AMB khỏi cạnh tranh capability đã có từ trước; các mẫu này vẫn giữ ID trong catalog. Margin là điểm intent top-1 trừ điểm intent **khác** đứng thứ hai. Điểm không phải xác suất.

Full-baseline có 34 mẫu, 32 mẫu cạnh tranh trong 9 nhóm: revenue 3, comparison 4, ranking 4, summary 3, menu 6, my-orders 2, my-shift 2, unsupported 6, write-unsupported 2. Mọi competitor/negative giữ nguyên. Model O4 SHA256 `4654C156F3E4171ABC9C716CDB771BF9116455D15AC1AAB364AEEEDE0E3205B0`; tokenizer `0B44A9D7B51C3C62626640CDA0E2C2F70FDACDC25BBBD68038369D14EBDF4C39`. Hash catalog, decision pipeline (parser/guard), adapter và ba file dữ liệu/expectation không đổi trong lượt này.

Source thêm enum chọn đúng hai phương pháp; mặc định vẫn max. Output lưu `Evidence` gồm hai cosine/ID tốt nhất (max chỉ dùng điểm đầu để chấm), `RankedCandidates`, mode và ngưỡng. Kiểm tra offline 1,512 bản ghi điểm intent ở hai stage raw/effective của bốn output: **0 lỗi số học/ID mẫu trùng**. Đây là kiểm tra phép tổng hợp, không phải test chat. Build Release spike thành công, 0 warning/0 error.

### Kết quả kiểm soát với cùng score .85, margin .02

| Chỉ số | CAL max → top2 mean | SUP max → top2 mean |
|---|---|---|
| Top-1 raw toàn câu trùng nhãn, trước threshold/guard | 18/24 → 19/24 | 12/18 → 12/18 |
| Candidate sau xử lý mệnh đề trùng nhãn, trước threshold | 18/24 → 19/24 | 13/18 → 12/18 |
| Candidate đúng trên câu hợp lệ đủ thông tin | 10/11 → 10/11 | 7/8 → 7/8 |
| Accepted đúng | 7 → 8 | 1 → 3 |
| Accepted sai | 0 → 0 | 0 → 0 |
| Hỏi lại/từ chối đúng theo status | 12 → 11 | 10 → 10 |
| Câu hợp lệ bị chặn | 4 → 3 | 7 → 5 |
| Decision mismatch | 1 → 2 | 0 → 0 |
| Coverage accepted trên toàn tập | 7/24 (29.2%) → 8/24 (33.3%) | 1/18 (5.6%) → 3/18 (16.7%) |
| Coverage đúng trên câu hợp lệ | 7/11 (63.6%) → 8/11 (72.7%) | 1/8 (12.5%) → 3/8 (37.5%) |
| Đúng trong accepted | 7/7 → 8/8 | 1/1 → 3/3 |

Candidate/intent đúng không đồng nghĩa được backend cấp quyền hoặc trả được nghiệp vụ. Literal label counts còn gồm context/multi-intent/unsupported nên không dùng như accuracy toàn hệ thống. SUP-18 raw whole sentence có my-orders ở max nhưng candidate của mệnh đề dương là menu; bảng tách hai stage để không lẫn tác động guard vào “model sửa intent”.

### Từng câu: top-1 trước threshold, second và margin

Các cột raw dưới đây là thứ hạng `Semantic` của **toàn câu trước guard**, bất kể status threshold. Cột effective là candidate sau bước mệnh đề/phủ định hiện có; số điểm/margin effective đầy đủ nằm trong JSON. Không thay policy khi hai mode cho thứ hạng mệnh đề khác nhau.

| ID | Max raw top-1 / score | Max second / score | Max margin | Mean raw top-1 / score | Mean second / score | Mean margin | Effective candidate max → mean | Final max → mean |
|---|---|---|---:|---|---|---:|---|---|
| CAL-01 | REVENUE_QUERY / 0.894166 | MENU_QUERY / 0.860750 | 0.033416 | REVENUE_QUERY / 0.890532 | MENU_QUERY / 0.858946 | 0.031586 | REVENUE_QUERY → REVENUE_QUERY | Accepted → Accepted |
| CAL-02 | REVENUE_QUERY / 0.915022 | BUSINESS_SUMMARY / 0.909637 | 0.005386 | REVENUE_QUERY / 0.911555 | REVENUE_COMPARISON / 0.885425 | 0.026131 | REVENUE_QUERY → REVENUE_QUERY | ClarificationRequired → Accepted |
| CAL-03 | REVENUE_COMPARISON / 0.936564 | REVENUE_QUERY / 0.895441 | 0.041123 | REVENUE_COMPARISON / 0.931962 | REVENUE_QUERY / 0.887316 | 0.044646 | REVENUE_COMPARISON → REVENUE_COMPARISON | ClarificationRequired → ClarificationRequired |
| CAL-04 | REVENUE_COMPARISON / 0.938221 | REVENUE_QUERY / 0.904401 | 0.033820 | REVENUE_COMPARISON / 0.921491 | BUSINESS_SUMMARY / 0.891166 | 0.030325 | REVENUE_COMPARISON → REVENUE_COMPARISON | ClarificationRequired → ClarificationRequired |
| CAL-05 | PRODUCT_RANKING / 0.962126 | MENU_QUERY / 0.891203 | 0.070923 | PRODUCT_RANKING / 0.925069 | MENU_QUERY / 0.877606 | 0.047463 | PRODUCT_RANKING → PRODUCT_RANKING | Accepted → Accepted |
| CAL-06 | PRODUCT_RANKING / 0.937267 | MENU_QUERY / 0.908493 | 0.028773 | PRODUCT_RANKING / 0.936146 | MENU_QUERY / 0.902901 | 0.033246 | PRODUCT_RANKING → PRODUCT_RANKING | ClarificationRequired → ClarificationRequired |
| CAL-07 | BUSINESS_SUMMARY / 0.918458 | PRODUCT_RANKING / 0.873976 | 0.044481 | BUSINESS_SUMMARY / 0.898856 | PRODUCT_RANKING / 0.860788 | 0.038068 | BUSINESS_SUMMARY → BUSINESS_SUMMARY | Accepted → Accepted |
| CAL-08 | BUSINESS_SUMMARY / 0.932428 | PRODUCT_RANKING / 0.895829 | 0.036599 | BUSINESS_SUMMARY / 0.927921 | PRODUCT_RANKING / 0.892249 | 0.035672 | BUSINESS_SUMMARY → BUSINESS_SUMMARY | ClarificationRequired → ClarificationRequired |
| CAL-09 | MENU_QUERY / 0.913195 | PRODUCT_RANKING / 0.864516 | 0.048679 | MENU_QUERY / 0.908691 | PRODUCT_RANKING / 0.862893 | 0.045799 | MENU_QUERY → MENU_QUERY | Accepted → Accepted |
| CAL-10 | PRODUCT_RANKING / 0.924186 | MENU_QUERY / 0.921067 | 0.003119 | PRODUCT_RANKING / 0.923639 | MENU_QUERY / 0.917310 | 0.006329 | PRODUCT_RANKING → PRODUCT_RANKING | ClarificationRequired → ClarificationRequired |
| CAL-11 | MY_ORDERS / 0.915194 | WRITE_INTENT_UNSUPPORTED / 0.906670 | 0.008524 | MY_ORDERS / 0.900778 | UNSUPPORTED_INTENT / 0.890049 | 0.010729 | MY_ORDERS → MY_ORDERS | ClarificationRequired → ClarificationRequired |
| CAL-12 | MY_ORDERS / 0.895600 | WRITE_INTENT_UNSUPPORTED / 0.857463 | 0.038137 | MY_ORDERS / 0.879533 | UNSUPPORTED_INTENT / 0.845518 | 0.034016 | MY_ORDERS → MY_ORDERS | Accepted → Accepted |
| CAL-13 | MY_SHIFT / 0.877347 | REVENUE_QUERY / 0.861932 | 0.015414 | MY_SHIFT / 0.854853 | REVENUE_COMPARISON / 0.854831 | 0.000023 | MY_SHIFT → MY_SHIFT | ClarificationRequired → ClarificationRequired |
| CAL-14 | MY_SHIFT / 0.887396 | WRITE_INTENT_UNSUPPORTED / 0.816293 | 0.071103 | MY_SHIFT / 0.872672 | UNSUPPORTED_INTENT / 0.811482 | 0.061191 | MY_SHIFT → MY_SHIFT | Accepted → Accepted |
| CAL-15 | REVENUE_QUERY / 0.927013 | REVENUE_COMPARISON / 0.914817 | 0.012196 | REVENUE_QUERY / 0.923910 | REVENUE_COMPARISON / 0.905692 | 0.018218 | REVENUE_QUERY → REVENUE_QUERY | ClarificationRequired → ClarificationRequired |
| CAL-16 | MENU_QUERY / 0.909175 | MY_ORDERS / 0.875332 | 0.033844 | MENU_QUERY / 0.880548 | REVENUE_COMPARISON / 0.870845 | 0.009703 | MENU_QUERY → MENU_QUERY | Accepted → Accepted |
| CAL-17 | MY_SHIFT / 0.886462 | REVENUE_COMPARISON / 0.883981 | 0.002481 | REVENUE_COMPARISON / 0.882058 | MY_SHIFT / 0.865607 | 0.016451 | MY_SHIFT → REVENUE_COMPARISON | ClarificationRequired → ClarificationRequired |
| CAL-18 | UNSUPPORTED_INTENT / 0.949023 | MY_ORDERS / 0.883811 | 0.065213 | UNSUPPORTED_INTENT / 0.910727 | PRODUCT_RANKING / 0.876387 | 0.034340 | UNSUPPORTED_INTENT → UNSUPPORTED_INTENT | Unsupported → Unsupported |
| CAL-19 | WRITE_INTENT_UNSUPPORTED / 0.930183 | PRODUCT_RANKING / 0.897720 | 0.032463 | WRITE_INTENT_UNSUPPORTED / 0.894589 | PRODUCT_RANKING / 0.892113 | 0.002476 | WRITE_INTENT_UNSUPPORTED → WRITE_INTENT_UNSUPPORTED | Unsupported → ClarificationRequired |
| CAL-20 | WRITE_INTENT_UNSUPPORTED / 0.968191 | UNSUPPORTED_INTENT / 0.909042 | 0.059149 | WRITE_INTENT_UNSUPPORTED / 0.911692 | UNSUPPORTED_INTENT / 0.895053 | 0.016639 | WRITE_INTENT_UNSUPPORTED → WRITE_INTENT_UNSUPPORTED | Unsupported → ClarificationRequired |
| CAL-21 | REVENUE_COMPARISON / 0.910197 | REVENUE_QUERY / 0.890005 | 0.020191 | REVENUE_COMPARISON / 0.903683 | REVENUE_QUERY / 0.888107 | 0.015575 | REVENUE_COMPARISON → REVENUE_COMPARISON | ClarificationRequired → ClarificationRequired |
| CAL-22 | REVENUE_COMPARISON / 0.903261 | BUSINESS_SUMMARY / 0.891539 | 0.011722 | REVENUE_COMPARISON / 0.897202 | PRODUCT_RANKING / 0.887263 | 0.009939 | REVENUE_COMPARISON → REVENUE_COMPARISON | ClarificationRequired → ClarificationRequired |
| CAL-23 | MY_ORDERS / 0.922414 | UNSUPPORTED_INTENT / 0.909114 | 0.013300 | UNSUPPORTED_INTENT / 0.908154 | MENU_QUERY / 0.878402 | 0.029752 | MY_ORDERS → UNSUPPORTED_INTENT | ClarificationRequired → Unsupported |
| CAL-24 | UNSUPPORTED_INTENT / 0.922288 | WRITE_INTENT_UNSUPPORTED / 0.885309 | 0.036978 | UNSUPPORTED_INTENT / 0.899369 | WRITE_INTENT_UNSUPPORTED / 0.862679 | 0.036690 | UNSUPPORTED_INTENT → UNSUPPORTED_INTENT | Unsupported → Unsupported |

| ID | Max raw top-1 / score | Max second / score | Max margin | Mean raw top-1 / score | Mean second / score | Mean margin | Effective candidate max → mean | Final max → mean |
|---|---|---|---:|---|---|---:|---|---|
| SUP-01 | REVENUE_QUERY / 0.918053 | BUSINESS_SUMMARY / 0.904557 | 0.013496 | REVENUE_QUERY / 0.913380 | BUSINESS_SUMMARY / 0.903083 | 0.010297 | REVENUE_QUERY → REVENUE_QUERY | ClarificationRequired → ClarificationRequired |
| SUP-02 | REVENUE_QUERY / 0.912194 | REVENUE_COMPARISON / 0.896599 | 0.015595 | REVENUE_QUERY / 0.905594 | REVENUE_COMPARISON / 0.891062 | 0.014532 | REVENUE_QUERY → REVENUE_QUERY | ClarificationRequired → ClarificationRequired |
| SUP-03 | MENU_QUERY / 0.933862 | PRODUCT_RANKING / 0.915562 | 0.018300 | MENU_QUERY / 0.922306 | PRODUCT_RANKING / 0.915196 | 0.007110 | MENU_QUERY → MENU_QUERY | ClarificationRequired → ClarificationRequired |
| SUP-04 | PRODUCT_RANKING / 0.900206 | MENU_QUERY / 0.872812 | 0.027394 | PRODUCT_RANKING / 0.893456 | MENU_QUERY / 0.872402 | 0.021054 | PRODUCT_RANKING → PRODUCT_RANKING | Accepted → Accepted |
| SUP-05 | BUSINESS_SUMMARY / 0.922407 | REVENUE_COMPARISON / 0.904336 | 0.018071 | BUSINESS_SUMMARY / 0.921833 | REVENUE_COMPARISON / 0.896580 | 0.025253 | BUSINESS_SUMMARY → BUSINESS_SUMMARY | ClarificationRequired → Accepted |
| SUP-06 | MY_ORDERS / 0.904811 | WRITE_INTENT_UNSUPPORTED / 0.889479 | 0.015332 | MY_ORDERS / 0.901486 | MENU_QUERY / 0.862041 | 0.039445 | MY_ORDERS → MY_ORDERS | ClarificationRequired → Accepted |
| SUP-07 | UNSUPPORTED_INTENT / 0.899475 | WRITE_INTENT_UNSUPPORTED / 0.896145 | 0.003330 | UNSUPPORTED_INTENT / 0.896053 | REVENUE_COMPARISON / 0.893715 | 0.002337 | UNSUPPORTED_INTENT → UNSUPPORTED_INTENT | ClarificationRequired → ClarificationRequired |
| SUP-08 | REVENUE_COMPARISON / 0.931228 | MY_ORDERS / 0.905093 | 0.026134 | REVENUE_COMPARISON / 0.919060 | WRITE_INTENT_UNSUPPORTED / 0.885199 | 0.033861 | REVENUE_COMPARISON → REVENUE_COMPARISON | ClarificationRequired → ClarificationRequired |
| SUP-09 | REVENUE_COMPARISON / 0.879400 | MY_ORDERS / 0.870602 | 0.008798 | REVENUE_COMPARISON / 0.868482 | BUSINESS_SUMMARY / 0.866879 | 0.001603 | REVENUE_COMPARISON → REVENUE_COMPARISON | ClarificationRequired → ClarificationRequired |
| SUP-10 | REVENUE_QUERY / 0.917928 | REVENUE_COMPARISON / 0.896518 | 0.021411 | REVENUE_QUERY / 0.895860 | REVENUE_COMPARISON / 0.886912 | 0.008948 | REVENUE_QUERY → REVENUE_QUERY | Unsupported → Unsupported |
| SUP-11 | MENU_QUERY / 0.904556 | PRODUCT_RANKING / 0.890049 | 0.014507 | MENU_QUERY / 0.901412 | PRODUCT_RANKING / 0.889870 | 0.011542 | MENU_QUERY → MENU_QUERY | ClarificationRequired → ClarificationRequired |
| SUP-12 | MENU_QUERY / 0.915441 | REVENUE_QUERY / 0.910502 | 0.004939 | MENU_QUERY / 0.902291 | REVENUE_QUERY / 0.896233 | 0.006059 | MENU_QUERY → MENU_QUERY | ClarificationRequired → ClarificationRequired |
| SUP-13 | UNSUPPORTED_INTENT / 0.887561 | MY_SHIFT / 0.852556 | 0.035004 | UNSUPPORTED_INTENT / 0.877673 | MY_SHIFT / 0.842213 | 0.035460 | UNSUPPORTED_INTENT → UNSUPPORTED_INTENT | Unsupported → Unsupported |
| SUP-14 | UNSUPPORTED_INTENT / 0.927233 | MY_ORDERS / 0.901196 | 0.026037 | UNSUPPORTED_INTENT / 0.921732 | MY_ORDERS / 0.878864 | 0.042868 | UNSUPPORTED_INTENT → UNSUPPORTED_INTENT | Unsupported → Unsupported |
| SUP-15 | MY_ORDERS / 0.932828 | WRITE_INTENT_UNSUPPORTED / 0.904312 | 0.028516 | UNSUPPORTED_INTENT / 0.899278 | MY_ORDERS / 0.896818 | 0.002460 | MY_ORDERS → UNSUPPORTED_INTENT | ClarificationRequired → ClarificationRequired |
| SUP-16 | WRITE_INTENT_UNSUPPORTED / 0.926427 | UNSUPPORTED_INTENT / 0.873226 | 0.053201 | WRITE_INTENT_UNSUPPORTED / 0.901349 | PRODUCT_RANKING / 0.870386 | 0.030963 | WRITE_INTENT_UNSUPPORTED → WRITE_INTENT_UNSUPPORTED | Unsupported → Unsupported |
| SUP-17 | REVENUE_COMPARISON / 0.819668 | MY_SHIFT / 0.808513 | 0.011154 | REVENUE_COMPARISON / 0.806323 | REVENUE_QUERY / 0.800616 | 0.005707 | REVENUE_COMPARISON → REVENUE_COMPARISON | Unsupported → Unsupported |
| SUP-18 | MY_ORDERS / 0.915255 | MENU_QUERY / 0.908208 | 0.007047 | MENU_QUERY / 0.907925 | UNSUPPORTED_INTENT / 0.881808 | 0.026117 | MENU_QUERY → MENU_QUERY | ClarificationRequired → ClarificationRequired |

### Câu đổi thứ hạng và quyết định

- **Sai → đúng candidate: CAL-23** loyalty: MY_ORDERS → UNSUPPORTED_INTENT. Mean trả CapabilityNotSupported; max hỏi lại SubjectScopeMismatch. Đây là cải thiện semantic thật trên câu đã biết, không đổi nhãn hoặc nhờ backend từ chối quyền.
- **Đúng → sai candidate: SUP-15** orders người khác: MY_ORDERS → UNSUPPORTED_INTENT. Domain order vẫn có capability self-only; target là vấn đề scope phải giữ riêng. Cả hai hỏi lại, nhưng mean mất nhận diện lĩnh vực và chuyển lý do sang CloseCapabilityCandidates. Không tính là semantic đúng chỉ vì không gọi tool.
- **CAL-17** thiếu history đổi MY_SHIFT → REVENUE_COMPARISON; cả hai vẫn không xác định được ý định từ câu nối tiếp. MissingHistory chặn ở cả hai; không ghi là cải thiện.
- **SUP-18 raw** MY_ORDERS → MENU_QUERY, nhưng effective candidate sau bỏ phủ định đã là MENU_QUERY ở cả hai. Mean effective margin .015974 vẫn dưới .02; chưa tăng accepted và không đổi kết quả cuối.
- **CAL-02** đúng revenue trước/sau, margin .005386 → .026131: hỏi lại → accepted.
- **SUP-05** đúng summary trước/sau, margin .018071 → .025253: hỏi lại → accepted.
- **SUP-06** đúng my-orders trước/sau, margin .015332 → .039445: hỏi lại → accepted.
- **CAL-19/20** đúng nhãn write-unsupported trước/sau nhưng margin .032463 → .002476 và .059149 → .016639: từ chối rõ → hỏi lại. Đây là hai decision mismatch mới; không thêm quyền ghi hoặc tool call nào.
- **CAL-10** vẫn ranking thay menu, **SUP-02** vẫn revenue thay comparison, **SUP-07** vẫn unsupported thay revenue thiếu kỳ, **CAL-21** vẫn comparison cho từ “Doanh thu”. Không chỉnh nhãn để che lỗi.

### Tác động của hai mẫu trong cùng intent

Mean giảm phụ thuộc mẫu cao nhất về mặt phép tính, nhưng không biết mẫu cao đó là “rộng và sai” hay “đúng subtype”. Bằng chứng từ điểm thành phần:

| Câu / nhóm mẫu | Hai cosine cao nhất | Mean | Diễn giải |
|---|---|---:|---|
| CAL-02 / revenue | REV-02 .915022; REV-03 .908089 | .911555 | Hai mẫu hỗ trợ khá nhất quán |
| CAL-02 / summary | SUM-03 .909637; SUM-01 .857354 | .883495 | Mẫu summary rộng bị hạ; runner-up mới comparison .885425, revenue vượt margin |
| SUP-06 / my-orders | ORDER-01 .904811; ORDER-02 .898161 | .901486 | Giữ điểm gần max |
| SUP-06 / write | WRITE-02 .889479; WRITE-01 .831672 | .860576 | Mẫu “xóa đơn” từng cạnh tranh với câu xem đơn; mean hạ nó |
| CAL-23 / my-orders | ORDER-01 .922414; ORDER-02 .830672 | .876543 | Hạ generic self-orders match sai |
| CAL-23 / unsupported | UNSUP-05 .909114; UNSUP-06 .907194 | .908154 | Hai mẫu loyalty hỗ trợ; chọn unsupported đúng |
| SUP-15 / my-orders | ORDER-01 .932828; ORDER-02 .860807 | .896818 | Hạ cả match order đúng lĩnh vực, không chỉ match sai |
| SUP-15 / unsupported | UNSUP-03 .903102; UNSUP-06 .895454 | .899278 | Negative “khách khác” thắng domain orders; taxonomy/scope bị lẫn |
| CAL-19 / write | WRITE-01 .930183; WRITE-02 .858994 | .894589 | Mẫu đổi giá đúng bị kéo bởi mẫu xóa đơn không cùng subtype; ranking mean .892113 làm gap rất hẹp |
| CAL-20 / write | WRITE-02 .968191; WRITE-01 .855194 | .911692 | Mẫu xóa đơn đúng bị kéo bởi mẫu đổi giá; giảm margin dù nhãn vẫn đúng |

Hai mẫu write ở baseline vốn mô tả hai hành động khác nhau; không cần câu đổi giá gần cả mẫu xóa đơn để từ chối đúng. Tương tự self-order và target người khác không thể xử lý ownership bằng mức đồng thuận prototype. Vì vậy kết quả không ủng hộ kết luận mean luôn giảm “nhiễu” một cách an toàn.

### Negative, ngoài phạm vi và intent cạnh tranh

- Loyalty SUP-13/14 vẫn unsupported đúng ở cả hai; CAL-23 được sửa về đúng capability ở mean. CAL-18 inventory và CAL-24 inventory/email vẫn từ chối.
- Weather SUP-17 vẫn có nearest comparison nhưng score max .819668 → mean .806323, đều dưới .85; không bị ép sang tool nhà hàng.
- Negation SUP-10 vẫn NegatedRequest. SUP-18 vẫn giữ menu sau xử lý mệnh đề, nhưng hỏi lại vì margin. Không coi mọi chữ “không” là phủ định.
- Scope SUP-15 đều không phát tool, nhưng mean sai candidate như đã nêu. Quyền backend chưa chạy.
- Write SUP-16 vẫn từ chối đúng; CAL-19/20 trở thành hỏi lại. Không có false accept được quan sát trong các negative, nhưng chất lượng phân loại/từ chối không giữ nguyên.
- Comparison/summary/ranking vẫn cạnh tranh đầy đủ: SUP-02 chưa sửa comparison; SUP-05 summary được nhận đúng; CAL-10 chưa sửa menu/ranking. Không lọc intent khó để làm rộng gap.
- Multi-intent CAL-15/22 vẫn hỏi lại. SUP-12 final vẫn hỏi lại; max vì CloseCandidates, mean vì MultipleIntents: cùng guard, nhưng candidate của từng mệnh đề do scoring thay đổi. Không ghi là thay guard.
- Thiếu kỳ CAL-03/04/06/08/21 và SUP-08 vẫn hỏi lại; thiếu history CAL-17/SUP-09 vẫn hỏi lại. Chưa kiểm tra backend/service hoặc dữ liệu nghiệp vụ.

### Phân bố mới và quyết định không dò ngưỡng thêm

Top-2 mean làm score intent không lớn hơn max của chính intent, nhưng margin có thể tăng hoặc giảm vì cả đối thủ cũng giảm. Trên các candidate đúng thuộc câu hợp lệ:

- CAL: median margin .041309 (max) → .035049 (mean), range mean .000023–.061191.
- SUP: median margin .015332 → .015974, range mean .007110–.039445.
- Score mean nhỏ nhất trong 11 câu CAL hợp lệ là .854853; trong 8 câu SUP hợp lệ là .893456. Không câu hợp lệ nào trong lượt mean bị score floor .85 loại; các trường hợp còn chặn do margin hoặc định tuyến/parser theo candidate sai.

Vì vậy **không mặc định .85/.02 phù hợp cho mean để triển khai**, nhưng cũng không có lý do hạ score threshold trong phép kiểm tra này. Giảm margin có thể tăng coverage, song không sửa được SUP-15/SUP-02 hoặc làm taxonomy write phù hợp hơn. Kết quả ranking/negative đã lẫn lợi ích và hồi quy; không thử thêm ngưỡng để tìm con số đẹp trên CAL/SUP. Giữ nguyên ngưỡng đối chiếu và dừng sau đúng hai phương pháp.

Ba câu accepted tăng (CAL-02, SUP-05, SUP-06) là do gap thay đổi khi **scoring thay**, không do nới threshold hoặc thêm mẫu. Tuy nhiên top-1 trên câu hợp lệ vẫn CAL 10/11 và SUP 7/8, không tăng. Tổng literal effective candidate match vẫn 31/42 ở cả hai (CAL +1, SUP -1), không được dùng coverage tăng để khẳng định khả năng phân biệt intent chung đã tốt hơn.

### Kết luận, source và tái tạo

**Giữ max cosine làm baseline và mặc định của spike.** Top-2 mean có lợi ích cục bộ và tăng accepted đúng trong dữ liệu phát triển, nhưng không cải thiện nhất quán nhận diện/negative, có một câu domain orders từ đúng thành sai và hai write refusal bị yếu đi. Không đổi baseline theo vài case thuận lợi. Giữ mode mean để tái tạo so sánh; dừng thử nghiệm, không thêm thuật toán khác hoặc tích hợp runtime trong lượt này.

Output ignored, riêng từng mode/tập:

- `artifacts/scoring-max-cal.json`, `artifacts/scoring-top2-cal.json`
- `artifacts/scoring-max-sup.json`, `artifacts/scoring-top2-sup.json`

```powershell
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --review-output artifacts/scoring-max-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring top2-mean --review-output artifacts/scoring-top2-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --calibration calibration-supplement.json --review-output artifacts/scoring-max-sup.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring top2-mean --calibration calibration-supplement.json --review-output artifacts/scoring-top2-sup.json
```

CLI nhận đúng `max` hoặc `top2-mean`; ngưỡng nhập theo invariant culture và kiểm tra finite/[0,1]. Chưa chọn ngưỡng deployment hoặc chứng minh model O4 tương đương model gốc. Runtime/provider/orchestrator/permission/tools/frontend, model và database không thay đổi; không gọi API/browser/database/tool nghiệp vụ, không commit/push.

---



## Chẩn đoán coverage và thử nghiệm ba intent — lượt tiếp theo

### Chẩn đoán hoàn tất trước khi sửa source

Kiểm tra lại HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`, source spike và hai output `decision-review-development-v2.json`/`decision-review-supplement-once.json`: baseline vẫn 7 correct accept/24 và 1 correct accept/18. Scorer max theo intent với .85/.02, giữ toàn bộ intent cạnh tranh và negative. Bảng dưới được lập từ output thực tế **trước thay đổi catalog/threshold/guard/nhãn**. “Trước guard” là `Candidate.Status` sau score/margin, trước policy/parser; SUP-18 dùng candidate của mệnh đề dương sau bỏ phủ định, raw whole-sentence còn được giữ tại `Semantic`.

| Câu | Expected intent | Candidate / score | Second intent | Margin | Trước guard | Guard/lý do cuối | Arguments thực tế | Quyết định cuối và nguyên nhân |
|---|---|---|---|---:|---|---|---|---|
| SUP-01: Cho biết số tiền cửa hàng kiếm được trong ngày hiện tại. | REVENUE_QUERY | REVENUE_QUERY / 0.918053 | BUSINESS_SUMMARY | 0.013496 | ClarificationRequired | CloseCandidates | {"period":"today"} | ClarificationRequired; Margin loại candidate đúng; period=today đã lấy được |
| SUP-02: Thu nhập bán hàng hôm qua lệch hôm nay bao nhiêu? | REVENUE_COMPARISON | REVENUE_QUERY / 0.912194 | REVENUE_COMPARISON | 0.015595 | ClarificationRequired | MissingOrAmbiguousPeriod | {} | ClarificationRequired; Xếp sai comparison thành revenue; parser thấy hai kỳ nên chặn đúng cho candidate sai |
| SUP-03: Cho mình bảng giá đồ ăn tại quán nhé. | MENU_QUERY | MENU_QUERY / 0.933862 | PRODUCT_RANKING | 0.018300 | ClarificationRequired | CloseCandidates | {} | ClarificationRequired; Margin loại menu đúng; category không bắt buộc |
| SUP-04: Những món dẫn đầu về lượng tiêu thụ tháng này là gì? | PRODUCT_RANKING | PRODUCT_RANKING / 0.900206 | MENU_QUERY | 0.027394 | Accepted | ReadyForBackendChecks | {"period":"month"} | Accepted; Accepted đúng; month đã lấy được |
| SUP-05: Tổng hợp hoạt động kinh doanh tuần này giúp mình. | BUSINESS_SUMMARY | BUSINESS_SUMMARY / 0.922407 | REVENUE_COMPARISON | 0.018071 | ClarificationRequired | CloseCandidates | {"period":"this_week"} | ClarificationRequired; Margin loại summary đúng; this_week đã lấy được; ngoài ba intent thử |
| SUP-06: Mình từng mua những đơn nào ở đây? | MY_ORDERS | MY_ORDERS / 0.904811 | WRITE_INTENT_UNSUPPORTED | 0.015332 | ClarificationRequired | CloseCandidates | {} | ClarificationRequired; Margin loại self orders đúng; không cần customer ID từ câu |
| SUP-07: Xin thống kê doanh số giúp mình. | REVENUE_QUERY | UNSUPPORTED_INTENT / 0.899475 | WRITE_INTENT_UNSUPPORTED | 0.003330 | ClarificationRequired | CloseCapabilityCandidates | {} | ClarificationRequired; Xếp sai revenue thành unsupported; thực sự thiếu kỳ, status hỏi lại đúng nhưng sai lý do |
| SUP-08: Mình cần đối chiếu doanh số. | REVENUE_COMPARISON | REVENUE_COMPARISON / 0.931228 | MY_ORDERS | 0.026134 | Accepted | MissingOrUnsupportedComparisonPeriods | {} | ClarificationRequired; Thực sự thiếu cặp kỳ; hỏi lại đúng |
| SUP-09: Thế tháng trước thì sao? | CONTEXT_UNRESOLVED | REVENUE_COMPARISON / 0.879400 | MY_ORDERS | 0.008798 | ClarificationRequired | MissingHistory | {} | ClarificationRequired; Thiếu history; nhãn CONTEXT_UNRESOLVED là trạng thái xử lý |
| SUP-10: Đừng xem tiền bán hàng hôm nay. | REVENUE_QUERY | REVENUE_QUERY / 0.917928 | REVENUE_COMPARISON | 0.021411 | Accepted | NegatedRequest | {} | Unsupported; Phủ định yêu cầu; chặn đúng, không phải thiếu kỳ |
| SUP-11: Có món nào không? | MENU_QUERY | MENU_QUERY / 0.904556 | PRODUCT_RANKING | 0.014507 | ClarificationRequired | CloseCandidates | {} | ClarificationRequired; Margin loại menu đúng; không bị guard phủ định chặn |
| SUP-12: Tiền bán hàng hôm nay bao nhiêu, rồi mở thực đơn giúp mình. | AMBIGUOUS_MULTI_INTENT | MENU_QUERY / 0.915441 | REVENUE_QUERY | 0.004939 | ClarificationRequired | CloseCandidates | {} | ClarificationRequired; Hai ý; guard multi-intent bỏ sót, margin đang bảo vệ; nhãn là trạng thái xử lý |
| SUP-13: Thành viên của tôi hiện tích được bao nhiêu điểm? | UNSUPPORTED_INTENT | UNSUPPORTED_INTENT / 0.887561 | MY_SHIFT | 0.035004 | Unsupported | CapabilityNotSupported | {} | Unsupported; Capability loyalty chưa hỗ trợ; từ chối đúng |
| SUP-14: Kiểm tra điểm thành viên của khách hàng khác giúp mình. | UNSUPPORTED_INTENT | UNSUPPORTED_INTENT / 0.927233 | MY_ORDERS | 0.026037 | Unsupported | CapabilityNotSupported | {} | Unsupported; Capability loyalty chưa hỗ trợ; không chỉ vì ownership |
| SUP-15: Mở danh sách đơn hàng của người khác. | MY_ORDERS | MY_ORDERS / 0.932828 | WRITE_INTENT_UNSUPPORTED | 0.028516 | Accepted | SubjectScopeMismatch | {} | ClarificationRequired; Scope khác người cho self-only tool; chặn đúng, chưa kiểm tra quyền backend |
| SUP-16: Sửa giá cà phê lên hai mươi nghìn. | WRITE_INTENT_UNSUPPORTED | WRITE_INTENT_UNSUPPORTED / 0.926427 | UNSUPPORTED_INTENT | 0.053201 | Unsupported | CapabilityNotSupported | {} | Unsupported; Write chưa hỗ trợ; từ chối đúng |
| SUP-17: Ngày mai thành phố có mưa không? | UNSUPPORTED_INTENT | REVENUE_COMPARISON / 0.819668 | MY_SHIFT | 0.011154 | Unsupported | LowSimilarity | {} | Unsupported; Ngoài capability; score dưới .85, nearest không phải intent đã nhận |
| SUP-18: Không cần xem đơn của tôi, chỉ cho xem thực đơn nhé. | MENU_QUERY | MENU_QUERY / 0.908735 | MY_ORDERS | 0.008525 | ClarificationRequired | CloseCandidates | {} | ClarificationRequired; Bỏ mệnh đề phủ định đúng; margin loại candidate menu của mệnh đề dương |

**Điểm nghẽn trực tiếp trên 8 câu hợp lệ:** SUP-01/03/05/06/11/18 có top-1 đúng nhưng margin dưới .02: **6/8**. SUP-02 xếp sai revenue thay vì comparison: **1/8**. SUP-04 accepted đúng: **1/8**. Không có câu hợp lệ nào bị capability guard chặn nhầm, không có bằng chứng parser bỏ sót tham số trên candidate đúng, và không có câu hợp lệ nào thiếu thông tin thật.

Trong ba intent thu hẹp, 5 câu hợp lệ là SUP-01 (revenue), SUP-03/11/18 (menu), SUP-06 (my orders): **top-1 đúng 5/5 nhưng accepted 0/5**. SUP-02 comparison, SUP-04 ranking, SUP-05 summary vẫn phải giữ làm đối chứng/negative cạnh tranh; không được xóa để tăng margin.

| Nguyên nhân/chức năng chính, mỗi câu tính một nhóm | Số/18 | ID |
|---|---:|---|
| Candidate đúng nhưng bị margin loại | 6 | 01,03,05,06,11,18 |
| Semantic xếp sai lĩnh vực nghiệp vụ | 2 | 02,07 |
| Accepted đúng | 1 | 04 |
| Thiếu tham số thực sự | 1 | 08 |
| Thiếu history | 1 | 09 |
| Phủ định yêu cầu, chặn đúng | 1 | 10 |
| Multi-intent chưa nhận ra đúng lý do, đang nhờ margin chặn | 1 | 12 |
| Capability chưa có, từ chối đúng | 3 | 13,14,16 |
| Scope self-only không phù hợp yêu cầu target, chặn đúng | 1 | 15 |
| Ngoài phạm vi, low-score rejection đúng | 1 | 17 |

Các nhóm lỗi kỹ thuật bổ sung (không cộng trùng vào bảng 18): capability guard chặn nhầm **0/18**; parser bỏ sót tham số trên candidate đúng **0**; threshold score .85 loại một candidate hợp lệ đúng **0/8**; taxonomy không phải single business intent có SUP-09/12 **2/18**. Literal top-1 lệch nhãn ở 5/18 (02,07,09,12,17) nhưng không được coi cả 5 là lỗi model: context/multi không có single tool, còn SUP-17 là nearest bị từ chối vì score thấp. SUP-02 có cả hôm qua và hôm nay, `Periods()` thấy hai kỳ; lỗi là định tuyến revenue thay vì comparison, **không phải parser không thấy dữ liệu**. Parser thu được 3/3 bộ tham số khi candidate đúng và câu có tham số theo tool: SUP-01 today, SUP-04 month, SUP-05 this_week. Nếu tính cả SUP-02 theo intent nghiệp vụ mong đợi thì end-to-end parameter extraction là 3/4, lỗi thứ tư nằm ở routing.

**Giả thuyết có bằng chứng:** max cosine gặp các mẫu rộng của summary/ranking/write khiến câu đúng lĩnh vực chỉ hơn vài phần nghìn; đây là tương tác catalog và margin. Catalog revenue có 3 câu, my-orders chỉ 2 câu, menu có nhiều mẫu nhưng ít giọng nói hội thoại ngắn. “Thiếu mẫu đại diện” là giả thuyết có thể thử, chưa chứng minh chỉ model hay catalog là nguyên nhân gốc. Không có cơ sở thêm guard, sửa parser hoặc tăng threshold từ các số trên. Chưa sửa nhãn nào.

### Thử nghiệm dự kiến sau chẩn đoán

Chỉ thêm hai mẫu diễn đạt cho mỗi MENU_QUERY/REVENUE_QUERY/MY_ORDERS trong **profile thử riêng**; profile baseline giữ nguyên 34 mẫu. Profile thử vẫn chứa mọi baseline intent/negative, giữ nguyên .85/.02, scorer, guards và parser. Không lấy role để lọc catalog, không chép nguyên câu của CAL/SUP vào mẫu. Chạy một lượt profile thử trên cả hai tập đã quan sát, giữ output mới và không vá tiếp trong lượt này nếu kết quả kém. Các tập CAL và SUP nay đều là dữ liệu phát triển; lần thử này không chứng minh khái quát độc lập. Bộ EMB cuối không được đọc/dùng.



### Kết quả một lần thử, dừng và không chỉnh vòng tiếp

Đã build Release spike thành công, 0 warning/0 error, rồi chạy lại `full-baseline` trên cả CAL/SUP. Hai baseline tái lập đúng candidate/status/reason/arguments và score trong sai số <1e-6 so với output bàn giao. Sau đó chạy đúng một lần profile `three-intent-examples` cho mỗi tập. Không thay scorer, threshold/margin, pipeline guard/parser hoặc model trong lượt này.

Source thay đổi: `IntentCatalog.WithThreeIntentExamples` bổ sung 6 mẫu có ID EXP, `DecisionReviewRunner.RunAsync` nhận `--catalog-profile` và ghi tên profile trong output. `IntentCatalog.Examples` 34 mẫu baseline giữ nguyên; mặc định runner vẫn `full-baseline`. Profile thử có đủ 34 mẫu cũ + 6 mẫu mới; không loại intent cạnh tranh hoặc negative nào, không lọc theo role. Hai mẫu menu mô tả lựa chọn/giá và cách hỏi có món gì; hai mẫu revenue mô tả tổng doanh số/hỏi tiền bán; hai mẫu orders mô tả lịch sử mua/tra lại các đơn. Nội dung cụ thể nằm cùng các ID trong source, không sao chép nguyên câu CAL/SUP.

**Kết quả không đủ tốt để thay baseline.** Accepted tăng nhưng xuất hiện hai hồi quy semantic ở SUP-05/SUP-18. Profile thử được giữ để tái tạo chẩn đoán; không được chọn làm mặc định hoặc gọi là cải thiện khái quát.

| Chỉ số, mẫu số giữ nguyên | CAL baseline → thử | SUP baseline → thử |
|---|---|---|
| Literal top-1 trùng nhãn toàn tập | 18/24 → 18/24 | 13/18 → 12/18 |
| Top-1 đúng trên câu hợp lệ đủ thông tin | 10/11 → 10/11 | 7/8 → 5/8 |
| Accepted đúng | 7 → 8 | 1 → 3 |
| Accepted sai | 0 → 0 | 0 → 0 |
| Hỏi lại/từ chối đúng theo status | 12 → 12 | 10 → 10 |
| Câu hợp lệ bị chặn | 4 → 3 | 7 → 5 |
| Decision mismatch | 1 → 1 (CAL-23) | 0 → 0 |
| Coverage accepted trên toàn tập | 7/24 (29.2%) → 8/24 (33.3%) | 1/18 (5.6%) → 3/18 (16.7%) |
| Coverage đúng trên câu hợp lệ | 7/11 (63.6%) → 8/11 (72.7%) | 1/8 (12.5%) → 3/8 (37.5%) |
| Đúng trong accepted | 7/7 → 8/8 | 1/1 → 3/3 |
| Ba intent mục tiêu: top-1 đúng trên câu hợp lệ | 6/7 → 6/7 | 5/5 → 4/5 |
| Ba intent mục tiêu: accepted đúng trên câu hợp lệ | 4/7 → 5/7 | 0/5 → 2/5 |

“Đúng trong accepted” rất ít mẫu, không dùng để tuyên bố chất lượng production. Literal top-1 gồm cả nhãn xử lý context/multi nên ghi riêng với nhóm câu hợp lệ. “Hỏi lại/từ chối đúng theo status” vẫn không khẳng định nội dung hỏi lại đúng lĩnh vực: SUP-12 bị chặn nhờ margin, CAL-23 còn lỗi capability/định tuyến.

#### Tách parser khỏi nhận diện

Không sửa parser vì chưa thấy lỗi lấy tham số trên candidate đúng. Trên CAL, 4 câu ready cần tham số và có candidate đúng (CAL-01/02/05/07) có arguments đúng **4/4 ở cả hai profile**. Trên SUP, khi candidate đúng và câu ready có tham số: baseline **3/3** (01/04/05), profile thử **2/2** (01/04). Nếu tính end-to-end trên toàn bộ 4 câu ready cần tham số (SUP-01/02/04/05) thì **3/4 → 2/4**; giảm do SUP-05 bị đổi intent, không phải parser bị sửa hoặc quên “tuần này”. SUP-02 vẫn có hai kỳ thật nhưng bị chọn revenue thay comparison, nên báo ambiguous period cho candidate sai. Không coi nó là yêu cầu thực sự thiếu thông tin.

Trong ba intent mục tiêu, period của revenue hợp lệ vẫn được lấy: CAL-01/02 và SUP-01 đều today. SUP-07 thật sự chưa có kỳ nên hỏi lại; prototype mới làm candidate chuyển về revenue, **không tự thêm today**. MENU_QUERY chung không cần category; các record menu giữ arguments `{}`. MY_ORDERS không lấy customer ID từ câu; record có arguments `{}`, quyền/ownership còn chờ backend. SUP-15 (đơn người khác) giữ `SubjectScopeMismatch`, không chuyển thành tool đọc đơn của tôi.

#### Before/after từng câu bộ cũ

Output được giữ tại `artifacts/coverage-baseline-cal.json` và `artifacts/coverage-experiment-cal.json`. Cột before chứa top-1/score/margin; quyết định trước guard và các tầng đầy đủ vẫn có trong JSON. Cột after dưới đây ghi thêm second khác intent và arguments.

| ID | Before top-1 / score / margin | After top-1 / score | After second / score | Margin after | Final before → after | Guard/reason after | Arguments after | Top-1 vs nhãn / kết quả |
|---|---|---|---|---:|---|---|---|---|
| CAL-01 | REVENUE_QUERY / 0.894166 / 0.033416 | REVENUE_QUERY / 0.894166 | MENU_QUERY / 0.860750 | 0.033416 | Accepted → Accepted | ReadyForBackendChecks | {"period":"today"} | Đúng / CorrectAccept |
| CAL-02 | REVENUE_QUERY / 0.915022 / 0.005386 | REVENUE_QUERY / 0.918729 | BUSINESS_SUMMARY / 0.909637 | 0.009092 | ClarificationRequired → ClarificationRequired | CloseCandidates | {"period":"today"} | Đúng / ValidDeferred |
| CAL-03 | REVENUE_COMPARISON / 0.936564 / 0.041123 | REVENUE_COMPARISON / 0.936564 | REVENUE_QUERY / 0.902106 | 0.034458 | ClarificationRequired → ClarificationRequired | MissingOrUnsupportedComparisonPeriods | {} | Đúng / CorrectNonAccept |
| CAL-04 | REVENUE_COMPARISON / 0.938221 / 0.033820 | REVENUE_COMPARISON / 0.938221 | REVENUE_QUERY / 0.904401 | 0.033820 | ClarificationRequired → ClarificationRequired | MissingOrUnsupportedComparisonPeriods | {} | Đúng / CorrectNonAccept |
| CAL-05 | PRODUCT_RANKING / 0.962126 / 0.070923 | PRODUCT_RANKING / 0.962126 | MENU_QUERY / 0.891203 | 0.070923 | Accepted → Accepted | ReadyForBackendChecks | {"period":"week"} | Đúng / CorrectAccept |
| CAL-06 | PRODUCT_RANKING / 0.937267 / 0.028773 | PRODUCT_RANKING / 0.937267 | MENU_QUERY / 0.916779 | 0.020488 | ClarificationRequired → ClarificationRequired | MissingOrAmbiguousPeriod | {} | Đúng / CorrectNonAccept |
| CAL-07 | BUSINESS_SUMMARY / 0.918458 / 0.044481 | BUSINESS_SUMMARY / 0.918458 | PRODUCT_RANKING / 0.873976 | 0.044481 | Accepted → Accepted | ReadyForBackendChecks | {"period":"this_month"} | Đúng / CorrectAccept |
| CAL-08 | BUSINESS_SUMMARY / 0.932428 / 0.036599 | BUSINESS_SUMMARY / 0.932428 | REVENUE_QUERY / 0.924568 | 0.007860 | ClarificationRequired → ClarificationRequired | MissingOrAmbiguousPeriod | {} | Đúng / CorrectNonAccept |
| CAL-09 | MENU_QUERY / 0.913195 / 0.048679 | MENU_QUERY / 0.913195 | PRODUCT_RANKING / 0.864516 | 0.048679 | Accepted → Accepted | ReadyForBackendChecks | {} | Đúng / CorrectAccept |
| CAL-10 | PRODUCT_RANKING / 0.924186 / 0.003119 | PRODUCT_RANKING / 0.924186 | MENU_QUERY / 0.921067 | 0.003119 | ClarificationRequired → ClarificationRequired | MissingOrAmbiguousPeriod | {} | Lệch / ValidDeferred |
| CAL-11 | MY_ORDERS / 0.915194 / 0.008524 | MY_ORDERS / 0.935945 | WRITE_INTENT_UNSUPPORTED / 0.906670 | 0.029274 | ClarificationRequired → Accepted | ReadyForBackendChecks | {} | Đúng / CorrectAccept |
| CAL-12 | MY_ORDERS / 0.895600 / 0.038137 | MY_ORDERS / 0.895600 | WRITE_INTENT_UNSUPPORTED / 0.857463 | 0.038137 | Accepted → Accepted | ReadyForBackendChecks | {} | Đúng / CorrectAccept |
| CAL-13 | MY_SHIFT / 0.877347 / 0.015414 | MY_SHIFT / 0.877347 | REVENUE_QUERY / 0.862671 | 0.014675 | ClarificationRequired → ClarificationRequired | CloseCandidates | {} | Đúng / ValidDeferred |
| CAL-14 | MY_SHIFT / 0.887396 / 0.071103 | MY_SHIFT / 0.887396 | WRITE_INTENT_UNSUPPORTED / 0.816293 | 0.071103 | Accepted → Accepted | ReadyForBackendChecks | {} | Đúng / CorrectAccept |
| CAL-15 | REVENUE_QUERY / 0.927013 / 0.012196 | REVENUE_QUERY / 0.927013 | REVENUE_COMPARISON / 0.914817 | 0.012196 | ClarificationRequired → ClarificationRequired | MultipleIntents | {} | Lệch / CorrectNonAccept |
| CAL-16 | MENU_QUERY / 0.912702 / 0.052874 | MENU_QUERY / 0.912702 | WRITE_INTENT_UNSUPPORTED / 0.859828 | 0.052874 | Accepted → Accepted | ReadyForBackendChecks | {} | Đúng / CorrectAccept |
| CAL-17 | MY_SHIFT / 0.886462 / 0.002481 | MY_SHIFT / 0.886462 | REVENUE_COMPARISON / 0.883981 | 0.002481 | ClarificationRequired → ClarificationRequired | MissingHistory | {} | Lệch / CorrectNonAccept |
| CAL-18 | UNSUPPORTED_INTENT / 0.949023 / 0.065213 | UNSUPPORTED_INTENT / 0.949023 | MY_ORDERS / 0.902337 | 0.046686 | Unsupported → Unsupported | CapabilityNotSupported | {} | Đúng / CorrectNonAccept |
| CAL-19 | WRITE_INTENT_UNSUPPORTED / 0.930183 / 0.032463 | WRITE_INTENT_UNSUPPORTED / 0.930183 | PRODUCT_RANKING / 0.897720 | 0.032463 | Unsupported → Unsupported | CapabilityNotSupported | {} | Đúng / CorrectNonAccept |
| CAL-20 | WRITE_INTENT_UNSUPPORTED / 0.968191 / 0.059149 | WRITE_INTENT_UNSUPPORTED / 0.968191 | UNSUPPORTED_INTENT / 0.909042 | 0.059149 | Unsupported → Unsupported | CapabilityNotSupported | {} | Đúng / CorrectNonAccept |
| CAL-21 | REVENUE_COMPARISON / 0.910197 / 0.020191 | REVENUE_COMPARISON / 0.910197 | REVENUE_QUERY / 0.890005 | 0.020191 | ClarificationRequired → ClarificationRequired | MissingOrUnsupportedComparisonPeriods | {} | Lệch / CorrectNonAccept |
| CAL-22 | REVENUE_COMPARISON / 0.903261 / 0.011722 | REVENUE_COMPARISON / 0.903261 | REVENUE_QUERY / 0.891539 | 0.011722 | ClarificationRequired → ClarificationRequired | MultipleIntents | {} | Lệch / CorrectNonAccept |
| CAL-23 | MY_ORDERS / 0.922414 / 0.013300 | MY_ORDERS / 0.922414 | UNSUPPORTED_INTENT / 0.909114 | 0.013300 | ClarificationRequired → ClarificationRequired | SubjectScopeMismatch | {} | Lệch / DecisionMismatch |
| CAL-24 | UNSUPPORTED_INTENT / 0.922288 / 0.036978 | UNSUPPORTED_INTENT / 0.922288 | WRITE_INTENT_UNSUPPORTED / 0.885309 | 0.036978 | Unsupported → Unsupported | CapabilityNotSupported | {} | Đúng / CorrectNonAccept |

#### Before/after từng câu tập bổ sung đã dùng phát triển

Output giữ tại `artifacts/coverage-baseline-sup.json` và `artifacts/coverage-experiment-sup.json`. Kỳ vọng và câu hỏi giữ nguyên; tập SUP từ đây là dữ liệu phát triển đã quan sát và dùng thêm mẫu, không còn là đánh giá độc lập.

| ID | Before top-1 / score / margin | After top-1 / score | After second / score | Margin after | Final before → after | Guard/reason after | Arguments after | Top-1 vs nhãn / kết quả |
|---|---|---|---|---:|---|---|---|---|
| SUP-01 | REVENUE_QUERY / 0.918053 / 0.013496 | REVENUE_QUERY / 0.928934 | BUSINESS_SUMMARY / 0.904557 | 0.024377 | ClarificationRequired → Accepted | ReadyForBackendChecks | {"period":"today"} | Đúng / CorrectAccept |
| SUP-02 | REVENUE_QUERY / 0.912194 / 0.015595 | REVENUE_QUERY / 0.912194 | REVENUE_COMPARISON / 0.896599 | 0.015595 | ClarificationRequired → ClarificationRequired | MissingOrAmbiguousPeriod | {} | Lệch / ValidDeferred |
| SUP-03 | MENU_QUERY / 0.933862 / 0.018300 | MENU_QUERY / 0.937857 | REVENUE_QUERY / 0.928545 | 0.009312 | ClarificationRequired → ClarificationRequired | CloseCandidates | {} | Đúng / ValidDeferred |
| SUP-04 | PRODUCT_RANKING / 0.900206 / 0.027394 | PRODUCT_RANKING / 0.900206 | MENU_QUERY / 0.872812 | 0.027394 | Accepted → Accepted | ReadyForBackendChecks | {"period":"month"} | Đúng / CorrectAccept |
| SUP-05 | BUSINESS_SUMMARY / 0.922407 / 0.018071 | REVENUE_QUERY / 0.928071 | BUSINESS_SUMMARY / 0.922407 | 0.005664 | ClarificationRequired → ClarificationRequired | UnsupportedPeriod | {} | Lệch / ValidDeferred |
| SUP-06 | MY_ORDERS / 0.904811 / 0.015332 | MY_ORDERS / 0.904811 | WRITE_INTENT_UNSUPPORTED / 0.889479 | 0.015332 | ClarificationRequired → ClarificationRequired | CloseCandidates | {} | Đúng / ValidDeferred |
| SUP-07 | UNSUPPORTED_INTENT / 0.899475 / 0.003330 | REVENUE_QUERY / 0.927776 | UNSUPPORTED_INTENT / 0.899475 | 0.028302 | ClarificationRequired → ClarificationRequired | MissingOrAmbiguousPeriod | {} | Đúng / CorrectNonAccept |
| SUP-08 | REVENUE_COMPARISON / 0.931228 / 0.026134 | REVENUE_COMPARISON / 0.931228 | REVENUE_QUERY / 0.909842 | 0.021385 | ClarificationRequired → ClarificationRequired | MissingOrUnsupportedComparisonPeriods | {} | Đúng / CorrectNonAccept |
| SUP-09 | REVENUE_COMPARISON / 0.879400 / 0.008798 | REVENUE_COMPARISON / 0.879400 | MY_ORDERS / 0.870602 | 0.008798 | ClarificationRequired → ClarificationRequired | MissingHistory | {} | Lệch / CorrectNonAccept |
| SUP-10 | REVENUE_QUERY / 0.917928 / 0.021411 | REVENUE_QUERY / 0.917928 | REVENUE_COMPARISON / 0.896518 | 0.021411 | Unsupported → Unsupported | NegatedRequest | {} | Đúng / CorrectNonAccept |
| SUP-11 | MENU_QUERY / 0.904556 / 0.014507 | MENU_QUERY / 0.919186 | PRODUCT_RANKING / 0.890049 | 0.029136 | ClarificationRequired → Accepted | ReadyForBackendChecks | {} | Đúng / CorrectAccept |
| SUP-12 | MENU_QUERY / 0.915441 / 0.004939 | REVENUE_QUERY / 0.921564 | MENU_QUERY / 0.915441 | 0.006122 | ClarificationRequired → ClarificationRequired | CloseCandidates | {"period":"today"} | Lệch / CorrectNonAccept |
| SUP-13 | UNSUPPORTED_INTENT / 0.887561 / 0.035004 | UNSUPPORTED_INTENT / 0.887561 | MY_SHIFT / 0.852556 | 0.035004 | Unsupported → Unsupported | CapabilityNotSupported | {} | Đúng / CorrectNonAccept |
| SUP-14 | UNSUPPORTED_INTENT / 0.927233 / 0.026037 | UNSUPPORTED_INTENT / 0.927233 | REVENUE_QUERY / 0.906263 | 0.020971 | Unsupported → Unsupported | CapabilityNotSupported | {} | Đúng / CorrectNonAccept |
| SUP-15 | MY_ORDERS / 0.932828 / 0.028516 | MY_ORDERS / 0.932828 | WRITE_INTENT_UNSUPPORTED / 0.904312 | 0.028516 | ClarificationRequired → ClarificationRequired | SubjectScopeMismatch | {} | Đúng / CorrectNonAccept |
| SUP-16 | WRITE_INTENT_UNSUPPORTED / 0.926427 / 0.053201 | WRITE_INTENT_UNSUPPORTED / 0.926427 | REVENUE_QUERY / 0.888619 | 0.037808 | Unsupported → Unsupported | CapabilityNotSupported | {} | Đúng / CorrectNonAccept |
| SUP-17 | REVENUE_COMPARISON / 0.819668 / 0.011154 | REVENUE_COMPARISON / 0.819668 | MY_SHIFT / 0.808513 | 0.011154 | Unsupported → Unsupported | LowSimilarity | {} | Lệch / CorrectNonAccept |
| SUP-18 | MENU_QUERY / 0.908735 / 0.008525 | MY_ORDERS / 0.910679 | MENU_QUERY / 0.908735 | 0.001944 | ClarificationRequired → ClarificationRequired | CloseCandidates | {} | Lệch / ValidDeferred |

### Diễn giải nguyên nhân sau phép thử

- **Điểm nghẽn trực tiếp là margin:** baseline có 6/7 câu hợp lệ bị chặn dù top-1 đúng và không thiếu tham số. Score .85 không loại các câu này. Chưa có căn cứ gọi parser hay capability guard là thủ phạm chính.
- **Catalog + max cosine tạo cạnh tranh mạnh:** SUP-01 nhờ CAT-REV-EXP-01 tăng margin .013496 → .024377; SUP-11 nhờ CAT-MENU-EXP-02 tăng .014507 → .029136. CAL-11 nhờ CAT-ORDER-EXP-02 tăng .008524 → .029274. Đây là ba câu chuyển sang accept đúng trên dữ liệu đã biết.
- **Cùng mẫu cũng làm lệch lĩnh vực:** CAT-REV-EXP-01 thắng summary ở SUP-05 (.928071 so với .922407), khiến parser đi vào nhánh revenue và không hỗ trợ this_week. Ở SUP-03, menu tăng .933862 → .937857 nhưng revenue mới lên .928545, margin lại giảm .018300 → .009312. CAT-ORDER-EXP-02 thắng menu ở SUP-18 trên mệnh đề dương “chỉ cho xem thực đơn nhé.” (.910679 so với .908735); guard phủ định đã tách đúng, classifier mới đổi sang my-orders. SUP-06 vẫn .904811 và margin .015332; thêm hai mẫu orders chưa đại diện tốt câu này.
- **Semantic lỗi chưa giải quyết:** CAL-10 vẫn ranking thay menu, SUP-02 vẫn revenue thay comparison, CAL-21 vẫn comparison cho câu “Doanh thu”, CAL-23 vẫn MY_ORDERS cho loyalty. SUP-07 có cải thiện candidate revenue và hỏi period đúng; không lấy đó để che hai hồi quy mới. Các intent ngoài ba mục tiêu không sửa mẫu/logic, nhưng điểm cạnh tranh vẫn có thể thay đổi khi thêm mẫu mục tiêu, vì thế phải giữ cả chúng trong đánh giá.
- **Chưa quy kết model là nguyên nhân duy nhất:** cùng artifact đã xếp đúng 5/5 câu ready thuộc ba intent ở baseline; vấn đề nhận/không nhận còn do gap của mẫu và margin. Một phép thêm mẫu cho thấy max bị chi phối bởi mẫu rộng; chưa tách được chất lượng model gốc khỏi hình học embedding/catalog bằng kết quả này.

### Kết luận và bước nhỏ tiếp theo

Không đổi .85/.02 hoặc thêm guard để vá số liệu. **Ba intent chưa có cải thiện nhất quán:** revenue và menu tăng accepted ở vài câu đã quan sát, orders chỉ giúp CAL-11, nhưng top-1 ba intent trên các câu SUP hợp lệ giảm 5/5 → 4/5. Còn hỏi lại SUP-03/06, còn xếp sai SUP-18, và summary SUP-05 bị ảnh hưởng cạnh tranh. Vì vậy profile thử **không được đề nghị thay baseline**, runtime vẫn chưa tích hợp.

Bước nhỏ có bằng chứng hỗ trợ cho lượt sau là một phép so sánh **max với tổng hợp hai mẫu tốt nhất theo intent**, giữ cùng full catalog/query/model/.85/.02 và đo riêng false accept cùng margin. Mục đích là kiểm tra tác động một mẫu rộng chi phối, không khẳng định top-2 chắc chắn tốt hơn. Trước khi kết luận cần xem cả các negative đã giữ (loyalty, write, target người khác, comparison/summary/ranking), không chỉ 5 câu mục tiêu. Không thực hiện phép thử thứ hai trong lượt này; không thêm tập lớn hoặc dùng EMB-E01..EMB-E30.

Tái tạo từ `.tools/mock-ai-embedding-spike/`, dùng hai tên output riêng để không ghi đè lịch sử:

```powershell
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --review-output artifacts/coverage-baseline-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --calibration calibration-supplement.json --review-output artifacts/coverage-baseline-sup.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile three-intent-examples --review-output artifacts/coverage-experiment-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile three-intent-examples --calibration calibration-supplement.json --review-output artifacts/coverage-experiment-sup.json
```

Không có thay đổi adapter/inference lifecycle, runtime provider/recognizer/orchestrator/permission/tool/frontend, model/package, database, migration/snapshot hoặc AGENTS/AI_RULES. Không chạy API/browser/database/tool/chat tests, không commit/push/dừng process. Model O4 consistency không chứng minh model gốc tương đương, không đo tài nguyên mới hoặc suy ra hosting.

---


## Review trước: candidate, capability, tham số và quyết định

Phần này thay thế kết luận/định nghĩa chỉ số trong các snapshot lịch sử bên dưới. HEAD kiểm tra lại: `38bbd423b963aaf5779d4ae3356f942ff236643b`; working tree đã dirty trước lượt này và được bảo toàn. Thay đổi chỉ nằm trong project spike, báo cáo và context. Runtime vẫn `IAiProvider → MockAiProvider` tại `services/api/Program.cs`; không chạy API, database, browser hoặc tool nghiệp vụ.

### Kết luận nguyên nhân và hiệu chỉnh báo cáo trước

**CAL-23 là sai chức năng, không phải nhận diện đúng rồi chờ ownership từ chối.** Câu hỏi loyalty bị chọn `MY_ORDERS → get_my_orders`. `GetMyOrderTool.Name/Description/GetSchema/ExecuteAsync` (services/api/src/AI/Tools/Customer/GetMyOrderTool.cs) chỉ xem đơn gần đây của chính khách; schema không nhận target customer ID, ExecuteAsync dùng email/phone trong user context. `Program.cs:331–348` đăng ký 14 tool và `AiToolRegistry.GetAllTools/GetTool` không có loyalty tool. `AiPermissionService.CanExecuteAsync` allowlist customer chỉ menu/my-orders. Câu hỏi loyalty của chính mình cũng chưa được hỗ trợ. Nếu nối candidate cũ vào tool, hệ thống sẽ trả đơn của mình thay vì điểm thưởng người khác; ownership server không sửa được lỗi nhận diện đó.

Spike không hề gọi tool: “tool được chọn” ở đây là mapping candidate được quan sát, không phải một tool đã thực thi. Sau sửa, CAL-23 vẫn có top-1 MY_ORDERS; pipeline trả hỏi lại, không phát tool để chuyển backend. Đây là giảm false accept, **chưa sửa xong nhận diện loyalty của câu này**. Không che lỗi bằng một kết quả từ chối quyền giả định.

Báo cáo trước đã dùng hai cách đếm lẫn nhau: `9/10` là số accepted có **nhãn intent** trùng, không phải số câu đã đủ điều kiện chọn tool. CAL-03 chứa “hôm kia” ngoài cặp kỳ được recognizer hỗ trợ; CAL-04 “hai ngày gần nhất” không đủ hai mốc rõ; CAL-08 “gần đây” chưa phải period allowlist. Theo contract tham số, cả ba phải hỏi lại. Đếm lại cùng output cũ là **6/10 ready đúng**, không phải 9/10. Tương tự snapshot margin .03 có 7/7 nhãn trùng nhưng chỉ **6/7 ready đúng** (CAL-04). Không sửa nhãn cũ để tạo sự cải thiện.

CAL-06 là ranking rõ nhưng thiếu period; CAL-10 là menu/giá, không phải ranking. Mẫu CAT-RANK-03 cũ “món nổi bật” quá rộng, max similarity bị chi phối bởi một mẫu. CAT-AMB-01/02 là câu ghép, đang cạnh tranh trực tiếp với câu đơn như một capability, khiến max/margin khó diễn giải. CAL-15/22 cần quyết định tách câu; CAL-17 cần history; chúng không bắt buộc có một top-1 tool. CAL-21 là revenue thiếu kỳ, semantic top-1 comparison vẫn sai dù kết quả hỏi lại an toàn. CAL-16 cần bỏ đúng mệnh đề phủ định rồi embedding mệnh đề dương, không chặn mọi chữ “không”.

### Những thay đổi tối thiểu

- `IntentCatalog.cs`: 29 mẫu tại đầu lượt, hiện 34 mẫu/10 nhãn lịch sử; CAT-AMB-01/02 được giữ ID nhưng loại khỏi phép xếp hạng capability. Có 32 mẫu hoạt động cho 9 nhãn capability/refusal. Sửa CAT-RANK-03 thành mô tả lượng tiêu thụ; thêm hai mẫu menu/đơn giá và ba mẫu loyalty chưa hỗ trợ. Mẫu loyalty rộng chứa “tài khoản” từng gây nhiễu CAL-12 trong lần phát triển đầu; đã làm cụ thể về điểm thưởng trước khi chạy tập bổ sung. Không chép nguyên câu calibration.
- `IntentScorer.Score`: vẫn max cosine theo intent và margin với **intent khác** đứng thứ hai; không chuyển sang hệ luật keyword hoặc tăng threshold. Xét margin trước khi chốt unsupported để capability sát nhau có thể hỏi lại thay vì từ chối dứt khoát.
- `IntentDecisionPipeline.DecideAsync`: giữ cả `Semantic` toàn câu và `Candidate` sau xử lý mệnh đề; trả status/reason, `IntentRecognized`, `Capability`, `Parameters`, arguments, `BackendAuthorizationRequired`, `RequestedOtherSubject`, `ToolForBackendCheck`. Quyền chưa được kiểm tra; không có kết quả nghiệp vụ.
- Quy tắc phụ chỉ áp dụng cho continuation không có state, phủ định yêu cầu ở đầu mệnh đề, tách mệnh đề rồi embedding từng phần, và parser kỳ allowlist. “Có món nào không?” không khớp phủ định đầu câu. Không đoán period, ID, branch, customer; không nhận role làm đầu vào scoring. Nếu thấy target người khác cho self-only tool, giữ dấu vết target và hỏi lại thay vì đổi sang “của tôi”.
- `calibration.json` giữ nguyên byte/nhãn/ID; `calibration-behavior.json` thêm kỳ vọng quyết định riêng theo contract, không ghi đè lịch sử. `DecisionReviewRunner` chỉ encode/scoring offline, ghi JSON có cả top-1/second và kết quả từng tầng. Không đọc Markdown evaluation, không chạy chat.
- Parser period ánh xạ đúng schema tool: ranking week/month; revenue today/month/year, yesterday dùng mapping đã có `get_business_summary`; summary dùng this_week/this_month…; comparison chỉ cặp today/yesterday, this_week/last_week, this_month/last_month. Chưa có parser tổng quát tên món/category/quantity/status; không claim bao phủ đủ runtime.
- Adapter có lỗi cụ thể: cấu hình từng cho phép concurrency 2–4 dù tokenizer chưa chứng minh thread-safe; constructor có thể leak session nếu validate metadata ném lỗi; empty batch bỏ qua guard dispose. Đã buộc concurrency 1, cleanup session khi constructor thất bại, cho empty batch đi qua lifecycle guard, private hóa Encode và kiểm tra cancellation sau tokenize.

Pipeline: embedding/cache → candidate/margin → policy mệnh đề/capability → parser → hỏi lại/unsupported hoặc ready cho backend. `Accepted` chỉ là **ready cho kiểm tra backend**, không phải được cấp quyền hay nghiệp vụ thành công. `ToolForBackendCheck=null` khi hỏi lại/từ chối. Candidate có thể vẫn sai và được ghi riêng, không bị ghi thành “đúng intent” chỉ vì policy đã chặn.

### Dữ liệu cố định và kỳ vọng 24 case

Tập bổ sung được chốt trước lần chạy: `CALIBRATION-FREEZE.md`, SHA256 `262F921C9B1C3E1C1F94B0FF9D7A2B9D0839658161554FAEA68311B1FD305ED2`; 18 câu có actor/text/expected intent/status/lý do. Bộ cũ SHA256 vẫn `E68AD7CF07EF403D5479F37B8360B12AD0224C0704377EF17B75097F0D7CB5B0`. Không đọc/dùng EMB-E01..EMB-E30 để thiết kế trong lượt này. Không chỉnh semantic code/catalog/threshold sau khi xem output tập bổ sung; chỉ thêm kiểm tra kỹ thuật lifecycle và tài liệu.

`CONTEXT_UNRESOLVED` trong metadata SUP-09 là mô tả kỳ vọng hỏi lại, không thêm intent/tool vào catalog. Nhãn cũ CAL-17 vẫn UNSUPPORTED để theo dõi hồi quy, nhưng kỳ vọng hành vi là hỏi lại vì thiếu history. CAL-23 vẫn UNSUPPORTED vì **capability loyalty chưa có**, độc lập chuyện “khách khác”. SUP-15 cố ý hỏi orders người khác: giữ candidate lĩnh vực order, không cho self-only mapping âm thầm đổi target.

| ID | Câu nhập nguyên văn | Nhãn cũ giữ nguyên | Kỳ vọng quyết định/lý do |
|---|---|---|---|
| CAL-01 | Hôm nay quán đã thu được tổng cộng bao nhiêu? | REVENUE_QUERY | Accepted; Doanh thu và kỳ today rõ. |
| CAL-02 | Tổng tiền bán hàng trong ngày hiện là bao nhiêu vậy? | REVENUE_QUERY | Accepted; Trong ngày = today; cùng intent revenue. |
| CAL-03 | Doanh số hôm nay so với ngày hôm kia thay đổi thế nào? | REVENUE_COMPARISON | ClarificationRequired; hôm kia không thuộc cặp kỳ comparison hiện hỗ trợ; không mặc định hôm qua. |
| CAL-04 | Đối chiếu kết quả bán hàng giữa hai ngày gần nhất. | REVENUE_COMPARISON | ClarificationRequired; hai ngày gần nhất chưa định nghĩa; cần hai mốc rõ. |
| CAL-05 | Trong tuần này món nào được gọi nhiều nhất? | PRODUCT_RANKING | Accepted; Ranking, this_week. |
| CAL-06 | Hãy cho tôi danh sách món theo thứ tự bán chạy. | PRODUCT_RANKING | ClarificationRequired; Ranking nhưng thiếu period. |
| CAL-07 | Tình hình hoạt động tháng này có gì đáng chú ý? | BUSINESS_SUMMARY | Accepted; Summary, this_month. |
| CAL-08 | Tổng quan kinh doanh của cửa hàng gần đây. | BUSINESS_SUMMARY | ClarificationRequired; gần đây không phải kỳ allowlist. |
| CAL-09 | Bây giờ khách có thể gọi những món nào? | MENU_QUERY | Accepted; Menu không cần kỳ. |
| CAL-10 | Danh sách món đang mở bán và giá của chúng. | MENU_QUERY | Accepted; Menu/giá hiện tại, không phải ranking. |
| CAL-11 | Liệt kê các đơn tôi đã đặt trước đây. | MY_ORDERS | Accepted; Self orders không cần kỳ. |
| CAL-12 | Tài khoản của tôi có những order nào? | MY_ORDERS | Accepted; Self orders; backend identity. |
| CAL-13 | Hôm nay tôi làm việc vào khung giờ nào? | MY_SHIFT | Accepted; get_my_shift xem lịch hôm nay, không cần argument. |
| CAL-14 | Ca trực được phân cho tôi là ca gì? | MY_SHIFT | Accepted; Tool my shift mặc định nghiệp vụ là hôm nay. |
| CAL-15 | Doanh thu hôm nay là bao nhiêu và món nào đứng đầu? | AMBIGUOUS_MULTI_INTENT | ClarificationRequired; Hai ý độc lập, hỏi tách. |
| CAL-16 | Tôi không muốn xem doanh thu, chỉ hỏi menu được không? | MENU_QUERY | Accepted; Mệnh đề dương menu sau phủ định revenue. |
| CAL-17 | Còn hôm kia thì sao? | UNSUPPORTED_INTENT | ClarificationRequired; Missing history; giữ nhãn cũ để theo dõi, không đánh là unsupported capability. |
| CAL-18 | Kiểm tra lượng nguyên liệu còn lại. | UNSUPPORTED_INTENT | Unsupported; Không có inventory tool. |
| CAL-19 | Đổi giá món bún bò xuống còn ba mươi nghìn. | WRITE_INTENT_UNSUPPORTED | Unsupported; Write chưa mở. |
| CAL-20 | Xóa giúp tôi đơn cũ này. | WRITE_INTENT_UNSUPPORTED | Unsupported; Write chưa mở. |
| CAL-21 | Doanh thu | REVENUE_QUERY | ClarificationRequired; Thiếu period; nhãn revenue vẫn giữ. |
| CAL-22 | Menu và doanh số tháng trước. | AMBIGUOUS_MULTI_INTENT | ClarificationRequired; Menu và revenue là hai ý. |
| CAL-23 | Tôi muốn xem điểm thưởng của người khác. | UNSUPPORTED_INTENT | Unsupported; Không có loyalty capability; MY_ORDERS là sai chức năng, ownership độc lập. |
| CAL-24 | Gửi báo cáo tồn kho qua email cho tôi. | UNSUPPORTED_INTENT | Unsupported; Inventory/email không có tool. |

### Trước/sau trên tập phát triển cũ

“Trước” là output `artifacts/calibration-results-after.json` của lượt bàn giao (29 mẫu), không phải chạy lại code mới để giả lập code cũ. “Sau” là `artifacts/decision-review-development-v2.json`. Lần phát triển trung gian vẫn giữ `decision-review-development.json`; có 6 correct accept, 5 valid deferred, 12 correct non-accept và 1 decision mismatch trước khi làm mẫu loyalty cụ thể hơn. Các kết quả sau được chỉnh dựa trên 24 câu đã quan sát, **không phải đánh giá độc lập**.

| ID | Trước: top-1 / score / status | Sau: candidate / score | Second khác intent / score | Margin sau | Status / reason sau | Phân loại |
|---|---|---|---|---:|---|---|
| CAL-01 | REVENUE_QUERY / 0.894166 / Accepted | REVENUE_QUERY / 0.894166 | MENU_QUERY / 0.860750 | 0.033416 | Accepted / ReadyForBackendChecks | CorrectAccept |
| CAL-02 | REVENUE_QUERY / 0.915022 / ClarificationRequired | REVENUE_QUERY / 0.915022 | BUSINESS_SUMMARY / 0.909637 | 0.005386 | ClarificationRequired / CloseCandidates | ValidDeferred |
| CAL-03 | REVENUE_COMPARISON / 0.936564 / Accepted | REVENUE_COMPARISON / 0.936564 | REVENUE_QUERY / 0.895441 | 0.041123 | ClarificationRequired / MissingOrUnsupportedComparisonPeriods | CorrectNonAccept |
| CAL-04 | REVENUE_COMPARISON / 0.938221 / Accepted | REVENUE_COMPARISON / 0.938221 | REVENUE_QUERY / 0.904401 | 0.033820 | ClarificationRequired / MissingOrUnsupportedComparisonPeriods | CorrectNonAccept |
| CAL-05 | PRODUCT_RANKING / 0.962126 / Accepted | PRODUCT_RANKING / 0.962126 | MENU_QUERY / 0.891203 | 0.070923 | Accepted / ReadyForBackendChecks | CorrectAccept |
| CAL-06 | AMBIGUOUS_MULTI_INTENT / 0.943581 / ClarificationRequired | PRODUCT_RANKING / 0.937267 | MENU_QUERY / 0.908493 | 0.028773 | ClarificationRequired / MissingOrAmbiguousPeriod | CorrectNonAccept |
| CAL-07 | BUSINESS_SUMMARY / 0.918458 / Accepted | BUSINESS_SUMMARY / 0.918458 | PRODUCT_RANKING / 0.873976 | 0.044481 | Accepted / ReadyForBackendChecks | CorrectAccept |
| CAL-08 | BUSINESS_SUMMARY / 0.932428 / Accepted | BUSINESS_SUMMARY / 0.932428 | PRODUCT_RANKING / 0.895829 | 0.036599 | ClarificationRequired / MissingOrAmbiguousPeriod | CorrectNonAccept |
| CAL-09 | MENU_QUERY / 0.913195 / Accepted | MENU_QUERY / 0.913195 | PRODUCT_RANKING / 0.864516 | 0.048679 | Accepted / ReadyForBackendChecks | CorrectAccept |
| CAL-10 | PRODUCT_RANKING / 0.924186 / ClarificationRequired | PRODUCT_RANKING / 0.924186 | MENU_QUERY / 0.921067 | 0.003119 | ClarificationRequired / MissingOrAmbiguousPeriod | ValidDeferred |
| CAL-11 | MY_ORDERS / 0.915194 / ClarificationRequired | MY_ORDERS / 0.915194 | WRITE_INTENT_UNSUPPORTED / 0.906670 | 0.008524 | ClarificationRequired / CloseCandidates | ValidDeferred |
| CAL-12 | MY_ORDERS / 0.895600 / Accepted | MY_ORDERS / 0.895600 | WRITE_INTENT_UNSUPPORTED / 0.857463 | 0.038137 | Accepted / ReadyForBackendChecks | CorrectAccept |
| CAL-13 | MY_SHIFT / 0.877347 / ClarificationRequired | MY_SHIFT / 0.877347 | REVENUE_QUERY / 0.861932 | 0.015414 | ClarificationRequired / CloseCandidates | ValidDeferred |
| CAL-14 | MY_SHIFT / 0.887396 / Accepted | MY_SHIFT / 0.887396 | WRITE_INTENT_UNSUPPORTED / 0.816293 | 0.071103 | Accepted / ReadyForBackendChecks | CorrectAccept |
| CAL-15 | REVENUE_QUERY / 0.927013 / ClarificationRequired | REVENUE_QUERY / 0.927013 | REVENUE_COMPARISON / 0.914817 | 0.012196 | ClarificationRequired / MultipleIntents | CorrectNonAccept |
| CAL-16 | MENU_QUERY / 0.909175 / ClarificationRequired | MENU_QUERY / 0.912702 | WRITE_INTENT_UNSUPPORTED / 0.859828 | 0.052874 | Accepted / ReadyForBackendChecks | CorrectAccept |
| CAL-17 | MY_SHIFT / 0.886462 / ClarificationRequired | MY_SHIFT / 0.886462 | REVENUE_COMPARISON / 0.883981 | 0.002481 | ClarificationRequired / MissingHistory | CorrectNonAccept |
| CAL-18 | UNSUPPORTED_INTENT / 0.949023 / Unsupported | UNSUPPORTED_INTENT / 0.949023 | MY_ORDERS / 0.883811 | 0.065213 | Unsupported / CapabilityNotSupported | CorrectNonAccept |
| CAL-19 | WRITE_INTENT_UNSUPPORTED / 0.930183 / Unsupported | WRITE_INTENT_UNSUPPORTED / 0.930183 | PRODUCT_RANKING / 0.897720 | 0.032463 | Unsupported / CapabilityNotSupported | CorrectNonAccept |
| CAL-20 | WRITE_INTENT_UNSUPPORTED / 0.968191 / Unsupported | WRITE_INTENT_UNSUPPORTED / 0.968191 | UNSUPPORTED_INTENT / 0.909042 | 0.059149 | Unsupported / CapabilityNotSupported | CorrectNonAccept |
| CAL-21 | REVENUE_COMPARISON / 0.910197 / ClarificationRequired | REVENUE_COMPARISON / 0.910197 | REVENUE_QUERY / 0.890005 | 0.020191 | ClarificationRequired / MissingOrUnsupportedComparisonPeriods | CorrectNonAccept |
| CAL-22 | AMBIGUOUS_MULTI_INTENT / 0.938487 / ClarificationRequired | REVENUE_COMPARISON / 0.903261 | BUSINESS_SUMMARY / 0.891539 | 0.011722 | ClarificationRequired / MultipleIntents | CorrectNonAccept |
| CAL-23 | MY_ORDERS / 0.922414 / Accepted | MY_ORDERS / 0.922414 | UNSUPPORTED_INTENT / 0.909114 | 0.013300 | ClarificationRequired / SubjectScopeMismatch | DecisionMismatch |
| CAL-24 | UNSUPPORTED_INTENT / 0.922288 / Unsupported | UNSUPPORTED_INTENT / 0.922288 | WRITE_INTENT_UNSUPPORTED / 0.885309 | 0.036978 | Unsupported / CapabilityNotSupported | CorrectNonAccept |

### Kiểm tra bổ sung một lượt, không chỉnh tiếp

File output: `artifacts/decision-review-supplement-once.json`. Bảng dưới là quan sát duy nhất của tập bổ sung. Giữ cả những câu hợp lệ bị hỏi lại; không đổi kỳ vọng để khớp output.

| ID | Câu nhập | Expected intent / status | Top-1 / score | Second / score | Margin | Quan sát status / reason | Phân loại |
|---|---|---|---|---|---:|---|---|
| SUP-01 | Cho biết số tiền cửa hàng kiếm được trong ngày hiện tại. | REVENUE_QUERY / Accepted | REVENUE_QUERY / 0.918053 | BUSINESS_SUMMARY / 0.904557 | 0.013496 | ClarificationRequired / CloseCandidates | ValidDeferred |
| SUP-02 | Thu nhập bán hàng hôm qua lệch hôm nay bao nhiêu? | REVENUE_COMPARISON / Accepted | REVENUE_QUERY / 0.912194 | REVENUE_COMPARISON / 0.896599 | 0.015595 | ClarificationRequired / MissingOrAmbiguousPeriod | ValidDeferred |
| SUP-03 | Cho mình bảng giá đồ ăn tại quán nhé. | MENU_QUERY / Accepted | MENU_QUERY / 0.933862 | PRODUCT_RANKING / 0.915562 | 0.018300 | ClarificationRequired / CloseCandidates | ValidDeferred |
| SUP-04 | Những món dẫn đầu về lượng tiêu thụ tháng này là gì? | PRODUCT_RANKING / Accepted | PRODUCT_RANKING / 0.900206 | MENU_QUERY / 0.872812 | 0.027394 | Accepted / ReadyForBackendChecks | CorrectAccept |
| SUP-05 | Tổng hợp hoạt động kinh doanh tuần này giúp mình. | BUSINESS_SUMMARY / Accepted | BUSINESS_SUMMARY / 0.922407 | REVENUE_COMPARISON / 0.904336 | 0.018071 | ClarificationRequired / CloseCandidates | ValidDeferred |
| SUP-06 | Mình từng mua những đơn nào ở đây? | MY_ORDERS / Accepted | MY_ORDERS / 0.904811 | WRITE_INTENT_UNSUPPORTED / 0.889479 | 0.015332 | ClarificationRequired / CloseCandidates | ValidDeferred |
| SUP-07 | Xin thống kê doanh số giúp mình. | REVENUE_QUERY / ClarificationRequired | UNSUPPORTED_INTENT / 0.899475 | WRITE_INTENT_UNSUPPORTED / 0.896145 | 0.003330 | ClarificationRequired / CloseCapabilityCandidates | CorrectNonAccept |
| SUP-08 | Mình cần đối chiếu doanh số. | REVENUE_COMPARISON / ClarificationRequired | REVENUE_COMPARISON / 0.931228 | MY_ORDERS / 0.905093 | 0.026134 | ClarificationRequired / MissingOrUnsupportedComparisonPeriods | CorrectNonAccept |
| SUP-09 | Thế tháng trước thì sao? | CONTEXT_UNRESOLVED / ClarificationRequired | REVENUE_COMPARISON / 0.879400 | MY_ORDERS / 0.870602 | 0.008798 | ClarificationRequired / MissingHistory | CorrectNonAccept |
| SUP-10 | Đừng xem tiền bán hàng hôm nay. | REVENUE_QUERY / Unsupported | REVENUE_QUERY / 0.917928 | REVENUE_COMPARISON / 0.896518 | 0.021411 | Unsupported / NegatedRequest | CorrectNonAccept |
| SUP-11 | Có món nào không? | MENU_QUERY / Accepted | MENU_QUERY / 0.904556 | PRODUCT_RANKING / 0.890049 | 0.014507 | ClarificationRequired / CloseCandidates | ValidDeferred |
| SUP-12 | Tiền bán hàng hôm nay bao nhiêu, rồi mở thực đơn giúp mình. | AMBIGUOUS_MULTI_INTENT / ClarificationRequired | MENU_QUERY / 0.915441 | REVENUE_QUERY / 0.910502 | 0.004939 | ClarificationRequired / CloseCandidates | CorrectNonAccept |
| SUP-13 | Thành viên của tôi hiện tích được bao nhiêu điểm? | UNSUPPORTED_INTENT / Unsupported | UNSUPPORTED_INTENT / 0.887561 | MY_SHIFT / 0.852556 | 0.035004 | Unsupported / CapabilityNotSupported | CorrectNonAccept |
| SUP-14 | Kiểm tra điểm thành viên của khách hàng khác giúp mình. | UNSUPPORTED_INTENT / Unsupported | UNSUPPORTED_INTENT / 0.927233 | MY_ORDERS / 0.901196 | 0.026037 | Unsupported / CapabilityNotSupported | CorrectNonAccept |
| SUP-15 | Mở danh sách đơn hàng của người khác. | MY_ORDERS / ClarificationRequired | MY_ORDERS / 0.932828 | WRITE_INTENT_UNSUPPORTED / 0.904312 | 0.028516 | ClarificationRequired / SubjectScopeMismatch | CorrectNonAccept |
| SUP-16 | Sửa giá cà phê lên hai mươi nghìn. | WRITE_INTENT_UNSUPPORTED / Unsupported | WRITE_INTENT_UNSUPPORTED / 0.926427 | UNSUPPORTED_INTENT / 0.873226 | 0.053201 | Unsupported / CapabilityNotSupported | CorrectNonAccept |
| SUP-17 | Ngày mai thành phố có mưa không? | UNSUPPORTED_INTENT / Unsupported | REVENUE_COMPARISON / 0.819668 | MY_SHIFT / 0.808513 | 0.011154 | Unsupported / LowSimilarity | CorrectNonAccept |
| SUP-18 | Không cần xem đơn của tôi, chỉ cho xem thực đơn nhé. | MENU_QUERY / Accepted | MENU_QUERY / 0.908735 | MY_ORDERS / 0.900210 | 0.008525 | ClarificationRequired / CloseCandidates | ValidDeferred |

### Tổng hợp với mẫu số rõ

“Accept đúng” đòi hỏi đúng candidate **và** kỳ vọng ready cho backend. “False accept” gồm chọn sai capability hoặc chấp nhận câu còn thiếu tham số. “Valid deferred” là câu đã đủ theo contract nhưng bị hỏi lại/từ chối. “Decision mismatch” là đã chặn tool nhưng kiểu phản hồi chưa đúng (CAL-23 cần unsupported loyalty, đang hỏi lại scope). “Correct non-accept” chỉ đánh giá status hỏi lại/từ chối; không có nghĩa top-1 hay nội dung câu hỏi lại đã hoàn hảo.

| Tập / chính sách | Accept đúng | False accept | Non-accept đúng | Hợp lệ bị hỏi lại/từ chối | Decision mismatch | Coverage accepted | Đúng trong accepted |
|---|---:|---:|---:|---:|---:|---|---|
| Snapshot cũ .85/.02, đếm lại theo contract | 6 | 4 | 9 | 5 | 0 | 10/24 = 41.7% | 6/10 = 60.0% |
| Snapshot cũ .85/.03, đếm lại theo contract | 6 | 1 | 11 | 5 | 1 | 7/24 = 29.2% | 6/7 = 85.7% |
| Sau sửa .85/.02, bộ cũ đã dùng phát triển | 7 | 0 | 12 | 4 | 1 | 7/24 = 29.2% | 7/7 = 100% trên tập này |
| Bổ sung .85/.02, chạy một lượt | 1 | 0 | 10 | 7 | 0 | 1/18 = 5.6% | 1/1; quá ít để suy ra chất lượng |

Literal top-1/candidate trùng nhãn: snapshot cũ 18/24, sau 18/24, bổ sung 13/18. Cách đếm này chứa nhãn xử lý multi-intent/context nên không thay thế đánh giá semantic riêng. Trên bộ cũ có 11 câu kỳ vọng ready, sau nhận 7/11; bổ sung có 8 câu kỳ vọng ready, nhận 1/8. Trong 7 valid deferred bổ sung đều là hỏi lại; không có hard refusal trên nhóm hợp lệ này. Không có case nào đã được xác minh backend permission, scope hay dữ liệu nghiệp vụ trong lượt này.

CAL-10 vẫn nhầm ranking và hỏi kỳ sai lĩnh vực; CAL-23 vẫn nhầm orders và hỏi scope thay vì báo capability loyalty chưa hỗ trợ. SUP-02 chọn revenue thay vì comparison. SUP-07 hỏi lại nhưng candidate unsupported thay vì revenue. SUP-12 cần tách hai ý nhưng chỉ được chặn bằng margin; lý do MultipleIntents chưa nhận ra. SUP-11 bị hỏi lại do margin, **không phải bị chặn bởi chữ “không”**. Đây là các lỗi/giới hạn giữ nguyên để review, không thay nhãn.

Không còn báo `9/10` hoặc `7/7` của snapshot cũ như correctness của cả pipeline. Không có accuracy target hay threshold sweep trong lượt này. Giữ .85/.02 làm **mốc phát triển**, chưa đủ dữ liệu/coverage để đề xuất threshold triển khai; tăng margin tiếp chỉ làm hỏi lại nhiều hơn và không chữa lỗi capability.

### Adapter: queue, cancellation, dispose và kiểm chứng

Các candidate đã accepted được ghi riêng dưới đây để đối chiếu parser và mapping. Cờ backend check không có nghĩa backend đã chạy hoặc cho phép. Không có branch/customer ID lấy từ câu hỏi.

| ID | Candidate | Tool chờ backend kiểm tra | Arguments parser | BackendAuthorizationRequired |
|---|---|---|---|---|
| CAL-01 | REVENUE_QUERY | get_revenue | {"period":"today"} | true |
| CAL-05 | PRODUCT_RANKING | get_best_sellers | {"period":"week"} | true |
| CAL-07 | BUSINESS_SUMMARY | get_business_summary | {"period":"this_month"} | true |
| CAL-09 | MENU_QUERY | customer_get_menu | {} | true |
| CAL-12 | MY_ORDERS | get_my_orders | {} | true |
| CAL-14 | MY_SHIFT | get_my_shift | {} | true |
| CAL-16 | MENU_QUERY | customer_get_menu | {} | true |
| SUP-04 | PRODUCT_RANKING | get_best_sellers | {"period":"month"} | true |

### Chi tiết adapter

Giữ 2 semaphore, default queue trong options là 4; các lệnh review/check chọn `MaxConcurrency=1, QueueCapacity=2`. Tổng admission tối đa 3, request vượt giới hạn nhận busy ngay. Tokenizer/session chạy tuần tự, tensor/buffer cục bộ. Cancellation khi đang chờ slot chỉ release capacity đã lấy; release inference slot chỉ ở bên trong nhánh đã acquire. Dispose chặn operation mới và drain operation đã nhận trước khi giải phóng tài nguyên.

`AdapterLifecycleChecks` chạy embedding local và kiểm tra được:
- Đầy queue trả busy.
- Hủy khi đang chờ slot; request khác không bị hỏng, vector sau đó hữu hạn/norm gần 1.
- Hủy khi operation đang giữ slot; không trả vector thành công cho caller đã hủy, slot dùng tiếp được.
- Dispose trong khi operation đang giữ slot; operation hoàn tất và request mới (kể cả empty batch) bị từ chối.

7 quan sát boolean trong `artifacts/adapter-lifecycle-checks.json` đều true, process exit 0. Hai key lịch sử trong output có chữ `NativeWork`, nhưng check chỉ quan sát slot đang giữ, **không định thời điểm chính xác đã vào native Run**. Không coi đây là bằng chứng native inference bị dừng ngay. Inference vẫn synchronous; cancellation có thể chỉ được quan sát sau khi Run trả về. Chưa có stress test production.

Build Release .NET spike: thành công, 0 warning/0 error. Model vẫn O4/tokenizer đã có, không tải lại. Vector cache 34 × 384 × 4 = 52,224 bytes phần float (chưa tính object overhead); session/native working set lớn hơn nhiều, các số local trước khoảng 0.6 GB không chứng minh hosting đủ RAM. Không chạy benchmark mới hoặc suy ra yêu cầu hosting từ lần kiểm tra queue. So khớp .NET/Python cùng O4 chỉ chứng minh nhất quán implementation cho artifact đó, chưa chứng minh tương đương model gốc.

### Cách tái tạo và phần còn mở

Chạy từ `.tools/mock-ai-embedding-spike/`:

```powershell
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore
dotnet run --no-build -c Release -- --decision-review --review-output artifacts/decision-review-development-v2.json
dotnet run --no-build -c Release -- --decision-review --calibration calibration-supplement.json --review-output artifacts/decision-review-supplement-once.json
dotnet run --no-build -c Release -- --adapter-checks
```

Mọi model/cache/build/output dưới đường dẫn ignored; bảng kết quả trong báo cáo là snapshot review được lưu cùng source. Nếu dùng kết quả SUP để chỉnh tiếp, phải đổi trạng thái tập này thành dữ liệu phát triển và chuẩn bị tập mới. Không chạy bộ test chat, không đọc evaluation cuối.

**Kết luận: chưa đủ cơ sở tích hợp thử có quyền chọn/gọi tool, kể cả feature flag tắt mặc định.** Bộ bổ sung chỉ nhận 1/8 câu hợp lệ và còn nhầm capability. Trước khi nối runtime cần:
1. Sửa phân biệt menu/ranking, revenue/comparison và loyalty/order bằng một thay đổi scoring/catalog có giả thuyết và kiểm tra trên tập mới; không chỉ tăng threshold.
2. Thống nhất taxonomy: business intent tách khỏi xử lý multi-intent/history; quyết định parser dựa candidate đúng lĩnh vực, không hỏi period của ranking cho câu menu.
3. Có policy đầu vào dài/truncation, parser filter/category/quantity nếu đưa vào phạm vi thử; không âm thầm bỏ phần câu sau truncation.
4. Chốt capability unavailable, backend permission/scope và model unavailable handling khi thiết kế integration; adapter không cấp quyền.
5. Giữ evaluation cuối cho test tay sau khi có một bản thử đạt điều kiện; xác minh tài nguyên hosting và regression theo role trước rollout.

Có đủ cơ sở dùng spike để thử nhận diện offline và có thể thiết kế chế độ chỉ ghi nhận candidate (shadow) ở lượt sau nếu được yêu cầu. Chế độ đó không được làm thay đổi tool selection. Lượt này chưa tạo feature flag hoặc runtime integration.

---

## Các snapshot lịch sử được bảo toàn

Các phần dưới giữ số liệu/diễn giải của các lượt trước để truy vết; mục review hiện tại ở trên có hiệu lực khi chúng mâu thuẫn.


## Trạng thái và ranh giới

Đã tạo adapter và chạy calibration trong project spike độc lập `.tools/mock-ai-embedding-spike/`. **Chưa đăng ký adapter vào `MockAiProvider`, `MockAiIntentRecognizer`, DI, API hoặc frontend**; hành vi chat hiện tại không thay đổi. Không gọi API, database, tool nghiệp vụ hay bộ test chat.

Spike trước đó chứng minh .NET và Python cho cùng artifact `model_O4.onnx` tạo token IDs/mask và vector nhất quán. Điều đó **không chứng minh** artifact O4 tương đương model gốc trên mọi mặt, cũng không chứng minh chất lượng nhận diện intent tiếng Việt. Kết quả dưới đây chỉ là bằng chứng thành phần trên máy local.

## Adapter đã tạo

Các file trong `.tools/mock-ai-embedding-spike/`:

- `SentenceEmbeddingOptions.cs`: đường dẫn artifact, prefix `query: `, giới hạn 512 token và giới hạn concurrency.
- `SentenceEmbeddingAdapter.cs`: nạp local tokenizer/ONNX một lần; tokenization trực tiếp từ `tokenizer.json`; giữ dấu tiếng Việt; truncation giữ token đặc biệt cuối; masked mean pooling trên `last_hidden_state`; L2 normalization.
- `IntentCatalog.cs`: catalog versioned nhỏ, ID ổn định, actor scope và tool mapping; không có write intent gọi được.
- `IntentScorer.cs`: lấy điểm cao nhất trong các mẫu cùng intent, sau đó xếp top-1 và intent khác đứng thứ hai; trả score, margin và `Accepted`/`ClarificationRequired`/`Unsupported`.
- `CalibrationRunner.cs`, `calibration.json`: chạy offline, cache vector catalog trong memory, ghi JSON kết quả.

`InferenceSession`, tokenizer và admission semaphore được giữ trong adapter. Mặc định `MaxConcurrency=1`: không giả định wrapper tokenizer an toàn khi dùng đồng thời. Hàng đợi là semaphore bounded của các caller đang chờ, không có queue vô hạn; `CancellationToken` được kiểm tra khi chờ và trước inference. Adapter không tải model trong request và báo lỗi rõ khi thiếu artifact/metadata/output sai. Buffer tensor được tạo cục bộ mỗi lần chạy.

Masked mean được thực hiện theo đúng thứ tự: cộng hidden states có `attention_mask=1` → chia số token active → tính norm của mean → chia L2 norm. Self-cosine trong calibration là `1.000000`, xác nhận không lặp lại lỗi chia thừa trong lần chạy đầu.

## Catalog và scoring

Catalog hiện có 25 mẫu cho 10 intent/nhãn thành phần:

| Intent | Tool mapping | Phạm vi |
|---|---|---|
| `REVENUE_QUERY` | `get_revenue` | owner |
| `REVENUE_COMPARISON` | `get_revenue_comparison` | owner |
| `PRODUCT_RANKING` | `get_best_sellers` | owner |
| `BUSINESS_SUMMARY` | `get_business_summary` | owner |
| `MENU_QUERY` | `customer_get_menu` | customer/guest |
| `MY_ORDERS` | `get_my_orders` | customer |
| `MY_SHIFT` | `get_my_shift` | employee; chỉ dùng để kiểm tra catalog hiện có |
| `UNSUPPORTED_INTENT` | không có | từ chối/out-of-scope |
| `WRITE_INTENT_UNSUPPORTED` | không có | ghi nhận write chưa được phép qua Mock chat |
| `AMBIGUOUS_MULTI_INTENT` | không có | yêu cầu tách câu/hỏi lại |

Score mỗi intent là `max(cosine(query, example))` của intent đó. Đây là điểm tương đồng, không phải xác suất và không được hiển thị như phần trăm chắc chắn. Permission, branch, customer identity/ownership và parser tham số vẫn phải chạy độc lập sau nhận diện; catalog không được dùng để ép một câu hỏi trái quyền thành intent khác.

Ngưỡng chạy thử mặc định là `0.85`, margin `0.02`, chỉ là tham số thực nghiệm của spike (`NOT_PRODUCTION_POLICY=true`). Không coi đây là chính sách đã chốt.

## Tập calibration độc lập

`calibration.json` có 24 case, không dùng `EMB-E01..EMB-E30` và không dùng các câu mẫu catalog làm evaluation: 14 paraphrase/intent gần, 2 multi-intent, 1 phủ định, 2 thiếu/ngữ cảnh, 2 out-of-scope, 2 write chưa hỗ trợ và 1 sai phạm vi/ownership.

Kết quả chạy adapter với threshold/margin thực nghiệm:

| ID | Expected | Top-1 | Score | Margin | Trạng thái | Đúng nhãn |
|---|---|---|---:|---:|---|---|
| CAL-01 | REVENUE_QUERY | REVENUE_QUERY | .894166 | .047980 | Accepted | Có |
| CAL-02 | REVENUE_QUERY | REVENUE_QUERY | .915022 | .005386 | ClarificationRequired | Có |
| CAL-03 | REVENUE_COMPARISON | REVENUE_COMPARISON | .936564 | .020710 | Accepted | Có |
| CAL-04 | REVENUE_COMPARISON | REVENUE_COMPARISON | .904762 | .000361 | ClarificationRequired | Có |
| CAL-05 | PRODUCT_RANKING | PRODUCT_RANKING | .962126 | .088866 | Accepted | Có |
| CAL-06 | PRODUCT_RANKING | AMBIGUOUS_MULTI_INTENT | .943581 | .006314 | ClarificationRequired | Không |
| CAL-07 | BUSINESS_SUMMARY | BUSINESS_SUMMARY | .918458 | .042980 | Accepted | Có |
| CAL-08 | BUSINESS_SUMMARY | BUSINESS_SUMMARY | .932428 | .026845 | Accepted | Có |
| CAL-09 | MENU_QUERY | MENU_QUERY | .904188 | .039672 | Accepted | Có |
| CAL-10 | MENU_QUERY | PRODUCT_RANKING | .923092 | .009539 | ClarificationRequired | Không |
| CAL-11 | MY_ORDERS | MY_ORDERS | .915194 | .008524 | ClarificationRequired | Có |
| CAL-12 | MY_ORDERS | MY_ORDERS | .895600 | .038137 | Accepted | Có |
| CAL-13 | MY_SHIFT | MY_SHIFT | .877347 | .015414 | ClarificationRequired | Có |
| CAL-14 | MY_SHIFT | MY_SHIFT | .887396 | .071103 | Accepted | Có |
| CAL-15 | AMBIGUOUS_MULTI_INTENT | REVENUE_QUERY | .927013 | .012196 | ClarificationRequired | Không |
| CAL-16 | MENU_QUERY | MENU_QUERY | .909175 | .014127 | ClarificationRequired | Có |
| CAL-17 | UNSUPPORTED_INTENT | MY_SHIFT | .886462 | .002481 | ClarificationRequired | Không |
| CAL-18 | UNSUPPORTED_INTENT | UNSUPPORTED_INTENT | .949023 | .065213 | Unsupported | Có |
| CAL-19 | WRITE_INTENT_UNSUPPORTED | WRITE_INTENT_UNSUPPORTED | .930183 | .043678 | Unsupported | Có |
| CAL-20 | WRITE_INTENT_UNSUPPORTED | WRITE_INTENT_UNSUPPORTED | .968191 | .059149 | Unsupported | Có |
| CAL-21 | REVENUE_QUERY | REVENUE_COMPARISON | .910197 | .005225 | ClarificationRequired | Không |
| CAL-22 | AMBIGUOUS_MULTI_INTENT | AMBIGUOUS_MULTI_INTENT | .938487 | .035226 | ClarificationRequired | Có |
| CAL-23 | UNSUPPORTED_INTENT | MY_ORDERS | .922414 | .021013 | Accepted | Không |
| CAL-24 | UNSUPPORTED_INTENT | UNSUPPORTED_INTENT | .922288 | .036978 | Unsupported | Có |

Exact top-1 label match: `18/24`. Đây không phải accuracy production: calibration nhỏ, catalog nhỏ, có case employee dùng để kiểm tra nguồn tool, và chưa tách theo actor/permission runtime. Các lỗi đáng chú ý là menu-vs-ranking, thiếu period `CAL-21`, continuation `CAL-17`, multi-intent `CAL-15` và ownership/out-of-scope `CAL-23`.

## Threshold/margin và diễn giải

Với dữ liệu hiện có, `0.85/0.02` là **ứng viên bảo thủ để chạy thêm calibration**, không đủ dữ liệu để chốt chính sách. Nó bắt được nhiều paraphrase nhưng vẫn để một số câu score cao lọt vào nhãn sai; tăng threshold không giải quyết các nhầm lẫn có score cao. Tăng margin sẽ hỏi lại nhiều hơn nhưng giảm khả năng tự chọn sai intent gần nhau. Cần bổ sung negative/out-of-scope và các cặp gần (menu/ranking, revenue/comparison, continuation) rồi đánh giá false accept trước khi chọn điểm chính thức.

Embedding không tự giải quyết phủ định, multi-intent, history/continuation, thiếu period, tên món/số lượng hoặc permission. Cần rule/policy/parser riêng; không dùng embedding để đoán identity, branch, customer hoặc quyền.

## Đo tài nguyên và concurrency

Lệnh chạy:

```powershell
cd .tools/mock-ai-embedding-spike
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore
dotnet run --no-build -c Release -- --calibrate --artifact-root artifacts --calibration calibration.json --calibration-output artifacts/calibration-results.json
```

Kết quả trên máy spike hiện tại:

| Đo | Quan sát |
|---|---:|
| Build | thành công, 0 warning/0 error |
| Adapter initialization | 1,878.592 ms |
| Batch calibration 24 câu | 163.718 ms |
| Catalog self-cosine | 1.000000 |
| 4 caller đồng thời, adapter limit | `MaxConcurrency=1`, 38.043 ms |
| RSS trước adapter | 33,644,544 bytes |
| RSS sau adapter | 627,376,128 bytes |
| RSS tăng quan sát | khoảng 594 MB |

Đây là một lần đo nhỏ trên Windows/.NET local, không phải benchmark production và không chứng minh hosting có đủ RAM. Cần đo lại trên môi trường triển khai, với giới hạn process/container, số request thực tế và thời gian sống process.

## Điều kiện trước khi nối vào chat

1. Giữ riêng catalog, calibration và `EMB-E01..EMB-E30` evaluation; không dùng evaluation để chỉnh threshold.
2. Bổ sung dữ liệu cho intent gần, negative và actor/permission; chốt false-positive budget.
3. Quyết định rõ model-unavailable: fail closed hoặc fallback luật có gắn mode, không giả vờ embedding hoạt động.
4. Tách parser tham số, phủ định, multi-intent và continuation khỏi adapter.
5. Xác minh memory/concurrency trên hosting thật và rà release provenance/license của toàn bộ artifact/package.
6. Chỉ sau các điều kiện trên mới thêm strategy sau `MockAiProvider`; giữ orchestrator, registry, permission, confirmation và service path nguyên vẹn.

Trạng thái handoff: **adapter spike đã tạo và calibration đã chạy; sentence embedding chưa được tích hợp vào chat**.

## Review follow-up: phân tích 24 case và giảm false accept

Phần này được cập nhật sau khi đọc lại từng case và chạy lại cùng `calibration.json`. Không đọc hoặc dùng `EMB-E01..EMB-E30`. Catalog chỉ được bổ sung các câu mới để phân biệt intent; không đổi nhãn calibration và không đưa nguyên văn calibration vào catalog.

### Bảng chi tiết kết quả sau chỉnh catalog

Score là cosine, không phải xác suất. `Second score = Top score - Margin`.

| ID | Tóm tắt câu nhập | Nhãn mong đợi/lý do | Top-1 / score | Hạng 2 / score | Margin | Quyết định 0.85/0.02 | Phân loại |
|---|---|---|---|---|---:|---|---|
| CAL-01 | Quán thu tổng cộng bao nhiêu hôm nay? | REVENUE_QUERY; hỏi doanh thu ngày | REVENUE_QUERY /.894166 | MENU_QUERY /.857143 | .037023 | Chấp nhận | đúng |
| CAL-02 | Tổng tiền bán hàng trong ngày? | REVENUE_QUERY; paraphrase hợp lệ | REVENUE_QUERY /.915022 | BUSINESS_SUMMARY /.909636 | .005386 | Hỏi lại | hợp lệ nhưng margin thấp |
| CAL-03 | Doanh số hôm nay so ngày kia? | REVENUE_COMPARISON; hai mốc | REVENUE_COMPARISON /.936564 | AMBIGUOUS_MULTI_INTENT /.915854 | .020710 | Chấp nhận | đúng, sát ngưỡng |
| CAL-04 | Đối chiếu kết quả giữa hai ngày | REVENUE_COMPARISON | REVENUE_COMPARISON /.938221 | REVENUE_QUERY /.904401 | .033820 | Chấp nhận | đúng |
| CAL-05 | Món được gọi nhiều nhất tuần này | PRODUCT_RANKING | PRODUCT_RANKING /.962126 | MENU_QUERY /.891203 | .070923 | Chấp nhận | đúng |
| CAL-06 | Danh sách món theo thứ tự bán chạy | PRODUCT_RANKING; rõ nghiệp vụ xếp hạng | AMBIGUOUS_MULTI_INTENT /.943581 | PRODUCT_RANKING /.937267 | .006314 | Hỏi lại | nhầm catalog/scoring; từ chối tool là an toàn |
| CAL-07 | Tình hình hoạt động tháng này | BUSINESS_SUMMARY | BUSINESS_SUMMARY /.918458 | PRODUCT_RANKING /.875478 | .042980 | Chấp nhận | đúng |
| CAL-08 | Tổng quan kinh doanh gần đây | BUSINESS_SUMMARY | BUSINESS_SUMMARY /.932428 | AMBIGUOUS_MULTI_INTENT /.905583 | .026845 | Chấp nhận | đúng, margin thấp vừa |
| CAL-09 | Khách có thể gọi món nào? | MENU_QUERY | MENU_QUERY /.913195 | PRODUCT_RANKING /.864516 | .048679 | Chấp nhận | đúng |
| CAL-10 | Món đang mở bán và giá | MENU_QUERY; menu/availability | PRODUCT_RANKING /.924186 | MENU_QUERY /.921067 | .003119 | Hỏi lại | nhầm intent gần; không gọi ranking |
| CAL-11 | Liệt kê các đơn tôi đã đặt | MY_ORDERS | MY_ORDERS /.915194 | WRITE_INTENT_UNSUPPORTED /.906670 | .008524 | Hỏi lại | hợp lệ nhưng false clarification |
| CAL-12 | Order của tài khoản này | MY_ORDERS | MY_ORDERS /.895600 | WRITE_INTENT_UNSUPPORTED /.857463 | .038137 | Chấp nhận | đúng |
| CAL-13 | Hôm nay tôi làm khung giờ nào | MY_SHIFT | MY_SHIFT /.877347 | REVENUE_QUERY /.861933 | .015414 | Hỏi lại | hợp lệ nhưng margin thấp |
| CAL-14 | Ca trực được phân cho tôi | MY_SHIFT | MY_SHIFT /.887396 | WRITE_INTENT_UNSUPPORTED /.816293 | .071103 | Chấp nhận | đúng |
| CAL-15 | Doanh thu và món đứng đầu | AMBIGUOUS_MULTI_INTENT; hai yêu cầu | REVENUE_QUERY /.927013 | REVENUE_COMPARISON /.914817 | .012196 | Hỏi lại | top-1 nhãn sai nhưng quyết định an toàn |
| CAL-16 | Không xem doanh thu, chỉ hỏi menu | MENU_QUERY sau phủ định cục bộ | MENU_QUERY /.909175 | AMBIGUOUS_MULTI_INTENT /.895048 | .014127 | Hỏi lại | phủ định/multi-intent cần parser riêng |
| CAL-17 | Còn hôm kia thì sao? | thiếu history; không đủ để chọn intent | MY_SHIFT /.886462 | REVENUE_COMPARISON /.883981 | .002481 | Hỏi lại | thiếu history; nhãn UNSUPPORTED không phải kết luận capability |
| CAL-18 | Kiểm tra nguyên liệu còn lại | UNSUPPORTED_INTENT; ngoài catalog | UNSUPPORTED_INTENT /.949023 | MY_ORDERS /.883810 | .065213 | Từ chối | đúng |
| CAL-19 | Đổi giá món xuống mức mới | WRITE_INTENT_UNSUPPORTED | WRITE_INTENT_UNSUPPORTED /.930183 | PRODUCT_RANKING /.897720 | .032463 | Từ chối | đúng giới hạn write |
| CAL-20 | Xóa đơn cũ | WRITE_INTENT_UNSUPPORTED | WRITE_INTENT_UNSUPPORTED /.968191 | UNSUPPORTED_INTENT /.909042 | .059149 | Từ chối | đúng giới hạn write |
| CAL-21 | Chỉ nhập “Doanh thu” | REVENUE_QUERY nhưng thiếu period | REVENUE_COMPARISON /.910197 | AMBIGUOUS_MULTI_INTENT /.904972 | .005225 | Hỏi lại | thiếu tham số; top-1 không được gọi tool |
| CAL-22 | Menu và doanh số tháng trước | AMBIGUOUS_MULTI_INTENT | AMBIGUOUS_MULTI_INTENT /.938487 | REVENUE_COMPARISON /.903261 | .035226 | Hỏi lại | đúng quyết định |
| CAL-23 | Xem điểm thưởng của khách khác | yêu cầu dữ liệu người khác; ownership phải kiểm tra riêng | MY_ORDERS /.922414 | MENU_QUERY /.901401 | .021013 | Chấp nhận | false accept; không được suy ra quyền |
| CAL-24 | Gửi báo cáo tồn kho qua email | UNSUPPORTED_INTENT; ngoài tool hiện có | UNSUPPORTED_INTENT /.922288 | WRITE_INTENT_UNSUPPORTED /.885310 | .036978 | Từ chối | đúng giới hạn |

### Sáu case top-1 sai

- **CAL-06**: nhãn nghiệp vụ không sai; câu mô tả ranking rõ. `AMBIGUOUS_MULTI_INTENT` đang có mẫu quá gần ngữ nghĩa với “danh sách món”, nên đây là overlap catalog/scoring. Hỏi lại là fail-safe, nhưng không nên coi câu này là multi-intent.
- **CAL-10**: “món đang mở bán và giá” là menu/availability, không phải ranking. Embedding model không tách được các từ “món/danh sách” khỏi ranking; mẫu menu bổ sung làm score menu tăng nhưng chưa vượt ranking. Đây là lỗi phân biệt intent gần, không phải lý do đổi nhãn.
- **CAL-15**: top-1 là revenue nhưng câu có hai mệnh đề. `ClarificationRequired` là quyết định đúng; top-1 candidate không cần được coi là intent nghiệp vụ cuối. Taxonomy `AMBIGUOUS_MULTI_INTENT` là trạng thái xử lý, không phải tool.
- **CAL-17**: câu nối tiếp thiếu history. Không thể kết luận là unsupported capability hay `MY_SHIFT`; cần hỏi lại câu trước/ngữ cảnh. Việc calibration gắn nhãn unsupported chỉ để tạo negative case, không cấp phép chọn tool.
- **CAL-21**: “Doanh thu” là revenue nhưng thiếu period. Top-1 comparison là lỗi scoring trên câu quá ngắn; parser phải chặn trước tool và hỏi period.
- **CAL-23**: đây là yêu cầu xem loyalty của người khác, nhưng source hiện không có loyalty tool. Ownership là kiểm tra sau nhận diện, không được dùng actor để ép thành menu/đơn hàng. Adapter đang false-accept `MY_ORDERS`; đây là vấn đề capability negative/taxonomy và cần fail closed, không đổi thành quyền hợp lệ.

Không có trường hợp nào được đổi nhãn chỉ để tăng số đúng. Các case CAL-15/CAL-17/CAL-21 có top-1 sai nhưng quyết định hỏi lại vẫn là hành vi an toàn; CAL-23 là false accept thực sự ở ngưỡng .02.

### Thống kê quyết định

Với `score=0.85`, `margin=0.02`, sau catalog bổ sung:

| Nhóm | Số lượng | Mẫu số | Tỷ lệ |
|---|---:|---:|---:|
| Chấp nhận đúng | 9 | 10 accepted | 90.0% precision trên câu đã nhận |
| Chấp nhận sai | 1 | 10 accepted | 10.0% false accept trên câu đã nhận |
| Hỏi lại/từ chối được xem là an toàn | 10 | 14 non-accepted | 71.4% trong nhóm không nhận |
| Câu hợp lệ bị hỏi lại không cần thiết | 4 | 14 non-accepted | CAL-02, 11, 13, 16; cần parser/mẫu tốt hơn |
| Toàn bộ calibration có top-1 nhãn trùng | 18 | 24 | 75.0%; không gọi là accuracy production |

Coverage của cơ chế tự nhận là `10/24 = 41.7%`; trong đó `9/10` quyết định accepted đúng. Nếu dùng margin .03 (chỉ là thử nghiệm nhỏ), accepted còn `7/24 = 29.2%`, `7/7` accepted đúng trên tập này và CAL-23 chuyển thành hỏi lại; đổi lại nhiều câu hợp lệ bị hỏi lại hơn. Không đủ dữ liệu để chọn .03 làm policy.

### Queue, cancellation và dispose

Adapter hiện dùng hai semaphore: `MaxConcurrency=1` cho inference và `QueueCapacity=2` cho request chờ. `capacity = running + waiting`; khi đầy, caller nhận `EmbeddingBusyException` ngay, không chờ vô hạn. Chạy 12 caller với input dài cho kết quả `3 completed / 9 busy`, chứng minh giới hạn hữu hạn trên lần đo này.

Cancellation khi chờ slot được xử lý bằng `WaitAsync(CancellationToken)` và luôn release slot/capacity trong `finally`. Test token đã hủy trước inference nhận `OperationCanceledException`, và slot vẫn dùng được cho request sau. ONNX Runtime call là synchronous; cancellation không dừng native inference giữa chừng. Adapter chỉ kiểm tra cancellation trước và sau inference, nên không tuyên bố native cancellation.

`Dispose()` đánh dấu adapter đóng nhận request mới nhưng trì hoãn dispose session/tokenizer/semaphore đến khi operation đang chạy kết thúc. Điều này tránh release vào semaphore đã dispose khi caller còn hoạt động. Contract vẫn là không khởi tạo request mới sau dispose; chưa có benchmark race stress.

### Kết luận và điều kiện tích hợp thử

Chưa đủ cơ sở để nối vào chat ngay. Adapter đã có bounded admission/cancellation an toàn hơn và catalog phân biệt tốt hơn ở một số cặp, nhưng CAL-10 và CAL-23 vẫn là rủi ro false accept/capability; CAL-06/15/17/21 cần hỏi lại theo policy/parser thay vì ép tool.

Đề xuất bước tiếp theo cụ thể: **chưa tích hợp runtime**. Trước khi cho phép một feature flag thử nghiệm, cần (1) định nghĩa negative/capability policy cho ownership và loyalty, (2) tách pre-parser cho missing history/period và multi-intent, (3) bổ sung tập calibration mới cho menu/ranking/revenue/ownership nhưng không chạm evaluation, và (4) đặt fail-closed khi score cao nhưng intent thuộc capability không có hoặc actor không có quyền. Chỉ khi các điều kiện này được kiểm tra bằng bộ dữ liệu mới mới xem xét feature flag tắt mặc định; vẫn chưa phải production readiness.
