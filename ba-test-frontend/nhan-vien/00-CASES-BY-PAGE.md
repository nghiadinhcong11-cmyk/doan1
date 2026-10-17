# Checklist theo trang — employee

Các case được chuyển khỏi checklist legacy và phân loại theo trang/nhóm. Giữ nguyên ID, trạng thái và minh chứng; cập nhật kết quả tại đây.

| Case ID | Trang/nhóm đích |
|---|---|
| EMP-15 | Realtime |
| EMP-16 | Staff resilience |
| EMP-01 | Staff operations |
| EMP-02 | Staff operations |
| EMP-03 | Staff operations |
| EMP-04 | Staff operations |
| EMP-05 | Tables |
| EMP-06 | Session |
| EMP-07 | Inventory |
| EMP-08 | Notifications |
| EMP-10 | Direct URL / authorization |
| EMP-11 | Branch isolation |
| EMP-12 | Runtime errors |
| EMP-13 | Navigation |
| EMP-14 | Mock AI |

## Realtime

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-15 | Notification Center nhÄ‚Â¢n viÄ‚Âªn | Employee Branch A vÄ‚Â  notification mĂ¡Â»â€ºi nĂ¡ÂºÂ¿u Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p | MĂ¡Â»Å¸ chuÄ‚Â´ng desktop/mobile, vÄ‚Â o `/staff/notifications`, mark-read/mark-all, thĂ¡Â»Â­ ID/branch khÄ‚Â¡c quyĂ¡Â»Ân, reconnect vÄ‚Â  logout/login | Badge chĂ¡Â»â€° unread persistent; khÄ‚Â´ng thĂ¡ÂºÂ¥y branch/role ngoÄ‚Â i quyĂ¡Â»Ân; nĂ¡ÂºÂ¿u chĂ†Â°a cÄ‚Â³ producer hĂ¡Â»Â£p lĂ¡Â»â€¡ thÄ‚Â¬ hiĂ¡Â»Æ’n thĂ¡Â»â€¹ rĂ¡Â»â€”ng; click khÄ‚Â´ng tĂ¡Â»Â± mutation | ChĂ†Â°a test | KhÄ‚Â´ng tĂ¡ÂºÂ¡o notification giĂ¡ÂºÂ£ cho employee |

## Staff resilience

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-16 | MÄ‚Â³n/Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi hoÄ‚Â n thÄ‚Â nh | Employee cÄ‚Â¹ng branch cÄ‚Â³ Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi chuyĂ¡Â»Æ’n Completed | HoÄ‚Â n thÄ‚Â nh mĂ¡Â»â„¢t Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi, thĂ¡Â»Â­ lĂ¡ÂºÂ·p/retry vÄ‚Â  mĂ¡Â»Å¸ target tĂ¡Â»Â« chuÄ‚Â´ng | ChĂ¡Â»â€° employee Ă„â€˜Ä‚Âºng branch nhĂ¡ÂºÂ­n; khÄ‚Â´ng bÄ‚Â¡o toÄ‚Â n Ă„â€˜Ă†Â¡n sai; click mĂ¡Â»Å¸ `/staff` vÄ‚Â  khÄ‚Â´ng tĂ¡Â»Â± nhĂ¡ÂºÂ­n/Ă„â€˜Ä‚Â³ng Ă„â€˜Ă†Â¡n; hai user Ă„â€˜Ă¡Â»Âc Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p | ChĂ†Â°a test | Producer backend Ă„â€˜Ä‚Â£ thÄ‚Âªm; chĂ†Â°a kiĂ¡Â»Æ’m tra runtime |

