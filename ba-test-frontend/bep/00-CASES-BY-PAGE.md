# Checklist theo trang — kitchen

Các case được chuyển khỏi checklist legacy và phân loại theo trang/nhóm. Giữ nguyên ID, trạng thái và minh chứng; cập nhật kết quả tại đây.

| Case ID | Trang/nhóm đích |
|---|---|
| KIT-17 | Kitchen state |
| KIT-18 | Kitchen resilience |
| KIT-01 | Kitchen |
| KIT-02 | Kitchen |
| KIT-03 | Kitchen |
| KIT-04 | Kitchen |
| KIT-05 | Notifications |
| KIT-06 | Session & access |
| KIT-07 | Session & access |
| KIT-08 | Kitchen history |
| KIT-09 | Reservations |
| KIT-10 | Attendance / schedule |
| KIT-11 | Direct URL / authorization |
| KIT-12 | Branch isolation |
| KIT-13 | Runtime errors |
| KIT-14 | Navigation |
| KIT-15 | Mock AI |
| KIT-16 | Realtime |

## Kitchen state

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-17 | Notification Center KDS | Kitchen Branch A cÄ‚Â³ notification Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi mÄ‚Â³n | MĂ¡Â»Å¸ chuÄ‚Â´ng desktop/mobile, vÄ‚Â o `/kitchen/notifications`, mark-read/mark-all, thĂ¡Â»Â­ reconnect vÄ‚Â  logout/login | ChĂ¡Â»â€° thĂ¡ÂºÂ¥y notification Ă„â€˜Ä‚Âºng role/branch; badge khÄ‚Â´ng tÄ‚Â­nh pending; click khÄ‚Â´ng tĂ¡Â»Â± Ă„â€˜Ă¡Â»â€¢i trĂ¡ÂºÂ¡ng thÄ‚Â¡i mÄ‚Â³n; loading/error/rĂ¡Â»â€”ng Ă„â€˜Ä‚Âºng | ChĂ†Â°a test | Migration read-state Ă„â€˜Ä‚Â£ cÄ‚Â³ trong history; runtime chĂ†Â°a kiĂ¡Â»Æ’m tra |

## Kitchen resilience

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-18 | HoÄ‚Â n thÄ‚Â nh Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi Ă¢â€ â€™ employee | Kitchen chuyĂ¡Â»Æ’n mĂ¡Â»â„¢t request sang Completed | KiĂ¡Â»Æ’m tra request/item summary, retry cÄ‚Â¹ng trĂ¡ÂºÂ¡ng thÄ‚Â¡i vÄ‚Â  reconnect | Producer chĂ¡Â»â€° phÄ‚Â¡t sau commit; khÄ‚Â´ng tĂ¡ÂºÂ¡o trÄ‚Â¹ng khi lĂ¡ÂºÂ·p/concurrent; kitchen khÄ‚Â´ng bĂ¡Â»â€¹ Ă„â€˜Ă¡Â»â€¢i semantics; employee nhĂ¡ÂºÂ­n theo branch | ChĂ†Â°a test | Notification lĂ¡Â»â€”i SignalR khÄ‚Â´ng lÄ‚Â m rollback trĂ¡ÂºÂ¡ng thÄ‚Â¡i; runtime chĂ†Â°a kiĂ¡Â»Æ’m tra |

