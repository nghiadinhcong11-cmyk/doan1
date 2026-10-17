# Checklist theo trang — guest

Các case được chuyển khỏi checklist legacy và phân loại theo trang/nhóm. Giữ nguyên ID, trạng thái và minh chứng; cập nhật kết quả tại đây.

| Case ID | Trang/nhóm đích |
|---|---|
| GST-01 | QR menu & ordering |
| GST-02 | QR menu & ordering |
| GST-03 | QR menu & ordering |
| GST-04 | QR menu & ordering |
| GST-05 | Reservation |
| GST-06 | Session |
| GST-07 | Notifications |
| GST-08 | Direct URL / authorization |
| GST-11 | Session transition |
| GST-12 | Runtime errors |
| GST-13 | QR scope |
| GST-14 | QR scope |
| GST-16 | QR menu regression |

## QR menu & ordering

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-01 | VÄ‚Â o menu cÄ‚Â´ng khai | MĂ¡Â»Å¸ customer web khi chĂ†Â°a cÄ‚Â³ customer token | Truy cĂ¡ÂºÂ­p `/` khÄ‚Â´ng cÄ‚Â³ query QR | Menu/guest entry hiĂ¡Â»Æ’n thĂ¡Â»â€¹ theo UI; khÄ‚Â´ng tĂ¡Â»Â± nhĂ¡ÂºÂ­n lÄ‚Â  customer Ă„â€˜Ä‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p | ChĂ†Â°a test | |
| GST-02 | QR bootstrap | CÄ‚Â³ QR token hĂ¡Â»Â£p lĂ¡Â»â€¡ gĂ¡ÂºÂ¯n vĂ¡Â»â€ºi bÄ‚Â n duy nhĂ¡ÂºÂ¥t | MĂ¡Â»Å¸ URL cÄ‚Â³ `?qr=...`, chĂ¡Â»Â bootstrap | Guest session Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p Ă„â€˜Ä‚Âºng branch/table; menu hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng context | ChĂ†Â°a test | |
| GST-03 | QR sai/khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡ | CÄ‚Â³ QR giĂ¡ÂºÂ£, QR hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n hoĂ¡ÂºÂ·c bÄ‚Â n khÄ‚Â´ng active | MĂ¡Â»Å¸ URL QR lĂ¡Â»â€”i vÄ‚Â  refresh | HiĂ¡Â»Æ’n thĂ¡Â»â€¹ lĂ¡Â»â€”i an toÄ‚Â n; khÄ‚Â´ng tĂ¡ÂºÂ¡o session hoĂ¡ÂºÂ·c truy cĂ¡ÂºÂ­p dĂ¡Â»Â¯ liĂ¡Â»â€¡u branch khÄ‚Â¡c | ChĂ†Â°a test | |
| GST-04 | Direct guest session | CÄ‚Â³ branch active, cÄ‚Â³/khÄ‚Â´ng cÄ‚Â³ table hĂ¡Â»Â£p lĂ¡Â»â€¡ | VÄ‚Â o menu trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p, chĂ¡Â»Ân branch/table nĂ¡ÂºÂ¿u UI yÄ‚Âªu cĂ¡ÂºÂ§u | Server xÄ‚Â¡c minh branch/table; token guest cÄ‚Â³ thĂ¡Â»Âi hĂ¡ÂºÂ¡n; context hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng | ChĂ†Â°a test | |

## Reservation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-05 | Menu/topping/promotion | API public menu hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng | MĂ¡Â»Å¸ menu, xem giÄ‚Â¡, topping, khuyĂ¡ÂºÂ¿n mÄ‚Â£i, thĂ¡Â»Â­ dĂ¡Â»Â¯ liĂ¡Â»â€¡u rĂ¡Â»â€”ng | ChĂ¡Â»â€° dĂ¡Â»Â¯ liĂ¡Â»â€¡u public Ă„â€˜Ă†Â°Ă¡Â»Â£c hiĂ¡Â»Æ’n thĂ¡Â»â€¹; lĂ¡Â»â€”i tĂ¡Â»Â«ng endpoint cÄ‚Â³ trĂ¡ÂºÂ¡ng thÄ‚Â¡i riÄ‚Âªng; khÄ‚Â´ng lĂ¡Â»â„¢ thÄ‚Â´ng tin nĂ¡Â»â„¢i bĂ¡Â»â„¢ | ChĂ†Â°a test | |

## Session

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-06 | GiĂ¡Â»Â hÄ‚Â ng guest | Ă„ÂÄ‚Â£ cÄ‚Â³ guest session | ThÄ‚Âªm/xÄ‚Â³a/Ă„â€˜Ă¡Â»â€¢i sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng mÄ‚Â³n, refresh | GiĂ¡Â»Â Ă„â€˜Ä‚Âºng session/UX hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; quantity khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡ bĂ¡Â»â€¹ chĂ¡ÂºÂ·n; khÄ‚Â´ng tin giÄ‚Â¡ client | ChĂ†Â°a test | |

## Notifications

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-07 | GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u/Ă„â€˜Ă†Â¡n guest | Guest cÄ‚Â³ context bÄ‚Â n/branch vÄ‚Â  mÄ‚Â³n | GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u tĂ¡Â»Â« menu; bĂ¡ÂºÂ¥m gĂ¡Â»Â­i nhiĂ¡Â»Âu lĂ¡ÂºÂ§n | Server tĂ¡ÂºÂ¡o Ă„â€˜Ä‚Âºng guest order/context; khÄ‚Â´ng duplicate; tĂ¡Â»â€¢ng do backend tÄ‚Â­nh | ChĂ†Â°a test | |

