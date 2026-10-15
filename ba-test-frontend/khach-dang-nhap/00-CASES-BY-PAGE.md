# Checklist theo trang — customer

Các case được chuyển khỏi checklist legacy và phân loại theo trang/nhóm. Giữ nguyên ID, trạng thái và minh chứng; cập nhật kết quả tại đây.

| Case ID | Trang/nhóm đích |
|---|---|
| CUS-01 | Menu & ordering |
| CUS-02 | Menu & ordering |
| CUS-03 | Menu & ordering |
| CUS-04 | Menu & ordering |
| CUS-05 | Profile |
| CUS-06 | Reservation |
| CUS-07 | Session |
| CUS-08 | Notifications |
| CUS-09 | Direct URL / authorization |
| CUS-10 | Branch isolation |
| CUS-11 | Runtime errors |
| CUS-12 | Navigation |
| CUS-13 | Mock AI |
| CUS-14 | Realtime |
| CUS-15 | Customer scope |
| CUS-16 | Order state |
| CUS-17 | Customer resilience |

## Menu & ordering

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-01 | Ă„ÂĂ„Æ’ng kÄ‚Â½/Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p | SĂ¡Â»â€˜ Ă„â€˜iĂ¡Â»â€¡n thoĂ¡ÂºÂ¡i test chĂ†Â°a cÄ‚Â³ hoĂ¡ÂºÂ·c Ă„â€˜Ä‚Â£ cÄ‚Â³ tÄ‚Â i khoĂ¡ÂºÂ£n | Ă„ÂĂ„Æ’ng kÄ‚Â½ hoĂ¡ÂºÂ·c Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p qua `/api/Auth/customer-token` trong UI, refresh | VÄ‚Â o menu; token/customer info Ă„â€˜Ă†Â°Ă¡Â»Â£c dÄ‚Â¹ng Ă„â€˜Ä‚Âºng; khÄ‚Â´ng hiĂ¡Â»Æ’n thĂ¡Â»â€¹ mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u/token | ChĂ†Â°a test | |
| CUS-02 | Menu vÄ‚Â  lĂ¡Â»Âc mÄ‚Â³n | CÄ‚Â³ branch/mÄ‚Â³n/topping Ă„â€˜ang bÄ‚Â¡n | MĂ¡Â»Å¸ `/`, tĂ¡ÂºÂ£i lĂ¡ÂºÂ¡i, xem sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m, topping, khuyĂ¡ÂºÂ¿n mÄ‚Â£i | Menu vÄ‚Â  giÄ‚Â¡ tĂ¡ÂºÂ£i Ă„â€˜Ä‚Âºng; lĂ¡Â»â€”i mĂ¡Â»â„¢t endpoint khÄ‚Â´ng che mĂ¡ÂºÂ¥t dĂ¡Â»Â¯ liĂ¡Â»â€¡u hĂ¡Â»Â£p lĂ¡Â»â€¡ cÄ‚Â²n lĂ¡ÂºÂ¡i; trĂ¡ÂºÂ¡ng thÄ‚Â¡i rĂ¡Â»â€”ng rÄ‚Âµ | ChĂ†Â°a test | |
| CUS-03 | GiĂ¡Â»Â hÄ‚Â ng | CÄ‚Â³ mÄ‚Â³n vÄ‚Â  topping | ThÄ‚Âªm nhiĂ¡Â»Âu mÄ‚Â³n, Ă„â€˜Ă¡Â»â€¢i sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng, xÄ‚Â³a, refresh | GiĂ¡Â»Â hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng; validation quantity; khÄ‚Â´ng gĂ¡Â»Â­i giÄ‚Â¡ do client lÄ‚Â m nguĂ¡Â»â€œn authoritative | ChĂ†Â°a test | |
| CUS-04 | GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n tĂ¡ÂºÂ¡i bÄ‚Â n/branch | Customer cÄ‚Â³ context branch/table hoĂ¡ÂºÂ·c chĂ¡Â»Ân branch hĂ¡Â»Â£p lĂ¡Â»â€¡ | XÄ‚Â¡c nhĂ¡ÂºÂ­n giĂ¡Â»Â vÄ‚Â  gĂ¡Â»Â­i Ă„â€˜Ă†Â¡n | Ă„ÂĂ†Â¡n tĂ¡ÂºÂ¡o Ă„â€˜Ä‚Âºng branch/table/customer; tĂ¡Â»â€¢ng vÄ‚Â  phÄ‚Â­ do backend tÄ‚Â­nh; lĂ¡Â»â€”i khÄ‚Â´ng tĂ¡ÂºÂ¡o duplicate | ChĂ†Â°a test | |

