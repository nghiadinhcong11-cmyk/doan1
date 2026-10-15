# 14. AI Assistant - Trá»£ lĂ½ áº£o thĂ´ng minh

## 1. Kiáº¿n trĂºc AI hiá»‡n táº¡i
Há»‡ thá»‘ng sá»­ dá»¥ng **Mock AI deterministic** cho mĂ´i trÆ°á»ng hiá»‡n táº¡i; khĂ´ng khá»Ÿi táº¡o hoáº·c gá»i AI provider, external AI provider hay provider bĂªn ngoĂ i.
- **Luá»“ng xá»­ lĂ½**: `ChatBot.tsx` -> `AiController` -> `AiOrchestrator` -> `MockAiProvider` -> `AiToolRegistry`/`AiPermissionService` -> `Application Service`.
- **Sequential Tool Calling**: Há»‡ thá»‘ng há»— trá»£ gá»i nhiá»u Tool liĂªn tiáº¿p trong má»™t lÆ°á»£t há»i (vĂ­ dá»¥: vá»«a xem doanh thu vá»«a phĂ¢n tĂ­ch mĂ³n bĂ¡n cháº¡y), giĂºp AI giáº£i quyáº¿t cĂ¡c yĂªu cáº§u phá»©c táº¡p.
- **Stateless Backend**: Há»‡ thá»‘ng khĂ´ng lÆ°u lá»‹ch sá»­ chat táº¡i server Ä‘á»ƒ Ä‘áº£m báº£o riĂªng tÆ°. Lá»‹ch sá»­ Ä‘Æ°á»£c frontend quáº£n lĂ½ vĂ  gá»­i kĂ¨m má»—i request.

## 2. PhĂ¢n loáº¡i cáº¥p Ä‘á»™ AI
Hiá»‡n táº¡i há»‡ thá»‘ng lĂ  **deterministic tool-using assistant**: Mock AI nháº­n diá»‡n má»™t allowlist intent vĂ  gá»i tool Ä‘Ă£ Ä‘Æ°á»£c cáº¥p quyá»n. CĂ¡c phĂ¢n tĂ­ch tĂ i chĂ­nh vĂ  phĂ¡t hiá»‡n báº¥t thÆ°á»ng Ä‘áº¿n tá»« Application Service/rule engine; Mock AI khĂ´ng pháº£i mĂ´ hĂ¬nh suy luáº­n tá»•ng quĂ¡t.

## 3. Danh sĂ¡ch cĂ¡c cĂ´ng cá»¥ AI (AI Tools) Ä‘Ă£ triá»ƒn khai
Há»‡ thá»‘ng hiá»‡n cĂ³ **14 cĂ´ng cá»¥** AI:

| Tool Name | Chá»©c nÄƒng | Äá»‘i tÆ°á»£ng sá»­ dá»¥ng |
| :--- | :--- | :--- |
| `get_financial_analysis`| **(Má»›i)** PhĂ¢n tĂ­ch tĂ i chĂ­nh chuyĂªn sĂ¢u (Doanh thu, Chi phĂ­, Lá»£i nhuáº­n, TÄƒng trÆ°á»Ÿng, Báº¥t thÆ°á»ng). | Admin, Manager |
| `get_business_summary`| TĂ³m táº¯t KPI (Doanh thu, Lá»£i nhuáº­n, AOV) & Advisor. | Admin, Manager |
| `get_revenue` | Xem doanh thu tá»•ng há»£p (ngĂ y/thĂ¡ng/nÄƒm). | Admin, Manager |
| `get_order_list` | Truy váº¥n danh sĂ¡ch Ä‘Æ¡n hĂ ng chi tiáº¿t. | Admin, Manager, Employee |
| `get_best_sellers` | Thá»‘ng kĂª cĂ¡c mĂ³n bĂ¡n cháº¡y nháº¥t. | Admin, Manager, Employee |
| `get_revenue_comparison`| So sĂ¡nh doanh thu giá»¯a cĂ¡c ká»³. | Admin, Manager |
| `get_active_staff` | Xem danh sĂ¡ch nhĂ¢n viĂªn Ä‘ang lĂ m. | Admin, Manager |
| `update_product_price`| Cáº­p nháº­t giĂ¡ bĂ¡n sáº£n pháº©m. | Admin |
| `customer_get_menu` | Tra cá»©u thá»±c Ä‘Æ¡n, giĂ¡, topping. | CĂ´ng khai |
| `get_my_orders` | Xem Ä‘Æ¡n hĂ ng cĂ¡ nhĂ¢n. | Customer |
| `create_booking` | Äáº·t bĂ n qua ngĂ´n ngá»¯ tá»± nhiĂªn. | Customer |
| `get_my_shift` | Xem lá»‹ch lĂ m viá»‡c cĂ¡ nhĂ¢n. | Employee |
| `employee_get_table_summary`| Xem thá»‘ng kĂª bĂ n táº¡i chi nhĂ¡nh. | Employee, Manager |
| `update_order_status` | Cáº­p nháº­t tráº¡ng thĂ¡i Ä‘Æ¡n hĂ ng. | Employee, Manager |

