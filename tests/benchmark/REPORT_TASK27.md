# REPORT TASK 27 â€” AI RBAC & VALIDATOR HARDENING

## Executive Summary
Há»‡ thá»‘ng AI Ä‘Ă£ Ä‘Æ°á»£c rĂ  soĂ¡t vĂ  tháº¯t cháº·t báº£o máº­t táº¡i táº§ng **Backend Authorization** vĂ  **Argument Validation**. CĂ¡c vai trĂ² váº­n hĂ nh (`cashier`, `kitchen`, `manager`) hiá»‡n Ä‘Ă£ cĂ³ quyá»n truy cáº­p Ä‘Ăºng Ä‘áº¯n vĂ o cĂ¡c cĂ´ng cá»¥ AI cáº§n thiáº¿t cho nghiá»‡p vá»¥ thá»±c táº¿, trong khi cĂ¡c giá»›i háº¡n báº£o máº­t quan trá»ng (Admin-only, Branch Isolation) váº«n Ä‘Æ°á»£c duy trĂ¬ nghiĂªm ngáº·t. `AiToolValidator` Ä‘Ă£ Ä‘Æ°á»£c nĂ¢ng cáº¥p Ä‘á»ƒ xá»­ lĂ½ cĂ¡c tham sá»‘ khĂ´ng há»£p lá»‡ mĂ  khĂ´ng gĂ¢y treo hoáº·c lá»—i server.

## RBAC Changes
ÄĂ£ cáº­p nháº­t `AllowedRoles` cho cĂ¡c cĂ´ng cá»¥ sau Ä‘á»ƒ phĂ¹ há»£p vá»›i luá»“ng nghiá»‡p vá»¥ thá»±c táº¿:

| Tool | Previous Roles | Updated Roles | Reason |
| :--- | :--- | :--- | :--- |
| `customer_get_menu` | admin, employee, customer | admin, manager, employee, cashier, kitchen, customer | Manager/Cashier/Kitchen cáº§n tra cá»©u menu Ä‘á»ƒ há»— trá»£ khĂ¡ch vĂ  chuáº©n bá»‹ mĂ³n. |
| `get_my_shift` | employee | employee, cashier, kitchen | Má»i nhĂ¢n viĂªn (bao gá»“m thu ngĂ¢n vĂ  báº¿p) Ä‘á»u cáº§n xem lá»‹ch lĂ m viá»‡c. |
| `employee_get_table_summary`| admin, manager, employee | admin, manager, employee, cashier, kitchen | Thu ngĂ¢n vĂ  báº¿p cáº§n biáº¿t tráº¡ng thĂ¡i bĂ n Ä‘á»ƒ Ä‘iá»u phá»‘i phá»¥c vá»¥. |
| `update_order_status` | admin, manager, employee | admin, manager, employee, cashier, kitchen | Thu ngĂ¢n xĂ¡c nháº­n hoĂ n thĂ nh, báº¿p xĂ¡c nháº­n báº¯t Ä‘áº§u cháº¿ biáº¿n. |
| `get_order_list` | admin, manager, employee | admin, manager, employee, cashier, kitchen | Thu ngĂ¢n cáº§n danh sĂ¡ch Ä‘Æ¡n Ä‘á»ƒ thanh toĂ¡n, báº¿p cáº§n danh sĂ¡ch Ä‘á»ƒ xá»­ lĂ½. |
| `get_best_sellers` | admin, manager, employee | admin, manager, employee, cashier, kitchen | Há»— trá»£ nhĂ¢n viĂªn tÆ° váº¥n mĂ³n cho khĂ¡ch. |

## Validator Changes
ÄĂ£ thá»±c hiá»‡n cĂ¡c cáº£i tiáº¿n táº¡i `AiToolValidator.cs`:
1.  **Object Enforcement**: ThĂªm phÆ°Æ¡ng thá»©c `EnsureObject` Ä‘á»ƒ kiá»ƒm tra `ValueKind == JsonValueKind.Object`. CĂ¡c hĂ m `GetString`, `GetInt32`, `GetDecimal` giá» Ä‘Ă¢y an toĂ n hÆ¡n khi nháº­n tham sá»‘ khĂ´ng pháº£i lĂ  object.
2.  **Graceful Error Handling**: Thay vĂ¬ Ä‘á»ƒ server nĂ©m ngoáº¡i lá»‡ khĂ´ng kiá»ƒm soĂ¡t, validator nĂ©m `ArgumentException` vá»›i thĂ´ng bĂ¡o tiáº¿ng Viá»‡t rĂµ rĂ ng (vĂ­ dá»¥: "Cáº¥u trĂºc tham sá»‘ khĂ´ng há»£p lá»‡", "Thiáº¿u tham sá»‘ báº¯t buá»™c").
3.  **Há»— trá»£ Null/Undefined**: `ValidateRequired` giá» Ä‘Ă¢y xá»­ lĂ½ an toĂ n khi toĂ n bá»™ arguments lĂ  `null` hoáº·c `undefined`.