## Profile

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-05 | GĂ¡Â»Âi thÄ‚Âªm mÄ‚Â³n | CÄ‚Â³ Ă„â€˜Ă†Â¡n Ă„â€˜ang mĂ¡Â»Å¸ cĂ¡Â»Â§a customer | ThÄ‚Âªm mÄ‚Â³n trong cÄ‚Â¹ng bĂ¡Â»Â¯a vÄ‚Â  gĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u | Backend nĂ¡Â»â€˜i Ă„â€˜Ä‚Âºng order/flow hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; POS/bĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi; UI phĂ¡ÂºÂ£n Ä‚Â¡nh trĂ¡ÂºÂ¡ng thÄ‚Â¡i | ChĂ†Â°a test | |

## Reservation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-06 | HĂ¡Â»â€œ sĂ†Â¡ cÄ‚Â¡ nhÄ‚Â¢n | Ă„ÂÄ‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p customer | MĂ¡Â»Å¸ `/profile`, tĂ¡ÂºÂ£i profile, sĂ¡Â»Â­a thÄ‚Â´ng tin hĂ¡Â»Â£p lĂ¡Â»â€¡ rĂ¡Â»â€œi reload | ChĂ¡Â»â€° sĂ¡Â»Â­a hĂ¡Â»â€œ sĂ†Â¡ cĂ¡Â»Â§a chÄ‚Â­nh mÄ‚Â¬nh; dĂ¡Â»Â¯ liĂ¡Â»â€¡u lĂ†Â°u qua API; lĂ¡Â»â€”i validation khÄ‚Â´ng mĂ¡ÂºÂ¥t form | ChĂ†Â°a test | |

## Session

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-07 | LĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n vÄ‚Â  Ă„â€˜iĂ¡Â»Æ’m | CÄ‚Â³ order/loyalty/reservation test | Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n, loyalty history vÄ‚Â  Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n trong profile | ChĂ¡Â»â€° thĂ¡ÂºÂ¥y dĂ¡Â»Â¯ liĂ¡Â»â€¡u cĂ¡Â»Â§a customer; chi tiĂ¡ÂºÂ¿t/trĂ¡ÂºÂ¡ng thÄ‚Â¡i khĂ¡Â»â€ºp API; dĂ¡Â»Â¯ liĂ¡Â»â€¡u rĂ¡Â»â€”ng cÄ‚Â³ thÄ‚Â´ng bÄ‚Â¡o | ChĂ†Â°a test | |

## Notifications

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-08 | Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n | CÄ‚Â³ branch vÄ‚Â  bÄ‚Â n hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng | MĂ¡Â»Å¸ `/reservation`, chĂ¡Â»Ân branch/bÄ‚Â n/thĂ¡Â»Âi gian, gĂ¡Â»Â­i; thĂ¡Â»Â­ dĂ¡Â»Â¯ liĂ¡Â»â€¡u thiĂ¡ÂºÂ¿u | Reservation hĂ¡Â»Â£p lĂ¡Â»â€¡ Ă„â€˜Ă†Â°Ă¡Â»Â£c tĂ¡ÂºÂ¡o; thiĂ¡ÂºÂ¿u/sai dĂ¡Â»Â¯ liĂ¡Â»â€¡u bĂ¡Â»â€¹ bÄ‚Â¡o rÄ‚Âµ; khÄ‚Â´ng gĂ¡Â»Â­i lĂ¡ÂºÂ·p | ChĂ†Â°a test | |

## Direct URL / authorization

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-09 | QR scan | CÄ‚Â³ QR bÄ‚Â n hĂ¡Â»Â£p lĂ¡Â»â€¡ vÄ‚Â  QR sai | MĂ¡Â»Å¸ `/scan`, quÄ‚Â©t/nhĂ¡ÂºÂ­p trong Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n test | QR hĂ¡Â»Â£p lĂ¡Â»â€¡ tĂ¡ÂºÂ¡o Ă„â€˜Ä‚Âºng context; QR sai/expired khÄ‚Â´ng cho truy cĂ¡ÂºÂ­p nhĂ¡ÂºÂ§m branch/table | ChĂ†Â°a test | |

## Branch isolation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-10 | Chatbot Mock AI customer | Ă„ÂÄ‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p vÄ‚Â  Mock bĂ¡ÂºÂ­t | HĂ¡Â»Âi menu, Ă„â€˜Ă†Â¡n cĂ¡Â»Â§a mÄ‚Â¬nh, Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n; hĂ¡Â»Âi dĂ¡Â»Â¯ liĂ¡Â»â€¡u customer khÄ‚Â¡c hoĂ¡ÂºÂ·c yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡Â»â€¢i giÄ‚Â¡ | `/api/Ai/chat` dÄ‚Â¹ng role `customer`; tool customer Ă„â€˜Ä‚Âºng scope; cÄ‚Â¢u hĂ¡Â»Âi trÄ‚Â¡i quyĂ¡Â»Ân bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i; mutation cĂ¡ÂºÂ§n confirmation | ChĂ†Â°a test | |

