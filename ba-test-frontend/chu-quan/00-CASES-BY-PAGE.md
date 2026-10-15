# Checklist theo trang — admin

Các case được chuyển khỏi checklist legacy và phân loại theo trang/nhóm. Giữ nguyên ID, trạng thái và minh chứng; cập nhật kết quả tại đây.

| Case ID | Trang/nhóm đích |
|---|---|
| ADM-01 | Session & quyền |
| ADM-02 | Dashboard & insights |
| ADM-03 | Branch context |
| ADM-04 | Catalog |
| ADM-05 | Tables & reservations |
| ADM-06 | POS & payment |
| ADM-07 | Invoices & expenses |
| ADM-08 | Employees / attendance / schedule |
| ADM-09 | Inventory |
| ADM-10 | Settings & branches |
| ADM-11 | Navigation / notifications / runtime |
| ADM-12 | Other |
| ADM-13 | Navigation / notifications / runtime |
| ADM-14 | POS & payment |
| ADM-15 | POS & payment |
| ADM-16 | Dashboard & insights |
| ADM-17 | Navigation / notifications / runtime |
| ADM-18 | Settings & branches |
| ADM-19 | Settings & branches |
| ADM-20 | POS & payment |
| ADM-21 | Navigation / notifications / runtime |

## Session & quyền

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-01 | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p management | TÄ‚Â i khoĂ¡ÂºÂ£n role `admin` hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p nhÄ‚Â³m quĂ¡ÂºÂ£n lÄ‚Â½, refresh trang, Ă„â€˜Ă„Æ’ng xuĂ¡ÂºÂ¥t rĂ¡Â»â€œi Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p lĂ¡ÂºÂ¡i | VÄ‚Â o `/dashboard`; role/branch lĂ¡ÂºÂ¥y tĂ¡Â»Â« phiÄ‚Âªn server; refresh khÄ‚Â´ng mĂ¡Â»Å¸ mÄ‚Â n hÄ‚Â¬nh trÄ‚Â¡i quyĂ¡Â»Ân | ChĂ†Â°a test | |

## Dashboard & insights

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-02 | Dashboard vÄ‚Â  business insights | CÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜Ă†Â¡n/doanh thu vÄ‚Â  cĂ¡ÂºÂ£nh bÄ‚Â¡o | MĂ¡Â»Å¸ `/dashboard`, `/business-insights`, mĂ¡Â»Å¸ chi tiĂ¡ÂºÂ¿t rĂ¡Â»â€œi Ă„â€˜Ä‚Â¡nh dĂ¡ÂºÂ¥u Ă„â€˜Ă¡Â»Âc/resolve | KPI, cĂ¡ÂºÂ£nh bÄ‚Â¡o vÄ‚Â  chi tiĂ¡ÂºÂ¿t tĂ¡ÂºÂ£i Ă„â€˜Ä‚Âºng; lĂ¡Â»â€”i API cÄ‚Â³ thÄ‚Â´ng bÄ‚Â¡o; khÄ‚Â´ng lĂ¡Â»â„¢ dĂ¡Â»Â¯ liĂ¡Â»â€¡u ngoÄ‚Â i phĂ¡ÂºÂ¡m vi Ă„â€˜Ă†Â°Ă¡Â»Â£c API trĂ¡ÂºÂ£ | ChĂ†Â°a test | |
| ADM-16 | Cache vÄ‚Â  Ă„â€˜Ă¡Â»â€˜i soÄ‚Â¡t bÄ‚Â¡o cÄ‚Â¡o | CÄ‚Â³ Ă„â€˜Ă†Â¡n/expense thÄ‚Â nh cÄ‚Â´ng vÄ‚Â  mĂ¡Â»â„¢t thao tÄ‚Â¡c bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i | Ghi KPI trĂ†Â°Ă¡Â»â€ºc; thĂ¡Â»Â±c hiĂ¡Â»â€¡n thao tÄ‚Â¡c thÄ‚Â nh cÄ‚Â´ng rĂ¡Â»â€œi thao tÄ‚Â¡c lĂ¡Â»â€”i/rollback; tĂ¡ÂºÂ£i lĂ¡ÂºÂ¡i dashboard vÄ‚Â  Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u invoice | Cache chĂ¡Â»â€° Ă„â€˜Ă¡Â»â€¢i sau commit thÄ‚Â nh cÄ‚Â´ng; thao tÄ‚Â¡c lĂ¡Â»â€”i khÄ‚Â´ng lÄ‚Â m KPI nhĂ¡ÂºÂ£y hoĂ¡ÂºÂ·c tĂ¡ÂºÂ¡o bĂ¡ÂºÂ£n ghi trÄ‚Â¹ng | ChĂ†Â°a test | |

