# KhÄ‚Â¡ch QR Ă¢â‚¬â€ truy cĂ¡ÂºÂ­p trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p /reservation vÄ‚Â  /profile

- Source: apps/customer-web/src/App.tsx, ReservationPage.tsx, CustomerProfile.tsx, CustomerLogin.tsx.
- Source hiĂ¡Â»â€¡n khÄ‚Â´ng cÄ‚Â³ route guard riÄ‚Âªng cho guest Ă„â€˜Ă¡Â»â€˜i vĂ¡Â»â€ºi hai path nÄ‚Â y. Khi chĂ†Â°a Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p vÄ‚Â  khÄ‚Â´ng Ă¡Â»Å¸ guest/QR mode, App render CustomerLogin; khi guest mode hoĂ¡ÂºÂ·c cÄ‚Â³ query qr, App render Routes vÄ‚Â  hai path Ă„â€˜Ă†Â°Ă¡Â»Â£c khai bÄ‚Â¡o.
- QuyĂ¡Â»Ân dĂ¡Â»Â¯ liĂ¡Â»â€¡u cuĂ¡Â»â€˜i cÄ‚Â¹ng vĂ¡ÂºÂ«n phĂ¡ÂºÂ£i do API xÄ‚Â¡c minh. ChÄ‚Â­nh sÄ‚Â¡ch sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m Ă¢â‚¬Å“guest cÄ‚Â³ Ă„â€˜Ă†Â°Ă¡Â»Â£c xem reservation/profile hay khÄ‚Â´ngĂ¢â‚¬Â chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»â€˜t tĂ¡Â»Â« source.

| ID | ChĂ¡Â»Â©c nĂ„Æ’ng | Ă„ÂiĂ¡Â»Âu kiĂ¡Â»â€¡n/dĂ¡Â»Â¯ liĂ¡Â»â€¡u | CÄ‚Â¡c bĂ†Â°Ă¡Â»â€ºc | KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ mong Ă„â€˜Ă¡Â»Â£i tĂ¡Â»Â« source | TrĂ¡ÂºÂ¡ng thÄ‚Â¡i | KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ thĂ¡Â»Â±c tĂ¡ÂºÂ¿ | Minh chĂ¡Â»Â©ng/finding |
|---|---|---|---|---|---|---|---|
| KQR-01 | Direct /reservation khi chĂ†Â°a Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p | XÄ‚Â³a customerToken, guest mode vÄ‚Â  mĂ¡Â»Å¸ Customer Web | NhĂ¡ÂºÂ­p trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p /reservation; refresh; dÄ‚Â¹ng Back/Forward | App hiĂ¡Â»Æ’n thĂ¡Â»â€¹ CustomerLogin thay vÄ‚Â¬ khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh guest Ă„â€˜Ă†Â°Ă¡Â»Â£c xem reservation; chĂ†Â°a cÄ‚Â³ redirect route riÄ‚Âªng | ChĂ†Â°a test | | COV-001 |
| KQR-02 | Direct /profile khi chĂ†Â°a Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p | XÄ‚Â³a customerToken, guest mode vÄ‚Â  mĂ¡Â»Å¸ Customer Web | NhĂ¡ÂºÂ­p trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p /profile; refresh | App hiĂ¡Â»Æ’n thĂ¡Â»â€¹ CustomerLogin; khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c coi lÄ‚Â  Ă„â€˜Ä‚Â£ xÄ‚Â¡c minh quyĂ¡Â»Ân profile | ChĂ†Â°a test | | COV-001 |
| KQR-03 | Direct /reservation trong guest session | TĂ¡ÂºÂ¡o guest QR session hĂ¡Â»Â£p lĂ¡Â»â€¡ Ă¡Â»Å¸ Branch A | MĂ¡Â»Å¸ /reservation trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p; quan sÄ‚Â¡t page/API; thĂ¡Â»Â­ dĂ¡Â»Â¯ liĂ¡Â»â€¡u khÄ‚Â´ng thuĂ¡Â»â„¢c session | Route Ă„â€˜Ă†Â°Ă¡Â»Â£c render theo shell hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; API phĂ¡ÂºÂ£i quyĂ¡ÂºÂ¿t Ă„â€˜Ă¡Â»â€¹nh dĂ¡Â»Â¯ liĂ¡Â»â€¡u guest Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p xem; nĂ¡ÂºÂ¿u policy khÄ‚Â´ng cho phÄ‚Â©p phĂ¡ÂºÂ£i redirect/error rÄ‚Âµ | ChĂ†Â°a test | | COV-001 |
| KQR-04 | Direct /profile trong guest session | Guest QR session hợp lệ, không có customerToken | Mở /profile; theo dõi Network; thử reload và quay lại menu | Hiện lời mời đăng nhập rõ ràng; không loading vô hạn; không gọi customer profile API; không tự xóa guest token, giỏ hàng hoặc QR context | Chưa test | | COV-001 |
| KQR-05 | /reservation vÄ‚Â  /profile vĂ¡Â»â€ºi query qr | MĂ¡Â»Å¸ /reservation?qr=... hoĂ¡ÂºÂ·c /profile?qr=... vĂ¡Â»â€ºi QR hĂ¡Â»Â£p lĂ¡Â»â€¡/sai/hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n | MĂ¡Â»Å¸ URL; refresh; theo dÄ‚Âµi bootstrap vÄ‚Â  authorization | isQrBootstrap chĂ¡Â»â€° Ă„â€˜iĂ¡Â»Âu khiĂ¡Â»Æ’n shell theo source; bootstrap/session/API phĂ¡ÂºÂ£i quyĂ¡ÂºÂ¿t Ă„â€˜Ă¡Â»â€¹nh scope; QR sai/hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n khÄ‚Â´ng mĂ¡Â»Å¸ dĂ¡Â»Â¯ liĂ¡Â»â€¡u ngoÄ‚Â i scope | ChĂ†Â°a test | | COV-001 |
| KQR-06 | Logout/chuyĂ¡Â»Æ’n mode sau direct route | CÄ‚Â³ guest session vÄ‚Â  customer login khĂ¡ÂºÂ£ dĂ¡Â»Â¥ng | MĂ¡Â»Å¸ direct route; logout/login; reload | Token/session cĂ…Â© bĂ¡Â»â€¹ dĂ¡Â»Ân theo App; dĂ¡Â»Â¯ liĂ¡Â»â€¡u hiĂ¡Â»Æ’n thĂ¡Â»â€¹ thuĂ¡Â»â„¢c session hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; khÄ‚Â´ng trĂ¡Â»â„¢n guest/customer state | ChĂ†Â°a test | | COV-001 |