## Runtime errors

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-11 | HĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n phiÄ‚Âªn vÄ‚Â  Ă„â€˜Ă„Æ’ng xuĂ¡ÂºÂ¥t | CÄ‚Â³ token hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n | GĂ¡Â»Âi profile/order sau 401, Ă„â€˜Ă„Æ’ng xuĂ¡ÂºÂ¥t, mĂ¡Â»Å¸ lĂ¡ÂºÂ¡i URL | Token/customer cache bĂ¡Â»â€¹ xÄ‚Â³a; trĂ¡Â»Å¸ vĂ¡Â»Â trĂ¡ÂºÂ¡ng thÄ‚Â¡i chĂ†Â°a Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p; khÄ‚Â´ng gĂ¡Â»Â­i token cĂ…Â© | ChĂ†Â°a test | |

## Navigation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-12 | Hai tÄ‚Â i khoĂ¡ÂºÂ£n vÄ‚Â  branch isolation | CÄ‚Â³ customer A/B, nĂ¡ÂºÂ¿u dĂ¡Â»Â¯ liĂ¡Â»â€¡u branch khÄ‚Â¡c cÄ‚Â³ | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p tĂ¡Â»Â«ng tÄ‚Â i khoĂ¡ÂºÂ£n, xem order/profile bĂ¡ÂºÂ±ng URL trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p | KhÄ‚Â´ng Ă„â€˜Ă¡Â»Âc/sĂ¡Â»Â­a order, profile, Ă„â€˜iĂ¡Â»Æ’m hoĂ¡ÂºÂ·c reservation cĂ¡Â»Â§a tÄ‚Â i khoĂ¡ÂºÂ£n khÄ‚Â¡c | ChĂ†Â°a test | |

## Mock AI

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-13 | IDOR order/profile/reservation | Customer A cÄ‚Â³ resource; biĂ¡ÂºÂ¿t ID/phone cĂ¡Â»Â§a customer B | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p A, thay ID/phone trong URL hoĂ¡ÂºÂ·c form Ă„â€˜Ă¡Â»Æ’ mĂ¡Â»Å¸/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t resource cĂ¡Â»Â§a B | API tĂ¡Â»Â« chĂ¡Â»â€˜i; khÄ‚Â´ng trĂ¡ÂºÂ£ dĂ¡Â»Â¯ liĂ¡Â»â€¡u chi tiĂ¡ÂºÂ¿t hoĂ¡ÂºÂ·c cho mutation; UI khÄ‚Â´ng bÄ‚Â¡o thÄ‚Â nh cÄ‚Â´ng giĂ¡ÂºÂ£ | ChĂ†Â°a test | |

## Realtime

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-14 | MĂ¡ÂºÂ­t khĂ¡ÂºÂ©u sai vÄ‚Â  tÄ‚Â i khoĂ¡ÂºÂ£n inactive | CÄ‚Â³ customer active, inactive vÄ‚Â  mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u sai | ThĂ¡Â»Â­ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p tĂ¡Â»Â«ng trĂ†Â°Ă¡Â»Âng hĂ¡Â»Â£p, reload sau lĂ¡Â»â€”i | KhÄ‚Â´ng cĂ¡ÂºÂ¥p token khi sai/inactive; thÄ‚Â´ng bÄ‚Â¡o an toÄ‚Â n, khÄ‚Â´ng lĂ¡Â»â„¢ account tĂ¡Â»â€œn tĂ¡ÂºÂ¡i hoĂ¡ÂºÂ·c mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u | ChĂ†Â°a test | |

## Customer scope

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-15 | Branch inactive khi Ă„â€˜Ă¡ÂºÂ·t Ă„â€˜Ă†Â¡n | CÄ‚Â³ branch active vÄ‚Â  inactive trong dĂ¡Â»Â¯ liĂ¡Â»â€¡u test | ChĂ¡Â»Ân/giĂ¡ÂºÂ£ lĂ¡ÂºÂ­p branch inactive rĂ¡Â»â€œi gĂ¡Â»Â­i order hoĂ¡ÂºÂ·c reservation | Server tĂ¡Â»Â« chĂ¡Â»â€˜i branch inactive; khÄ‚Â´ng tĂ¡ÂºÂ¡o order/reservation vÄ‚Â  khÄ‚Â´ng Ă„â€˜Ă¡Â»â€¢i dĂ¡Â»Â¯ liĂ¡Â»â€¡u customer | ChĂ†Â°a test | |