## Kitchen

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-01 | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p vÄ‚Â  menu bĂ¡ÂºÂ¿p | TÄ‚Â i khoĂ¡ÂºÂ£n `kitchen` thuĂ¡Â»â„¢c Branch A | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p nhÄ‚Â³m bĂ¡ÂºÂ¿p, refresh, mĂ¡Â»Å¸ menu tÄ‚Â i khoĂ¡ÂºÂ£n | VÄ‚Â o `/kitchen`; chĂ¡Â»â€° thĂ¡ÂºÂ¥y cÄ‚Â¡c mĂ¡Â»Â¥c bĂ¡ÂºÂ¿p, hĂ¡Â»â€œ sĂ†Â¡, chĂ¡ÂºÂ¥m cÄ‚Â´ng, lĂ¡Â»â€¹ch, Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n read-only | ChĂ†Â°a test | |
| KIT-02 | Danh sÄ‚Â¡ch yÄ‚Âªu cĂ¡ÂºÂ§u bĂ¡ÂºÂ¿p | CÄ‚Â³ Ă„â€˜Ă†Â¡n/Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi mÄ‚Â³n Branch A | MĂ¡Â»Å¸ `/kitchen`, tĂ¡ÂºÂ£i lĂ¡ÂºÂ¡i, lĂ¡Â»Âc branch nĂ¡ÂºÂ¿u cÄ‚Â³ | Danh sÄ‚Â¡ch active requests tĂ¡ÂºÂ£i Ă„â€˜Ä‚Âºng; trĂ¡ÂºÂ¡ng thÄ‚Â¡i loading/error rÄ‚Âµ; khÄ‚Â´ng hiĂ¡Â»â€¡n Ă„â€˜Ă†Â¡n branch khÄ‚Â¡c | ChĂ†Â°a test | |
| KIT-03 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i mÄ‚Â³n | CÄ‚Â³ yÄ‚Âªu cĂ¡ÂºÂ§u Ă¡Â»Å¸ trĂ¡ÂºÂ¡ng thÄ‚Â¡i mĂ¡Â»â€ºi | ChuyĂ¡Â»Æ’n mÄ‚Â³n qua cÄ‚Â¡c trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â°Ă¡Â»Â£c UI cho phÄ‚Â©p, bĂ¡ÂºÂ¥m lĂ¡ÂºÂ·p nhanh | Backend chĂ¡ÂºÂ¥p nhĂ¡ÂºÂ­n chuyĂ¡Â»Æ’n hĂ¡Â»Â£p lĂ¡Â»â€¡; thao tÄ‚Â¡c lĂ¡ÂºÂ·p khÄ‚Â´ng tĂ¡ÂºÂ¡o trĂ¡ÂºÂ¡ng thÄ‚Â¡i mÄ‚Â¢u thuĂ¡ÂºÂ«n | ChĂ†Â°a test | |
| KIT-04 | Realtime KitchenHub | MĂ¡Â»Å¸ POS/nhÄ‚Â¢n viÄ‚Âªn Ă¡Â»Å¸ phiÄ‚Âªn khÄ‚Â¡c | GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n hoĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Âi tĂ¡Â»Â« POS, quan sÄ‚Â¡t bĂ¡ÂºÂ¿p; cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t tĂ¡Â»Â« bĂ¡ÂºÂ¿p | BĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n sĂ¡Â»Â± kiĂ¡Â»â€¡n Ă„â€˜Ä‚Âºng branch, khÄ‚Â´ng cĂ¡ÂºÂ§n refresh; reconnect rĂ¡Â»â€œi Ă„â€˜Ă¡Â»â€œng bĂ¡Â»â„¢ lĂ¡ÂºÂ¡i | ChĂ†Â°a test | |

## Notifications

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-05 | LĂ¡Â»â€¹ch sĂ¡Â»Â­ vÄ‚Â  chi tiĂ¡ÂºÂ¿t | CÄ‚Â³ request Ă„â€˜Ä‚Â£ xĂ¡Â»Â­ lÄ‚Â½ | MĂ¡Â»Å¸ `/kitchen/history`, lĂ¡Â»Âc ngÄ‚Â y/trĂ¡ÂºÂ¡ng thÄ‚Â¡i/bÄ‚Â n, mĂ¡Â»Å¸ chi tiĂ¡ÂºÂ¿t | LĂ¡Â»â€¹ch sĂ¡Â»Â­/chi tiĂ¡ÂºÂ¿t Ă„â€˜Ä‚Âºng kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ API; phÄ‚Â¢n trang hoĂ¡ÂºÂ·c trĂ¡ÂºÂ¡ng thÄ‚Â¡i rĂ¡Â»â€”ng rÄ‚Âµ | ChĂ†Â°a test | |

