# Quy trình nghiên cứu và chốt từng intent cho AI Chat

## Phạm vi hiện tại

Tài liệu này là **quy tắc nghiên cứu**, chưa chọn role, intent, câu tham chiếu, model mới hoặc cấu hình triển khai. Không đưa dữ liệu nghiên cứu vào AI Chat chỉ vì câu hỏi nghe hợp lý. Không sửa Mock provider, tool registry hay quyền hiện có trong bước chuẩn bị này.

Bốn nguồn câu phải được quản lý riêng:

1. **Câu gợi ý UI:** Nút/câu hiển thị để người dùng bấm trong ChatBot. Bộ cũ đã được gỡ khỏi Admin Web và Customer Web; không dùng UI làm dữ liệu huấn luyện hoặc đánh giá.
2. **Luật Mock runtime:** Cụm từ cố định trong `services/api/src/AI/Services/MockAiIntentRecognizer.cs`. Chúng duy trì chức năng hiện tại, không phải bằng chứng embedding hiểu intent.
3. **Câu mẫu embedding spike:** Catalog và CAL/SUP/EMB ở `.tools/mock-ai-embedding-spike/`. Đây là dữ liệu nghiên cứu lịch sử; nhiều tập đã được quan sát/hiệu chỉnh nên không coi là test độc lập mới.
4. **Case kiểm thử chat:** Checklist `ba-test-frontend/ai-chat/` và bằng chứng E2E khi được chạy. Checklist không tự động trở thành câu tham chiếu.

Các dữ kiện kỹ thuật hiện có và giới hạn được ghi trong `AI-EMBEDDING-CURRENT-STATE-AUDIT.md` và `AI-EMBEDDING-SPIKE-VERIFICATION.md`. Không mở rộng scope chức năng dựa trên số tool đã đăng ký; phải xác minh tool có thể dùng qua route, role gate, service và branch scope thực tế.

## Hồ sơ bắt buộc cho một intent

Mỗi intent có một hồ sơ riêng. Một cách diễn đạt mới của cùng một mục tiêu nghiệp vụ thuộc **cùng intent**, không tự tạo intent ID mới. Hồ sơ tối thiểu gồm:

| Trường | Nội dung phải ghi |
|---|---|
| Identity | Intent ID ổn định, tên, mô tả mục tiêu nghiệp vụ, trạng thái và owner phê duyệt. |
| Access | Role hiện có được phép qua AI route, tool `AllowedRoles`, permission/risk gate; không suy quyền từ UI hoặc Position. |
| Backend | Tool thực tế, input schema, Application Service, endpoint nếu liên quan, read/write/confirmation, trạng thái có gọi được từ provider hiện tại hay không. |
| Scope | Nguồn BranchId/identity đáng tin cậy, nhánh admin global nếu có, điều kiện dữ liệu được service thực thi; không lấy scope từ câu người dùng khi chưa được backend xác thực. |
| Parameters | Trường bắt buộc/tùy chọn, định dạng, giá trị cho phép, nguồn xác minh, câu thiếu/mâu thuẫn tham số và phản hồi làm rõ. |
| Language boundary | Các cách diễn đạt tương đương, câu gần intent khác, câu phủ định/đa ý, ngoài phạm vi và điều kiện từ chối. Chưa điền câu cụ thể ở bước chuẩn bị này. |
| Safety | Điều kiện không gọi tool, abstain, yêu cầu làm rõ, kiểm tra quyền trước thực thi, confirmation nếu chức năng ghi được phê duyệt riêng. |
| Evidence | Phiên bản dữ liệu, model/tokenizer hash, scoring/threshold, source commit + trạng thái working tree, lệnh, môi trường, kết quả, giới hạn và phê duyệt. |

Hồ sơ phải tách **intent recognition** (muốn làm gì), **entity/parameter extraction** (đối tượng/kỳ/chi nhánh/điều kiện), và **tool execution** (backend có thực hiện đúng quyền và scope). Điểm cosine hoặc gọi được tool không thay cho kiểm chứng hai phần còn lại.

## Quy trình tuần tự cho một intent

Chỉ mở intent kế tiếp sau khi intent đang nghiên cứu có quyết định rõ ràng `APPROVED` hoặc `NOT APPROVED` và người dùng đồng ý chuyển bước.

