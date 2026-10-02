# UI/UX Consistency Fix — Batch 4: Kitchen KDS

## Scope

Batch này chỉ chỉnh presentation, feedback và interaction accessibility của Kitchen KDS. Không thay đổi backend business logic, database, migration, order state machine, payment, POS, Customer QR ordering hoặc SignalR protocol/event names.

## 1. Original Kitchen workflow

Source trace từ `apps/admin-web/src/App.tsx`, `apps/admin-web/src/features/kitchen/pages/KitchenPage.tsx` và `services/api/src/Application/Services/KitchenService.cs`:

1. Role `kitchen` được điều hướng tới route `/kitchen` và dùng `KitchenNavbar`.
2. KDS đọc `selectedBranchId` từ local storage và gọi `/api/Order/kitchen/active-requests?branchId=...`.
3. Active requests được hiển thị theo thứ tự `CreatedAt`, gồm bàn, request number, thời gian và các món/options.
4. Status thực tế của request là `Pending`, `Preparing`, `Ready`, `Completed`, `Cancelled`.
5. Action hiện tại giữ nguyên transition: `Pending → Preparing`, `Preparing → Ready`, `Ready → Completed`; hủy dùng `Cancelled` sau native confirmation.
6. KDS subscribe `NewOrderRequest` và `RequestStatusUpdated` trên `/kitchenHub`.
7. Khi có order mới, request được đưa vào danh sách và phát âm thanh `/kitchen-order.mp3`; khi status chuyển khỏi active, request được loại khỏi KDS.
8. Reconnect SignalR đặt trạng thái kết nối và fetch lại active requests; polling 8 giây bù các event bị lỡ.
9. KDS có thêm khu vực cập nhật availability của product, nhưng không thay đổi logic gọi API trong batch này.

## 2. Files/components changed

- `apps/admin-web/src/features/kitchen/pages/KitchenPage.tsx`
- `docs/system-audit/27-UI-UX-BATCH-4-KITCHEN-RESULT.md`

Không sửa `KitchenHistoryPage`, `KitchenNavbar`, backend, hub, notifier, controller hoặc API contract.

## 3. Information hierarchy

- Giữ thứ tự ưu tiên: bàn → request number → status → thời gian → món/số lượng → options/notes → action tiếp theo.
- Tên bàn được đặt trong vùng `min-w-0` và có `truncate`/`title` để tránh phá layout khi tên dài.
- Tên món/options được cho phép wrap; options được đặt trong vùng nền cam nhẹ để dễ nhận biết thông tin ảnh hưởng chế biến.
- Quantity không bị co khi card hẹp và có accessible label.

## 4. Status presentation

- Dùng shared `StatusBadge` cho trạng thái request.
- Giữ nguyên source status values và mapping text hiện tại: `Mới`, `Đang chế biến`, `Sẵn sàng`.
- `Pending`/`Preparing` dùng semantic pending tone; `Ready` dùng completed tone.
- Status vẫn có text, border và layout riêng; không phụ thuộc chỉ vào màu.

## 5. Status actions

- Giữ nguyên action semantic hiện tại: `Bắt đầu chế biến`, `Báo sẵn sàng`, `Đã trả món`.
- Giữ `busyIds` và disabled/loading state, không thêm optimistic behavior mới.
- Mỗi action có `type="button"`, accessible label, focus-visible ring và touch target tối thiểu.
- Nút hủy vẫn yêu cầu confirmation và không đổi status value.

## 6. Realtime/SignalR UX

- Không đổi `NewOrderRequest`, `RequestStatusUpdated`, URL `/kitchenHub`, reconnect policy hoặc polling interval.
- Giữ hiển thị connection state và refresh trực tiếp danh sách khi reconnect.
- Không thêm toast cho từng event realtime; request mới/status mới được thể hiện trực tiếp trên KDS.

## 7. Loading, empty and error

- Initial loading vẫn dùng spinner hiện có.
- Empty state vẫn phân biệt với trạng thái loading: “Bếp đang trống” khi không có active request.
- Error inline được chuyển sang shared `Feedback` với role/accessibility và dismiss action thống nhất.
- Refresh button có disabled/loading state.

