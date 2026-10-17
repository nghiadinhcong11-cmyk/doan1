# AI Task 21 â€“ Business Advisor Design

## 1. Current AI Baseline
Dá»±a trĂªn káº¿t quáº£ audit Task 20:
- **Kiáº¿n trĂºc**: Orchestration vá»›i AI provider 1.5 Flash/Pro.
- **Kháº£ nÄƒng**: ÄĂ£ há»— trá»£ Sequential Tool Calling (tá»‘i Ä‘a 5 lÆ°á»£t láº·p), cho phĂ©p gá»i chuá»—i cĂ¡c cĂ´ng cá»¥ Ä‘á»ƒ tá»•ng há»£p thĂ´ng tin.
- **CĂ´ng cá»¥ Analytics**: ÄĂ£ cĂ³ `get_revenue`, `get_order_list`, `get_best_sellers`, `get_revenue_comparison`.
- **Báº£o máº­t**: Äáº£m báº£o Branch Isolation vĂ  RBAC thĂ´ng qua `AiPermissionService`.

## 2. Available Business Data
Há»‡ thá»‘ng hiá»‡n cĂ³ dá»¯ liá»‡u phong phĂº táº¡i `DashboardService` vĂ  `OrderService`:
- **Doanh thu (Revenue)**: `PaidAmount` tá»« cĂ¡c Ä‘Æ¡n hĂ ng cĂ³ tráº¡ng thĂ¡i "HoĂ n thĂ nh".
- **Sá»‘ lÆ°á»£ng Ä‘Æ¡n (Order Count)**: Äáº¿m sá»‘ lÆ°á»£ng Ä‘Æ¡n hĂ ng há»£p lá»‡.
- **GiĂ¡ trá»‹ Ä‘Æ¡n hĂ ng trung bĂ¬nh (AOV)**: CĂ³ thá»ƒ tĂ­nh báº±ng `Revenue / Order Count`.
- **MĂ³n bĂ¡n cháº¡y (Best Sellers)**: Truy váº¥n tá»« `OrderDetails`.
- **Chi phĂ­ (Expense)**: Dá»¯ liá»‡u tá»« báº£ng `Expenses`.
- **Lá»£i nhuáº­n (Profit)**: ÄÆ°á»£c tĂ­nh sÆ¡ bá»™ báº±ng `Revenue - Expenses - COGS (dá»±a trĂªn CostPrice)`.
- **NhĂ¢n sá»±**: Thá»‘ng kĂª sá»‘ lÆ°á»£ng nhĂ¢n viĂªn Ä‘ang lĂ m viá»‡c (On-duty).

## 3. KPI Design
AI Business Advisor sáº½ táº­p trung vĂ o cĂ¡c chá»‰ sá»‘ hiá»‡u nÄƒng chĂ­nh (KPI):
- **Revenue**: Doanh thu thá»±c táº¿ Ä‘Ă£ thu tiá»n.
- **Order Count**: Tá»•ng sá»‘ lÆ°á»£t phá»¥c vá»¥.
- **Average Order Value (AOV)**: Má»©c chi tiĂªu trung bĂ¬nh cá»§a má»—i khĂ¡ch hĂ ng.
- **Growth Rate**: Tá»· lá»‡ tÄƒng trÆ°á»Ÿng doanh thu/Ä‘Æ¡n hĂ ng so vá»›i ká»³ trÆ°á»›c.
- **Top Products Contribution**: Tá»· trá»ng Ä‘Ă³ng gĂ³p doanh thu cá»§a cĂ¡c mĂ³n bĂ¡n cháº¡y nháº¥t.
- **Profit Margin (SÆ¡ bá»™)**: Tá»· suáº¥t lá»£i nhuáº­n dá»±a trĂªn dá»¯ liá»‡u giĂ¡ vá»‘n hiá»‡n cĂ³.

## 4. Business Insight Design
AI sáº½ thá»±c hiá»‡n suy luáº­n (Inference) tá»« dá»¯ liá»‡u Ä‘á»ƒ tĂ¬m ra Insight:
- **Volume vs Value**: "Doanh thu tÄƒng nhá» AOV tÄƒng máº·c dĂ¹ sá»‘ lÆ°á»£ng Ä‘Æ¡n giáº£m" -> KhĂ¡ch hĂ ng Ä‘ang mua cĂ¡c mĂ³n Ä‘áº¯t tiá»n hÆ¡n hoáº·c combo lá»›n hÆ¡n.
- **Product Concentration**: "MĂ³n X Ä‘Ă³ng gĂ³p 40% doanh thu nhÆ°ng chá»‰ chiáº¿m 10% sá»‘ lÆ°á»£ng Ä‘Æ¡n" -> MĂ³n X lĂ  sáº£n pháº©m cao cáº¥p chá»§ lá»±c.
- **Trend Detection**: PhĂ¡t hiá»‡n xu hÆ°á»›ng tÄƒng/giáº£m liĂªn tá»¥c trong 7 ngĂ y gáº§n nháº¥t.
- **Operational Insight**: "Sá»‘ Ä‘Æ¡n tÄƒng cao nhÆ°ng nhĂ¢n sá»± on-duty tháº¥p" -> Cáº£nh bĂ¡o quĂ¡ táº£i váº­n hĂ nh.

