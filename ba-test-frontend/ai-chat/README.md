# Bộ test tay AI Chat — 104 case sau hợp nhất

Bộ này là checklist thủ công chính cho Mock AI, không phải runner và không gọi API tự động. Sau hợp nhất có 104 ID duy nhất:

- `OWN-AI-01..54`: chủ quán và các biến thể read/deny nhỏ cho manager, employee, cashier.
- `CUS-AI-01..35`: customer đăng nhập.
- `QR-AI-01..15`: khách QR/guest.

Các ID kỹ thuật không cộng vào 104 case giao diện: `AI-01..AI-12` ở `../ky-thuat-runtime/README.md`. `FLOW-09`, `FLOW-14` và `REF-07` vẫn là case kỹ thuật/liên vai trò. Các ID cũ vẫn được giữ trong legacy index để truy vết, nhưng không còn là checklist AI hiện hành độc lập.

## Ánh xạ hợp nhất

| ID cũ | ID chính hoặc vị trí mới | Lý do |
|---|---|---|
| ADM-12 | OWN-AI-01..54 | Đã phân rã theo intent và điều kiện, không nhân bản case cũ |
| MGR-10 | OWN-AI-51 + AI-02 | Giữ branch scope ở kỹ thuật, bổ sung một biến thể UI role |
| EMP-09 | OWN-AI-52, OWN-AI-54 + AI-01/02 | `get_my_shift` có positive path; quyền/ownership cần runtime |
| CSH-11 | OWN-AI-53, OWN-AI-54 + AI-01 | Giữ read được phép và denial, không thêm write |
| KIT-12 | OWN-AI-49 + AI-01/02 | Chuyển thành direct endpoint denial; UI kitchen không còn ChatBot |
| CUS-10 | CUS-AI-01..35 | Bao phủ bằng các case customer tách ownership/session/error |
| GST-09 | QR-AI-01..15 | Bao phủ guest scope và dữ liệu không thuộc guest |
| GST-10 | AI-12 | Environment/provider gate cần API/runtime, không giả lập bằng chat |
| GST-15 | QR-AI-07..13 + AI-01 | Guest denial được tách theo loại dữ liệu và quyền |
| AI-01..AI-11 | AI-01..AI-12 | Giữ ở kỹ thuật vì cần API/log/nhiều phiên |
| FLOW-09, FLOW-14, REF-07 | Tài liệu liên vai trò/kỹ thuật tương ứng | Không sao chép case UI |

Write/confirmation vẫn là policy mở: các case chỉ kiểm tra từ chối an toàn của Mock hiện tại, không mô tả tool write đã đăng ký là chức năng chat dùng được.

## Phạm vi

Case dựa trên intent/tool đang có trong source. Mock là bộ luật xác định, không phải mô hình ngôn ngữ tổng quát. `N/A — chưa hỗ trợ` là kết quả đúng đối với write intent hoặc capability chưa được chốt; không coi tool được đăng ký là tool có thể gọi từ chat.

Các case cần Network, log, nhiều session hoặc kiểm tra resource phải chạy theo checklist kỹ thuật tương ứng, không chỉ dựa vào text hiển thị:

- `AI-01..03`, `AI-04..11` trong `ba-test-frontend/ky-thuat-runtime/README.md`.
- `FLOW-09`, `FLOW-14` trong `ba-test-frontend/lien-vai-tro/order-to-payment.md` khi chính sách write/confirmation được chốt.
- `REF-07` trong `ba-test-frontend/chung/refresh-session.md` cho invalidation sau mutation.

## Chuẩn bị an toàn

- Dùng môi trường Development/Mock và dữ liệu kiểm thử riêng: owner, customer A/B, guest QR ở Branch A/B, sản phẩm active/inactive, order của A/B, dữ liệu có và không có doanh thu.
- Không nhập password, JWT, connection string, dữ liệu khách thật hoặc token vào chat/screenshot/log.
- Với owner, chuẩn bị branch A/B và tài khoản owner hợp lệ; branch selector phải được ghi lại trong kết quả.
- Với customer, chuẩn bị customer A có order và customer B có dữ liệu riêng. Không dùng số điện thoại thật trong minh chứng.
- Với QR, chuẩn bị session/token test có cart test; không xóa `guestToken`, QR session hoặc cart chỉ để reset chat.

## Cách ghi kết quả

Giữ trạng thái ban đầu `Chưa test`. Khi test, điền `Kết quả thực tế / minh chứng` bằng commit, role/branch/session loại nào, thời gian, request path/status đã che dữ liệu nhạy cảm, screenshot hoặc correlation ID nếu có. Ghi `AI-001..AI-004`, `SEC-005` hoặc finding liên quan khi phát hiện lệch.

Không ghi PASS chỉ vì UI render được hoặc source có tool. Case API/log/ownership/confirmation phải có bằng chứng đúng loại. Nếu policy write chưa chốt, ghi `Blocked — cần chốt policy`, không tự coi một hành vi là đúng.