## Branch context

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-03 | ChĂ¡Â»Ân branch | CÄ‚Â³ Ä‚Â­t nhĂ¡ÂºÂ¥t hai branch hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng | Ă„ÂĂ¡Â»â€¢i branch trÄ‚Âªn navbar, reload, Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u dashboard/bÄ‚Â n/Ă„â€˜Ă†Â¡n | DĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜Ă¡Â»â€¢i theo branch; khÄ‚Â´ng cÄ‚Â²n cache hiĂ¡Â»Æ’n thĂ¡Â»â€¹ branch cĂ…Â©; admin cÄ‚Â³ thĂ¡Â»Æ’ chĂ¡Â»Ân branch theo UI hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i | ChĂ†Â°a test | |

## Catalog

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-04 | SĂ¡ÂºÂ£n phĂ¡ÂºÂ©m, topping, khuyĂ¡ÂºÂ¿n mÄ‚Â£i | CÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u mÄ‚Â³n/topping test | MĂ¡Â»Å¸ `/products`, `/toppings`, `/promotions`; tĂ¡ÂºÂ¡o/sĂ¡Â»Â­a/tĂ¡ÂºÂ¯t mĂ¡Â»â„¢t dĂ¡Â»Â¯ liĂ¡Â»â€¡u test | Danh sÄ‚Â¡ch vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t; giÄ‚Â¡ hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng; thao tÄ‚Â¡c lĂ¡Â»â€”i hiĂ¡Â»Æ’n thĂ¡Â»â€¹ thÄ‚Â´ng bÄ‚Â¡o | ChĂ†Â°a test | |

## Tables & reservations

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-05 | BÄ‚Â n, QR vÄ‚Â  Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n | CÄ‚Â³ area/bÄ‚Â n test | MĂ¡Â»Å¸ `/tables`, xem QR, tĂ¡ÂºÂ¡o/sĂ¡Â»Â­a trĂ¡ÂºÂ¡ng thÄ‚Â¡i bÄ‚Â n, mĂ¡Â»Å¸ `/reservations` | BÄ‚Â n/QR/Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n Ă„â€˜Ä‚Âºng branch; khÄ‚Â´ng tĂ¡ÂºÂ¡o trÄ‚Â¹ng hoĂ¡ÂºÂ·c gĂ¡Â»Â­i thao tÄ‚Â¡c lĂ¡ÂºÂ·p ngoÄ‚Â i quy tĂ¡ÂºÂ¯c | ChĂ†Â°a test | |

