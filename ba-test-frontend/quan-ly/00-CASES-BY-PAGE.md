# Checklist theo trang — manager

Các case được chuyển khỏi checklist legacy và phân loại theo trang/nhóm. Giữ nguyên ID, trạng thái và minh chứng; cập nhật kết quả tại đây.

| Case ID | Trang/nhóm đích |
|---|---|
| MGR-01 | Session & branch scope |
| MGR-02 | Dashboard |
| MGR-03 | Catalog & customers |
| MGR-04 | Catalog & customers |
| MGR-05 | Tables & reservations |
| MGR-06 | Employees / attendance / schedule |
| MGR-07 | POS & invoices |
| MGR-08 | Kitchen |
| MGR-09 | Inventory |
| MGR-11 | Direct URL / authorization |
| MGR-12 | Branch isolation |
| MGR-13 | Runtime errors |
| MGR-14 | Navigation |
| MGR-15 | Notifications |
| MGR-16 | Mock AI |

## Session & branch scope

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-01 | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p vÄ‚Â  branch scope | Manager Ă„â€˜Ă†Â°Ă¡Â»Â£c gÄ‚Â¡n Branch A | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p nhÄ‚Â³m quĂ¡ÂºÂ£n lÄ‚Â½, refresh, kiĂ¡Â»Æ’m tra branch Ă„â€˜ang chĂ¡Â»Ân | VÄ‚Â o dashboard; branch lĂ¡ÂºÂ¥y tĂ¡Â»Â« claim/account; navbar khÄ‚Â´ng cho tĂ¡Â»Â± Ă„â€˜Ă¡Â»â€¢i sang branch khÄ‚Â¡c | ChĂ†Â°a test | |

## Dashboard

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-02 | Dashboard/insight trong branch | Branch A cÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u | MĂ¡Â»Å¸ `/dashboard`, `/business-insights`, lĂ¡Â»Âc ngÄ‚Â y vÄ‚Â  Ă„â€˜Ă¡Â»Âc/resolve insight | ChĂ¡Â»â€° thĂ¡ÂºÂ¥y sĂ¡Â»â€˜ liĂ¡Â»â€¡u/insight Branch A; trĂ¡ÂºÂ¡ng thÄ‚Â¡i cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ä‚Âºng | ChĂ†Â°a test | |

## Catalog & customers

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-03 | POS vÄ‚Â  bÄ‚Â n | CÄ‚Â³ bÄ‚Â n/mÄ‚Â³n/Ă„â€˜Ă†Â¡n Branch A | MĂ¡Â»Å¸ `/pos`, `/tables`, xem Ă„â€˜Ă†Â¡n vÄ‚Â  cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p | TÄ‚Â¡c vĂ¡Â»Â¥ phĂ¡Â»Â¥c vĂ¡Â»Â¥ hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng trong Branch A; yÄ‚Âªu cĂ¡ÂºÂ§u tĂ¡Â»â€ºi Branch B bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i | ChĂ†Â°a test | |
| MGR-04 | Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n vÄ‚Â  khÄ‚Â¡ch hÄ‚Â ng | CÄ‚Â³ reservation/customer Branch A | MĂ¡Â»Å¸ `/reservations`, `/customers`, tÄ‚Â¬m hĂ¡Â»â€œ sĂ†Â¡ vÄ‚Â  Ă„â€˜Ă¡Â»â€¢i trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n | HiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng phĂ¡ÂºÂ¡m vi branch; dĂ¡Â»Â¯ liĂ¡Â»â€¡u cÄ‚Â¡ nhÄ‚Â¢n khÄ‚Â´ng lĂ¡ÂºÂ«n branch khÄ‚Â¡c | ChĂ†Â°a test | |

## Tables & reservations

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-05 | Chi phÄ‚Â­ vÄ‚Â  kho | CÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u test Branch A | MĂ¡Â»Å¸ `/expenses`, `/inventory`, thĂ¡Â»Â±c hiĂ¡Â»â€¡n thao tÄ‚Â¡c quĂ¡ÂºÂ£n lÄ‚Â½ phÄ‚Â¹ hĂ¡Â»Â£p | CRUD/phiĂ¡ÂºÂ¿u kho Ă„â€˜Ă†Â°Ă¡Â»Â£c backend cho phÄ‚Â©p; sĂ¡Â»â€˜ liĂ¡Â»â€¡u branch chÄ‚Â­nh xÄ‚Â¡c | ChĂ†Â°a test | |