| KQR-07 | Guest nháº¥n menu CĂ¡ nhĂ¢n | Guest QR/direct session cĂ²n hiá»‡u lá»±c, cĂ³ giá» hĂ ng vĂ  QR context | Nháº¥n CĂ¡ nhĂ¢n tá»« menu; theo dĂµi Network; quay láº¡i menu | Hiá»‡n lá»i má»i Ä‘Äƒng nháº­p vĂ  nĂºt Ä‘Äƒng nháº­p; khĂ´ng loading vĂ´ háº¡n; khĂ´ng gá»i `/api/Customer/{phone}`; session/giá» hĂ ng/QR context khĂ´ng bá»‹ xĂ³a | ChÆ°a test | | COV-001 |
| KQR-08 | Customer đăng nhập từ lời mời profile | Guest session còn hiệu lực; có tài khoản customer test | Nhấn đăng nhập; hoàn tất login; quay lại /profile; đăng xuất | Login thay đổi session theo luồng hợp lệ; profile load/error kết thúc; không trộn guest/customer state | Chưa test | | COV-001 |

Finding COV-001: source chĂ†Â°a xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh chÄ‚Â­nh sÄ‚Â¡ch sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m cho guest tĂ¡ÂºÂ¡i /reservation vÄ‚Â  /profile. KhÄ‚Â´ng tĂ¡Â»Â± coi route declaration lÄ‚Â  quyĂ¡Â»Ân truy cĂ¡ÂºÂ­p; cĂ¡ÂºÂ§n quyĂ¡ÂºÂ¿t Ă„â€˜Ă¡Â»â€¹nh nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ hoĂ¡ÂºÂ·c kiĂ¡Â»Æ’m chĂ¡Â»Â©ng API trÄ‚Âªn mÄ‚Â´i trĂ†Â°Ă¡Â»Âng test riÄ‚Âªng.