## Staff operations

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-01 | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p vÄ‚Â  tĂ¡Â»â€¢ng quan | TÄ‚Â i khoĂ¡ÂºÂ£n `employee` Branch A | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p, refresh, mĂ¡Â»Å¸ `/staff` | Redirect tĂ¡Â»â€ºi `/staff/profile` sau login theo code, sau Ă„â€˜Ä‚Â³ menu tĂ¡Â»â€¢ng quan hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng; khÄ‚Â´ng vÄ‚Â o management navbar | ChĂ†Â°a test | |
| EMP-02 | BÄ‚Â n vÄ‚Â  gĂ¡Â»Âi mÄ‚Â³n | CÄ‚Â³ bÄ‚Â n trĂ¡Â»â€˜ng vÄ‚Â  mÄ‚Â³n test | MĂ¡Â»Å¸ `/staff/sales`, chĂ¡Â»Ân bÄ‚Â n, thÄ‚Âªm mÄ‚Â³n/topping, sĂ¡Â»Â­a sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng | GiÄ‚Â¡/tĂ¡Â»â€¢ng lĂ¡ÂºÂ¥y tĂ¡Â»Â« backend; dĂ¡Â»Â¯ liĂ¡Â»â€¡u bÄ‚Â n vÄ‚Â  Ă„â€˜Ă†Â¡n Ă„â€˜Ä‚Âºng branch; validation quantity hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng | ChĂ†Â°a test | |
| EMP-03 | GĂ¡Â»Â­i bĂ¡ÂºÂ¿p | CÄ‚Â³ Ă„â€˜Ă†Â¡n Ă„â€˜ang mĂ¡Â»Å¸ | GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n/Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi vÄ‚Â o bĂ¡ÂºÂ¿p, quan sÄ‚Â¡t phĂ¡ÂºÂ£n hĂ¡Â»â€œi | Request Ă„â€˜Ă†Â°Ă¡Â»Â£c tĂ¡ÂºÂ¡o mĂ¡Â»â„¢t lĂ¡ÂºÂ§n, trĂ¡ÂºÂ¡ng thÄ‚Â¡i hiĂ¡Â»Æ’n thĂ¡Â»â€¹; bĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ä‚Âºng items | ChĂ†Â°a test | |
| EMP-04 | NhĂ¡ÂºÂ­n vÄ‚Â  xĂ¡Â»Â­ lÄ‚Â½ yÄ‚Âªu cĂ¡ÂºÂ§u web | CÄ‚Â³ guest/customer tĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u | MĂ¡Â»Å¸ tĂ¡Â»â€¢ng quan/thÄ‚Â´ng bÄ‚Â¡o, nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n hoĂ¡ÂºÂ·c yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p phÄ‚Â©p | ThÄ‚Â´ng bÄ‚Â¡o hiĂ¡Â»Æ’n thĂ¡Â»â€¹; nhĂ¡ÂºÂ­n mĂ¡Â»â„¢t lĂ¡ÂºÂ§n; lĂ¡Â»â€”i API khÄ‚Â´ng xÄ‚Â³a hÄ‚Â ng chĂ¡Â»Â | ChĂ†Â°a test | |

## Tables

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-05 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â¡n | CÄ‚Â³ Ă„â€˜Ă†Â¡n Ă„â€˜ang phĂ¡Â»Â¥c vĂ¡Â»Â¥ | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i theo UI, thĂ¡Â»Â­ refresh giĂ¡Â»Â¯a chĂ¡Â»Â«ng | ChuyĂ¡Â»Æ’n trĂ¡ÂºÂ¡ng thÄ‚Â¡i hĂ¡Â»Â£p lĂ¡Â»â€¡; khÄ‚Â´ng sĂ¡Â»Â­a Ă„â€˜Ă†Â¡n Ă„â€˜Ä‚Â£ thanh toÄ‚Â¡n/hĂ¡Â»Â§y trÄ‚Â¡i quy tĂ¡ÂºÂ¯c | ChĂ†Â°a test | |

## Session

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-06 | HĂ¡Â»â€œ sĂ†Â¡, chĂ¡ÂºÂ¥m cÄ‚Â´ng, lĂ¡Â»â€¹ch | CÄ‚Â³ profile/lĂ¡Â»â€¹ch test | MĂ¡Â»Å¸ `/staff/profile`, `/staff/attendance`, `/staff/schedule` | DĂ¡Â»Â¯ liĂ¡Â»â€¡u cÄ‚Â¡ nhÄ‚Â¢n/lĂ¡Â»â€¹ch Ă„â€˜Ä‚Âºng; khÄ‚Â´ng xem hĂ¡Â»â€œ sĂ†Â¡ ngĂ†Â°Ă¡Â»Âi khÄ‚Â¡c qua URL | ChĂ†Â°a test | |

## Inventory

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-07 | TĂ¡Â»â€¢ng quan kho | CÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u kho Branch A | MĂ¡Â»Å¸ `/staff/inventory`, kiĂ¡Â»Æ’m tra item vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i | ChĂ¡Â»â€° xem/thao tÄ‚Â¡c Ă„â€˜Ă†Â°Ă¡Â»Â£c API cho phÄ‚Â©p; khÄ‚Â´ng tĂ¡Â»Â± suy diĂ¡Â»â€¦n tĂ¡Â»â€œn kho khi API lĂ¡Â»â€”i | ChĂ†Â°a test | |

## Notifications

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-08 | URL cĂ¡ÂºÂ¥u hÄ‚Â¬nh trÄ‚Â¡i quyĂ¡Â»Ân | Ă„ÂÄ‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p employee | MĂ¡Â»Å¸ `/dashboard`, `/employees`, `/settings`, `/settings/receipt`, `/print-templates`, `/branches`, `/pos/invoices` trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p | KhÄ‚Â´ng thĂ¡ÂºÂ¥y bÄ‚Â¡nh rĂ„Æ’ng hoĂ¡ÂºÂ·c page cĂ¡ÂºÂ¥u hÄ‚Â¬nh; route/API mutation bĂ¡Â»â€¹ chĂ¡ÂºÂ·n theo contract; khÄ‚Â´ng bypass bĂ¡ÂºÂ±ng URL hoĂ¡ÂºÂ·c sĂ¡Â»Â­a localStorage role | ChĂ†Â°a test | |