## Session & access

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-06 | BÄ‚Â¡o hĂ¡ÂºÂ¿t mÄ‚Â³n | CÄ‚Â³ sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m test Ă„â€˜ang bÄ‚Â¡n vÄ‚Â  quyĂ¡Â»Ân endpoint | DÄ‚Â¹ng giao diĂ¡Â»â€¡n quĂ¡ÂºÂ£n lÄ‚Â½ mÄ‚Â³n nĂ¡ÂºÂ¿u Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p hoĂ¡ÂºÂ·c luĂ¡Â»â€œng thĂ¡Â»Â±c tĂ¡ÂºÂ¿ Ă„â€˜Ă¡Â»Æ’ bÄ‚Â¡o unavailable, quay lĂ¡ÂºÂ¡i menu/POS | MÄ‚Â³n hĂ¡ÂºÂ¿t Ă„â€˜Ă†Â°Ă¡Â»Â£c phĂ¡ÂºÂ£n Ä‚Â¡nh theo hÄ‚Â nh vi hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; khÄ‚Â´ng tĂ¡Â»Â± Ă„â€˜Ä‚Â¡nh dĂ¡ÂºÂ¥u lĂ¡ÂºÂ¡i khi refresh | ChĂ†Â°a test | |
| KIT-07 | SĂ†Â¡ Ă„â€˜Ă¡Â»â€œ bÄ‚Â n read-only | CÄ‚Â³ bÄ‚Â n Branch A | MĂ¡Â»Å¸ `/kitchen/tables`, thĂ¡Â»Â­ xem trĂ¡ÂºÂ¡ng thÄ‚Â¡i vÄ‚Â  tÄ‚Â¬m nÄ‚Âºt sĂ¡Â»Â­a | Xem Ă„â€˜Ă†Â°Ă¡Â»Â£c trĂ¡ÂºÂ¡ng thÄ‚Â¡i bÄ‚Â n; khÄ‚Â´ng cÄ‚Â³ hoĂ¡ÂºÂ·c khÄ‚Â´ng thĂ¡Â»Â±c hiĂ¡Â»â€¡n Ă„â€˜Ă†Â°Ă¡Â»Â£c thao tÄ‚Â¡c quĂ¡ÂºÂ£n trĂ¡Â»â€¹ trÄ‚Â¡i quyĂ¡Â»Ân | ChĂ†Â°a test | |

## Kitchen history

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-08 | Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n read-only | CÄ‚Â³ reservation test | MĂ¡Â»Å¸ `/kitchen/reservations`, mĂ¡Â»Å¸ chi tiĂ¡ÂºÂ¿t vÄ‚Â  thĂ¡Â»Â­ thay Ă„â€˜Ă¡Â»â€¢i | BĂ¡ÂºÂ¿p xem Ă„â€˜Ă†Â°Ă¡Â»Â£c khi backend cho phÄ‚Â©p; thao tÄ‚Â¡c ngoÄ‚Â i quyĂ¡Â»Ân bĂ¡Â»â€¹ Ă¡ÂºÂ©n hoĂ¡ÂºÂ·c 403 | ChĂ†Â°a test | |

## Reservations

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-09 | ChĂ¡ÂºÂ¥m cÄ‚Â´ng/lĂ¡Â»â€¹ch/hĂ¡Â»â€œ sĂ†Â¡ | TÄ‚Â i khoĂ¡ÂºÂ£n kitchen cÄ‚Â³ lĂ¡Â»â€¹ch test | MĂ¡Â»Å¸ `/kitchen/attendance`, `/kitchen/schedule`, `/kitchen/profile` | Xem vÄ‚Â  thao tÄ‚Â¡c cÄ‚Â¡ nhÄ‚Â¢n Ă„â€˜Ä‚Âºng; dĂ¡Â»Â¯ liĂ¡Â»â€¡u cÄ‚Â¡ nhÄ‚Â¢n khÄ‚Â´ng lĂ¡ÂºÂ«n ngĂ†Â°Ă¡Â»Âi khÄ‚Â¡c | ChĂ†Â°a test | |

