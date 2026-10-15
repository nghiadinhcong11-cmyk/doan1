# Checklist theo trang — cashier

Các case được chuyển khỏi checklist legacy và phân loại theo trang/nhóm. Giữ nguyên ID, trạng thái và minh chứng; cập nhật kết quả tại đây.

| Case ID | Trang/nhóm đích |
|---|---|
| CSH-01 | POS & payment |
| CSH-02 | POS & payment |
| CSH-03 | POS & payment |
| CSH-04 | POS & payment |
| CSH-05 | Invoices |
| CSH-06 | Session |
| CSH-07 | Shift |
| CSH-08 | Reservations |
| CSH-09 | Notifications |
| CSH-10 | Direct URL / authorization |
| CSH-11 | Branch isolation |
| CSH-12 | Runtime errors |
| CSH-13 | Navigation |
| CSH-14 | Mock AI |
| CSH-15 | Realtime |
| CSH-16 | Payment resilience |
| CSH-17 | Other |
| CSH-18 | Other |
| CSH-19 | Other |
| CSH-20 | Other |
| CSH-21 | Other |
| CSH-22 | Other |
| CSH-23 | Other |
| CSH-24 | Other |
| CSH-25 | Other |
| CSH-26 | Other |
| CSH-27 | Other |
| CSH-28 | Other |
| CSH-29 | Other |
| CSH-30 | Other |
| CSH-31 | Other |
| CSH-32 | Other |
| CSH-33 | Other |
| CSH-34 | Other |
| CSH-35 | Other |
| CSH-36 | Other |
| CSH-37 | Other |
| CSH-38 | Other |
| CSH-39 | Other |

## POS & payment

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-01 | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p vÄ‚Â  POS | TÄ‚Â i khoĂ¡ÂºÂ£n `cashier` thuĂ¡Â»â„¢c Branch A | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p nhÄ‚Â³m staff/thu ngÄ‚Â¢n, refresh | Redirect `/pos`; navbar chĂ¡Â»â€° cÄ‚Â³ cÄ‚Â¡c mĂ¡Â»Â¥c thu ngÄ‚Â¢n; branch lĂ¡ÂºÂ¥y tĂ¡Â»Â« tÄ‚Â i khoĂ¡ÂºÂ£n | ChĂ†Â°a test | |
| CSH-02 | MĂ¡Â»Å¸/Ă„â€˜Ä‚Â³ng ca | NhÄ‚Â¢n viÄ‚Âªn chĂ†Â°a cÄ‚Â³ ca mĂ¡Â»Å¸ | MĂ¡Â»Å¸ `/pos/shifts`, mĂ¡Â»Å¸ ca, refresh, Ă„â€˜Ä‚Â³ng ca theo luĂ¡Â»â€œng hĂ¡Â»Â£p lĂ¡Â»â€¡ | Ca Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi Ă„â€˜Ä‚Âºng nhÄ‚Â¢n viÄ‚Âªn/branch; khÄ‚Â´ng mĂ¡Â»Å¸ trÄ‚Â¹ng hoĂ¡ÂºÂ·c Ă„â€˜Ä‚Â³ng ca cĂ¡Â»Â§a ngĂ†Â°Ă¡Â»Âi khÄ‚Â¡c | ChĂ†Â°a test | |
| CSH-03 | TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n web/QR | CÄ‚Â³ Ă„â€˜Ă†Â¡n chĂ¡Â»Â tĂ¡Â»Â« customer/guest | MĂ¡Â»Å¸ thÄ‚Â´ng bÄ‚Â¡o, nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n, Ă„â€˜Ă†Â°a vÄ‚Â o POS | Ă„ÂĂ†Â¡n xuĂ¡ÂºÂ¥t hiĂ¡Â»â€¡n Ă„â€˜Ä‚Âºng bÄ‚Â n/khÄ‚Â¡ch; nhĂ¡ÂºÂ­n lĂ¡ÂºÂ·p khÄ‚Â´ng tĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n thĂ¡Â»Â© hai | ChĂ†Â°a test | |
| CSH-04 | TÄ‚Â­nh tĂ¡Â»â€¢ng vÄ‚Â  giĂ¡ÂºÂ£m giÄ‚Â¡ | CÄ‚Â³ nhiĂ¡Â»Âu mÄ‚Â³n/topping, VAT/phÄ‚Â­ vÄ‚Â  khuyĂ¡ÂºÂ¿n mÄ‚Â£i test | TĂ¡ÂºÂ¡o/tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n, kiĂ¡Â»Æ’m tra subtotal, phÄ‚Â­, VAT, giĂ¡ÂºÂ£m giÄ‚Â¡, final total | HiĂ¡Â»Æ’n thĂ¡Â»â€¹ khĂ¡Â»â€ºp backend; khÄ‚Â´ng tin tĂ¡Â»â€¢ng gĂ¡Â»Â­i tĂ¡Â»Â« client; tiĂ¡Â»Ân khÄ‚Â´ng sai do chuyĂ¡Â»Æ’n double | ChĂ†Â°a test | |

## Invoices

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-05 | Thanh toÄ‚Â¡n | CÄ‚Â³ Ă„â€˜Ă†Â¡n chĂ†Â°a thanh toÄ‚Â¡n | Thanh toÄ‚Â¡n bĂ¡ÂºÂ±ng luĂ¡Â»â€œng UI, thĂ¡Â»Â­ gĂ¡Â»Â­i lĂ¡ÂºÂ¡i request hoĂ¡ÂºÂ·c sĂ¡Â»Â­a total Ă¡Â»Å¸ devtools | Thanh toÄ‚Â¡n dÄ‚Â¹ng `Order.TotalAmount` persisted; request lĂ¡ÂºÂ·p/total giĂ¡ÂºÂ£ bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i; trĂ¡ÂºÂ¡ng thÄ‚Â¡i chuyĂ¡Â»Æ’n Ă„â€˜Ä‚Âºng | ChĂ†Â°a test | |