## POS & payment

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-06 | POS vÄ‚Â  tĂ¡Â»â€¢ng tiĂ¡Â»Ân | CÄ‚Â³ mÄ‚Â³n, topping, bÄ‚Â n vÄ‚Â  Ă„â€˜Ă†Â¡n test | MĂ¡Â»Å¸ `/pos`, thÄ‚Âªm nhiĂ¡Â»Âu mÄ‚Â³n/topping, xem subtotal, phÄ‚Â­, VAT, giĂ¡ÂºÂ£m giÄ‚Â¡ rĂ¡Â»â€œi thanh toÄ‚Â¡n test | Frontend hiĂ¡Â»Æ’n thĂ¡Â»â€¹ tĂ¡Â»â€¢ng tĂ¡Â»Â« backend; khÄ‚Â´ng tin `clientTotalAmount`; tiĂ¡Â»Ân thĂ¡ÂºÂ­p phÄ‚Â¢n vÄ‚Â  nhiĂ¡Â»Âu khoĂ¡ÂºÂ£n khÄ‚Â´ng bĂ¡Â»â€¹ sai do float | ChĂ†Â°a test | |
| ADM-14 | LÄ‚Â m trÄ‚Â²n nhiĂ¡Â»Âu khoĂ¡ÂºÂ£n tiĂ¡Â»Ân | Branch A Ă„â€˜Ă¡ÂºÂ·t service fee 5%, VAT 8%; sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m cÄ‚Â³ giÄ‚Â¡ vÄ‚Â  topping thĂ¡ÂºÂ­p phÄ‚Â¢n theo dĂ¡Â»Â¯ liĂ¡Â»â€¡u test | TĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n nhiĂ¡Â»Âu mÄ‚Â³n, ghi subtotal; Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u service fee lÄ‚Â m trÄ‚Â²n 0 chĂ¡Â»Â¯ sĂ¡Â»â€˜, VAT tÄ‚Â­nh trÄ‚Âªn subtotal + fee vÄ‚Â  final total | CÄ‚Â¡c khoĂ¡ÂºÂ£n khĂ¡Â»â€ºp persisted order, khÄ‚Â´ng lĂ¡Â»â€¡ch do double/float vÄ‚Â  khÄ‚Â´ng lÄ‚Â m trÄ‚Â²n lĂ¡ÂºÂ·p giĂ¡Â»Â¯a cÄ‚Â¡c dÄ‚Â²ng | ChĂ†Â°a test | |
| ADM-15 | Snapshot sau Ă„â€˜Ă¡Â»â€¢i cÄ‚Â i Ă„â€˜Ă¡ÂºÂ·t | CÄ‚Â³ Ă„â€˜Ă†Â¡n mĂ¡Â»â€ºi vÄ‚Â  quyĂ¡Â»Ân sĂ¡Â»Â­a system settings Branch A | TĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n, ghi VAT/service fee; Ă„â€˜Ă¡Â»â€¢i settings rĂ¡Â»â€œi reload Ă„â€˜Ă†Â¡n cĂ…Â© vÄ‚Â  tĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n mĂ¡Â»â€ºi | Ă„ÂĂ†Â¡n cĂ…Â© giĂ¡Â»Â¯ snapshot lĂ¡Â»â€¹ch sĂ¡Â»Â­; Ă„â€˜Ă†Â¡n mĂ¡Â»â€ºi dÄ‚Â¹ng settings mĂ¡Â»â€ºi; bÄ‚Â¡o cÄ‚Â¡o khÄ‚Â´ng tÄ‚Â­nh lĂ¡ÂºÂ¡i Ă„â€˜Ă†Â¡n cĂ…Â© theo settings hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i | ChĂ†Â°a test | |
| ADM-20 | NhĂ¡ÂºÂ¥t quÄ‚Â¡n tĂ¡Â»â€¢ng tiĂ¡Â»Ân POS/thanh toÄ‚Â¡n/VietQR | CÄ‚Â³ branch cĂ¡ÂºÂ¥u hÄ‚Â¬nh VAT/phÄ‚Â­; cÄ‚Â³ order mĂ¡Â»â€ºi vÄ‚Â  order Ă„â€˜Ä‚Â£ lĂ†Â°u | KiĂ¡Â»Æ’m tra giĂ¡Â»Â khÄ‚Â´ng VAT/phÄ‚Â­, chĂ¡Â»â€° VAT, chĂ¡Â»â€° phÄ‚Â­, cĂ¡ÂºÂ£ hai; dÄ‚Â¹ng vÄ‚Â­ dĂ¡Â»Â¥ 100.000 + phÄ‚Â­ 5% + VAT 8%; thÄ‚Âªm mÄ‚Â³n rĂ¡Â»â€œi mĂ¡Â»Å¸ thanh toÄ‚Â¡n ngay; thĂ¡Â»Â­ autosave chĂ¡ÂºÂ­m/lĂ¡Â»â€”i, nhĂ¡ÂºÂ­n web order, redeem Ă„â€˜iĂ¡Â»Æ’m, Ă„â€˜Ă¡Â»â€¢i bÄ‚Â n/branch vÄ‚Â  mÄ‚Â´ phĂ¡Â»Âng mÄ‚Â¡y khÄ‚Â¡c thay Ă„â€˜Ă¡Â»â€¢i order; Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u footer, payment modal, breakdown, amount QR, payment vÄ‚Â  receipt | GiĂ¡Â»Â chĂ†Â°a snapshot chĂ¡Â»â€° ghi Ă¢â‚¬Å“TĂ¡ÂºÂ¡m tÄ‚Â­nh mÄ‚Â³nĂ¢â‚¬Â, khÄ‚Â´ng cÄ‚Â³ QR preview; snapshot backend hiĂ¡Â»Æ’n thĂ¡Â»â€¹ SubTotal/discount/phÄ‚Â­/VAT/TotalAmount; modal, QR vÄ‚Â  payment dÄ‚Â¹ng cÄ‚Â¹ng snapshot; tĂ¡Â»â€¢ng vÄ‚Â­ dĂ¡Â»Â¥ lÄ‚Â  113.400; conflict/tĂ¡Â»â€¢ng Ă„â€˜Ă¡Â»â€¢i dĂ¡Â»Â«ng thanh toÄ‚Â¡n vÄ‚Â  yÄ‚Âªu cĂ¡ÂºÂ§u xÄ‚Â¡c nhĂ¡ÂºÂ­n lĂ¡ÂºÂ¡i; trĂ¡ÂºÂ¡ng thÄ‚Â¡i vĂ¡ÂºÂ«n `ChĂ†Â°a test` | ChĂ†Â°a test | |

