# UI/UX Consistency Fix — Batch 5: Customer Digital Menu / QR Ordering

## Scope

Batch này chỉ chỉnh trải nghiệm customer digital menu/QR ordering trong `apps/customer-web`. Không thay đổi backend, API contract, authentication semantics, guest token, registered customer token, payment, order state machine, POS, Kitchen, database hoặc migration.

## 1. Original QR ordering workflow

Source trace từ `apps/customer-web/src/pages/QRScan.tsx`, `CustomerLogin.tsx`, `DigitalMenu.tsx` và `App.tsx`:

1. `/scan` đọc QR bằng camera; QR chứa URL có `tableId` hoặc chính là table ID, sau đó điều hướng về `/?tableId=...`.
2. Customer app trước hết thực hiện customer context flow trong `App.tsx`.
3. Guest chọn “Tiếp tục với tư cách khách vãng lai”; frontend gọi `POST /api/Auth/customer-token` với guest payload và lưu token/context.
4. Registered customer nhập phone; nếu tồn tại, app lấy customer token; nếu chưa tồn tại, app tạo customer record rồi lấy customer token. UI không yêu cầu password.
5. `DigitalMenu` gọi `/api/Table/{tableId}` để resolve table và branch; branch được cố định theo table khi có QR context.
6. Menu gọi Product và Topping, lọc sản phẩm active/available, dựng categories và cho phép lọc category.
7. Product có size/topping mở configuration modal; product thường được thêm nhanh vào cart.
8. Cart giữ quantity, size, toppings và note; customer có thể chỉnh số lượng hoặc xóa item.
9. Submit mở bước xác nhận, hiển thị cart, table/branch, tổng tiền; `POST /api/Order` gửi payload giữ nguyên options/note.
10. Sau khi tạo order, nếu có table ID, frontend gọi cập nhật table status; sau đó hiển thị success state.

## 2. Guest vs registered customer semantics

- Guest và registered customer vẫn đi qua các flow/token hiện tại trong `CustomerLogin.tsx`.
- Batch này không thêm password, không đổi endpoint `/api/Auth/customer-token`, không đổi localStorage key `token`/`customerInfo` và không trộn guest với registered customer.
- Guest vẫn có thể vào menu và gửi order qua QR; registered customer vẫn giữ loyalty/profile context hiện tại.

## 3. Customer screens/components changed

- `apps/customer-web/src/pages/DigitalMenu.tsx`
- `docs/system-audit/28-UI-UX-BATCH-5-CUSTOMER-QR-RESULT.md`

Không sửa `QRScan.tsx`, `CustomerLogin.tsx`, `App.tsx`, backend hoặc customer authentication code.

## 4. Mobile information architecture

Giữ flow hiện tại nhưng làm rõ thứ tự thao tác:

1. Header hiển thị nhà hàng, bàn và branch khi có QR context.
2. Category navigation nằm sticky, có touch target rõ.
3. Product card giữ tên, ảnh, giá, mô tả, option indicators và action.
4. Cart summary luôn hiển thị khi có món, nằm fixed ở đáy với layout co giãn cho mobile.
5. Branch/table confirmation và product configuration modal được giới hạn theo viewport và có vùng cuộn riêng.

Không ép Customer UI theo Admin management layout.

## 5. QR/table context

- Hiển thị `Bàn: ...` và tên branch khi API resolve được table.
- Giữ branch selection cố định theo table khi QR cung cấp table context.
- Khi table ID invalid hoặc request lỗi, hiển thị feedback rõ thay vì chỉ reset im lặng.
- Không hiển thị token, JWT hoặc internal identifier cho người dùng.

## 6. Category navigation and product discovery

- Category buttons có `type="button"`, touch target tối thiểu, focus-visible ring và vẫn giữ horizontal scrolling.
- Product card co ảnh từ `24` trên mobile lên `28` từ breakpoint lớn hơn, tránh chiếm quá nhiều chiều rộng ở 375–430px.
- Nội dung product column có `min-w-0`; tên/mô tả dài không ép overflow ngang.
- Add, increase, decrease và remove actions có accessible labels.
- Empty category có meaningful empty state thay vì vùng trống.
- Không thay product filtering, availability filtering, price/options logic.

## 7. Product configuration

- Giữ modal size/topping hiện tại.
- Modal được giới hạn bởi `100dvh`, phần nội dung cuộn độc lập và footer action vẫn nằm cuối modal.
- Close, size, topping, quantity và confirm actions là semantic buttons với touch/focus support.
- Không thay cách tính size/topping price hoặc cart item identity.

## 8. Cart UX

- Cart summary mobile có khoảng cách co giãn, button không ép tràn ở viewport hẹp.
- Quantity controls có target tối thiểu và aria label.
- Remove item có aria label/title rõ hơn.
- Branch/order confirmation modal giữ options, toppings, note, item price và total hiện tại.
- Submit button có loading/disabled state hiện hữu và label rõ “GỬI YÊU CẦU GỌI MÓN”.

## 9. Order submission and success