## Session

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-06 | In bill vÄ‚Â  lĂ¡Â»â€¹ch sĂ¡Â»Â­ hÄ‚Â³a Ă„â€˜Ă†Â¡n | CÄ‚Â³ Ă„â€˜Ă†Â¡n Ă„â€˜Ä‚Â£ thanh toÄ‚Â¡n | MĂ¡Â»Å¸ `/pos/invoices`, xem/in bill, refresh | Bill khĂ¡Â»â€ºp persisted order/receipt settings; lĂ¡Â»â€”i mÄ‚Â¡y in khÄ‚Â´ng Ă„â€˜Ă¡Â»â€¢i trĂ¡ÂºÂ¡ng thÄ‚Â¡i thanh toÄ‚Â¡n | ChĂ†Â°a test | |

## Shift

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-07 | BÄ‚Â n, tÄ‚Â¬m mÄ‚Â³n, giĂ¡Â»Â hÄ‚Â ng vÄ‚Â  gĂ¡Â»Âi thÄ‚Âªm | CÄ‚Â³ Ă„â€˜Ă†Â¡n Ă„â€˜ang mĂ¡Â»Å¸, cÄ‚Â³ mÄ‚Â³n thĂ†Â°Ă¡Â»Âng vÄ‚Â  mÄ‚Â³n cÄ‚Â³ size/topping | MĂ¡Â»Å¸ `/pos`, tÄ‚Â¬m theo tÄ‚Âªn/mÄ‚Â£, lĂ¡Â»Âc danh mĂ¡Â»Â¥c, chĂ¡Â»Ân/Ă„â€˜Ă¡Â»â€¢i bÄ‚Â n, kiĂ¡Â»Æ’m tra thÄ‚Â´ng tin Ă„â€˜Ă†Â¡n, thÄ‚Âªm mÄ‚Â³n, mĂ¡Â»Å¸ modal size/topping, sĂ¡Â»Â­a sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng vÄ‚Â  ghi chÄ‚Âº, xÄ‚Â³a mÄ‚Â³n chĂ†Â°a gĂ¡Â»Â­i; thĂ¡Â»Â­ thao tÄ‚Â¡c khi Ă„â€˜ang loading/lĂ†Â°u rĂ¡Â»â€œi mĂ¡Â»Å¸ `/pos/reservations` | KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ tÄ‚Â¬m/lĂ¡Â»Âc Ă„â€˜Ä‚Âºng; bÄ‚Â n vÄ‚Â  giĂ¡Â»Â hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng; modal Ă„â€˜Ä‚Â³ng/mĂ¡Â»Å¸ khÄ‚Â´ng lÄ‚Â m mĂ¡ÂºÂ¥t lĂ¡Â»Â±a chĂ¡Â»Ân; nÄ‚Âºt gĂ¡Â»Â­i bĂ¡ÂºÂ¿p/thanh toÄ‚Â¡n Ă„â€˜Ä‚Âºng quyĂ¡Â»Ân vÄ‚Â  loading/disabled; order update giĂ¡Â»Â¯ Ă„â€˜Ä‚Âºng tĂ¡Â»â€¢ng vÄ‚Â  bĂ¡ÂºÂ¿p; khÄ‚Â´ng tĂ¡ÂºÂ¡o order ngoÄ‚Â i Ä‚Â½ muĂ¡Â»â€˜n | ChĂ†Â°a test | |

## Reservations

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-08 | Receipt vĂ¡ÂºÂ­n hÄ‚Â nh, khÄ‚Â´ng cĂ¡ÂºÂ¥u hÄ‚Â¬nh | Admin Ă„â€˜Ä‚Â£ cĂ¡ÂºÂ¥u hÄ‚Â¬nh receipt test | MĂ¡Â»Å¸/in `/pos/invoices`; thĂ¡Â»Â­ trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p `/pos/settings/receipt`, `/settings/receipt` vÄ‚Â  API mutation cĂ¡ÂºÂ¥u hÄ‚Â¬nh trÄ‚Âªn mÄ‚Â´i trĂ†Â°Ă¡Â»Âng local an toÄ‚Â n; mĂ¡Â»Å¸ `/pos/profile` vÄ‚Â  Ă„â€˜Ă¡Â»â€¢i mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u chÄ‚Â­nh mÄ‚Â¬nh | Receipt Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă¡Â»Âc Ă„â€˜Ă¡Â»Æ’ in Ă„â€˜Ä‚Âºng branch; cashier khÄ‚Â´ng thĂ¡ÂºÂ¥y bÄ‚Â¡nh rĂ„Æ’ng, khÄ‚Â´ng mĂ¡Â»Å¸ page cĂ¡ÂºÂ¥u hÄ‚Â¬nh vÄ‚Â  mutation trĂ¡ÂºÂ£ 403; profile/Ă„â€˜Ă¡Â»â€¢i mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u cÄ‚Â¡ nhÄ‚Â¢n vĂ¡ÂºÂ«n hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng | ChĂ†Â°a test | |