## 8. Feedback and alerts

- Kitchen `alert()` trước batch: **0**.
- Kitchen `alert()` sau batch: **0**.
- Không tạo success toast cho status transition thường xuyên; card/status/action loading đã cung cấp feedback trực tiếp.
- `window.confirm` cho Cancel vẫn được giữ vì đây là thao tác hủy có tác động và chưa có ConfirmDialog dùng chung ổn định.
- Global `alert()` sau batch: **4**, đều nằm ở `EmployeeAttendance.tsx`; không thuộc phạm vi Kitchen và không thay đổi.

## 9. Responsive and touch UX

- Giữ grid responsive hiện có: 1 cột mặc định, 2 cột từ `md`, 3 cột từ `xl`, 4 cột từ `2xl`.
- Không thêm fixed card width hoặc horizontal scrolling.
- Card content có `min-w-0`, wrapping cho món/options và quantity `shrink-0`.
- Action buttons có `min-h-11`, cancel có `min-w-11`, `touch-manipulation` và không phụ thuộc hover.
- Connection/refresh controls và status actions có focus-visible styling.
- Static source review cho desktop/tablet/mobile đã thực hiện; chưa chạy visual runtime tại từng viewport.

## 10. Accessibility

- Semantic button và `type="button"` cho các action được sửa.
- Accessible labels cho refresh, status transitions, cancel và quantity.
- Icon decorative dùng `aria-hidden` ở các vùng đã chỉnh.
- Status có text qua `StatusBadge`, không dùng màu là tín hiệu duy nhất.
- Disabled/loading state giữ nguyên và được thể hiện trực quan.

## 11. Static regression

Đã xác minh từ source rằng các phần sau không bị thay đổi semantics:

- branch query và active request endpoint;
- `NewOrderRequest`/`RequestStatusUpdated` subscriptions;
- reconnect, cleanup và polling;
- status payload gửi tới API;
- item options/quantity rendering;
- completed/cancelled removal behavior.

Đây là **STATIC VERIFIED**. Không thực hiện runtime E2E nên không tuyên bố E2E verified.

## 12. Build and test results

| Check | Result |
|---|---|
| API build: `dotnet build services/api/RestaurantPOS.api.csproj --no-restore` | PASS |
| Backend tests: `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore` | PASS — 177/177 |
| Admin: `npm run build` | PASS — existing large-bundle warning |
| Customer: `npm run build` | PASS |
| `git diff --check` for Kitchen file | PASS |

## 13. Remaining Kitchen issues

1. Native `window.confirm` vẫn dùng cho Cancel; nên migrate sau khi shared ConfirmDialog được chuẩn hóa.
2. Chưa có automated E2E/visual regression cho SignalR event, reconnect và các viewport 1920/1366/1024/768/390.
3. Product availability controls chưa có per-item pending state riêng; đây là phần có thể cải thiện trong batch sau nếu cần.
4. Admin bundle warning vẫn còn ở build output và không thuộc business regression của KDS.

## 14. Recommended Batch 5

Ưu tiên visual/runtime verification cho các workflow realtime (POS ↔ Kitchen), sau đó chuẩn hóa shared ConfirmDialog và xử lý các màn hình nghiệp vụ còn thiếu loading/error state. Không nên thay đổi SignalR protocol trong batch UI tiếp theo.

## Final status

```text
KITCHEN UX:
CONSISTENT

KITCHEN STATUS UX:
CLEAR

KITCHEN RESPONSIVE:
PASS

KITCHEN TOUCH UX:
PASS

KITCHEN ALERT BEFORE:
0

KITCHEN ALERT AFTER:
0

GLOBAL ALERT AFTER:
4

SIGNALR LOGIC CHANGED:
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

TOP REMAINING KITCHEN ISSUES:
1. Native window.confirm chưa được thay bằng shared ConfirmDialog.
2. Chưa có automated runtime/visual regression cho realtime và viewport.
3. Product availability chưa có per-item pending feedback riêng.
```