## 4. Báº£o máº­t vĂ  PhĂ¢n quyá»n AI
- **Báº£o máº­t Role-based**: AI xĂ¡c thá»±c quyá»n qua JWT Token. Má»—i Tool cĂ³ `AllowedRoles` vĂ  `RiskLevel` (Read/Write/Destructive).
- **Branch Isolation**: ToĂ n bá»™ dá»¯ liá»‡u Analytics vĂ  Advisor Ä‘Æ°á»£c lá»c nghiĂªm ngáº·t theo `BranchId` cá»§a ngÆ°á»i dĂ¹ng. Admin Ä‘Æ°á»£c quyá»n xem toĂ n chuá»—i.
- **Hallucination Protection**: Prompt há»‡ thá»‘ng yĂªu cáº§u AI phĂ¢n biá»‡t rĂµ FACT (Sá»± tháº­t), INFERENCE (Suy luáº­n) vĂ  RECOMMENDATION (Khuyáº¿n nghá»‹).

## 5. Kháº£ nÄƒng TĂ i chĂ­nh thĂ´ng minh (Financial Intelligence)
AI hiá»‡n cĂ³ thá»ƒ thá»±c hiá»‡n cĂ¡c nhiá»‡m vá»¥ chuyĂªn sĂ¢u:
- **KPI Snapshot & Profitability**: Cung cáº¥p cĂ¡i nhĂ¬n tá»•ng thá»ƒ vá» doanh thu, sá»‘ Ä‘Æ¡n, AOV vĂ  lá»£i nhuáº­n Æ°á»›c tĂ­nh (Estimated Profit), biĂªn lá»£i nhuáº­n (Profit Margin).
- **PhĂ¢n tĂ­ch CÆ¡ cáº¥u Chi phĂ­ (Expense Breakdown)**: BĂ³c tĂ¡ch chi phĂ­ theo danh má»¥c (NguyĂªn liá»‡u, Äiá»‡n nÆ°á»›c, Váº­n hĂ nh...) Ä‘á»ƒ tá»‘i Æ°u hĂ³a dĂ²ng tiá»n.
- **PhĂ¢n tĂ­ch TÄƒng trÆ°á»Ÿng (Growth Analysis)**: So sĂ¡nh hiá»‡u quáº£ kinh doanh giá»¯a cĂ¡c ká»³ (Ká»³ nĂ y vs Ká»³ trÆ°á»›c) Ä‘á»ƒ Ä‘Ă¡nh giĂ¡ xu hÆ°á»›ng phĂ¡t triá»ƒn.
- **PhĂ¡t hiá»‡n Báº¥t thÆ°á»ng (Anomaly Detection)**: Tá»± Ä‘á»™ng cáº£nh bĂ¡o cĂ¡c biáº¿n Ä‘á»™ng Ä‘Ă¡ng ká»ƒ (VĂ­ dá»¥: chi phĂ­ má»™t nhĂ³m tÄƒng Ä‘á»™t biáº¿n > 50% hoáº·c doanh thu giáº£m máº¡nh > 20%).
- **PhĂ¢n tĂ­ch Insight**: Tá»± Ä‘á»™ng nháº­n diá»‡n xu hÆ°á»›ng (vĂ­ dá»¥: "Doanh thu tÄƒng nhá» khĂ¡ch chi tiĂªu nhiá»u hÆ¡n dĂ¹ lÆ°á»£ng Ä‘Æ¡n giáº£m").
- **Khuyáº¿n nghá»‹ hĂ nh Ä‘á»™ng (Advisory)**: Gá»£i Ă½ xĂ¢y dá»±ng combo, upsell hoáº·c Ä‘iá»u chá»‰nh chi phĂ­ dá»±a trĂªn báº±ng chá»©ng dá»¯ liá»‡u (FACT -> INFERENCE -> RECOMMENDATION).
- **LĂ½ giáº£i nguyĂªn nhĂ¢n**: Tráº£ lá»i cĂ¢u há»i "Táº¡i sao doanh thu tÄƒng nhÆ°ng lá»£i nhuáº­n giáº£m?" báº±ng cĂ¡ch kiá»ƒm tra Ä‘á»“ng thá»i nhiá»u chá»‰ sá»‘ tĂ i chĂ­nh.

