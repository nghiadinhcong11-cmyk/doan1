# AI Task 20 â€“ Analytics & Business Intelligence Design

## 1. Current AI Architecture
Dá»±a trĂªn mĂ£ nguá»“n thá»±c táº¿ táº¡i `services/api/src/AI`:
- **Pattern**: Orchestration vá»›i AI provider Tool Calling. 
- **Flow**: `AiController` nháº­n request -> `AiOrchestrator` xĂ¢y dá»±ng prompt vá»›i `AiContextService` -> AI provider tráº£ vá» `functionCall` -> `AiPermissionService` kiá»ƒm tra `Role` vĂ  `RiskLevel` thĂ´ng qua `AiAuthorization` -> Thá»±c thi Tool qua `Application Services` -> Tráº£ káº¿t quáº£ vá» AI provider tá»•ng há»£p.
- **Háº¡n cháº¿ hiá»‡n táº¡i**: `AiOrchestrator` xá»­ lĂ½ vĂ²ng láº·p tool call nhÆ°ng tráº£ vá» (`return`) ngay sau tool Ä‘áº§u tiĂªn, cáº£n trá»Ÿ viá»‡c AI gá»i nhiá»u tool liĂªn tiáº¿p trong má»™t lÆ°á»£t (Sequential Tool Calling).

## 2. Existing Business Data
Dá»¯ liá»‡u hiá»‡n táº¡i Ä‘Ă£ khĂ¡ Ä‘áº§y Ä‘á»§ cho Analytics cÆ¡ báº£n:
- **Order**: `Id`, `InvoiceCode`, `CreatedAt` (UTC), `PaymentAt` (UTC), `PaidAmount`, `TotalAmount`, `Status`, `BranchId`, `PaymentMethod`.
- **OrderDetail**: `ProductId`, `ProductName`, `Quantity`, `UnitPrice`.
- **Product**: `Name`, `Price`, `CostPrice`.
- **Expense**: `Amount`, `Category`, `ExpenseDate`, `BranchId`.
- **Branch**: `Id`, `Name`.

## 3. Date/Time Handling
- **Database**: LÆ°u `DateTime.UtcNow`.
- **Logic hiá»‡n táº¡i**: `DashboardService` sá»­ dá»¥ng `SE Asia Standard Time` Ä‘á»ƒ xĂ¡c Ä‘á»‹nh Ä‘iá»ƒm báº¯t Ä‘áº§u/káº¿t thĂºc ngĂ y theo giá» Viá»‡t Nam, sau Ä‘Ă³ chuyá»ƒn Ä‘á»•i sang UTC Ä‘á»ƒ truy váº¥n database.
- **Äá» xuáº¥t**: CĂ¡c Tool má»›i sáº½ káº¿ thá»«a logic nĂ y. AI sáº½ nháº­n input lĂ  string (VD: "2026-09-10") hoáº·c cĂ¡c tá»« khĂ³a (today, yesterday, this_week, this_month). Backend sáº½ chá»‹u trĂ¡ch nhiá»‡m quy Ä‘á»•i sang UTC range chĂ­nh xĂ¡c.

## 4. Revenue Source of Truth
Dá»±a trĂªn `DashboardService.cs`, doanh thu (Revenue) Ä‘Æ°á»£c Ä‘á»‹nh nghÄ©a lĂ :
- `Sum(PaidAmount)`
- Äiá»u kiá»‡n: `Status == "HoĂ n thĂ nh"` (hoáº·c "Completed")
- Chá»‰ tĂ­nh cĂ¡c Ä‘Æ¡n cĂ³ `PaidAmount > 0`.
- **Refund/Reversal**: Hiá»‡n há»‡ thá»‘ng chÆ°a cĂ³ logic Refund rĂµ rĂ ng trong `OrderService`, chá»‰ cĂ³ `Status == "ÄĂ£ há»§y"`. Cáº§n thá»‘ng nháº¥t: ÄÆ¡n há»§y khĂ´ng tĂ­nh doanh thu.