## Order state

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-16 | Regression giao diĂ¡Â»â€¡n DigitalMenu | Customer Ă„â€˜Ä‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p, cÄ‚Â³ menu vÄ‚Â  giĂ¡Â»Â hÄ‚Â ng | Ă„ÂĂ¡Â»â€¢i danh mĂ¡Â»Â¥c, lÄ‚Â m mĂ¡Â»â€ºi, mĂ¡Â»Å¸/Ă„â€˜Ä‚Â³ng lĂ¡ÂºÂ¡i modal tÄ‚Â¹y chĂ¡Â»Ân, chĂ¡Â»Ân/bĂ¡Â»Â chĂ¡Â»Ân size-topping, sĂ¡Â»Â­a ghi chÄ‚Âº rĂ¡Â»â€œi mĂ¡Â»Å¸/hĂ¡Â»Â§y/xÄ‚Â¡c nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n | Component trÄ‚Â¬nh bÄ‚Â y giĂ¡Â»Â¯ Ă„â€˜Ä‚Âºng loading/rĂ¡Â»â€”ng/availability, giÄ‚Â¡/sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng/tĂ¡Â»â€¢ng, disabled/loading khi lĂ¡Â»â€”i hoĂ¡ÂºÂ·c gĂ¡Â»Â­i, responsive vÄ‚Â  context branch/table; khÄ‚Â´ng gĂ¡Â»Â­i lĂ¡ÂºÂ·p | ChĂ†Â°a test | |

## Customer resilience

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| CUS-17 | Chi tiĂ¡ÂºÂ¿t lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u hÄ‚Â³a Ă„â€˜Ă†Â¡n | Customer cÄ‚Â³ Ă„â€˜Ă†Â¡n dine-in/mang vĂ¡Â»Â, Ă„â€˜Ă†Â¡n Ă„â€˜Ä‚Â£/chĂ†Â°a thanh toÄ‚Â¡n/hĂ¡Â»Â§y, cÄ‚Â³ thĂ¡Â»Æ’ cÄ‚Â³ snapshot tÄ‚Â i chÄ‚Â­nh/loyalty thiĂ¡ÂºÂ¿u | MĂ¡Â»Å¸ chi tiĂ¡ÂºÂ¿t tĂ¡Â»Â« danh sÄ‚Â¡ch, chĂ¡Â»Ân nhanh Ă„â€˜Ă†Â¡n A rĂ¡Â»â€œi B, thĂ¡Â»Â­ URL trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p cĂ¡Â»Â§a Ă„â€˜Ă†Â¡n customer khÄ‚Â¡c, xem trÄ‚Âªn mobile vÄ‚Â  khi API lĂ¡Â»â€”i | ChĂ¡Â»â€° thĂ¡ÂºÂ¥y Ă„â€˜Ă†Â¡n cĂ¡Â»Â§a chÄ‚Â­nh mÄ‚Â¬nh; chi tiĂ¡ÂºÂ¿t hiĂ¡Â»Æ’n thĂ¡Â»â€¹ mÄ‚Â£, branch/bÄ‚Â n hoĂ¡ÂºÂ·c mang vĂ¡Â»Â, thĂ¡Â»Âi gian, trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â¡n/thanh toÄ‚Â¡n, mÄ‚Â³n/SL/Ă„â€˜Ă†Â¡n giÄ‚Â¡/options, snapshot subtotal/discount/phÄ‚Â­/VAT/tĂ¡Â»â€¢ng/Ă„â€˜Ä‚Â£ trĂ¡ÂºÂ£ khi backend cÄ‚Â³; thiĂ¡ÂºÂ¿u snapshot khÄ‚Â´ng bĂ¡Â»â€¹ suy ra thÄ‚Â nh 0; loyalty Ă„â€˜Ä‚Âºng order; phĂ¡ÂºÂ£n hĂ¡Â»â€œi cĂ…Â© khÄ‚Â´ng ghi Ă„â€˜Ä‚Â¨ | ChĂ†Â°a test | Ă„ÂĂ¡Â»â€˜i chiĂ¡ÂºÂ¿u cÄ‚Â¹ng Ă„â€˜Ă†Â¡n vĂ¡Â»â€ºi Admin ReceiptPreview; guest khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c mĂ¡Â»Å¸ rĂ¡Â»â„¢ng lĂ¡Â»â€¹ch sĂ¡Â»Â­ |