## Notifications

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-09 | ChĂ¡ÂºÂ¥m cÄ‚Â´ng, lĂ¡Â»â€¹ch, hĂ¡Â»â€œ sĂ†Â¡ | CÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u cÄ‚Â¡ nhÄ‚Â¢n | MĂ¡Â»Å¸ `/pos/attendance`, `/pos/schedule`, `/pos/profile` | DĂ¡Â»Â¯ liĂ¡Â»â€¡u cÄ‚Â¡ nhÄ‚Â¢n/ca chÄ‚Â­nh xÄ‚Â¡c; lĂ¡Â»â€”i tĂ¡ÂºÂ£i cÄ‚Â³ trĂ¡ÂºÂ¡ng thÄ‚Â¡i rÄ‚Âµ | ChĂ†Â°a test | |

## Direct URL / authorization

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-10 | Realtime trĂ¡ÂºÂ¡ng thÄ‚Â¡i bĂ¡ÂºÂ¿p | CÄ‚Â³ POS vÄ‚Â  bĂ¡ÂºÂ¿p mĂ¡Â»Å¸ hai phiÄ‚Âªn | GĂ¡Â»Â­i mÄ‚Â³n, bĂ¡ÂºÂ¿p cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t, quan sÄ‚Â¡t POS | POS nhĂ¡ÂºÂ­n trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ä‚Âºng item/Ă„â€˜Ă†Â¡n; mĂ¡ÂºÂ¥t kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i rĂ¡Â»â€œi reconnect khÄ‚Â´ng nhÄ‚Â¢n Ă„â€˜Ä‚Â´i | ChĂ†Â°a test | |

## Branch isolation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-11 | Mock AI cashier | Mock bĂ¡ÂºÂ­t | HĂ¡Â»Âi menu, bÄ‚Â n, Ă„â€˜Ă†Â¡n; thĂ¡Â»Â­ thao tÄ‚Â¡c cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t giÄ‚Â¡ hoĂ¡ÂºÂ·c xem tÄ‚Â i chÄ‚Â­nh admin | ChĂ¡Â»â€° tool Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p chĂ¡ÂºÂ¡y; thao tÄ‚Â¡c nhĂ¡ÂºÂ¡y cĂ¡ÂºÂ£m bĂ¡Â»â€¹ chĂ¡ÂºÂ·n/confirmation theo server; khÄ‚Â´ng gĂ¡Â»Âi provider ngoÄ‚Â i | ChĂ†Â°a test | |

## Runtime errors

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-12 | URL, branch vÄ‚Â  session | CÄ‚Â³ Branch B hoĂ¡ÂºÂ·c token hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n | MĂ¡Â»Å¸ trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p invoice/order Branch B; thao tÄ‚Â¡c sau 401 | Backend giĂ¡Â»Â¯ branch isolation; app logout/redirect Ă„â€˜Ä‚Âºng; khÄ‚Â´ng dĂ¡Â»Â±a vÄ‚Â o localStorage role Ă„â€˜Ă¡Â»Æ’ cĂ¡ÂºÂ¥p quyĂ¡Â»Ân | ChĂ†Â°a test | |

## Navigation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-13 | Hai thanh toÄ‚Â¡n Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi | CÄ‚Â³ mĂ¡Â»â„¢t order unpaid vÄ‚Â  hai phiÄ‚Âªn cashier hĂ¡Â»Â£p lĂ¡Â»â€¡ | BĂ¡ÂºÂ¥m thanh toÄ‚Â¡n gĂ¡ÂºÂ§n nhĂ†Â° Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi Ă¡Â»Å¸ hai phiÄ‚Âªn rĂ¡Â»â€œi reload invoice | ChĂ¡Â»â€° mĂ¡Â»â„¢t request chuyĂ¡Â»Æ’n paid; request cÄ‚Â²n lĂ¡ÂºÂ¡i bÄ‚Â¡o conflict/Ă„â€˜Ä‚Â£ thanh toÄ‚Â¡n; khÄ‚Â´ng ghi nhĂ¡ÂºÂ­n thu tiĂ¡Â»Ân hai lĂ¡ÂºÂ§n | ChĂ†Â°a test | |

## Mock AI

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-14 | Payload tĂ¡Â»â€¢ng tiĂ¡Â»Ân bĂ¡Â»â€¹ sĂ¡Â»Â­a | CÄ‚Â³ order unpaid, mĂ¡Â»Å¸ DevTools local Ă„â€˜Ă¡Â»Æ’ sĂ¡Â»Â­a clientTotalAmount trĂ†Â°Ă¡Â»â€ºc submit | GĂ¡Â»Â­i thanh toÄ‚Â¡n vĂ¡Â»â€ºi total giĂ¡ÂºÂ£ thĂ¡ÂºÂ¥p/cao rĂ¡Â»â€œi thĂ¡Â»Â­ lĂ¡ÂºÂ¡i bĂ¡ÂºÂ±ng tĂ¡Â»â€¢ng hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng | Server Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u `Order.TotalAmount` persisted; tĂ¡Â»â€¢ng giĂ¡ÂºÂ£ bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i vÄ‚Â  khÄ‚Â´ng Ă„â€˜Ă¡Â»â€¢i payment state | ChĂ†Â°a test | |