## 5. Recommendation Design
Khuyáº¿n nghá»‹ (Recommendation) pháº£i mang tĂ­nh tham kháº£o vĂ  dá»±a trĂªn báº±ng chá»©ng dá»¯ liá»‡u:
- **Upselling**: Náº¿u AOV tháº¥p -> Gá»£i Ă½ khuyáº¿n khĂ­ch combo hoáº·c topping.
- **Menu Optimization**: Náº¿u mĂ³n cĂ³ doanh thu cao nhÆ°ng sá»‘ lÆ°á»£ng Ă­t -> Gá»£i Ă½ Ä‘áº©y máº¡nh quáº£ng bĂ¡.
- **Cost Control**: Náº¿u lá»£i nhuáº­n tháº¥p dĂ¹ doanh thu cao -> Gá»£i Ă½ rĂ  soĂ¡t chi phĂ­ váº­n hĂ nh (Expenses).
- **Staffing**: Gá»£i Ă½ Ä‘iá»u chá»‰nh nhĂ¢n sá»± dá»±a trĂªn dá»± bĂ¡o Ä‘Æ¡n hĂ ng tá»« xu hÆ°á»›ng quĂ¡ khá»©.

## 6. Tool Architecture Options
### Option A: Phá»‘i há»£p cĂ¡c Tool hiá»‡n cĂ³
AI tá»± gá»i `get_revenue`, `get_best_sellers`, `get_revenue_comparison` rá»“i tá»± tĂ­nh toĂ¡n AOV vĂ  Insight.
- **Æ¯u Ä‘iá»ƒm**: Linh hoáº¡t, khĂ´ng cáº§n code thĂªm tool má»›i.
- **NhÆ°á»£c Ä‘iá»ƒm**: TiĂªu tá»‘n nhiá»u token, Ä‘á»™ trá»… cao do gá»i nhiá»u láº§n, AI cĂ³ thá»ƒ tĂ­nh toĂ¡n sai AOV (halucination logic).

### Option B: Tool tá»•ng há»£p `get_business_summary`
Táº¡o má»™t Tool má»›i tráº£ vá» toĂ n bá»™ KPI summary cho má»™t khoáº£ng thá»i gian.
- **Æ¯u Ä‘iá»ƒm**: Dá»¯ liá»‡u nháº¥t quĂ¡n (nhÆ° Dashboard), tiáº¿t kiá»‡m lÆ°á»£t gá»i tool, giáº£m thiá»ƒu sai sá»‘ tĂ­nh toĂ¡n.
- **NhÆ°á»£c Ä‘iá»ƒm**: ThĂªm má»™t chĂºt code táº¡i Application Layer.

## 7. Recommended Architecture
**Äá» xuáº¥t chá»n PhÆ°Æ¡ng Ă¡n B**: Triá»ƒn khai thĂªm tool `get_business_summary`.
- Tool nĂ y sáº½ gá»i `IDashboardService.GetSummaryAsync` (Ä‘Ă£ cĂ³ sáºµn logic tĂ­nh toĂ¡n KPI chĂ­nh xĂ¡c).
- AI sáº½ nháº­n Ä‘Æ°á»£c má»™t "snapshot" toĂ n diá»‡n vá» tĂ¬nh hĂ¬nh kinh doanh Ä‘á»ƒ tá»« Ä‘Ă³ thá»±c hiá»‡n Reasoning chuyĂªn sĂ¢u.

## 8. Multi-tool Reasoning
Nhá» cÆ¡ cháº¿ Sequential Tool Calling hiá»‡n táº¡i, AI cĂ³ thá»ƒ:
1. Gá»i `get_business_summary` Ä‘á»ƒ láº¥y bá»©c tranh tá»•ng quan.
2. Náº¿u phĂ¡t hiá»‡n doanh thu giáº£m, AI cĂ³ thá»ƒ gá»i tiáº¿p `get_order_list` Ä‘á»ƒ xem cĂ¡c Ä‘Æ¡n hĂ ng gáº§n nháº¥t cĂ³ váº¥n Ä‘á» gĂ¬ khĂ´ng (vĂ­ dá»¥ Ä‘Æ¡n bá»‹ há»§y nhiá»u).
3. Tá»•ng há»£p Insight vĂ  Recommendation cuá»‘i cĂ¹ng.