## Direct URL / authorization

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-08 | Guest scope | Guest session Ă¡Â»Å¸ Branch A | ThĂ¡Â»Â­ thay branch/table/order ID trong URL hoĂ¡ÂºÂ·c request UI | Server tĂ¡Â»Â« chĂ¡Â»â€˜i resource ngoÄ‚Â i guest branch/table; frontend khÄ‚Â´ng chĂ¡Â»â€° dĂ¡Â»Â±a vÄ‚Â o Ă¡ÂºÂ©n nÄ‚Âºt | ChĂ†Â°a test | |

## QR scope

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-13 | Guest session scope theo phiÄ‚Âªn | CÄ‚Â³ hai guest session/JTI khÄ‚Â¡c nhau Ă¡Â»Å¸ Branch A/B | DÄ‚Â¹ng session A mĂ¡Â»Å¸ order/table hoĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¢i token/context sang resource cĂ¡Â»Â§a B | Server xÄ‚Â¡c minh cĂ¡ÂºÂ£ branch/table/session; session A khÄ‚Â´ng Ă„â€˜Ă¡Â»Âc hoĂ¡ÂºÂ·c mutate scope B | ChĂ†Â°a test | |
| GST-14 | Client gĂ¡Â»Â­i branch/table bĂ¡Â»â€¹ ghi Ă„â€˜Ä‚Â¨ | CÄ‚Â³ QR hĂ¡Â»Â£p lĂ¡Â»â€¡ cĂ¡Â»Â§a Branch A/Table 1 | SĂ¡Â»Â­a branchId/tableId trong request UI hoĂ¡ÂºÂ·c query sang B/Table 2 rĂ¡Â»â€œi gĂ¡Â»Â­i order | Backend dÄ‚Â¹ng context QR/guest Ă„â€˜Ä‚Â£ xÄ‚Â¡c minh, bĂ¡Â»Â giÄ‚Â¡ trĂ¡Â»â€¹ client giĂ¡ÂºÂ£; order chĂ¡Â»â€° thuĂ¡Â»â„¢c A/Table 1 | ChĂ†Â°a test | |

## Mock AI (centralized in primary suite)

Guest chat cases are maintained in [`../ai-chat/03-khach-qr.md`](../ai-chat/03-khach-qr.md); environment, API and multi-session checks are in [`../ky-thuat-runtime/README.md`](../ky-thuat-runtime/README.md).

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|

## Session transition

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-11 | HĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n/Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p chuyĂ¡Â»Æ’n mode | Guest token hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n hoĂ¡ÂºÂ·c customer login thÄ‚Â nh cÄ‚Â´ng | Ă„ÂĂ¡Â»Æ’ token hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n rĂ¡Â»â€œi tĂ¡ÂºÂ£i menu; sau Ă„â€˜Ä‚Â³ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p customer | Session guest bĂ¡Â»â€¹ xÄ‚Â³a; chuyĂ¡Â»Æ’n sang session customer sĂ¡ÂºÂ¡ch; khÄ‚Â´ng trĂ¡Â»â„¢n token/localStorage | ChĂ†Â°a test | |

## Runtime errors

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-12 | Loading/API/refresh | CÄ‚Â³ thĂ¡Â»Æ’ ngĂ¡ÂºÂ¯t API trong lÄ‚Âºc menu hoĂ¡ÂºÂ·c gĂ¡Â»Â­i Ă„â€˜Ă†Â¡n | NgĂ¡ÂºÂ¯t mĂ¡ÂºÂ¡ng khi Ă„â€˜ang tĂ¡ÂºÂ£i vÄ‚Â  khi submit, bĂ¡ÂºÂ­t lĂ¡ÂºÂ¡i rĂ¡Â»â€œi refresh | CÄ‚Â³ thÄ‚Â´ng bÄ‚Â¡o lĂ¡Â»â€”i vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i loading; retry khÄ‚Â´ng tĂ¡Â»Â± tĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n lĂ¡ÂºÂ·p | ChĂ†Â°a test | |

## Mock AI permissions

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|

## QR menu regression

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| GST-16 | Regression giao diĂ¡Â»â€¡n DigitalMenu guest | Guest cÄ‚Â³ QR/session hĂ¡Â»Â£p lĂ¡Â»â€¡ vÄ‚Â  menu | Ă„ÂĂ¡Â»â€¢i danh mĂ¡Â»Â¥c, refresh, mĂ¡Â»Å¸/Ă„â€˜Ä‚Â³ng modal tÄ‚Â¹y chĂ¡Â»Ân vÄ‚Â  xÄ‚Â¡c nhĂ¡ÂºÂ­n, thÄ‚Âªm/sĂ¡Â»Â­a/xÄ‚Â³a mÄ‚Â³n rĂ¡Â»â€œi hĂ¡Â»Â§y/gĂ¡Â»Â­i Ă„â€˜Ă†Â¡n; thĂ¡Â»Â­ lĂ¡Â»â€”i API | Guest session, branch/table, availability, giÄ‚Â¡/sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng, loading/empty/error, responsive vÄ‚Â  modal vĂ¡ÂºÂ«n Ă„â€˜Ä‚Âºng; khÄ‚Â´ng cĂ¡ÂºÂ¥p lĂ¡ÂºÂ¡i scope guest hoĂ¡ÂºÂ·c gĂ¡Â»Â­i lĂ¡ÂºÂ·p | ChĂ†Â°a test | |