## Realtime

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-15 | Ă„ÂiĂ¡Â»Æ’m khÄ‚Â¡ch ghi mĂ¡Â»â„¢t lĂ¡ÂºÂ§n | CÄ‚Â³ customer gĂ¡ÂºÂ¯n vĂ¡Â»â€ºi order; order Ă„â€˜Ă¡Â»Â§ Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n tÄ‚Â­ch Ă„â€˜iĂ¡Â»Æ’m | Thanh toÄ‚Â¡n, refresh/retry thao tÄ‚Â¡c tÄ‚Â­ch Ă„â€˜iĂ¡Â»Æ’m hoĂ¡ÂºÂ·c mĂ¡Â»Å¸ lĂ¡ÂºÂ¡i invoice | Ă„ÂiĂ¡Â»Æ’m Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡Â»â„¢ng Ă„â€˜Ä‚Âºng mĂ¡Â»â„¢t lĂ¡ÂºÂ§n; guest khÄ‚Â´ng bĂ¡Â»â€¹ gĂ¡ÂºÂ¯n customer ngoÄ‚Â i Ä‚Â½ muĂ¡Â»â€˜n; order paid khÄ‚Â´ng Ă„â€˜Ă¡Â»â€¢i member tÄ‚Â¹y tiĂ¡Â»â€¡n | ChĂ†Â°a test | |

## Payment resilience

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-16 | SĂ†Â¡ Ă„â€˜Ă¡Â»â€œ khu vĂ¡Â»Â±c, bÄ‚Â n vÄ‚Â  mang vĂ¡Â»Â | CÄ‚Â³ nhiĂ¡Â»Âu khu vĂ¡Â»Â±c; bÄ‚Â n trĂ¡Â»â€˜ng, cÄ‚Â³ khÄ‚Â¡ch, Ă„â€˜Ă¡ÂºÂ·t trĂ†Â°Ă¡Â»â€ºc vÄ‚Â  mĂ¡Â»â„¢t order mang vĂ¡Â»Â | MĂ¡Â»Å¸ `/pos`, chuyĂ¡Â»Æ’n khu vĂ¡Â»Â±c, chĂ¡Â»Ân tĂ¡Â»Â«ng trĂ¡ÂºÂ¡ng thÄ‚Â¡i bÄ‚Â n, chĂ¡Â»Ân Mang vĂ¡Â»Â rĂ¡Â»â€œi quay lĂ¡ÂºÂ¡i bÄ‚Â n | ThĂ¡Â»Â© tĂ¡Â»Â± khu vĂ¡Â»Â±c/bÄ‚Â n, nhÄ‚Â£n mÄ‚Â u, sĂ¡Â»â€˜ mÄ‚Â³n vÄ‚Â  bÄ‚Â n Ă„â€˜ang chĂ¡Â»Ân Ă„â€˜Ä‚Âºng; trĂ¡ÂºÂ¡ng thÄ‚Â¡i rĂ¡Â»â€”ng/loading khÄ‚Â´ng lÄ‚Â m mĂ¡ÂºÂ¥t lĂ¡Â»Â±a chĂ¡Â»Ân | ChĂ†Â°a test | |