> **LÆ°u Ă½ quan trá»ng vá» sá»‘ liá»‡u tĂ i chĂ­nh**:
> Estimated Profit lĂ  chá»‰ sá»‘ tham kháº£o. Do há»‡ thá»‘ng chÆ°a cĂ³ historical cost snapshot vĂ  inventory accounting, viá»‡c ghi nháº­n chi phĂ­ nguyĂªn liá»‡u trong Expense Ä‘á»“ng thá»i vá»›i viá»‡c Æ°á»›c tĂ­nh COGS tá»« CostPrice cĂ³ thá»ƒ dáº«n Ä‘áº¿n double-counting trong má»™t sá»‘ trÆ°á»ng há»£p. VĂ¬ váº­y chá»‰ sá»‘ nĂ y khĂ´ng Ä‘Æ°á»£c xem lĂ  lá»£i nhuáº­n káº¿ toĂ¡n chĂ­nh thá»©c.

## 6. Kháº£ nÄƒng Chá»§ Ä‘á»™ng (Proactive Insights)
Há»‡ thá»‘ng tĂ­ch há»£p kháº£ nÄƒng AI chá»§ Ä‘á»™ng phĂ¡t hiá»‡n cĂ¡c váº¥n Ä‘á» kinh doanh:
- **CÆ¡ cháº¿**: `BackgroundService` cháº¡y hĂ ng ngĂ y lĂºc 00:05 (giá» VN) Ä‘á»ƒ phĂ¢n tĂ­ch dá»¯ liá»‡u.
- **PhĂ¡t hiá»‡n**: Rule Engine deterministic Ä‘áº£m báº£o tĂ­nh chĂ­nh xĂ¡c 100% vá» sá»‘ liá»‡u.
- **Diá»…n giáº£i**: Mock AI táº¡o báº£n tĂ³m táº¯t cĂ³ cáº¥u trĂºc (`AiExplanation`) vĂ  khuyáº¿n nghá»‹ giá»›i háº¡n (`AiRecommendation`) tá»« dá»¯ liá»‡u báº±ng chá»©ng; khĂ´ng cĂ³ gá»i provider ngoĂ i.
- **Giao diá»‡n**: Trang "Cáº£nh bĂ¡o kinh doanh" cho phĂ©p Admin/Manager quáº£n lĂ½ vĂ  theo dĂµi cĂ¡c nháº­n Ä‘á»‹nh tá»« AI.
- **ThĂ´ng bĂ¡o**: TĂ­ch há»£p há»‡ thá»‘ng Notification vĂ  SignalR Ä‘á»ƒ cáº£nh bĂ¡o Real-time ngay khi phĂ¡t hiá»‡n báº¥t thÆ°á»ng.

## 7. Lá»™ trĂ¬nh phĂ¡t triá»ƒn AI (Roadmap)
- **Phase 1 (Analytics)**: ÄĂ£ hoĂ n thĂ nh vá»›i Sequential Tool Calling.
- **Phase 2 (BI)**: ÄĂ£ hoĂ n thĂ nh Financial Intelligence & Proactive Insights (API + UI + Real-time).
- **Phase 3 (Agent)**: ChÆ°a triá»ƒn khai; confirmation boundary vĂ  quyá»n server váº«n lĂ  báº¯t buá»™c cho má»i write tool.