## Security Verification
-   **Branch Isolation**: ÄĂ£ kiá»ƒm tra láº¡i logic `ValidateBranchIsolation`. Há»‡ thá»‘ng tiáº¿p tá»¥c sá»­ dá»¥ng `BranchId` tá»« JWT (UserContext) lĂ m nguá»“n tin cáº­y duy nháº¥t. User khĂ´ng thá»ƒ gá»­i `branchId` giáº£ máº¡o qua AI Tool Arguments Ä‘á»ƒ xem dá»¯ liá»‡u chi nhĂ¡nh khĂ¡c.
-   **RBAC Boundary**: CĂ¡c cĂ´ng cá»¥ tĂ i chĂ­nh (`get_revenue`, `get_financial_analysis`, `get_business_summary`) váº«n Ä‘Æ°á»£c giá»›i háº¡n cháº·t cháº½ cho `admin` vĂ  `manager`.
-   **Write Tool Safety**: CĂ¡c cĂ´ng cá»¥ thay Ä‘á»•i dá»¯ liá»‡u (`update_product_price`, `update_order_status`) váº«n tuĂ¢n thá»§ logic kiá»ƒm tra quyá»n táº¡i táº§ng Service cá»§a Backend.

## Test Results
-   **New Tests**: ÄĂ£ thĂªm `AiSecurityAuditTests.cs` bao gá»“m:
    -   Kiá»ƒm tra ma tráº­n quyá»n háº¡n (RBAC Matrix) cho má»i Role.
    -   Kiá»ƒm tra Ä‘á»™ bá»n cá»§a Validator vá»›i cĂ¡c tham sá»‘ `null`, `empty`, hoáº·c sai Ä‘á»‹nh dáº¡ng.
    -   Kiá»ƒm tra cÆ¡ cháº¿ cĂ¡ch ly chi nhĂ¡nh (Branch Isolation).
-   **Total Tests**: 173 bĂ i test (tÄƒng tá»« 135 bĂ i).
-   **Status**: **173/173 PASS**.

## Before/After
-   **Before**: NhĂ¢n viĂªn thu ngĂ¢n (cashier) vĂ  nhĂ¢n viĂªn báº¿p (kitchen) háº§u nhÆ° khĂ´ng thá»ƒ sá»­ dá»¥ng AI Ä‘á»ƒ tra cá»©u thĂ´ng tin váº­n hĂ nh cÆ¡ báº£n. Validator cĂ³ thá»ƒ nĂ©m ngoáº¡i lá»‡ khĂ´ng mong muá»‘n náº¿u model sinh JSON sai Ä‘á»‹nh dáº¡ng.
-   **After**: CĂ¡c vai trĂ² váº­n hĂ nh cĂ³ thá»ƒ sá»­ dá»¥ng AI Ä‘á»ƒ há»— trá»£ cĂ´ng viá»‡c hĂ ng ngĂ y. Há»‡ thá»‘ng á»•n Ä‘á»‹nh hÆ¡n trÆ°á»›c cĂ¡c lá»—i sinh tham sá»‘ cá»§a LLM.

## Remaining Issues
-   CĂ¡c thĂ´ng bĂ¡o lá»—i cá»§a Validator há»¯u Ă­ch cho AI nhÆ°ng Ä‘Ă´i khi cĂ³ thá»ƒ Ä‘Æ°á»£c AI provider tá»± Ă½ diá»…n Ä‘áº¡t láº¡i cho ngÆ°á»i dĂ¹ng.
-   ChÆ°a cĂ³ cÆ¡ cháº¿ giá»›i háº¡n táº§n suáº¥t gá»i Write Tools cho tá»«ng cĂ¡ nhĂ¢n (ngoĂ i Rate Limiting chung).

## Recommendations for TASK 29
-   Tá»‘i Æ°u hĂ³a **Tool Description** Ä‘á»ƒ hÆ°á»›ng dáº«n AI provider cung cáº¥p tham sá»‘ chĂ­nh xĂ¡c ngay tá»« lÆ°á»£t Ä‘áº§u tiĂªn (Ä‘áº·c biá»‡t lĂ  cĂ¡c giĂ¡ trá»‹ enum cho `period`).
-   Bá»• sung thĂªm cĂ¡c vĂ­ dá»¥ sá»­ dá»¥ng (few-shot) trong System Prompt Ä‘á»ƒ AI provider phĂ¢n biá»‡t rĂµ ranh giá»›i giá»¯a cĂ¡c cĂ´ng cá»¥ tÆ°Æ¡ng Ä‘á»“ng.