## 5. Tool Design: get_order_list
Cho phĂ©p AI tra cá»©u danh sĂ¡ch Ä‘Æ¡n hĂ ng Ä‘á»ƒ phĂ¢n tĂ­ch chi tiáº¿t.
- **Input Schema**:
    - `fromDate` (string, optional): ISO date.
    - `toDate` (string, optional): ISO date.
    - `status` (string, optional): VD: "HoĂ n thĂ nh", "Äang xá»­ lĂ½".
    - `limit` (number, default: 10, max: 50).
- **Security**: 
    - `BranchId`: Tá»± Ä‘á»™ng láº¥y tá»« `AiUserContext`. AI khĂ´ng Ä‘Æ°á»£c phĂ©p yĂªu cáº§u `branchId` khĂ¡c.
    - `Role`: Admin (ToĂ n chuá»—i), Manager/Employee (Chi nhĂ¡nh mĂ¬nh). Customer (KhĂ´ng Ä‘Æ°á»£c dĂ¹ng).
- **Data Safety**: KhĂ´ng tráº£ vá» `CustomerPhone`, `CustomerEmail` cho AI Ä‘á»ƒ trĂ¡nh lá»™ PII (trá»« khi cĂ³ yĂªu cáº§u nghiá»‡p vá»¥ cá»¥ thá»ƒ).

## 6. Tool Design: get_best_sellers
XĂ¡c Ä‘á»‹nh cĂ¡c mĂ³n bĂ¡n cháº¡y Ä‘á»ƒ tÆ° váº¥n kinh doanh.
- **Input Schema**:
    - `period` (string): "today", "yesterday", "week", "month".
    - `limit` (number, default: 5).
- **Logic**: 
    - Truy váº¥n `OrderDetails` thuá»™c cĂ¡c `Orders` cĂ³ `Status == "HoĂ n thĂ nh"` trong khoáº£ng thá»i gian.
    - Group by `ProductName`.
    - TĂ­nh `TotalQuantity` vĂ  `TotalRevenue`.
- **Financial Rule**: TuĂ¢n thá»§ logic doanh thu cá»§a há»‡ thá»‘ng (chá»‰ tĂ­nh Ä‘Æ¡n hoĂ n thĂ nh).

## 7. Tool Design: get_revenue_comparison
So sĂ¡nh hiá»‡u quáº£ kinh doanh giá»¯a cĂ¡c má»‘c thá»i gian.
- **Input Schema**:
    - `currentPeriod` (string): "today", "this_week", "this_month".
    - `comparisonPeriod` (string): "yesterday", "last_week", "last_month".
- **Output**:
    - Doanh thu, sá»‘ lÆ°á»£ng Ä‘Æ¡n cá»§a tá»«ng ká»³.
    - ChĂªnh lá»‡ch tuyá»‡t Ä‘á»‘i vĂ  tá»· lá»‡ pháº§n trÄƒm (%).
    - Xu hÆ°á»›ng (TÄƒng/Giáº£m/KhĂ´ng Ä‘á»•i).

## 8. Permission Matrix
| Tool | Admin | Manager | Employee | Customer | Risk Level |
| :--- | :---: | :---: | :---: | :---: | :---: |
| get_order_list | Global | Branch | Branch (Read) | X | Read |
| get_best_sellers | Global | Branch | Branch | X | Read |
| get_revenue_comparison | Global | Branch | X | X | Read |

## 9. Branch Isolation
- Má»i Tool Analytics **báº¯t buá»™c** sá»­ dá»¥ng `branchId` tá»« `AiUserContext`.
- Admin cĂ³ thá»ƒ Ä‘á»ƒ `branchId = null` Ä‘á»ƒ xem toĂ n há»‡ thá»‘ng.
- Logic lá»c branch pháº£i thá»±c hiá»‡n á»Ÿ táº§ng `Application Service`, AI Tool chá»‰ lĂ  lá»›p vá» bá»c.

## 10. Database / Performance
- **Váº¥n Ä‘á» phĂ¡t hiá»‡n**: Báº£ng `Orders` hiá»‡n thiáº¿u index trĂªn `CreatedAt` vĂ  `BranchId`.
- **Äá» xuáº¥t**: Cáº§n táº¡o composite index `(BranchId, CreatedAt, Status)` Ä‘á»ƒ tá»‘i Æ°u hĂ³a cĂ¡c cĂ¢u lá»‡nh Group By vĂ  Filter theo thá»i gian.
- **Limit**: Ăp dá»¥ng giá»›i háº¡n `limit` cháº·t cháº½ cho AI Ä‘á»ƒ trĂ¡nh tráº£ vá» JSON quĂ¡ lá»›n gĂ¢y overload memory hoáº·c quĂ¡ giá»›i háº¡n context cá»§a LLM.