## Other

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CSH-17 | YÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t mÄ‚Â³n vÄ‚Â  Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n | CÄ‚Â³ yÄ‚Âªu cĂ¡ÂºÂ§u web/QR vÄ‚Â  lĂ¡Â»â€¹ch hĂ¡ÂºÂ¹n chĂ¡Â»Â xĂ¡Â»Â­ lÄ‚Â½ | MĂ¡Â»Å¸ tab thÄ‚Â´ng bÄ‚Â¡o, nhĂ¡ÂºÂ­n/tĂ¡Â»Â« chĂ¡Â»â€˜i yÄ‚Âªu cĂ¡ÂºÂ§u, thĂ¡Â»Â­ thao tÄ‚Â¡c lĂ¡ÂºÂ·p khi Ă„â€˜ang xĂ¡Â»Â­ lÄ‚Â½, mĂ¡Â»Å¸ chi tiĂ¡ÂºÂ¿t lĂ¡Â»â€¹ch hĂ¡ÂºÂ¹n | SĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng, danh sÄ‚Â¡ch, trĂ¡ÂºÂ¡ng thÄ‚Â¡i disabled vÄ‚Â  Ă„â€˜iĂ¡Â»Âu hĂ†Â°Ă¡Â»â€ºng Ă„â€˜Ä‚Âºng; khÄ‚Â´ng nhĂ¡ÂºÂ­n/tĂ¡Â»Â« chĂ¡Â»â€˜i trÄ‚Â¹ng; dĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜Ä‚Âºng branch | ChĂ†Â°a test | |
| CSH-18 | Payment modal vÄ‚Â  khÄ‚Â¡ch hÄ‚Â ng/Ă„â€˜iĂ¡Â»Æ’m | CÄ‚Â³ khÄ‚Â¡ch Ă„â€˜Ä‚Â£ tĂ¡Â»â€œn tĂ¡ÂºÂ¡i, sĂ¡Â»â€˜ Ă„â€˜iĂ¡Â»â€¡n thoĂ¡ÂºÂ¡i khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i vÄ‚Â  dĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜iĂ¡Â»Æ’m | MĂ¡Â»Å¸ thanh toÄ‚Â¡n, tra cĂ¡Â»Â©u sĂ¡Â»â€˜ Ă„â€˜iĂ¡Â»â€¡n thoĂ¡ÂºÂ¡i hĂ¡Â»Â£p lĂ¡Â»â€¡/khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡, sĂ¡Â»Â­a tÄ‚Âªn, Ă„â€˜Ă¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m trong giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n, thĂ¡Â»Â­ consent vÄ‚Â  retry khi lĂ¡Â»â€”i | TĂ¡Â»â€¢ng preview trĂ†Â°Ă¡Â»â€ºc/sau Ă„â€˜Ă¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng implementation; consent, giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n Ă„â€˜iĂ¡Â»Æ’m, loading vÄ‚Â  lĂ¡Â»â€”i tra cĂ¡Â»Â©u Ă„â€˜Ä‚Âºng; khÄ‚Â´ng thanh toÄ‚Â¡n khi tra cĂ¡Â»Â©u chĂ†Â°a hoÄ‚Â n tĂ¡ÂºÂ¥t | ChĂ†Â°a test | |
| CSH-19 | PhĂ†Â°Ă†Â¡ng thĂ¡Â»Â©c, VietQR vÄ‚Â  xÄ‚Â¡c nhĂ¡ÂºÂ­n | CÄ‚Â³ branch Ă„â€˜Ă¡Â»Â§/thiĂ¡ÂºÂ¿u thÄ‚Â´ng tin ngÄ‚Â¢n hÄ‚Â ng; thĂ¡Â»Â­ tiĂ¡Â»Ân mĂ¡ÂºÂ·t vÄ‚Â  chuyĂ¡Â»Æ’n khoĂ¡ÂºÂ£n | Ă„ÂĂ¡Â»â€¢i phĂ†Â°Ă†Â¡ng thĂ¡Â»Â©c, kiĂ¡Â»Æ’m tra QR loading/lĂ¡Â»â€”i, Ă„â€˜Ä‚Â³ng/mĂ¡Â»Å¸ lĂ¡ÂºÂ¡i modal, bĂ¡ÂºÂ¥m xÄ‚Â¡c nhĂ¡ÂºÂ­n nhiĂ¡Â»Âu lĂ¡ÂºÂ§n hoĂ¡ÂºÂ·c khi Ă„â€˜ang lĂ†Â°u | Cash/transfer vÄ‚Â  QR hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng; thiĂ¡ÂºÂ¿u cĂ¡ÂºÂ¥u hÄ‚Â¬nh/lĂ¡Â»â€”i Ă¡ÂºÂ£nh cÄ‚Â³ thÄ‚Â´ng bÄ‚Â¡o; nÄ‚Âºt disabled/chĂ¡Â»â€˜ng lĂ¡ÂºÂ·p; chĂ†Â°a coi hiĂ¡Â»Æ’n thĂ¡Â»â€¹ QR lÄ‚Â  thanh toÄ‚Â¡n thÄ‚Â nh cÄ‚Â´ng | ChĂ†Â°a test | |
| CSH-20 | KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ sau thanh toÄ‚Â¡n | CÄ‚Â³ order Ă„â€˜ang xĂ¡Â»Â­ lÄ‚Â½ tĂ¡ÂºÂ¡i bÄ‚Â n vÄ‚Â  order Mang vĂ¡Â»Â | Thanh toÄ‚Â¡n thÄ‚Â nh cÄ‚Â´ng, API lĂ¡Â»â€”i rĂ¡Â»â€œi thĂ¡Â»Â­ lĂ¡ÂºÂ¡i, mĂ¡Â»Å¸ lĂ¡ÂºÂ¡i order/bÄ‚Â n vÄ‚Â  invoice | API lĂ¡Â»â€”i khÄ‚Â´ng xÄ‚Â³a nhĂ¡ÂºÂ§m giĂ¡Â»Â; thÄ‚Â nh cÄ‚Â´ng cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ä‚Âºng order/bÄ‚Â n, branch, receipt/invoice vÄ‚Â  khÄ‚Â´ng tĂ¡ÂºÂ¡o thanh toÄ‚Â¡n lĂ¡ÂºÂ·p | ChĂ†Â°a test | |
| CSH-21 | Header vÄ‚Â  Ă„â€˜Ă¡Â»â€¢i branch | TÄ‚Â i khoĂ¡ÂºÂ£n admin cÄ‚Â³ nhiĂ¡Â»Âu branch; manager/cashier cÄ‚Â³ branch Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p | MĂ¡Â»Å¸ dropdown branch, Ă„â€˜Ă¡Â»â€¢i branch theo quyĂ¡Â»Ân, thĂ¡Â»Â­ mĂ¡Â»Å¸ danh sÄ‚Â¡ch branch, refresh | Admin thĂ¡ÂºÂ¥y Ă„â€˜Ä‚Âºng danh sÄ‚Â¡ch vÄ‚Â  Ă„â€˜Ă¡Â»â€¢i branch; role khÄ‚Â¡c khÄ‚Â´ng cÄ‚Â³ thao tÄ‚Â¡c vĂ†Â°Ă¡Â»Â£t quyĂ¡Â»Ân; dĂ¡Â»Â¯ liĂ¡Â»â€¡u/branch context Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ä‚Âºng | ChĂ†Â°a test | |
| CSH-22 | Tab, order header vÄ‚Â  customer focus | CÄ‚Â³ bÄ‚Â n, order Mang vĂ¡Â»Â vÄ‚Â  sĂ¡Â»â€˜ Ă„â€˜iĂ¡Â»â€¡n thoĂ¡ÂºÂ¡i khÄ‚Â¡ch | ChuyĂ¡Â»Æ’n tab bÄ‚Â n/thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n/thÄ‚Â´ng bÄ‚Â¡o, refresh, Ă„â€˜Ă¡Â»â€¢i bÄ‚Â n/Mang vĂ¡Â»Â, dÄ‚Â¹ng nÄ‚Âºt focus hoĂ¡ÂºÂ·c phÄ‚Â­m tĂ¡ÂºÂ¯t tÄ‚Â¬m khÄ‚Â¡ch | Tab, branch, bÄ‚Â n/Ă„â€˜Ă†Â¡n, sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng thÄ‚Â´ng bÄ‚Â¡o, input vÄ‚Â  focus giĂ¡Â»Â¯ Ă„â€˜Ä‚Âºng; responsive khÄ‚Â´ng mĂ¡ÂºÂ¥t thao tÄ‚Â¡c | ChĂ†Â°a test | |
| CSH-23 | HĂ¡Â»Â§y/xÄ‚Â³a, ca, in vÄ‚Â  logout | CÄ‚Â³ mÄ‚Â³n chĂ†Â°a gĂ¡Â»Â­i, mÄ‚Â³n Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i, ca Ă„â€˜ang mĂ¡Â»Å¸ vÄ‚Â  order hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i | ThĂ¡Â»Â­ xÄ‚Â³a mÄ‚Â³n, hĂ¡Â»Â§y order, chĂ¡Â»â€˜t ca, xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ ca, in, logout | MÄ‚Â³n Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i bĂ¡Â»â€¹ giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n Ă„â€˜Ä‚Âºng; confirm/API lĂ¡Â»â€”i khÄ‚Â´ng xÄ‚Â³a sai; ca/in/logout giĂ¡Â»Â¯ Ă„â€˜Ä‚Âºng trĂ¡ÂºÂ¡ng thÄ‚Â¡i vÄ‚Â  redirect | ChĂ†Â°a test | |
| CSH-24 | Lifecycle mĂ¡Â»Å¸/chĂ¡Â»â€˜t ca | Cashier chĂ†Â°a cÄ‚Â³ ca vÄ‚Â  cashier Ă„â€˜Ä‚Â£ cÄ‚Â³ ca | MĂ¡Â»Å¸ `/pos`, nhĂ¡ÂºÂ­p tiĂ¡Â»Ân Ă„â€˜Ă¡ÂºÂ§u ca, chĂ¡Â»â€˜t ca vĂ¡Â»â€ºi tiĂ¡Â»Ân thĂ¡Â»Â±c tĂ¡ÂºÂ¿/ghi chÄ‚Âº, mĂ¡Â»Å¸ lĂ¡Â»â€¹ch sĂ¡Â»Â­ | Modal vÄ‚Â  dĂ¡Â»Â¯ liĂ¡Â»â€¡u ca Ă„â€˜Ä‚Âºng; khÄ‚Â´ng mĂ¡Â»Å¸/chĂ¡Â»â€˜t lĂ¡ÂºÂ·p ngoÄ‚Â i Ä‚Â½ muĂ¡Â»â€˜n; lĂ¡Â»â€¹ch sĂ¡Â»Â­ hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng branch/employee | ChĂ†Â°a test | |
| CSH-25 | LĂ¡Â»â€”i vÄ‚Â  Ă„â€˜Ă¡Â»â€¢i branch khi request ca Ă„â€˜ang chĂ¡ÂºÂ¡y | CÄ‚Â³ thĂ¡Â»Æ’ lÄ‚Â m chĂ¡ÂºÂ­m API kiĂ¡Â»Æ’m tra/mĂ¡Â»Å¸/chĂ¡Â»â€˜t ca; cÄ‚Â³ nhiĂ¡Â»Âu branch | Ă„ÂĂ¡Â»â€¢i branch hoĂ¡ÂºÂ·c logout/remount khi request Ă„â€˜ang chĂ¡ÂºÂ¡y; thĂ¡Â»Â­ API lĂ¡Â»â€”i vÄ‚Â  role admin/manager | KhÄ‚Â´ng tĂ¡ÂºÂ¡o request ca ngoÄ‚Â i lifecycle hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; role khÄ‚Â´ng dÄ‚Â¹ng ca khÄ‚Â´ng bĂ¡Â»â€¹ chĂ¡ÂºÂ·n; lĂ¡Â»â€”i giĂ¡Â»Â¯ trĂ¡ÂºÂ¡ng thÄ‚Â¡i UI Ă„â€˜Ä‚Âºng | ChĂ†Â°a test | |
| CSH-26 | Payment sau lifecycle ca | Cashier Ă„â€˜Ä‚Â£ mĂ¡Â»Å¸ ca hoĂ¡ÂºÂ·c vĂ¡Â»Â«a chĂ¡Â»â€˜t ca | Thanh toÄ‚Â¡n trĂ†Â°Ă¡Â»â€ºc/sau mĂ¡Â»Å¸ ca, thĂ¡Â»Â­ thao tÄ‚Â¡c khi modal ca Ă„â€˜ang mĂ¡Â»Å¸ | Payment dÄ‚Â¹ng cÄ‚Â¹ng `activeShift`; khÄ‚Â´ng tĂ¡ÂºÂ¡o state ca bĂ¡ÂºÂ£n sao; chĂ¡Â»â€˜t ca/logout giĂ¡Â»Â¯ hÄ‚Â nh vi hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i | ChĂ†Â°a test | |
| CSH-27 | LĂ¡Â»Âc lĂ¡Â»â€¹ch sĂ¡Â»Â­ hÄ‚Â³a Ă„â€˜Ă†Â¡n | CÄ‚Â³ invoice Ă¡Â»Å¸ nhiĂ¡Â»Âu ngÄ‚Â y/trĂ¡ÂºÂ¡ng thÄ‚Â¡i | MĂ¡Â»Å¸ lĂ¡Â»â€¹ch sĂ¡Â»Â­, tÄ‚Â¬m kiĂ¡ÂºÂ¿m, Ă„â€˜Ă¡Â»â€¢i ngÄ‚Â y, Ă„â€˜Ă¡Â»â€¢i trĂ¡ÂºÂ¡ng thÄ‚Â¡i, dÄ‚Â¹ng trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜ang xĂ¡Â»Â­ lÄ‚Â½ | Query/filter Ă„â€˜Ä‚Âºng; trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜ang xĂ¡Â»Â­ lÄ‚Â½ giĂ¡Â»Â¯ quy tĂ¡ÂºÂ¯c bĂ¡Â»Â lĂ¡Â»Âc ngÄ‚Â y; loading/rĂ¡Â»â€”ng hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng | ChĂ†Â°a test | |
| CSH-28 | Polling vÄ‚Â  Ă„â€˜Ă¡Â»â€¢i branch khi tĂ¡ÂºÂ£i history | Modal history mĂ¡Â»Å¸; cÄ‚Â³ thĂ¡Â»Æ’ lÄ‚Â m chĂ¡ÂºÂ­m request; cÄ‚Â³ nhiĂ¡Â»Âu branch | Ă„ÂÄ‚Â³ng/mĂ¡Â»Å¸ modal, chĂ¡Â»Â polling, Ă„â€˜Ă¡Â»â€¢i branch khi request Ă„â€˜ang chĂ¡ÂºÂ¡y, thĂ¡Â»Â­ API lĂ¡Â»â€”i | Polling chĂ¡Â»â€° chĂ¡ÂºÂ¡y khi modal mĂ¡Â»Å¸; cleanup Ă„â€˜Ä‚Âºng; kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ cĂ…Â© khÄ‚Â´ng ghi Ă„â€˜Ä‚Â¨ branch hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; lĂ¡Â»â€”i/abort khÄ‚Â´ng kĂ¡ÂºÂ¹t loading | ChĂ†Â°a test | |
| CSH-29 | Chi tiĂ¡ÂºÂ¿t vÄ‚Â  in hÄ‚Â³a Ă„â€˜Ă†Â¡n | CÄ‚Â³ invoice hĂ¡Â»Â£p lĂ¡Â»â€¡ vÄ‚Â  receipt settings | ChĂ¡Â»Ân chi tiĂ¡ÂºÂ¿t, Ă„â€˜Ä‚Â³ng/mĂ¡Â»Å¸ lĂ¡ÂºÂ¡i, in tĂ¡Â»Â« danh sÄ‚Â¡ch/chi tiĂ¡ÂºÂ¿t | ChĂ¡Â»Ân order trĂ†Â°Ă¡Â»â€ºc khi in; receipt/loyalty Ă„â€˜Ä‚Âºng invoice; in khÄ‚Â´ng Ă„â€˜Ă¡Â»â€¢i payment/order state | ChĂ†Â°a test | |
| CSH-30 | Ă„ÂĂ¡Â»â€¢i bĂ¡Â»â„¢ lĂ¡Â»Âc khi history Ă„â€˜ang tĂ¡ÂºÂ£i | Modal lĂ¡Â»â€¹ch sĂ¡Â»Â­ mĂ¡Â»Å¸; request cÄ‚Â³ thĂ¡Â»Æ’ chĂ¡ÂºÂ¡y chĂ¡ÂºÂ­m vÄ‚Â  cÄ‚Â³ invoice Ă¡Â»Å¸ nhiĂ¡Â»Âu trĂ¡ÂºÂ¡ng thÄ‚Â¡i/ngÄ‚Â y | Ă„ÂĂ¡Â»â€¢i tÄ‚Â¬m kiĂ¡ÂºÂ¿m/ngÄ‚Â y/trĂ¡ÂºÂ¡ng thÄ‚Â¡i liÄ‚Âªn tiĂ¡ÂºÂ¿p khi request trĂ†Â°Ă¡Â»â€ºc chĂ†Â°a xong rĂ¡Â»â€œi chĂ¡Â»Â kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ cuĂ¡Â»â€˜i | Request theo bĂ¡Â»â„¢ lĂ¡Â»Âc mĂ¡Â»â€ºi Ă„â€˜Ă†Â°Ă¡Â»Â£c dÄ‚Â¹ng; kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ cĂ…Â© khÄ‚Â´ng ghi Ă„â€˜Ä‚Â¨; loading khÄ‚Â´ng bĂ¡Â»â€¹ kĂ¡ÂºÂ¹t vÄ‚Â  polling vĂ¡ÂºÂ«n dĂ¡Â»Ân Ă„â€˜Ä‚Âºng | ChĂ†Â°a test | |
| CSH-31 | Realtime invoice vÄ‚Â  cleanup consumer | POS/invoice history mĂ¡Â»Å¸ cÄ‚Â¹ng session; cÄ‚Â³ thĂ¡Â»Æ’ tĂ¡ÂºÂ¡o PaymentCompleted vÄ‚Â  ngĂ¡ÂºÂ¯t mĂ¡ÂºÂ¡ng | Ă„ÂĂ¡Â»â€¢i filter rĂ¡Â»â€œi nhĂ¡ÂºÂ­n event, Ă„â€˜Ä‚Â³ng/mĂ¡Â»Å¸ modal, chuyĂ¡Â»Æ’n sang KDS hoĂ¡ÂºÂ·c unmount POS, reconnect vÄ‚Â  Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p lĂ¡ÂºÂ¡i tÄ‚Â i khoĂ¡ÂºÂ£n khÄ‚Â¡c | Event dÄ‚Â¹ng filter hiĂ¡Â»â€¡n hÄ‚Â nh; khÄ‚Â´ng xĂ¡Â»Â­ lÄ‚Â½ hai lĂ¡ÂºÂ§n ngoÄ‚Â i chĂ¡Â»Â§ Ă„â€˜Ä‚Â­ch; unmount mĂ¡Â»â„¢t consumer khÄ‚Â´ng lÄ‚Â m consumer khÄ‚Â¡c mĂ¡ÂºÂ¥t realtime; session cĂ…Â© khÄ‚Â´ng nhĂ¡ÂºÂ­n event | ChĂ†Â°a test | |
| CSH-32 | Debounce vÄ‚Â  latest-wins khi tĂ¡ÂºÂ£i danh sÄ‚Â¡ch quĂ¡ÂºÂ£n trĂ¡Â»â€¹ | Product/Customer/Employee/Invoice/Expense/Inventory cÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u lĂ¡Â»â€ºn hoĂ¡ÂºÂ·c API chĂ¡ÂºÂ­m | GÄ‚Âµ nhanh rĂ¡Â»â€œi xÄ‚Â³a search; Ă„â€˜Ă¡Â»â€¢i filter/branch/page khi request Ă„â€˜ang chĂ¡ÂºÂ¡y; unmount trĂ†Â°Ă¡Â»â€ºc khi request kĂ¡ÂºÂ¿t thÄ‚Âºc; thĂ¡Â»Â­ API lĂ¡Â»â€”i/Abort | Search chĂ¡Â»â€° gĂ¡Â»Â­i sau 350ms; filter/page mĂ¡Â»â€ºi khÄ‚Â´ng bĂ¡Â»â€¹ response cĂ…Â© ghi Ă„â€˜Ä‚Â¨; AbortError khÄ‚Â´ng hiĂ¡Â»â€¡n nhĂ†Â° lĂ¡Â»â€”i; page mĂ¡Â»â€ºi khÄ‚Â´ng tĂ¡ÂºÂ£i page cĂ…Â© trĂ†Â°Ă¡Â»â€ºc khi vĂ¡Â»Â page 1; loading/error thuĂ¡Â»â„¢c request mĂ¡Â»â€ºi | ChĂ†Â°a test | |
| CSH-33 | Consent tao member: tat/bat | Order chua gan customer; co so dien thoai chua tung dung va UI payment co consent | Thanh toan hai order test: mot lan tat consent, mot lan bat consent; kiem tra man hinh customer/order/loyalty sau tung lan | Tat consent khong tao member; bat consent chi tao/gan member theo rule hien co va tich diem dung; UI khong hien thanh cong gia | Chua test | UI khong du chung minh transaction; ghi Blocked neu khong quan sat duoc member/loyalty |
| CSH-34 | Customer da ton tai va khong doi ownership | Order da gan customer A; co customer B da ton tai | Mo payment, tra customer B, thu thanh toan va reload order/customer | Khong tu chuyen order tu A sang B; khong ghi de thong tin customer ngoai thao tac duoc phep; payment va loyalty khong sai | Chua test | Kiem tra order detail, customer detail va invoice; customer scope theo code hien tai can doi chieu server |
| CSH-35 | So dien thoai khong hop le | Order unpaid; chuan bi phone rong, sai dinh dang hoac khong du do dai | Nhap tung gia tri trong payment, tim customer va thu submit; reload customer/order | UI/API tra loi ro; khong tao member, khong gan association va khong ghi loyalty; order giu trang thai truoc do | Chua test | Khong dung so dien thoai that co du lieu nhay cam |
| CSH-36 | Payment fail/conflict va side effect member | Order chua thanh toan, customer chua ton tai, consent bat; co the tao conflict | Gay request loi/conflict trong local an toan, sau do reload order, customer va loyalty; thu thanh toan lai | Khong co member/association/loyalty du thua ngoai transaction; order khong chuyen paid neu payment fail; retry co phan hoi duoc kiem soat | Chua test | UI khong xac minh rollback hoan chinh; ghi log request da loc secret |
| CSH-37 | Hai may tao member cung phone | Hai order khac nhau; hai phien cashier/admin hop le; phone chua co member | Gan nhu dong thoi thanh toan hai order voi cung phone va consent bat; reload customer/order/loyalty ca hai may | Khong co member trung; mot request co the thanh cong va request con lai nhan conflict/loi duoc kiem soat; khong tich diem hai lan ngoai rule | Chua test | Schema/unique constraint va customer scope can xac minh; khong mac dinh ca hai request phai thanh cong |
| CSH-38 | Hai may thanh toan khi may A con mo payment modal | May A dang mo payment modal; may B co cung order va quyen thanh toan | May B thanh toan; de may A nhan polling/event hien hanh roi bam xac nhan lai; reload invoice/order | Chi ghi nhan mot payment; may A hien conflict/da thanh toan va khong ghi diem lap; modal khong doi thanh success gia | Chua test | Lien quan REF-05/CSH-13; tap trung vao modal cu va phan hoi sau polling |
| CSH-39 | Chuong va lich su thong bao cashier | Hai user cashier/manager cung branch; co notification va don cho xu ly | Mo chuong, doi chieu badge unread voi khu vuc don can xu ly; doc/mar-all; mo `/pos/notifications`, loc va phan trang; thu branch/ID khac quyen va target da xoa | Badge chi dem unread theo user; pending khong bi mark-read; lich su van giu muc da doc; API tu choi scope sai; trang thai van Chua test | Chua test | Can ap migration read-state truoc khi test runtime |

