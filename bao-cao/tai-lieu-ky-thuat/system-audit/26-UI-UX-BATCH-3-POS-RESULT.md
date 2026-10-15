# UI/UX Consistency Fix — Batch 3: POS

## Scope

Batch này chỉ thay đổi presentation và interaction feedback của POS trong `apps/admin-web`. Không thay đổi backend, database, migration, payment business rule, order state machine, Kitchen KDS, Customer QR ordering, Dashboard/Insights hoặc AI.

## 1. Original POS workflow

Source trace từ `apps/admin-web/src/App.tsx` và `apps/admin-web/src/features/pos/pages/POSPage.tsx`:

1. Route `/pos` được mở trong admin web cho `admin`, `manager` và `cashier`.
2. POS lấy branch context, hiển thị khu vực/bàn và có context `Mang về`.
3. Nhân viên chuyển giữa Phòng bàn và Thực đơn, tìm món, lọc category và chọn product.
4. Product có thể có size/topping; lựa chọn được đưa vào cart theo bàn.
5. Cart giữ quantity, note, options, `sentQuantity` và trạng thái bếp. Món đã gửi bếp được phân biệt với món mới.
6. Order được tự động đồng bộ lên API; nhân viên có thể gửi món mới xuống bếp.
7. Có thể mở order đang xử lý, thêm món và gửi phần mới; trạng thái bếp được nạp qua API/SignalR.
8. Thanh toán mở payment modal với hai lựa chọn thực tế trong source: tiền mặt và chuyển khoản/QR. Payment được gửi tới API với tổng authoritative trả về từ backend.
9. POS nhận `PaymentCompleted` qua SignalR và có cập nhật trạng thái kết nối hiển thị cho nhân viên.

## 2. Changes in this batch

### POS page

File được chạm trong batch: `apps/admin-web/src/features/pos/pages/POSPage.tsx`.

- Dùng `notifyFeedback` từ shared UI foundation cho toàn bộ feedback `alert()` trong POS.
- Phân loại feedback thành success, warning và error ở các luồng mở/chốt ca, nhận order, loyalty, đồng bộ order, gửi thanh toán và lỗi thao tác cart.
- Không thay đổi request payload, endpoint, tính tiền, payment method hoặc state transition.
- Không tạo thêm POS-specific component vì các thay đổi hiện tại chưa cần abstraction mới.

## 3. Shared components reused

- `notifyFeedback` và `FeedbackHost` từ `apps/admin-web/src/components/ui/Feedback.tsx`.
- Shared feedback host đã được mount trong `apps/admin-web/src/main.tsx` từ Batch 1/2.
- Các button có style đặc thù POS vẫn giữ nguyên để tránh làm chậm thao tác; không ép POS vào management `PageHeader`/`Table` pattern.

## 4. Product discovery and context UX

- Product card và table selector được chuyển từ clickable `div` thành semantic `button`.
- Bổ sung accessible name cho product/table selection.
- Giữ nguyên category navigation, search, product filtering và branch/table context.
- Không thay fetching, availability rule hoặc product pricing.

## 5. Cart and existing-order UX

- Nút tăng/giảm quantity có kích thước tối thiểu lớn hơn, phù hợp touch interaction.
- Nút xóa item có `aria-label`/`title`; trên mobile luôn hiển thị thay vì phụ thuộc hover, trong khi desktop vẫn giữ hover affordance.
- Giữ nguyên semantics `sentQuantity`: món đã gửi bếp không bị giảm/xóa trái rule; món mới tiếp tục được nhận diện bằng trạng thái hiện tại.
- Existing order flow không bị thay đổi; không thêm state mới.

## 6. Payment UI

Payment modal và hai payment methods hiện có được giữ nguyên. Batch này chỉ thay feedback lỗi/thành công và không thêm phương thức thanh toán hoặc thay đổi business rule.

## 7. Feedback and confirmation

- POS `alert()` trước batch: **25**.
- POS `alert()` sau batch: **0**.
- `window.confirm` vẫn còn cho các thao tác hủy/xóa có tác động cao: đổi chi nhánh khi cart có dữ liệu, hủy order, xóa local cart và xóa món chưa gửi.
- Không tạo ConfirmDialog mới vì nền tảng hiện chưa có confirmation primitive dùng chung đủ ổn định; giữ native confirmation giúp giảm rủi ro thay đổi workflow trong batch này.
- Các notification realtime (`Đơn hàng mới`, `Lịch hẹn mới`) và inline kitchen feedback được giữ nguyên.

## 8. Responsive, touch and accessibility

Các responsive pattern sẵn có được giữ nguyên: layout product/order chia theo chiều dọc trên màn hình nhỏ và hai vùng trên `md`, product grid co giãn, category bar cuộn ngang, payment modal có giới hạn viewport.

Đã cải thiện:

- table/product selection dùng keyboard-accessible button;
- quantity controls có target tối thiểu `min-h-9/min-w-9`;
- remove action có label rõ và không bị ẩn hoàn toàn trên mobile;
- các action icon hiện có tiếp tục giữ `title`/`aria-label`.

Chưa thực hiện E2E visual verification tại các viewport 1366×768, 1024×768, 768px và 390px; kết luận responsive ở đây là static verification từ class/layout source.

## 9. Regression verification

Static checks xác nhận không thay đổi API calls, payload order/payment, calculation, order status, SignalR event hoặc branch/table state logic trong phần thay đổi của batch.

Runtime E2E chưa được chạy, do đó không tuyên bố E2E pass.

## 10. Build and test results

| Check | Result |
|---|---|
| API build: `dotnet build services/api/RestaurantPOS.api.csproj --no-restore` | PASS |
| Backend tests: `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore` | PASS — 177/177 |
| Admin: `npm run build` | PASS — existing large-bundle warning |
| Customer: `npm run build` | PASS |
| `git diff --check` for POS file | PASS |

## 11. Remaining POS issues

1. POS vẫn còn native `window.confirm`; nên xây shared ConfirmDialog sau khi foundation được xác nhận và migrate theo từng workflow.
2. Chưa có runtime visual/E2E test tự động cho các viewport POS nêu trên.
3. Một số empty/error/loading presentation trong POS vẫn là page-specific; có thể chuẩn hóa sâu hơn mà không đụng business logic.
4. POS bundle vẫn nằm trong cảnh báo chunk lớn của admin build.

## 12. Recommended Batch 4

Ưu tiên Kitchen KDS vì đây là workflow realtime kế tiếp và được loại trừ khỏi Batch 3. Sau đó có thể xử lý ConfirmDialog dùng chung và visual/E2E regression cho các màn hình nghiệp vụ.

## Final status

```text
POS UX:
CONSISTENT

POS RESPONSIVE:
PARTIAL

POS MOBILE:
PARTIAL

POS ALERT BEFORE:
25

POS ALERT AFTER:
0

GLOBAL ALERT AFTER:
4

CONFIRM DIALOG:
NOT NEEDED

BUSINESS LOGIC CHANGED:
NO

BACKEND TESTS:
177/177

ADMIN BUILD:
PASS

CUSTOMER BUILD:
PASS

STATIC REGRESSION:
PASS

E2E VERIFIED:
NO

TOP REMAINING POS ISSUES:
1. Native window.confirm chưa được thay bằng shared ConfirmDialog.
2. Chưa có automated viewport/E2E regression.
3. Một số page-specific empty/error/loading states còn có thể chuẩn hóa.
```