## Attendance / schedule

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-10 | URL vÄ‚Â  branch isolation | CÄ‚Â³ Branch B Ă„â€˜Ă¡Â»Æ’ Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u | MĂ¡Â»Å¸ trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p endpoint/trang request Branch B hoĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¢i query branch | Backend tĂ¡Â»Â« chĂ¡Â»â€˜i dĂ¡Â»Â¯ liĂ¡Â»â€¡u ngoÄ‚Â i branch; frontend khÄ‚Â´ng dĂ¡Â»Â±a riÄ‚Âªng vÄ‚Â o Ă¡ÂºÂ©n menu | ChĂ†Â°a test | |

## Direct URL / authorization

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-11 | LĂ¡Â»â€”i API vÄ‚Â  mĂ¡ÂºÂ¥t kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i | CÄ‚Â³ thĂ¡Â»Æ’ ngĂ¡ÂºÂ¯t API/SignalR local | TĂ¡ÂºÂ¯t API hoĂ¡ÂºÂ·c ngĂ¡ÂºÂ¯t mĂ¡ÂºÂ¡ng khi danh sÄ‚Â¡ch/realtime Ă„â€˜ang hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng, sau Ă„â€˜Ä‚Â³ bĂ¡ÂºÂ­t lĂ¡ÂºÂ¡i | HiĂ¡Â»Æ’n thĂ¡Â»â€¹ lĂ¡Â»â€”i/kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i phÄ‚Â¹ hĂ¡Â»Â£p; khÄ‚Â´i phĂ¡Â»Â¥c khÄ‚Â´ng nhÄ‚Â¢n Ă„â€˜Ä‚Â´i request hoĂ¡ÂºÂ·c item | ChĂ†Â°a test | |

## Branch isolation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-12 | Mock AI bĂ¡ÂºÂ¿p | Chatbot hiĂ¡Â»Æ’n thĂ¡Â»â€¹ cho role kitchen; Mock bĂ¡ÂºÂ­t | HĂ¡Â»Âi trĂ¡ÂºÂ¡ng thÄ‚Â¡i bÄ‚Â n, Ă„â€˜Ă†Â¡n/mÄ‚Â³n bĂ¡ÂºÂ¿p vÄ‚Â  thĂ¡Â»Â­ yÄ‚Âªu cĂ¡ÂºÂ§u xem doanh thu nhĂ¡ÂºÂ¡y cĂ¡ÂºÂ£m | CÄ‚Â¢u hĂ¡Â»Âi Ă„â€˜Ă†Â°Ă¡Â»Â£c hĂ¡Â»â€” trĂ¡Â»Â£ trĂ¡ÂºÂ£ Ă„â€˜Ä‚Âºng scope; tool khÄ‚Â´ng thuĂ¡Â»â„¢c allowlist bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i; khÄ‚Â´ng lĂ¡Â»â„¢ dĂ¡Â»Â¯ liĂ¡Â»â€¡u tÄ‚Â i chÄ‚Â­nh ngoÄ‚Â i quyĂ¡Â»Ân | ChĂ†Â°a test | |