- Không thay duplicate-submit guard `isSubmittingOrder`.
- Không thay order payload hoặc `POST /api/Order`.
- Không dùng browser alert sau submit.
- Success message được đổi thành “Đơn của bạn đã được ghi nhận.”, không overclaim rằng bếp đã bắt đầu chuẩn bị.
- Không invent order number, ETA hoặc tracking state mà source hiện chưa trả về.

## 10. Feedback cleanup

- Customer QR `alert()` trước batch: **5** trong `DigitalMenu.tsx`.
- Customer QR `alert()` sau batch: **0**.
- Lỗi submit, đổi quà, thiếu điểm, lỗi table và lỗi tải menu dùng customer shared `Feedback` inline.
- Lỗi tải menu/table trước đây chỉ console hoặc reset im lặng nay có user-visible message.
- Customer app còn **2** `alert()` trong `ReservationPage.tsx`, ngoài QR ordering scope.
- Admin global count vẫn là **4**, thuộc `EmployeeAttendance.tsx` và không bị thay đổi.

## 11. Loading, empty and error

- Giữ loading spinner của menu.
- Thêm empty state khi category không có product.
- Thêm error state cho menu load và table resolution.
- Feedback có dismiss action và không expose raw exception.
- Branch list, promotion loading và QR camera errors vẫn là các phần có thể chuẩn hóa tiếp nếu cần.

## 12. Responsive results

Static source review đã kiểm tra các rủi ro chính cho 390×844, 375×667, 430×932, 768×1024, 1024×768 và 1366×768:

- không thêm fixed width mới;
- category bar tiếp tục scroll ngang có chủ đích;
- cart bar co padding/button theo mobile;
- product image giảm kích thước trên mobile;
- modal dùng `dvh` và scroll body;
- long product text/options được wrap;
- submit action có touch target rõ.

Chưa chạy visual runtime ở từng viewport, nên đây là static responsive verification, không phải E2E/visual PASS.

## 13. Touch and accessibility

- Category/product/cart/modal actions có `type="button"` ở các vùng đã sửa.
- Icon actions có `aria-label`/focus-visible support.
- Quantity controls đạt target tối thiểu khoảng 40–44px.
- Status/success/error dùng text, không phụ thuộc riêng vào màu.
- Image product vẫn có alt theo product name; decorative modal fallback image vẫn giữ alt rỗng.

## 14. Performance findings

- Không đổi state-management architecture.
- Không reload menu khi cart local thay đổi.
- Không thêm global state hoặc SignalR/polling.
- Menu vẫn polling product mỗi 10 giây như source hiện tại; đây là behavior giữ nguyên, chưa tối ưu trong batch này.

## 15. Static regression

Đã kiểm tra source không thay đổi:

- QR resolution và `tableId` query flow;
- branch/table resolution;
- guest và registered customer token flow;
- menu/product/category fetching;
- size/topping selection;
- cart quantity/remove/note;
- order payload và submit endpoint;
- table status update sau order;
- payment/authentication semantics.

Kết luận: **STATIC VERIFIED**. Chưa chạy camera QR hoặc runtime order E2E nên không tuyên bố E2E verified.

## 16. Build and test results

| Check | Result |
|---|---|
| API build: `dotnet build services/api/RestaurantPOS.api.csproj --no-restore` | PASS |
| Backend tests: `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore` | PASS — 177/177 |
| Admin: `npm run build` | PASS — existing large-bundle warning |
| Customer: `npm run build` | PASS |
| `git diff --check` for DigitalMenu | PASS |

## 17. Remaining Customer UX issues

1. Chưa có runtime visual/E2E test cho camera QR, 375–430px và order submit.
2. Focus trapping/keyboard escape cho các modal chưa được chuẩn hóa đầy đủ.
3. Product menu vẫn render danh sách theo layout hiện có và polling 10 giây; large-menu performance chưa được xử lý trong batch này.
4. Reservation ngoài QR scope vẫn còn 2 browser alerts.

## 18. Recommended next step

Ưu tiên chạy manual/E2E verification với camera QR thật, guest order và registered customer order trên mobile 375–430px; sau đó chuẩn hóa modal focus behavior và xử lý remaining customer Reservation feedback.

## Final status

```text
CUSTOMER QR UX:
CONSISTENT

MOBILE UX:
PARTIAL

TABLE CONTEXT:
CLEAR

PRODUCT DISCOVERY:
PASS

CART UX:
PASS

ORDER SUBMISSION UX:
PASS

GUEST QR FLOW:
PRESERVED

REGISTERED CUSTOMER FLOW:
PRESERVED

CUSTOMER QR ALERT BEFORE:
5

CUSTOMER QR ALERT AFTER:
0

GLOBAL ALERT AFTER:
4 (admin EmployeeAttendance; customer app còn 2 ngoài QR scope)

SIGNALR/POLLING LOGIC CHANGED:
NO

AUTH SEMANTICS CHANGED:
NO

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

TOP REMAINING CUSTOMER UX ISSUES:
1. Chưa có camera QR/order E2E trên mobile thật.
2. Modal focus/keyboard behavior chưa hoàn thiện.
3. Customer Reservation còn 2 alert ngoài QR scope.
```