## Kitchen handoff

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|

## Direct URL / authorization

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-10 | DĂ¡Â»Â¯ liĂ¡Â»â€¡u thiĂ¡ÂºÂ¿u vÄ‚Â  thao tÄ‚Â¡c lĂ¡ÂºÂ·p | KhÄ‚Â´ng chĂ¡Â»Ân bÄ‚Â n/mÄ‚Â³n hoĂ¡ÂºÂ·c mĂ¡ÂºÂ¡ng chĂ¡ÂºÂ­m | GĂ¡Â»Â­i form rĂ¡Â»â€”ng, bĂ¡ÂºÂ¥m gĂ¡Â»Â­i nhiĂ¡Â»Âu lĂ¡ÂºÂ§n, refresh khi Ă„â€˜ang gĂ¡Â»Â­i | Validation rÄ‚Âµ; khÄ‚Â´ng tĂ¡ÂºÂ¡o duplicate order/request; loading khÄ‚Â³a Ă„â€˜Ä‚Âºng nÄ‚Âºt | ChĂ†Â°a test | |

## Branch isolation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-11 | HĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n phiÄ‚Âªn | CÄ‚Â³ thĂ¡Â»Æ’ tĂ¡ÂºÂ¡o 401 | GĂ¡Â»Âi thao tÄ‚Â¡c sau khi token hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n | App xÄ‚Â³a phiÄ‚Âªn vÄ‚Â  yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p; khÄ‚Â´ng tiĂ¡ÂºÂ¿p tĂ¡Â»Â¥c gĂ¡Â»Â­i bearer cĂ…Â© | ChĂ†Â°a test | |

## Runtime errors

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-12 | NhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n idempotent | CÄ‚Â³ mĂ¡Â»â„¢t web/QR order chĂ¡Â»Â Ă¡Â»Å¸ Branch A | MĂ¡Â»Å¸ thÄ‚Â´ng bÄ‚Â¡o bĂ¡ÂºÂ±ng hai phiÄ‚Âªn employee hoĂ¡ÂºÂ·c bĂ¡ÂºÂ¥m nhĂ¡ÂºÂ­n lĂ¡ÂºÂ·p nhanh | ChĂ¡Â»â€° mĂ¡Â»â„¢t lĂ¡ÂºÂ§n nhĂ¡ÂºÂ­n thÄ‚Â nh cÄ‚Â´ng; lĂ¡ÂºÂ§n cÄ‚Â²n lĂ¡ÂºÂ¡i bÄ‚Â¡o conflict/trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ä‚Â£ xĂ¡Â»Â­ lÄ‚Â½; khÄ‚Â´ng tĂ¡ÂºÂ¡o order thĂ¡Â»Â© hai | ChĂ†Â°a test | |

## Navigation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-13 | Branch claim authoritative | Employee A Ă„â€˜Ă†Â°Ă¡Â»Â£c gÄ‚Â¡n Branch A; cÄ‚Â³ Branch B trÄ‚Âªn giao diĂ¡Â»â€¡n hoĂ¡ÂºÂ·c request | SĂ¡Â»Â­a branch Ă„â€˜Ă†Â°Ă¡Â»Â£c gĂ¡Â»Â­i trong form/query sang B khi tĂ¡ÂºÂ¡o/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t order | Server bĂ¡Â»Â qua branch client vÄ‚Â  giĂ¡Â»Â¯ Branch A; khÄ‚Â´ng tĂ¡ÂºÂ¡o hoĂ¡ÂºÂ·c chuyĂ¡Â»Æ’n order sang B | ChĂ†Â°a test | |

## Security / role boundaries (AI checklist centralized)

AI chat cases are maintained in [`../ai-chat/01-chu-quan.md`](../ai-chat/01-chu-quan.md) and API/runtime cases in [`../ky-thuat-runtime/README.md`](../ky-thuat-runtime/README.md). The row below is a non-chat terminal-status case.

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| EMP-14 | TrĂ¡ÂºÂ¡ng thÄ‚Â¡i terminal | CÄ‚Â³ order Ă„â€˜Ä‚Â£ paid vÄ‚Â  mĂ¡Â»â„¢t order cancelled | MĂ¡Â»Å¸ tĂ¡Â»Â«ng order tĂ¡Â»Â« employee, thĂ¡Â»Â­ Ă„â€˜Ă¡Â»â€¢i trĂ¡ÂºÂ¡ng thÄ‚Â¡i/xÄ‚Â³a/gĂ¡Â»Â­i thÄ‚Âªm | Server tĂ¡Â»Â« chĂ¡Â»â€˜i mĂ¡Â»Âi mutation terminal; UI hiĂ¡Â»Æ’n thĂ¡Â»â€¹ lĂ¡Â»â€”i/disabled vÄ‚Â  khÄ‚Â´ng Ă„â€˜Ă¡Â»â€¢i invoice/order | ChĂ†Â°a test | |