## Employees / attendance / schedule

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-06 | NhÄ‚Â¢n sĂ¡Â»Â±, lĂ¡Â»â€¹ch, chĂ¡ÂºÂ¥m cÄ‚Â´ng | CÄ‚Â³ nhÄ‚Â¢n sĂ¡Â»Â± thuĂ¡Â»â„¢c Branch A | MĂ¡Â»Å¸ `/employees`, `/attendance`, `/schedule`, xem/sĂ¡Â»Â­a thao tÄ‚Â¡c Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p | KhÄ‚Â´ng xem/sĂ¡Â»Â­a nhÄ‚Â¢n sĂ¡Â»Â± ngoÄ‚Â i branch; lĂ¡Â»â€”i quyĂ¡Â»Ân hiĂ¡Â»Æ’n thĂ¡Â»â€¹ rÄ‚Âµ | ChĂ†Â°a test | |

## POS & invoices

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-07 | ChĂ¡ÂºÂ·n khu vĂ¡Â»Â±c cĂ¡ÂºÂ¥u hÄ‚Â¬nh chĂ¡Â»Â§ quÄ‚Â¡n | Manager Branch A | KiĂ¡Â»Æ’m tra khÄ‚Â´ng cÄ‚Â³ bÄ‚Â¡nh rĂ„Æ’ng/sidebar cĂ¡ÂºÂ¥u hÄ‚Â¬nh; mĂ¡Â»Å¸ trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p `/settings`, `/settings/receipt`, `/print-templates`, `/branches` vÄ‚Â  gĂ¡Â»Âi mutation cĂ¡ÂºÂ¥u hÄ‚Â¬nh bĂ¡ÂºÂ±ng phiÄ‚Âªn manager trÄ‚Âªn mÄ‚Â´i trĂ†Â°Ă¡Â»Âng local an toÄ‚Â n; mĂ¡Â»Å¸ `/profile` vÄ‚Â  Ă„â€˜Ă¡Â»â€¢i mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u chÄ‚Â­nh mÄ‚Â¬nh | UI/route chuyĂ¡Â»Æ’n Forbidden trĂ†Â°Ă¡Â»â€ºc khi render layout; API mutation trĂ¡ÂºÂ£ 403; manager vĂ¡ÂºÂ«n Ă„â€˜Ă¡Â»Âc receipt cĂ¡ÂºÂ§n thiĂ¡ÂºÂ¿t khi xem/in hÄ‚Â³a Ă„â€˜Ă†Â¡n, lÄ‚Â m nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ Branch A vÄ‚Â  quĂ¡ÂºÂ£n lÄ‚Â½ tÄ‚Â i khoĂ¡ÂºÂ£n cĂ¡Â»Â§a chÄ‚Â­nh mÄ‚Â¬nh | ChĂ†Â°a test | |

## Kitchen

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-08 | HÄ‚Â³a Ă„â€˜Ă†Â¡n vÄ‚Â  thanh toÄ‚Â¡n | CÄ‚Â³ Ă„â€˜Ă†Â¡n Branch A Ă¡Â»Å¸ trĂ¡ÂºÂ¡ng thÄ‚Â¡i thanh toÄ‚Â¡n | MĂ¡Â»Å¸ `/invoices`, mĂ¡Â»Å¸ chi tiĂ¡ÂºÂ¿t, kiĂ¡Â»Æ’m tra tĂ¡Â»â€¢ng vÄ‚Â  thĂ¡Â»Â­ thanh toÄ‚Â¡n hĂ¡Â»Â£p lĂ¡Â»â€¡ | TĂ¡Â»â€¢ng dÄ‚Â¹ng order persisted; trĂ¡ÂºÂ¡ng thÄ‚Â¡i hÄ‚Â³a Ă„â€˜Ă†Â¡n nhĂ¡ÂºÂ¥t quÄ‚Â¡n; thao tÄ‚Â¡c trÄ‚Â¡i branch bĂ¡Â»â€¹ chĂ¡ÂºÂ·n | ChĂ†Â°a test | |