## Invoices & expenses

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-07 | HÄ‚Â³a Ă„â€˜Ă†Â¡n, chi phÄ‚Â­ vÄ‚Â  bÄ‚Â¡o cÄ‚Â¡o | CÄ‚Â³ Ă„â€˜Ă†Â¡n Ă„â€˜Ä‚Â£ thanh toÄ‚Â¡n vÄ‚Â  chi phÄ‚Â­ test | MĂ¡Â»Å¸ `/invoices`, `/expenses`, lĂ¡Â»Âc ngÄ‚Â y/branch, xem chi tiĂ¡ÂºÂ¿t/in bill | DĂ¡Â»Â¯ liĂ¡Â»â€¡u khĂ¡Â»â€ºp order Ă„â€˜Ä‚Â£ lĂ†Â°u; lĂ¡Â»Âc branch/ngÄ‚Â y Ă„â€˜Ä‚Âºng; thao tÄ‚Â¡c lĂ¡ÂºÂ·p khÄ‚Â´ng tĂ¡ÂºÂ¡o bĂ¡ÂºÂ£n ghi trÄ‚Â¹ng | ChĂ†Â°a test | |

## Employees / attendance / schedule

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-08 | NhÄ‚Â¢n sĂ¡Â»Â±, chĂ¡ÂºÂ¥m cÄ‚Â´ng, lĂ¡Â»â€¹ch, ca | CÄ‚Â³ nhÄ‚Â¢n viÄ‚Âªn test | MĂ¡Â»Å¸ `/employees`, `/attendance`, `/schedule`, `/shifts`; thĂ¡Â»Â±c hiĂ¡Â»â€¡n thao tÄ‚Â¡c Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p | ChĂ¡Â»â€° role Ă„â€˜Ă†Â°Ă¡Â»Â£c backend cho phÄ‚Â©p mĂ¡Â»â€ºi sĂ¡Â»Â­a; dĂ¡Â»Â¯ liĂ¡Â»â€¡u nhÄ‚Â¢n viÄ‚Âªn/ca hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng branch vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i | ChĂ†Â°a test | |

## Inventory

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-09 | Kho | CÄ‚Â³ item, phiĂ¡ÂºÂ¿u nhĂ¡ÂºÂ­p/xuĂ¡ÂºÂ¥t test | MĂ¡Â»Å¸ cÄ‚Â¡c trang `/inventory/*`, xem overview, tĂ¡ÂºÂ¡o hoĂ¡ÂºÂ·c xÄ‚Â¡c nhĂ¡ÂºÂ­n phiĂ¡ÂºÂ¿u test | TĂ¡Â»â€œn kho vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i phiĂ¡ÂºÂ¿u cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ä‚Âºng; lĂ¡Â»â€”i API khÄ‚Â´ng lÄ‚Â m mĂ¡ÂºÂ¥t dĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜ang nhĂ¡ÂºÂ­p | ChĂ†Â°a test | |