1. **Xác minh backend và role.** Đọc route/controller, registry, permission, tool schema, service, dữ liệu branch/ownership và confirmation. Nếu capability không tồn tại hoặc quyền chưa rõ, dừng thiết kế câu và ghi `BLOCKED`.
2. **Chốt hợp đồng nghiên cứu trước khi lấy điểm.** Thống nhất intent ID, phạm vi role, tiêu chí thành công/thất bại, false tool call tối đa, mức abstain và cách tính metric. Quy tắc tiền xử lý/parameter validation phải được định nghĩa riêng. Ghi phiên bản source/model/tokenizer và điều kiện máy.
3. **Thiết kế câu tham chiếu.** Ghi các cách diễn đạt tương đương, biến thể từ vựng/trật tự, câu gần intent khác, phủ định, đa ý, ngoài phạm vi; gắn nhãn và lý do. Không đưa câu vào catalog chính thức trước khi được rà và kiểm thử. Không lấy câu gợi ý UI làm nhãn mặc định.
4. **Tách development và test độc lập trước khi tuning.** Khóa danh sách ID/text/label của test bằng version và checksum, hạn chế người tuning xem score test. Những tập lịch sử đã dùng để hiệu chỉnh chỉ là development/regression. Nếu test bị dùng để sửa model/catalog/threshold, đổi trạng thái tập đó thành development và chuẩn bị test mới.
5. **Đánh giá embedding offline.** Dùng artifact/checksum cố định, ghi token/prefix/pooling, cosine, top candidates, score/margin, thời gian và lỗi. Không gọi API hoặc tool trong phép đo semantic component.
6. **Kiểm tra nhầm lẫn.** So với các intent đã chốt và nhóm ngoài phạm vi; xem từng false accept, false reject, close margin và câu đa ý. Không chọn chỉ các câu dễ để tăng tỷ lệ đạt.
7. **Kiểm tra abstain và yêu cầu làm rõ.** Câu score thấp, sát ngưỡng, thiếu tham số, nhắc đối tượng khác, phủ định hoặc ngoài quyền phải cho quyết định thích hợp trước khi backend có thể được gọi. Ghi cả quyết định sai và vùng chưa biết.
8. **Đánh giá tham số riêng.** Chỉ khi intent được nhận diện đúng mới chấm việc trích thời gian/đối tượng/điều kiện. Kiểm tra thiếu, mơ hồ, xung đột và dữ liệu không xác minh được. Branch/identity vẫn do backend cung cấp và kiểm tra.
9. **Đối chiếu tiêu chí đã phê duyệt.** Báo cáo theo từng tập và lỗi, không chỉ một accuracy tổng. Phân biệt `PASS`, `FAIL`, `BLOCKED`, `NOT RUN`; kết quả development không được quảng bá như test độc lập.
10. **Lưu bằng chứng và xin quyết định người dùng.** Ghi lệnh, source commit, working tree, môi trường, artifact hash, dữ liệu/checksum, kết quả từng case, giới hạn và nhận định. Chỉ khi tiêu chí đạt **và** người dùng đồng ý mới đặt `APPROVED`; còn lại `NOT APPROVED` hoặc tiếp tục nghiên cứu theo phiên bản mới. Không tự suy phê duyệt từ một PASS kỹ thuật.

## Baseline và regression

Khi `APPROVED`, khóa hồ sơ thành baseline bất biến với:

- Intent ID; role/tool/permission/branch mapping; tham số và rule làm rõ/từ chối.
- Phiên bản catalog tham chiếu, development và test độc lập; ID, checksum, nguồn và trạng thái đã/ chưa quan sát.
- Model revision, SHA-256 ONNX/tokenizer, prefix, pooling/normalize, scorer, threshold/margin, abstain policy, phiên bản adapter/source.
- Lệnh và môi trường chạy; metric/test độc lập; lỗi còn được chấp nhận; người và thời điểm phê duyệt; trạng thái `APPROVED` hoặc `NOT APPROVED`.

Không sửa trực tiếp câu hoặc ngưỡng của baseline đã chốt để tăng điểm. Mọi thay đổi tạo **version mới** với changelog, lý do và so sánh trước/sau; baseline cũ giữ để truy vết. Khi bổ sung intent, chạy regression trên toàn bộ baseline đã chốt và báo cáo xung đột giữa intent cũ/mới, gồm quyết định abstain. Nếu xung đột, giữ baseline cũ, đề xuất phiên bản mới và chờ đánh giá/phê duyệt; không âm thầm đổi dữ liệu cũ.

## Điều kiện trước thí nghiệm nhỏ

Ở thời điểm lập tài liệu, **chưa chọn role hay hai intent**. Trước khi bắt đầu cần người dùng phê duyệt phạm vi nghiệp vụ/role, tiêu chí chấp nhận và phương án tập test độc lập. Sau đó mới lập hồ sơ và dữ liệu cho **một intent đầu tiên**, hoàn tất quy trình trên rồi mới chuyển intent tiếp theo. Việc tích hợp vào API, thay đổi Mock/registry, fine-tune hoặc thay đổi database là các nhiệm vụ khác và cần yêu cầu riêng.


## Current status / Superseded by

Current research/approval process; keep as governing process document.