## Inventory

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-09 | BĂ¡ÂºÂ¿p vÄ‚Â  realtime | CÄ‚Â³ Ă„â€˜Ă†Â¡n gĂ¡Â»Â­i bĂ¡ÂºÂ¿p Branch A; mĂ¡Â»Å¸ POS vÄ‚Â  bĂ¡ÂºÂ¿p | GĂ¡Â»Â­i/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t mÄ‚Â³n bĂ¡ÂºÂ¿p, xem `/kitchen` vÄ‚Â  `/kitchen/history` | SĂ¡Â»Â± kiĂ¡Â»â€¡n realtime Ă„â€˜Ä‚Âºng branch; refresh vĂ¡ÂºÂ«n khĂ¡Â»â€ºp API; khÄ‚Â´ng broadcast dĂ¡Â»Â¯ liĂ¡Â»â€¡u branch khÄ‚Â¡c | ChĂ†Â°a test | |

## Expenses

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|

## Direct URL / authorization

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-11 | URL/menu trÄ‚Â¡i quyĂ¡Â»Ân | Ă„ÂÄ‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p manager | Truy cĂ¡ÂºÂ­p `/branches`, thĂ¡Â»Â­ URL CRUD admin, thĂ¡Â»Â­ Ă„â€˜Ă¡Â»â€¢i branch bĂ¡ÂºÂ±ng reload | `/branches` vÄ‚Â  thao tÄ‚Â¡c admin-only bĂ¡Â»â€¹ chĂ¡ÂºÂ·n; menu/URL khÄ‚Â´ng bypass Ă„â€˜Ă†Â°Ă¡Â»Â£c backend | ChĂ†Â°a test | |

## Branch isolation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-12 | LĂ¡Â»â€”i, rĂ¡Â»â€”ng, refresh, hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n phiÄ‚Âªn | CÄ‚Â³ thĂ¡Â»Æ’ tĂ¡ÂºÂ¯t API hoĂ¡ÂºÂ·c lÄ‚Â m token hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n | MĂ¡Â»Å¸ trang lÄ‚Âºc API lĂ¡Â»â€”i, refresh khi Ă„â€˜ang tĂ¡ÂºÂ£i, thao tÄ‚Â¡c sau 401 | CÄ‚Â³ loading/error Ă„â€˜Ä‚Âºng; 401 yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p lĂ¡ÂºÂ¡i; khÄ‚Â´ng mĂ¡ÂºÂ¥t dĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜Ä‚Â£ lĂ†Â°u | ChĂ†Â°a test | |

## Runtime errors

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-13 | URL/query branch giĂ¡ÂºÂ£ mĂ¡ÂºÂ¡o | Manager A vÄ‚Â  Branch B cÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u | SĂ¡Â»Â­a query/route branch, order, expense, table hoĂ¡ÂºÂ·c customer sang Branch B rĂ¡Â»â€œi tĂ¡ÂºÂ£i lĂ¡ÂºÂ¡i | Server vĂ¡ÂºÂ«n dÄ‚Â¹ng branch trong JWT; trĂ¡ÂºÂ£ 403/404 phÄ‚Â¹ hĂ¡Â»Â£p vÄ‚Â  khÄ‚Â´ng trĂ¡ÂºÂ£ dĂ¡Â»Â¯ liĂ¡Â»â€¡u B | ChĂ†Â°a test | |