## Settings & branches

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-10 | Khu vĂ¡Â»Â±c cĂ¡ÂºÂ¥u hÄ‚Â¬nh chĂ¡Â»Â§ quÄ‚Â¡n | TÄ‚Â i khoĂ¡ÂºÂ£n `admin`; branch vÄ‚Â  cĂ¡ÂºÂ¥u hÄ‚Â¬nh test | MĂ¡Â»Å¸ bÄ‚Â¡nh rĂ„Æ’ng rĂ¡Â»â€œi chuyĂ¡Â»Æ’n Ă„â€˜Ă¡Â»Â§ `/settings`, `/settings/receipt`, `/print-templates`, `/branches` bĂ¡ÂºÂ±ng sidebar desktop vÄ‚Â  menu mobile; refresh/deep-link tĂ¡Â»Â«ng URL, Ă„â€˜Ă¡Â»â€¢i VAT/phÄ‚Â­ hoĂ¡ÂºÂ·c receipt test rĂ¡Â»â€œi reload; mĂ¡Â»Å¸ `/profile` vÄ‚Â  Ă„â€˜Ă¡Â»â€¢i mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u cĂ¡Â»Â§a chÄ‚Â­nh mÄ‚Â¬nh | CĂ¡ÂºÂ£ bĂ¡Â»â€˜n trang cÄ‚Â³ cÄ‚Â¹ng sidebar; active state `/settings` khÄ‚Â´ng sÄ‚Â¡ng nhĂ¡ÂºÂ§m tĂ¡ÂºÂ¡i `/settings/receipt`; branch hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i khÄ‚Â´ng Ă„â€˜Ă¡Â»â€¢i khi chuyĂ¡Â»Æ’n trang; form/preview khÄ‚Â´ng bĂ¡Â»â€¹ cĂ¡ÂºÂ¯t; mutation Ă„â€˜Ă†Â°Ă¡Â»Â£c API xÄ‚Â¡c nhĂ¡ÂºÂ­n; profile/Ă„â€˜Ă¡Â»â€¢i mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u khÄ‚Â´ng bĂ¡Â»â€¹ chuyĂ¡Â»Æ’n thÄ‚Â nh cĂ¡ÂºÂ¥u hÄ‚Â¬nh owner-only | ChĂ†Â°a test | |
| ADM-18 | ThĂ¡Â»â€˜ng nhĂ¡ÂºÂ¥t thÄ‚Â´ng tin branch vÄ‚Â  cĂ¡ÂºÂ¥u hÄ‚Â¬nh theo scope | CÄ‚Â³ Branch A/B Ă„â€˜ang hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng, thÄ‚Â´ng tin bank/MST/thuĂ¡ÂºÂ¿/phÄ‚Â­/receipt khÄ‚Â¡c nhau | ChĂ¡Â»Ân A rĂ¡Â»â€œi B trÄ‚Âªn Navbar; mĂ¡Â»Å¸ `/settings` vÄ‚Â  `/settings/receipt`; kiĂ¡Â»Æ’m tra thÄ‚Â´ng tin chĂ¡Â»â€° Ă„â€˜Ă¡Â»Âc, VAT/phÄ‚Â­, receipt; tĂ¡Â»Â« `/settings` bĂ¡ÂºÂ¥m chĂ¡Â»â€°nh sĂ¡Â»Â­a; tĂ¡ÂºÂ¡i `/branches` sĂ¡Â»Â­a mĂ¡Â»â„¢t field, tĂ¡ÂºÂ¡o branch phĂ¡Â»Â¥ mĂ¡Â»â€ºi rĂ¡Â»â€œi kiĂ¡Â»Æ’m tra lĂ¡ÂºÂ¡i bĂ¡Â»â„¢ chĂ¡Â»Ân | `/settings` khÄ‚Â´ng PUT Branch; hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă„â€˜Ä‚Âºng branch Ă„â€˜ang chĂ¡Â»Ân, khÄ‚Â´ng tĂ¡Â»Â± rĂ†Â¡i vĂ¡Â»Â `IsMain`/branch Ă„â€˜Ă¡ÂºÂ§u; nÄ‚Âºt chĂ¡Â»â€°nh sĂ¡Â»Â­a giĂ¡Â»Â¯ `branchId`; `/branches` sĂ¡Â»Â­a Ă„â€˜Ă¡Â»Â§ DTO khÄ‚Â´ng lÄ‚Â m mĂ¡ÂºÂ¥t bank/phÄ‚Â¡p lÄ‚Â½; branch mĂ¡Â»â€ºi xuĂ¡ÂºÂ¥t hiĂ¡Â»â€¡n trong bĂ¡Â»â„¢ chĂ¡Â»Ân; logo/MST/receipt vÄ‚Â  thuĂ¡ÂºÂ¿/phÄ‚Â­ Ă„â€˜Ä‚Âºng scope; trĂ¡ÂºÂ¡ng thÄ‚Â¡i vĂ¡ÂºÂ«n `ChĂ†Â°a test` | ChĂ†Â°a test | |
| ADM-19 | ChĂ¡Â»Â§ tÄ‚Â i khoĂ¡ÂºÂ£n vÄ‚Â  loĂ¡ÂºÂ¡i hÄ‚Â¬nh kinh doanh | CÄ‚Â³ branch mĂ¡Â»â€ºi hoĂ¡ÂºÂ·c branch Ă„â€˜Ä‚Â£ cÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u `AccountHolder`/`BusinessType` | MĂ¡Â»Å¸ `/branches`, tĂ¡ÂºÂ¡o/sĂ¡Â»Â­a hai trĂ†Â°Ă¡Â»Âng Ă¡Â»Å¸ Ă„â€˜Ä‚Âºng nhÄ‚Â³m ngÄ‚Â¢n hÄ‚Â ng vÄ‚Â  phÄ‚Â¡p lÄ‚Â½/ngÄ‚Â nh nghĂ¡Â»Â; Ă„â€˜Ä‚Â³ng/mĂ¡Â»Å¸ lĂ¡ÂºÂ¡i form; lĂ†Â°u rĂ¡Â»â€œi mĂ¡Â»Å¸ `/settings` vÄ‚Â  `/branches`; thĂ¡Â»Â­ form dÄ‚Â i trÄ‚Âªn desktop/mobile, cuĂ¡Â»â„¢n phĂ¡ÂºÂ§n thÄ‚Â¢n, dÄ‚Â¹ng bÄ‚Â n phÄ‚Â­m mÄ‚Â n hÄ‚Â¬nh vÄ‚Â  kiĂ¡Â»Æ’m tra nÄ‚Âºt X, HĂ¡Â»Â§y, LĂ†Â°u | Hai giÄ‚Â¡ trĂ¡Â»â€¹ tĂ¡ÂºÂ£i lĂ¡ÂºÂ¡i Ă„â€˜Ä‚Âºng, gĂ¡Â»Â­i Ă„â€˜Ä‚Âºng field; khÄ‚Â´ng tĂ¡Â»Â± Ă„â€˜iĂ¡Â»Ân tĂ¡Â»Â« tÄ‚Âªn branch/ngĂ†Â°Ă¡Â»Âi Ă„â€˜Ă¡ÂºÂ¡i diĂ¡Â»â€¡n; bank, MST, Ă„â€˜Ă¡ÂºÂ¡i diĂ¡Â»â€¡n, logo vÄ‚Â  field khÄ‚Â¡c khÄ‚Â´ng bĂ¡Â»â€¹ mĂ¡ÂºÂ¥t; header/nÄ‚Âºt X vÄ‚Â  footer HĂ¡Â»Â§y/LĂ†Â°u luÄ‚Â´n hiĂ¡Â»Æ’n thĂ¡Â»â€¹, chĂ¡Â»â€° body cuĂ¡Â»â„¢n, X/HĂ¡Â»Â§y khÄ‚Â´ng submit; trĂ¡ÂºÂ¡ng thÄ‚Â¡i vĂ¡ÂºÂ«n `ChĂ†Â°a test` | ChĂ†Â°a test | |