## 9. Fact vs Inference vs Recommendation
Há»‡ thá»‘ng Prompt sáº½ Ä‘Æ°á»£c tinh chá»‰nh Ä‘á»ƒ AI phĂ¢n biá»‡t rĂµ:
- **Fact**: Sá»‘ liá»‡u thĂ´ tá»« Tool.
- **Inference**: Nháº­n Ä‘á»‹nh logic tá»« Fact (vĂ­ dá»¥: doanh thu tÄƒng 10%).
- **Recommendation**: HĂ nh Ä‘á»™ng gá»£i Ă½ dá»±a trĂªn nháº­n Ä‘á»‹nh.

## 10. Security & Branch Isolation
- **Identity**: LuĂ´n láº¥y `BranchId` tá»« `AiUserContext` (JWT).
- **Isolation**: Admin xem Ä‘Æ°á»£c toĂ n chuá»—i, Manager chá»‰ xem Ä‘Æ°á»£c chi nhĂ¡nh mĂ¬nh.
- **PII Protection**: Business Summary khĂ´ng tráº£ vá» thĂ´ng tin Ä‘á»‹nh danh khĂ¡ch hĂ ng (tĂªn, sá»‘ Ä‘iá»‡n thoáº¡i).

## 11. Performance
- **Reuse**: TĂ¡i sá»­ dá»¥ng `IDashboardService` giĂºp táº­n dá»¥ng cÆ¡ cháº¿ Memory Cache (2 phĂºt) hiá»‡n cĂ³.
- **Latency**: Giáº£m sá»‘ lÆ°á»£t láº·p giá»¯a AI vĂ  Server báº±ng cĂ¡ch cung cáº¥p dá»¯ liá»‡u KPI táº­p trung.

## 12. Test Plan
- **KPI Accuracy**: Äáº£m báº£o AOV, Profit tĂ­nh toĂ¡n khá»›p vá»›i logic Dashboard.
- **Reasoning Test**: User há»i "Táº¡i sao doanh thu giáº£m?", AI pháº£i biáº¿t gá»i Tool so sĂ¡nh vĂ  tĂ¬m nguyĂªn nhĂ¢n (vĂ­ dá»¥ sá»‘ Ä‘Æ¡n giáº£m).
- **Constraint Test**: AI khĂ´ng Ä‘Æ°á»£c Ä‘Æ°a ra khuyáº¿n nghá»‹ viá»ƒn vĂ´ng (vĂ­ dá»¥ "tÄƒng giĂ¡" mĂ  khĂ´ng cĂ³ cÄƒn cá»©).
- **Security Test**: Employee khĂ´ng Ä‘Æ°á»£c xem summary chá»©a dá»¯ liá»‡u lá»£i nhuáº­n náº¿u role nĂ y bá»‹ cáº¥m.

## 13. Demo Scenarios
1. "PhĂ¢n tĂ­ch tĂ¬nh hĂ¬nh kinh doanh hĂ´m nay vĂ  cho tĂ´i 3 lá»i khuyĂªn."
2. "Táº¡i sao doanh thu tuáº§n nĂ y láº¡i tháº¥p hÆ¡n tuáº§n trÆ°á»›c?"
3. "MĂ³n nĂ o Ä‘ang lĂ  'gĂ  Ä‘áº» trá»©ng vĂ ng' cá»§a nhĂ  hĂ ng?"
4. "TĂ¬nh hĂ¬nh chi phĂ­ vĂ  lá»£i nhuáº­n thĂ¡ng nĂ y tháº¿ nĂ o?"
5. "TĂ´i cĂ³ nĂªn thĂªm nhĂ¢n viĂªn vĂ o ca chiá»u nay khĂ´ng?" (Dá»±a trĂªn sá»‘ Ä‘Æ¡n hiá»‡n táº¡i).

## 14. Implementation Plan
- **Phase 1**: Táº¡o tool `get_business_summary` káº¿t ná»‘i vá»›i `IDashboardService`.
- **Phase 2**: Cáº­p nháº­t `AdminPrompt` vá»›i cĂ¡c quy táº¯c suy luáº­n Business Advisor.
- **Phase 3**: Triá»ƒn khai unit test cho tool má»›i vĂ  cĂ¡c ká»‹ch báº£n reasoning.
- **Phase 4**: Cáº­p nháº­t tĂ i liá»‡u `baocaodoan`.

## 15. Risks / Open Questions
- **Dá»¯ liá»‡u chi phĂ­**: Náº¿u nhĂ  hĂ ng khĂ´ng nháº­p `Expenses` Ä‘áº§y Ä‘á»§, KPI Lá»£i nhuáº­n sáº½ bá»‹ sai lá»‡ch. AI cáº§n cĂ³ cĂ¢u lÆ°u Ă½ vá» viá»‡c nĂ y.
- **Context Window**: Business Summary quĂ¡ lá»›n cĂ³ thá»ƒ chiáº¿m nhiá»u context. Cáº§n format JSON tráº£ vá» tá»‘i giáº£n nháº¥t.