## Navigation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-14 | Chi phÄ‚Â­ idempotency | CÄ‚Â³ quyĂ¡Â»Ân tĂ¡ÂºÂ¡o expense Branch A vÄ‚Â  mĂ¡Â»â„¢t khÄ‚Â³a idempotency mĂ¡Â»â€ºi | GĂ¡Â»Â­i cÄ‚Â¹ng form/khÄ‚Â³a hai lĂ¡ÂºÂ§n; sau Ă„â€˜Ä‚Â³ gĂ¡Â»Â­i lĂ¡ÂºÂ¡i cÄ‚Â¹ng khÄ‚Â³a vĂ¡Â»â€ºi amount/category khÄ‚Â¡c | LĂ¡ÂºÂ§n Ă„â€˜Ă¡ÂºÂ§u tĂ¡ÂºÂ¡o mĂ¡Â»â„¢t bĂ¡ÂºÂ£n ghi; lĂ¡ÂºÂ§n lĂ¡ÂºÂ·p trĂ¡ÂºÂ£ kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ gĂ¡Â»â€˜c; payload khÄ‚Â¡c bĂ¡Â»â€¹ conflict; dashboard khÄ‚Â´ng invalidate thÄ‚Âªm | ChĂ†Â°a test | |

## Notifications

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-15 | Doanh thu loĂ¡ÂºÂ¡i Ă„â€˜Ă†Â¡n | Branch A cÄ‚Â³ order paid, unpaid vÄ‚Â  cancelled trong cÄ‚Â¹ng khoĂ¡ÂºÂ£ng ngÄ‚Â y | MĂ¡Â»Å¸ dashboard/financial analysis vÄ‚Â  Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u invoice tĂ¡Â»Â«ng trĂ¡ÂºÂ¡ng thÄ‚Â¡i | Doanh thu chĂ¡Â»â€° gĂ¡Â»â€œm phĂ¡ÂºÂ¡m vi trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â°Ă¡Â»Â£c nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ quy Ă„â€˜Ă¡Â»â€¹nh; branch/date boundary Ă„â€˜Ä‚Âºng, khÄ‚Â´ng tÄ‚Â­nh nhĂ¡ÂºÂ§m unpaid/cancelled | ChĂ†Â°a test | |

## Security / role boundaries (AI checklist centralized)

AI chat cases are maintained in [`../ai-chat/01-chu-quan.md`](../ai-chat/01-chu-quan.md) and API/runtime cases in [`../ky-thuat-runtime/README.md`](../ky-thuat-runtime/README.md). The row below is a non-chat navigation/security case.

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| MGR-16 | Navigation chÄ‚Â­nh vÄ‚Â  bÄ‚Â¡nh rĂ„Æ’ng | TÄ‚Â i khoĂ¡ÂºÂ£n `manager` Branch A hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng; cÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u POS/KDS Branch A | MĂ¡Â»Å¸ trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p `/dashboard`, `/inventory`, `/pos`, `/tables`, `/invoices`, `/kitchen`, `/shifts`; Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u menu chÄ‚Â­nh, submenu NhÄ‚Â¢n viÄ‚Âªn vÄ‚Â  bÄ‚Â¡nh rĂ„Æ’ng, rĂ¡Â»â€œi thĂ¡Â»Â­ `/branches` | Route hĂ¡Â»Â£p lĂ¡Â»â€¡ giĂ¡Â»Â¯ cÄ‚Â¹ng menu chÄ‚Â­nh vÄ‚Â  active state trÄ‚Âªn mĂ¡Â»Âi route; `/shifts` chĂ¡Â»â€° lÄ‚Â  submenu Ca lÄ‚Â m cĂ¡Â»Â§a NhÄ‚Â¢n viÄ‚Âªn vÄ‚Â  active Ă„â€˜Ä‚Âºng trÄ‚Âªn desktop/mobile. MĂ¡ÂºÂ«u in `/print-templates` vÄ‚Â  CĂ¡ÂºÂ¥u hÄ‚Â¬nh in bill `/settings/receipt` nĂ¡ÂºÂ±m trong bÄ‚Â¡nh rĂ„Æ’ng; POS/KDS vĂ¡ÂºÂ«n dÄ‚Â¹ng Ă„â€˜Ä‚Âºng branch; `/branches` bĂ¡Â»â€¹ chĂ¡ÂºÂ·n nhĂ†Â° trĂ†Â°Ă¡Â»â€ºc vÄ‚Â  menu khÄ‚Â´ng mĂ¡Â»Å¸ rĂ¡Â»â„¢ng quyĂ¡Â»Ân | ChĂ†Â°a test | |