## Navigation / notifications / runtime

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-11 | QuyĂ¡Â»Ân qua URL trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p | Ă„ÂÄ‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p admin | MĂ¡Â»Å¸ trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p cÄ‚Â¡c URL `/branches`, `/settings`, `/employees`, `/pos`, `/kitchen` vÄ‚Â  URL khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i | Trang hĂ¡Â»Â£p lĂ¡Â»â€¡ tĂ¡ÂºÂ£i; URL lĂ¡ÂºÂ¡ redirect; khÄ‚Â´ng cÄ‚Â³ menu nÄ‚Â o mĂ¡Â»Å¸ Ă„â€˜Ă†Â°Ă¡Â»Â£c endpoint trÄ‚Â¡i quyĂ¡Â»Ân | ChĂ†Â°a test | |
| ADM-13 | DĂ¡Â»Â¯ liĂ¡Â»â€¡u sai/rĂ¡Â»â€”ng vÄ‚Â  lĂ¡Â»â€”i phiÄ‚Âªn | TĂ¡ÂºÂ¯t API hoĂ¡ÂºÂ·c dÄ‚Â¹ng payload khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡ qua UI | MĂ¡Â»Å¸ trang Ă„â€˜ang tĂ¡ÂºÂ£i, refresh giĂ¡Â»Â¯a lÄ‚Âºc tĂ¡ÂºÂ£i, hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n phiÄ‚Âªn rĂ¡Â»â€œi thao tÄ‚Â¡c lĂ¡ÂºÂ¡i | Loading/error/401 Ă„â€˜Ă†Â°Ă¡Â»Â£c xĂ¡Â»Â­ lÄ‚Â½; Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p lĂ¡ÂºÂ¡i; khÄ‚Â´ng hiĂ¡Â»â€¡n stack trace, token hoĂ¡ÂºÂ·c secret | ChĂ†Â°a test | |
| ADM-17 | Navigation chÄ‚Â­nh vÄ‚Â  bÄ‚Â¡nh rĂ„Æ’ng | TÄ‚Â i khoĂ¡ÂºÂ£n `admin` hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng; cÄ‚Â³ dĂ¡Â»Â¯ liĂ¡Â»â€¡u test cho dashboard, POS vÄ‚Â  bĂ¡ÂºÂ¿p | TĂ¡Â»Â« `/dashboard`, `/pos`, `/kitchen`, `/invoices`, `/shifts` Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u cÄ‚Â¹ng thĂ¡Â»Â© tĂ¡Â»Â±: TĂ¡Â»â€¢ng quan Ă¢â€ â€™ CĂ¡ÂºÂ£nh bÄ‚Â¡o Ă¢â€ â€™ Chi phÄ‚Â­ Ă¢â€ â€™ Kho hÄ‚Â ng Ă¢â€ â€™ ThĂ¡Â»Â±c Ă„â€˜Ă†Â¡n Ă¢â€ â€™ KhÄ‚Â¡ch hÄ‚Â ng Ă¢â€ â€™ NhÄ‚Â¢n viÄ‚Âªn Ă¢â€ â€™ BÄ‚Â¡n hÄ‚Â ng Ă¢â€ â€™ NhÄ‚Â  bĂ¡ÂºÂ¿p Ă¢â€ â€™ PhÄ‚Â²ng bÄ‚Â n Ă¢â€ â€™ HÄ‚Â³a Ă„â€˜Ă†Â¡n Ă¢â€ â€™ Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n; mĂ¡Â»Å¸ submenu NhÄ‚Â¢n viÄ‚Âªn vÄ‚Â  bÄ‚Â¡nh rĂ„Æ’ng | `/shifts` chĂ¡Â»â€° nĂ¡ÂºÂ±m trong submenu NhÄ‚Â¢n viÄ‚Âªn; tĂ¡ÂºÂ¡i `/shifts`, cĂ¡ÂºÂ£ NhÄ‚Â¢n viÄ‚Âªn vÄ‚Â  Ca lÄ‚Â m active Ă„â€˜Ä‚Âºng trÄ‚Âªn desktop/mobile. MĂ¡ÂºÂ«u in `/print-templates` vÄ‚Â  CĂ¡ÂºÂ¥u hÄ‚Â¬nh in bill `/settings/receipt` chĂ¡Â»â€° Ă¡Â»Å¸ bÄ‚Â¡nh rĂ„Æ’ng; session, branch, Navbar/ChatBot vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i POS/KDS khÄ‚Â´ng bĂ¡Â»â€¹ reset ngoÄ‚Â i hÄ‚Â nh vi hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i | ChĂ†Â°a test | |
| ADM-21 | ChuÄ‚Â´ng vÄ‚Â  lĂ¡Â»â€¹ch sĂ¡Â»Â­ thÄ‚Â´ng bÄ‚Â¡o | Hai user cÄ‚Â¹ng nhĂ¡ÂºÂ­n notification; Branch A/B vÄ‚Â  Ă„â€˜Ă†Â¡n chĂ¡Â»Â xĂ¡Â»Â­ lÄ‚Â½ | Ă„ÂĂ„Æ’ng nhĂ¡ÂºÂ­p tĂ¡Â»Â«ng user, kiĂ¡Â»Æ’m tra chuÄ‚Â´ng unread/pending riÄ‚Âªng, Ă„â€˜Ă¡Â»Âc mĂ¡Â»â„¢t notification, mark-all, mĂ¡Â»Å¸ `/notifications`, lĂ¡Â»Âc loĂ¡ÂºÂ¡i/thĂ¡Â»Âi gian/branch vÄ‚Â  phÄ‚Â¢n trang; thĂ¡Â»Â­ URL trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p/ID khÄ‚Â¡c quyĂ¡Â»Ân, logout/login, target Ă„â€˜Ä‚Â£ xÄ‚Â³a vÄ‚Â  lĂ¡Â»â€”i mutation | Read-state Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p theo user; badge chĂ¡Â»â€° unread, pending khÄ‚Â´ng bĂ¡Â»â€¹ thay Ă„â€˜Ă¡Â»â€¢i; Ă„â€˜Ä‚Â£ Ă„â€˜Ă¡Â»Âc biĂ¡ÂºÂ¿n mĂ¡ÂºÂ¥t khĂ¡Â»Âi dropdown nhĂ†Â°ng cÄ‚Â²n lĂ¡Â»â€¹ch sĂ¡Â»Â­; branch/recipient Ă„â€˜Ä‚Âºng, mark-read idempotent; target khÄ‚Â´ng tĂ¡Â»Â± nhĂ¡ÂºÂ­n/hĂ¡Â»Â§y/thanh toÄ‚Â¡n; dĂ¡Â»Â¯ liĂ¡Â»â€¡u legacy Ă„â€˜Ă†Â°Ă¡Â»Â£c kiĂ¡Â»Æ’m tra sau khi Ä‚Â¡p migration; trĂ¡ÂºÂ¡ng thÄ‚Â¡i vĂ¡ÂºÂ«n `ChĂ†Â°a test` | ChĂ†Â°a test | |

## Other

| ID | Trang/chức năng | Điều kiện trước | Bước thực hiện | Kết quả mong đợi | Trạng thái | Ghi chú |
|---|---|---|---|---|---|---|
| ADM-12 | Mock AI chat | Mock AI local Ă„â€˜ang bĂ¡ÂºÂ­t | MĂ¡Â»Å¸ chatbot, hĂ¡Â»Âi menu/doanh thu/Ă„â€˜Ă†Â¡n trong phĂ¡ÂºÂ¡m vi, thĂ¡Â»Â­ thao tÄ‚Â¡c Ă„â€˜Ă¡Â»â€¢i giÄ‚Â¡ | Chatbot trĂ¡ÂºÂ£ kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ Mock; tool Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p theo `admin`; thao tÄ‚Â¡c Ă„â€˜Ă¡Â»â€¢i giÄ‚Â¡ dĂ¡Â»Â«ng Ă¡Â»Å¸ confirmation server, khÄ‚Â´ng tĂ¡Â»Â± xÄ‚Â¡c nhĂ¡ÂºÂ­n | ChĂ†Â°a test | |