## Runtime errors

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-13 | Quantity Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i khÄ‚Â´ng bĂ¡Â»â€¹ giĂ¡ÂºÂ£m | CÄ‚Â³ order Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i mĂ¡Â»â„¢t phĂ¡ÂºÂ§n mÄ‚Â³n vÄ‚Â o bĂ¡ÂºÂ¿p | TĂ¡Â»Â« POS sĂ¡Â»Â­a sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng mÄ‚Â³n Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i xuĂ¡Â»â€˜ng thĂ¡ÂºÂ¥p hĂ†Â¡n sĂ¡Â»â€˜ Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i, rĂ¡Â»â€œi mĂ¡Â»Å¸ lĂ¡ÂºÂ¡i POS vÄ‚Â  KDS | Server khÄ‚Â´ng xÄ‚Â³a/giĂ¡ÂºÂ£m phĂ¡ÂºÂ§n Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i; chĂ¡Â»â€° phĂ¡ÂºÂ§n chĂ†Â°a gĂ¡Â»Â­i Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»â€°nh theo contract; lĂ¡Â»â€¹ch sĂ¡Â»Â­ bĂ¡ÂºÂ¿p vĂ¡ÂºÂ«n nhĂ¡ÂºÂ¥t quÄ‚Â¡n | ChĂ†Â°a test | |

## Navigation

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-14 | Reconnect khÄ‚Â´ng nhÄ‚Â¢n Ă„â€˜Ä‚Â´i | POS vÄ‚Â  KDS cÄ‚Â¹ng mĂ¡Â»Å¸, cÄ‚Â³ request mĂ¡Â»â€ºi | NgĂ¡ÂºÂ¯t mĂ¡ÂºÂ¡ng lÄ‚Âºc event bĂ¡ÂºÂ¿p Ă„â€˜ang Ă„â€˜Ă¡ÂºÂ¿n, bĂ¡ÂºÂ­t lĂ¡ÂºÂ¡i, chĂ¡Â»Â reconnect rĂ¡Â»â€œi refresh danh sÄ‚Â¡ch | Item/request cuĂ¡Â»â€˜i cÄ‚Â¹ng xuĂ¡ÂºÂ¥t hiĂ¡Â»â€¡n Ă„â€˜Ä‚Âºng mĂ¡Â»â„¢t lĂ¡ÂºÂ§n; trĂ¡ÂºÂ¡ng thÄ‚Â¡i sau reconnect lĂ¡ÂºÂ¥y tĂ¡Â»Â« API, khÄ‚Â´ng nhÄ‚Â¢n Ă„â€˜Ä‚Â´i event | ChĂ†Â°a test | |

## Mock AI

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-15 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t request khÄ‚Â¡c branch | KDS user Branch A vÄ‚Â  request Branch B Ă„â€˜Ä‚Â£ biĂ¡ÂºÂ¿t mÄ‚Â£ | MĂ¡Â»Å¸/Ă„â€˜Ă¡Â»â€¢i trĂ¡ÂºÂ¡ng thÄ‚Â¡i request B bĂ¡ÂºÂ±ng URL hoĂ¡ÂºÂ·c thao tÄ‚Â¡c Ă„â€˜Ă†Â°Ă¡Â»Â£c sĂ¡Â»Â­a request | Backend tĂ¡Â»Â« chĂ¡Â»â€˜i; KDS A khÄ‚Â´ng nhĂ¡ÂºÂ­n broadcast hoĂ¡ÂºÂ·c dĂ¡Â»Â¯ liĂ¡Â»â€¡u chi tiĂ¡ÂºÂ¿t cĂ¡Â»Â§a Branch B | ChĂ†Â°a test | |

## Realtime

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| KIT-16 | CĂ¡ÂºÂ¥u hÄ‚Â¬nh chĂ¡Â»Â§ quÄ‚Â¡n trÄ‚Â¡i quyĂ¡Â»Ân | TÄ‚Â i khoĂ¡ÂºÂ£n kitchen Branch A | MĂ¡Â»Å¸ trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p `/settings`, `/settings/receipt`, `/print-templates`, `/branches` | KhÄ‚Â´ng cÄ‚Â³ bÄ‚Â¡nh rĂ„Æ’ng hoĂ¡ÂºÂ·c page cĂ¡ÂºÂ¥u hÄ‚Â¬nh; khÄ‚Â´ng thĂ¡Â»Æ’ sĂ¡Â»Â­a cĂ¡ÂºÂ¥u hÄ‚Â¬nh qua URL; Kitchen flow vĂ¡ÂºÂ«n hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng | ChĂ†Â°a test | |