## 11. Multi-tool Reasoning
- **YĂªu cáº§u**: NĂ¢ng cáº¥p `AiOrchestrator` Ä‘á»ƒ cho phĂ©p AI gá»i nhiá»u tool trong má»™t lÆ°á»£t (VD: xem doanh thu X vĂ  so sĂ¡nh vá»›i Y).
- **Giáº£i phĂ¡p**: Thay Ä‘á»•i vĂ²ng láº·p `foreach` trong `AiOrchestrator` Ä‘á»ƒ thu tháº­p táº¥t cáº£ `toolResult` trÆ°á»›c khi gá»­i pháº£n há»“i cuá»‘i cĂ¹ng cho AI provider.

## 12. Hallucination Protection
- System Prompt pháº£i yĂªu cáº§u AI: "Náº¿u Tool tráº£ vá» káº¿t quáº£ rá»—ng hoáº·c lá»—i, hĂ£y thĂ´ng bĂ¡o rĂµ, khĂ´ng Ä‘Æ°á»£c tá»± bá»‹a sá»‘ liá»‡u."
- AI chá»‰ Ä‘Æ°á»£c phĂ¢n tĂ­ch dá»±a trĂªn dá»¯ liá»‡u tá»« Tool, khĂ´ng Ä‘Æ°á»£c dĂ¹ng kiáº¿n thá»©c chung Ä‘á»ƒ dá»± Ä‘oĂ¡n doanh thu.

## 13. Test Plan
- **TC01 (Auth)**: KhĂ¡ch hĂ ng (Customer) gá»i `get_order_list` -> Mong Ä‘á»£i: 403 Forbidden.
- **TC02 (Isolation)**: Manager chi nhĂ¡nh A gá»i `get_order_list` kĂ¨m `branchId` cá»§a B -> Mong Ä‘á»£i: Chá»‰ tráº£ vá» dá»¯ liá»‡u A hoáº·c lá»—i.
- **TC03 (Validation)**: `fromDate` > `toDate` -> Mong Ä‘á»£i: Lá»—i validation rĂµ rĂ ng.
- **TC04 (Performance)**: Truy váº¥n khoáº£ng thá»i gian 1 nÄƒm -> Mong Ä‘á»£i: Backend tá»± Ä‘á»™ng giá»›i háº¡n hoáº·c yĂªu cáº§u rĂºt ngáº¯n range.
- **TC05 (Correctness)**: So sĂ¡nh káº¿t quáº£ `get_revenue` vá»›i bĂ¡o cĂ¡o Dashboard thá»±c táº¿.

## 14. Recommended Implementation Plan
1.  Bá»• sung composite index cho báº£ng `Orders`.
2.  Má»Ÿ rá»™ng `IDashboardService` Ä‘á»ƒ há»— trá»£ so sĂ¡nh doanh thu vĂ  láº¥y top sellers linh hoáº¡t hÆ¡n.
3.  Triá»ƒn khai 3 AI Tools theo thiáº¿t káº¿.
4.  Cáº­p nháº­t `AiOrchestrator` Ä‘á»ƒ há»— trá»£ Sequential Tool Calling.

## 15. Risks / Open Questions
- **Data Volume**: Vá»›i nhĂ  hĂ ng cĂ³ hĂ ng váº¡n Ä‘Æ¡n hĂ ng, query analytics cĂ³ thá»ƒ gĂ¢y cháº­m DB náº¿u khĂ´ng cĂ³ caching tá»‘t.
- **AI Token Limit**: Danh sĂ¡ch Ä‘Æ¡n hĂ ng quĂ¡ dĂ i sáº½ lĂ m vÆ°á»£t giá»›i háº¡n token cá»§a AI provider. Cáº§n pagination hoáº·c tĂ³m táº¯t (summarization) á»Ÿ táº§ng service.


