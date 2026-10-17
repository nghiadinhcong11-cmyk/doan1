# BÄ‚ÂO CÄ‚ÂO AUDIT CHUYÄ‚ÂN SÄ‚â€U VÄ‚â‚¬ CHUĂ¡ÂºÂ¨N HÄ‚â€œA MÄ‚â€ HÄ‚Å’NH USE CASE UML 2.5

**Ă„ÂĂ¡Â»Â tÄ‚Â i**: HĂ¡Â»â€¡ thĂ¡Â»â€˜ng quĂ¡ÂºÂ£n lÄ‚Â½ nhÄ‚Â  hÄ‚Â ng Ă„â€˜a chi nhÄ‚Â¡nh tÄ‚Â­ch hĂ¡Â»Â£p TrĂ¡Â»Â£ lÄ‚Â½ Ă¡ÂºÂ£o AI Agent  
**ChĂ¡Â»Â©c danh Ă„â€˜Ă¡ÂºÂ£m nhiĂ¡Â»â€¡m**: Senior System Analyst / UML 2.5 Analyst / Software Architect / Requirements Engineer / Code Auditor  
**NguyÄ‚Âªn tĂ¡ÂºÂ¯c chĂ¡Â»â€° Ă„â€˜Ă¡ÂºÂ¡o**: MÄ‚Â£ nguĂ¡Â»â€œn (Source Code) hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i lÄ‚Â  **Single Source of Truth** (NguĂ¡Â»â€œn sĂ¡Â»Â± thĂ¡ÂºÂ­t duy nhĂ¡ÂºÂ¥t). 

---

## PART A Ă¢â‚¬â€ FINAL ACTOR MODEL (MÄ‚â€ HÄ‚Å’NH ACTOR CHÄ‚ÂNH THĂ¡Â»Â¨C)

Sau khi audit lĂ¡ÂºÂ¡i toÄ‚Â n bĂ¡Â»â„¢ cĂ†Â¡ chĂ¡ÂºÂ¿ Authentication, JWT Claims, Authorization Policies (`[Authorize]`), Controllers, Services vÄ‚Â  UI Routes, **7 Actor nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ chÄ‚Â­nh thĂ¡Â»Â©c** Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c nhĂ¡ÂºÂ­n nhĂ†Â° sau:

| Actor | Valid? | Scope | Business Goal (MĂ¡Â»Â¥c tiÄ‚Âªu nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ cĂ¡Â»â€˜t lÄ‚Âµi) | Evidence Source (Code) | Notes & Constraints |
| :--- | :---: | :--- | :--- | :--- | :--- |
| **1. Admin tĂ¡Â»â€¢ng** | `VALID` | **Global Scope** (`BranchId = null` hoĂ¡ÂºÂ·c linh hoĂ¡ÂºÂ¡t) | QuĂ¡ÂºÂ£n trĂ¡Â»â€¹ cĂ¡ÂºÂ¥p chuĂ¡Â»â€”i: MĂ¡ÂºÂ¡ng lĂ†Â°Ă¡Â»â€ºi chi nhÄ‚Â¡nh, danh mĂ¡Â»Â¥c thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n & topping dÄ‚Â¹ng chung, Ă†Â°u Ă„â€˜Ä‚Â£i toÄ‚Â n chuĂ¡Â»â€”i, nhÄ‚Â¢n sĂ¡Â»Â± toÄ‚Â n hĂ¡Â»â€¡ thĂ¡Â»â€˜ng, cĂ¡ÂºÂ¥u hÄ‚Â¬nh Global, AI Write Tools cao cĂ¡ÂºÂ¥p. | `AuthController.cs`<br>`ProductController.cs`<br>`BranchController.cs`<br>`SystemSettingsController.cs` | QuĂ¡ÂºÂ£n trĂ¡Â»â€¹ viÄ‚Âªn tĂ¡Â»â€˜i cao. PhĂ¡ÂºÂ¡m vi khÄ‚Â´ng bĂ¡Â»â€¹ giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n bĂ¡Â»Å¸i `branchId`. LÄ‚Â  Primary Actor duy nhĂ¡ÂºÂ¥t cĂ¡Â»Â§a UC QuĂ¡ÂºÂ£n lÄ‚Â½ danh mĂ¡Â»Â¥c toÄ‚Â n chuĂ¡Â»â€”i (`UC10.1`). |
| **2. Manager** | `VALID` | **Branch Scope** (GĂ¡ÂºÂ¯n cĂ¡Â»â€˜ Ă„â€˜Ă¡Â»â€¹nh vĂ¡Â»â€ºi `branchId` JWT) | QuĂ¡ÂºÂ£n lÄ‚Â½ vĂ¡ÂºÂ­n hÄ‚Â nh chi nhÄ‚Â¡nh: NhÄ‚Â¢n sĂ¡Â»Â± chi nhÄ‚Â¡nh, sĂ†Â¡ Ă„â€˜Ă¡Â»â€œ bÄ‚Â n/khu vĂ¡Â»Â±c, phÄ‚Â¢n lĂ¡Â»â€¹ch lÄ‚Â m viĂ¡Â»â€¡c, chi phÄ‚Â­ chi nhÄ‚Â¡nh, Dashboard & Business Insights chi nhÄ‚Â¡nh. | `AuthController.cs`<br>`EmployeeController.cs`<br>`DashboardController.cs`<br>`BusinessInsightController.cs` | QuĂ¡ÂºÂ£n lÄ‚Â½ cĂ¡ÂºÂ¥p chi nhÄ‚Â¡nh. BĂ¡Â»â€¹ cÄ‚Â´ lĂ¡ÂºÂ­p dĂ¡Â»Â¯ liĂ¡Â»â€¡u theo `branchId`. **KhÄ‚Â´ng cÄ‚Â³ quyĂ¡Â»Ân CRUD danh mĂ¡Â»Â¥c thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n toÄ‚Â n chuĂ¡Â»â€”i** (`ProductController` trĂ¡ÂºÂ£ vĂ¡Â»Â `403`). |
| **3. Cashier** | `VALID` | **Branch Scope** (GĂ¡ÂºÂ¯n cĂ¡Â»â€˜ Ă„â€˜Ă¡Â»â€¹nh vĂ¡Â»â€ºi `branchId` JWT) | Thu ngÄ‚Â¢n & TÄ‚Â i chÄ‚Â­nh quĂ¡ÂºÂ§y: BÄ‚Â¡n hÄ‚Â ng POS, tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n Web, xĂ¡Â»Â­ lÄ‚Â½ thanh toÄ‚Â¡n, Ă„â€˜Ă¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m Loyalty, mĂ¡Â»Å¸/chĂ¡Â»â€˜t ca tiĂ¡Â»Ân mĂ¡ÂºÂ·t (`Shift`), in hÄ‚Â³a Ă„â€˜Ă†Â¡n. | `AuthController.cs`<br>`OrderController.cs`<br>`ShiftController.cs`<br>`ReceiptSettingsController.cs` | ChuyÄ‚Âªn trÄ‚Â¡ch giao dĂ¡Â»â€¹ch bÄ‚Â¡n hÄ‚Â ng vÄ‚Â  thanh toÄ‚Â¡n tĂ¡ÂºÂ¡i Ă„â€˜iĂ¡Â»Æ’m bÄ‚Â¡n. Ă„ÂĂ¡Â»â„¢c lĂ¡ÂºÂ­p vĂ¡Â»â€ºi cÄ‚Â´ng viĂ¡Â»â€¡c phĂ¡Â»Â¥c vĂ¡Â»Â¥ bÄ‚Â n thuĂ¡ÂºÂ§n tÄ‚Âºy cĂ¡Â»Â§a Employee. |
| **4. Employee** | `VALID` | **Branch Scope** (GĂ¡ÂºÂ¯n cĂ¡Â»â€˜ Ă„â€˜Ă¡Â»â€¹nh vĂ¡Â»â€ºi `branchId` JWT) | PhĂ¡Â»Â¥c vĂ¡Â»Â¥ bÄ‚Â n (Waitstaff): TĂ¡ÂºÂ¡o/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă†Â¡n POS tĂ¡ÂºÂ¡i bÄ‚Â n, tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n Web, chuyĂ¡Â»Æ’n trĂ¡ÂºÂ¡ng thÄ‚Â¡i bÄ‚Â n, gĂ¡Â»Â­i Ă„â€˜Ă¡Â»Â£t mÄ‚Â³n xuĂ¡Â»â€˜ng KDS bĂ¡ÂºÂ¿p, chĂ¡ÂºÂ¥m cÄ‚Â´ng QR. | `AuthController.cs`<br>`OrderController.cs`<br>`AttendanceController.cs`<br>`WorkScheduleController.cs` | NhÄ‚Â¢n viÄ‚Âªn phĂ¡Â»Â¥c vĂ¡Â»Â¥. **KhÄ‚Â´ng cÄ‚Â³ quyĂ¡Â»Ân xĂ¡Â»Â­ lÄ‚Â½ thanh toÄ‚Â¡n** (`PayOrderAsync`) hay ghi nhĂ¡ÂºÂ­n chi phÄ‚Â­ trong luĂ¡Â»â€œng nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ quĂ¡ÂºÂ§y. |
| **5. Kitchen** | `VALID` | **Branch Scope** (GĂ¡ÂºÂ¯n cĂ¡Â»â€˜ Ă„â€˜Ă¡Â»â€¹nh vĂ¡Â»â€ºi `branchId` JWT) | Ă„ÂiĂ¡Â»Âu phĂ¡Â»â€˜i chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n (KDS): NhĂ¡ÂºÂ­n Ă„â€˜Ă¡Â»Â£t nĂ¡ÂºÂ¥u thĂ¡Â»Âi gian thĂ¡Â»Â±c SignalR, cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i nĂ¡ÂºÂ¥u, bÄ‚Â¡o tĂ¡ÂºÂ¡m hĂ¡ÂºÂ¿t mÄ‚Â³n (`AvailabilityStatus`), xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ bĂ¡ÂºÂ¿p. | `KitchenHub.cs`<br>`KitchenService.cs`<br>`ProductController.cs` (`UpdateAvailability`) | NhÄ‚Â¢n viÄ‚Âªn nhÄ‚Â  bĂ¡ÂºÂ¿p. KhÄ‚Â´ng tĂ†Â°Ă†Â¡ng tÄ‚Â¡c vĂ¡Â»â€ºi mÄ‚Â n hÄ‚Â¬nh bÄ‚Â¡n hÄ‚Â ng POS, thanh toÄ‚Â¡n hay Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n. |
| **6. Customer** | `VALID` | **Personal Scope** (XÄ‚Â¡c thĂ¡Â»Â±c qua `CustomerId` / SĂ„ÂT) | KhÄ‚Â¡ch hÄ‚Â ng Ă„â€˜Ă¡Â»â€¹nh danh: QuĂ¡ÂºÂ£n lÄ‚Â½ hĂ¡Â»â€œ sĂ†Â¡ cÄ‚Â¡ nhÄ‚Â¢n, xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n cÄ‚Â¡ nhÄ‚Â¢n, tra cĂ¡Â»Â©u & tÄ‚Â­ch/tiÄ‚Âªu Ă„â€˜iĂ¡Â»Æ’m Loyalty, quĂ¡ÂºÂ£n lÄ‚Â½ lĂ¡Â»â€¹ch Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n cÄ‚Â¡ nhÄ‚Â¢n, AI Chatbot. | `AuthController.cs` (`customer-token`)<br>`CustomerController.cs`<br>`LoyaltyService.cs` | KhÄ‚Â¡ch hÄ‚Â ng Ă„â€˜Ä‚Â£ xÄ‚Â¡c thĂ¡Â»Â±c. ThĂ¡Â»Â«a hĂ†Â°Ă¡Â»Å¸ng toÄ‚Â n bĂ¡Â»â„¢ khĂ¡ÂºÂ£ nĂ„Æ’ng tĂ†Â°Ă†Â¡ng tÄ‚Â¡c cÄ‚Â´ng khai cĂ¡Â»Â§a Guest + QuyĂ¡Â»Ân cÄ‚Â¡ nhÄ‚Â¢n hÄ‚Â³a. |
| **7. Guest** | `VALID` | **Contextual Scope** (XÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh theo QR BÄ‚Â n / Context) | KhÄ‚Â¡ch vÄ‚Â£ng lai: QuÄ‚Â©t QR bÄ‚Â n, xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n cÄ‚Â´ng khai, Ă„â€˜Ă„Æ’ng kÄ‚Â½ tÄ‚Â i khoĂ¡ÂºÂ£n Customer, tĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n Ă„â€˜Ă¡ÂºÂ·t mÄ‚Â³n QR/Web vÄ‚Â£ng lai, tĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n vÄ‚Â£ng lai. | `AuthController.cs` (`[AllowAnonymous]`)<br>`ProductController.cs`<br>`OrderController.cs` | KhÄ‚Â¡ch chĂ†Â°a Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p. MĂ¡Â»Âi Ă„â€˜Ă†Â¡n Ă„â€˜Ă¡ÂºÂ·t mÄ‚Â³n/Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n tĂ¡ÂºÂ¡o ra Ă„â€˜Ă¡Â»Âu Ă¡Â»Å¸ trĂ¡ÂºÂ¡ng thÄ‚Â¡i chĂ¡Â»Â (`Pending`/`Ă„Âang xĂ¡Â»Â­ lÄ‚Â½`) cĂ¡ÂºÂ§n nhÄ‚Â¢n viÄ‚Âªn tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n. |

### NguyÄ‚Âªn tĂ¡ÂºÂ¯c cĂ¡Â»â€˜ Ă„â€˜Ă¡Â»â€¹nh vĂ¡Â»Â Actor Model:
- **`Staff` KHÄ‚â€NG PHĂ¡ÂºÂ¢I LÄ‚â‚¬ ACTOR**: Trong mÄ‚Â£ nguĂ¡Â»â€œn khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i Role `"staff"`. `Staff` lÄ‚Â  mĂ¡Â»â„¢t **KhÄ‚Â¡i niĂ¡Â»â€¡m nhÄ‚Â³m nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ (Business Grouping Concept)** Ă„â€˜Ă¡ÂºÂ¡i diĂ¡Â»â€¡n cho 4 Actor vĂ¡ÂºÂ­n hÄ‚Â nh (`Manager`, `Cashier`, `Employee`, `Kitchen`).
- **KhÄ‚Â´ng gĂ¡Â»â„¢p Role**: 4 Actor `Manager`, `Cashier`, `Employee`, `Kitchen` giĂ¡Â»Â¯ nguyÄ‚Âªn tĂ†Â° cÄ‚Â¡ch lÄ‚Â  4 Actor UML Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p.

---

## PART B Ă¢â‚¬â€ ACTOR GENERALIZATION AUDIT

Audit ngĂ¡Â»Â¯ nghĂ„Â©a UML 2.5 cho cÄ‚Â¡c mĂ¡Â»â€˜i quan hĂ¡Â»â€¡ giĂ¡Â»Â¯a cÄ‚Â¡c Actors:

| Source Actor | Relationship | Target Actor | Status | Semantic Reason & Code Justification |
| :--- | :---: | :--- | :---: | :--- |
| **Customer** | **`Generalization`** | **Guest** | `VALID` | `Customer` (KhÄ‚Â¡ch Ă„â€˜Ä‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p) kĂ¡ÂºÂ¿ thĂ¡Â»Â«a toÄ‚Â n bĂ¡Â»â„¢ khĂ¡ÂºÂ£ nĂ„Æ’ng tĂ†Â°Ă†Â¡ng tÄ‚Â¡c cÄ‚Â´ng khai cĂ¡Â»Â§a `Guest` (quÄ‚Â©t QR, xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n, tĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n/lĂ¡Â»â€¹ch hĂ¡ÂºÂ¹n vÄ‚Â£ng lai) vÄ‚Â  mĂ¡Â»Å¸ rĂ¡Â»â„¢ng thÄ‚Âªm cÄ‚Â¡c Use Case Ă„â€˜Ă¡Â»â€¹nh danh (xem profile, lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n, tÄ‚Â­ch Ă„â€˜iĂ¡Â»Æ’m Loyalty). Ă„ÂÄ‚Â¢y lÄ‚Â  quan hĂ¡Â»â€¡ Generalization (`is-a`) chuĂ¡ÂºÂ©n mĂ¡Â»Â±c UML 2.5. |
| **Admin** | *Generalization* | **Manager** | `INVALID (REMOVED)` | Admin (Global Scope) vÄ‚Â  Manager (Branch Scope) lÄ‚Â  2 Actor quĂ¡ÂºÂ£n trĂ¡Â»â€¹ Ă¡Â»Å¸ hai cĂ¡ÂºÂ¥p Ă„â€˜Ă¡Â»â„¢ quĂ¡ÂºÂ£n lÄ‚Â½ hoÄ‚Â n toÄ‚Â n khÄ‚Â¡c nhau. Admin khÄ‚Â´ng bĂ¡Â»â€¹ rÄ‚Â ng buĂ¡Â»â„¢c `branchId` nhĂ†Â° Manager. KhÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i quan hĂ¡Â»â€¡ kĂ¡ÂºÂ¿ thĂ¡Â»Â«a `is-a`. |
| **Manager / Cashier / Kitchen** | *Generalization* | **Employee** | `INVALID (REMOVED)` | 4 Actor nÄ‚Â y Ă„â€˜Ă¡ÂºÂ¡i diĂ¡Â»â€¡n cho 4 vĂ¡Â»â€¹ trÄ‚Â­ cÄ‚Â´ng tÄ‚Â¡c phÄ‚Â¢n biĂ¡Â»â€¡t vĂ¡Â»â€ºi tĂ¡ÂºÂ­p thĂ¡ÂºÂ©m quyĂ¡Â»Ân nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ riÄ‚Âªng biĂ¡Â»â€¡t trong code (`manager`, `cashier`, `employee`, `kitchen`). KhÄ‚Â´ng cÄ‚Â³ quan hĂ¡Â»â€¡ kĂ¡ÂºÂ¿ thĂ¡Â»Â«a class hay role trong Backend. |
| **Staff** | *Generalization* | **...** | `INVALID (REMOVED)` | `Staff` khÄ‚Â´ng phĂ¡ÂºÂ£i lÄ‚Â  mĂ¡Â»â„¢t Actor hay Role, do Ă„â€˜Ä‚Â³ khÄ‚Â´ng tham gia vÄ‚Â o bĂ¡ÂºÂ¥t kĂ¡Â»Â³ quan hĂ¡Â»â€¡ Generalization nÄ‚Â o. |

---

## PART C Ă¢â‚¬â€ FINAL USE CASE MODEL (12 HIGH-LEVEL BUSINESS GROUPS)

Danh sÄ‚Â¡ch **12 High-Level Business Groups (HLUC1 $\rightarrow$ HLUC12)** tĂ¡Â»â€¢ng quÄ‚Â¡t Ă„â€˜Ä‚Â³ng vai trÄ‚Â² lÄ‚Â  danh mĂ¡Â»Â¥c miĂ¡Â»Ân nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ cĂ¡ÂºÂ¥p cao:

| ID | High-Level Business Group | Primary Actor | Supporting Actor | Business Goal (MĂ¡Â»Â¥c tiÄ‚Âªu nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ cĂ¡Â»â€˜t lÄ‚Âµi) | Evidence Source | Status |
| :-: | :--- | :--- | :--- | :--- | :--- | :-: |
| **HLUC1** | **Tra cĂ¡Â»Â©u ThĂ¡Â»Â±c Ă„â€˜Ă†Â¡n & SĂ†Â¡ Ă„â€˜Ă¡Â»â€œ BÄ‚Â n** | Guest, Customer | -- | Cho phÄ‚Â©p khÄ‚Â¡ch hÄ‚Â ng xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n Ă„â€˜iĂ¡Â»â€¡n tĂ¡Â»Â­, tÄ‚Â¬m kiĂ¡ÂºÂ¿m mÄ‚Â³n Ă„Æ’n vÄ‚Â  nhĂ¡ÂºÂ­n diĂ¡Â»â€¡n bÄ‚Â n/chi nhÄ‚Â¡nh qua QR. | `ProductController.cs`, `TableController.cs` | `IMPLEMENTED` |
| **HLUC2** | **Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n qua QR / Web Order** | Guest, Customer | Cashier, Employee | Cho phÄ‚Â©p khÄ‚Â¡ch hÄ‚Â ng tĂ¡Â»Â± chĂ¡Â»Ân mÄ‚Â³n, chĂ¡Â»Ân Size/Topping vÄ‚Â  gĂ¡Â»Â­i Ă„â€˜Ă†Â¡n Ă„â€˜Ă¡ÂºÂ·t mÄ‚Â³n tĂ¡Â»Â± phĂ¡Â»Â¥c vĂ¡Â»Â¥ tĂ¡Â»Â« Ă„â€˜iĂ¡Â»â€¡n thoĂ¡ÂºÂ¡i. | `OrderController.cs`, `OrderService.cs` | `IMPLEMENTED` |
| **HLUC3** | **Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n trĂ¡Â»Â±c tuyĂ¡ÂºÂ¿n** | Guest, Customer | Cashier, Employee, Manager | Cho phÄ‚Â©p khÄ‚Â¡ch hÄ‚Â ng Ă„â€˜Ă¡ÂºÂ·t lĂ¡Â»â€¹ch hĂ¡ÂºÂ¹n bÄ‚Â n trĂ†Â°Ă¡Â»â€ºc; nhÄ‚Â¢n viÄ‚Âªn/quĂ¡ÂºÂ£n lÄ‚Â½ tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n vÄ‚Â  xĂ¡Â»Â­ lÄ‚Â½ lĂ¡Â»â€¹ch hĂ¡ÂºÂ¹n. | `ReservationController.cs`, `ReservationService.cs` | `IMPLEMENTED` |
| **HLUC4** | **QuĂ¡ÂºÂ£n lÄ‚Â½ Profile & Ă„ÂiĂ¡Â»Æ’m Loyalty** | Customer | Cashier | Cho phÄ‚Â©p khÄ‚Â¡ch hÄ‚Â ng Ă„â€˜Ă¡Â»â€¹nh danh quĂ¡ÂºÂ£n lÄ‚Â½ hĂ¡Â»â€œ sĂ†Â¡ cÄ‚Â¡ nhÄ‚Â¢n, tra cĂ¡Â»Â©u Ă„â€˜iĂ¡Â»Æ’m thĂ†Â°Ă¡Â»Å¸ng Loyalty vÄ‚Â  hĂ¡ÂºÂ¡ng thÄ‚Â nh viÄ‚Âªn. | `CustomerController.cs`, `LoyaltyService.cs` | `IMPLEMENTED` |
| **HLUC5** | **QuĂ¡ÂºÂ£n lÄ‚Â½ BÄ‚Â¡n hÄ‚Â ng POS & PhĂ¡Â»Â¥c vĂ¡Â»Â¥** | Employee, Cashier, Manager | Admin, Kitchen | Cho phÄ‚Â©p nhÄ‚Â¢n viÄ‚Âªn/thu ngÄ‚Â¢n lĂ¡ÂºÂ­p Ă„â€˜Ă†Â¡n POS tĂ¡ÂºÂ¡i bÄ‚Â n, tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n Web, chuyĂ¡Â»Æ’n Ă„â€˜Ă¡Â»Â£t mÄ‚Â³n xuĂ¡Â»â€˜ng bĂ¡ÂºÂ¿p KDS. | `OrderController.cs`, `OrderService.cs`, `POSPage.tsx` | `IMPLEMENTED` |
| **HLUC6** | **Thanh toÄ‚Â¡n & In hÄ‚Â³a Ă„â€˜Ă†Â¡n** | Cashier, Manager | Admin, Customer | Cho phÄ‚Â©p thu ngÄ‚Â¢n xĂ¡Â»Â­ lÄ‚Â½ thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n hÄ‚Â ng (Cash/Transfer), Ä‚Â¡p dĂ¡Â»Â¥ng Ă„â€˜Ă¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m Loyalty vÄ‚Â  in hÄ‚Â³a Ă„â€˜Ă†Â¡n. | `OrderController.cs` (`PayOrder`), `ReceiptSettingsController.cs` | `IMPLEMENTED` |
| **HLUC7** | **Ă„ÂiĂ¡Â»Âu phĂ¡Â»â€˜i ChĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n (KDS)** | Kitchen | Manager, Admin | Cho phÄ‚Â©p nhÄ‚Â  bĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n danh sÄ‚Â¡ch Ă„â€˜Ă¡Â»Â£t nĂ¡ÂºÂ¥u thĂ¡Â»Âi gian thĂ¡Â»Â±c, cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i nĂ¡ÂºÂ¥u vÄ‚Â  bÄ‚Â¡o tĂ¡ÂºÂ¡m hĂ¡ÂºÂ¿t mÄ‚Â³n. | `KitchenHub.cs`, `KitchenService.cs` | `IMPLEMENTED` |
| **HLUC8** | **QuĂ¡ÂºÂ£n lÄ‚Â½ Ca lÄ‚Â m viĂ¡Â»â€¡c & ChĂ¡ÂºÂ¥m cÄ‚Â´ng**| Employee, Cashier, Kitchen, Manager | Admin | Cho phÄ‚Â©p nhÄ‚Â¢n sĂ¡Â»Â± thĂ¡Â»Â±c hiĂ¡Â»â€¡n chĂ¡ÂºÂ¥m cÄ‚Â´ng QR, xem lĂ¡Â»â€¹ch lÄ‚Â m viĂ¡Â»â€¡c cÄ‚Â¡ nhÄ‚Â¢n, mĂ¡Â»Å¸ vÄ‚Â  chĂ¡Â»â€˜t ca lÄ‚Â m viĂ¡Â»â€¡c (Shift). | `AttendanceController.cs`, `ShiftController.cs` | `IMPLEMENTED` |
| **HLUC9** | **QuĂ¡ÂºÂ£n lÄ‚Â½ VĂ¡ÂºÂ­n hÄ‚Â nh Chi nhÄ‚Â¡nh** | Manager | Admin | Cho phÄ‚Â©p quĂ¡ÂºÂ£n lÄ‚Â½ Ă„â€˜iĂ¡Â»Âu hÄ‚Â nh sĂ†Â¡ Ă„â€˜Ă¡Â»â€œ bÄ‚Â n/khu vĂ¡Â»Â±c, quĂ¡ÂºÂ£n lÄ‚Â½ chi phÄ‚Â­ vĂ¡ÂºÂ­n hÄ‚Â nh vÄ‚Â  phÄ‚Â¢n lĂ¡Â»â€¹ch lÄ‚Â m viĂ¡Â»â€¡c chi nhÄ‚Â¡nh. | `TableController.cs`, `EmployeeController.cs`, `ExpenseController.cs` | `IMPLEMENTED` |
| **HLUC10**| **QuĂ¡ÂºÂ£n trĂ¡Â»â€¹ HĂ¡Â»â€¡ thĂ¡Â»â€˜ng & Danh mĂ¡Â»Â¥c** | Admin | Manager (Branch Settings) | Cho phÄ‚Â©p Admin quĂ¡ÂºÂ£n lÄ‚Â½ danh mĂ¡Â»Â¥c thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n toÄ‚Â n chuĂ¡Â»â€”i, topping, chi nhÄ‚Â¡nh, Ă†Â°u Ă„â€˜Ä‚Â£i vÄ‚Â  cĂ¡ÂºÂ¥u hÄ‚Â¬nh VAT/Fee. | `ProductController.cs`, `BranchController.cs`, `SystemSettingsController.cs` | `IMPLEMENTED` |
| **HLUC11**| **BÄ‚Â¡o cÄ‚Â¡o Doanh thu & Insights** | Manager, Admin | -- | Cho phÄ‚Â©p quĂ¡ÂºÂ£n trĂ¡Â»â€¹ viÄ‚Âªn xem Dashboard thĂ¡Â»â€˜ng kÄ‚Âª doanh thu vÄ‚Â  phÄ‚Â¢n tÄ‚Â­ch cĂ¡ÂºÂ£nh bÄ‚Â¡o bĂ¡ÂºÂ¥t thĂ†Â°Ă¡Â»Âng Business Insights. | `DashboardController.cs`, `BusinessInsightController.cs` | `IMPLEMENTED` |
| **HLUC12**| **TĂ†Â°Ă†Â¡ng tÄ‚Â¡c TrĂ¡Â»Â£ lÄ‚Â½ Ă¡ÂºÂ£o AI Agent** | Guest, Customer, Employee, Cashier, Kitchen, Manager, Admin | -- | Cho phÄ‚Â©p ngĂ†Â°Ă¡Â»Âi dÄ‚Â¹ng Ă„â€˜Ä‚Â m thoĂ¡ÂºÂ¡i ngÄ‚Â´n ngĂ¡Â»Â¯ tĂ¡Â»Â± nhiÄ‚Âªn vĂ¡Â»â€ºi AI Ă„â€˜Ă¡Â»Æ’ tra cĂ¡Â»Â©u thÄ‚Â´ng tin (Read) hoĂ¡ÂºÂ·c thĂ¡Â»Â±c hiĂ¡Â»â€¡n tÄ‚Â¡c vĂ¡Â»Â¥ (Write). | `AiController.cs`, `AiOrchestrator.cs` | `IMPLEMENTED` |

---

## PART D Ă¢â‚¬â€ INVALID / REMOVED / TECHNICAL USE CASES (BÄ‚ÂO CÄ‚ÂO BĂ¡Â»Â CÄ‚ÂC YĂ¡ÂºÂ¾U TĂ¡Â»Â KĂ¡Â»Â¸ THUĂ¡ÂºÂ¬T)

TĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ cÄ‚Â¡c yĂ¡ÂºÂ¿u tĂ¡Â»â€˜ kĂ¡Â»Â¹ thuĂ¡ÂºÂ­t, thuĂ¡ÂºÂ­t toÄ‚Â¡n nĂ¡Â»â„¢i bĂ¡Â»â„¢, middleware vÄ‚Â  tÄ‚Â­nh nĂ„Æ’ng Ă„â€˜Ä‚Â£ xÄ‚Â³a bĂ¡Â»â€¹ **LOĂ¡ÂºÂ I BĂ¡Â»Â HOÄ‚â‚¬N TOÄ‚â‚¬N** khĂ¡Â»Âi danh sÄ‚Â¡ch Use Case:

| Candidate Element | Why Invalid as Use Case? | Correct Semantic Placement (Where It Belongs) |
| :--- | :--- | :--- |
| *Validate Payment / Check Amount* | Quy tĂ¡ÂºÂ¯c xÄ‚Â¡c thĂ¡Â»Â±c tham sĂ¡Â»â€˜ sĂ¡Â»â€˜ tiĂ¡Â»Ân trong `PayOrderAsync`. | **Preconditions / Flow of Events** cĂ¡Â»Â§a `UC6.1: Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n hÄ‚Â ng`. |
| *Calculate VAT / Service Fee / Size Price* | ThuĂ¡ÂºÂ­t toÄ‚Â¡n tÄ‚Â­nh toÄ‚Â¡n tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng trong `RecalculateOrderFinancials`. | **Flow of Events / Business Rules** cĂ¡Â»Â§a `UC5.1: TĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n hÄ‚Â ng POS`. |
| *Earn Loyalty Points Automatically* | TÄ‚Â¡c Ă„â€˜Ă¡Â»â„¢ng phĂ¡Â»Â¥ tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng phÄ‚Â­a server (`EarnPointsAsync`) sau khi thanh toÄ‚Â¡n. | **Postconditions / System Side-Effect** cĂ¡Â»Â§a `UC6.1: Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n hÄ‚Â ng`. |
| *Check Branch Permission / Validate JWT* | CĂ†Â¡ chĂ¡ÂºÂ¿ kiĂ¡Â»Æ’m soÄ‚Â¡t truy cĂ¡ÂºÂ­p kĂ¡Â»Â¹ thuĂ¡ÂºÂ­t cĂ¡Â»Â§a Middleware & Policy. | **System Preconditions / Security Constraints**. |
| *SignalR Event Broadcasting* | Giao thĂ¡Â»Â©c truyĂ¡Â»Ân tin thĂ¡Â»Âi gian thĂ¡Â»Â±c giĂ¡Â»Â¯a Server vÄ‚Â  KDS Web Client. | **System Sequence / Technical Architecture**. |
| *Individual 14 AI Tools (GetRevenueTool...)* | CĂ†Â¡ chĂ¡ÂºÂ¿ thĂ¡Â»Â±c thi chĂ¡Â»Â©c nĂ„Æ’ng nĂ¡Â»â„¢i bĂ¡Â»â„¢ do AI Orchestrator gĂ¡Â»Âi qua AI provider. | Gom vÄ‚Â o 2 Sub-UC: Tra cĂ¡Â»Â©u AI (`UC12.1`) & ThĂ¡Â»Â±c hiĂ¡Â»â€¡n tÄ‚Â¡c vĂ¡Â»Â¥ AI (`UC12.2`). |
| **removed staff-finance module (QuĂ¡ÂºÂ£n lÄ‚Â½ & TÄ‚Â­nh bĂ¡ÂºÂ£ng lĂ†Â°Ă†Â¡ng)** | **Ă„ÂÄ‚Â£ bĂ¡Â»â€¹ xÄ‚Â³a hoÄ‚Â n toÄ‚Â n khĂ¡Â»Âi mÄ‚Â£ nguĂ¡Â»â€œn** qua Migration `20260919075618`. | **REMOVED FEATURE**. KhÄ‚Â´ng xuĂ¡ÂºÂ¥t hiĂ¡Â»â€¡n Ă¡Â»Å¸ bĂ¡ÂºÂ¥t kĂ¡Â»Â³ biĂ¡Â»Æ’u Ă„â€˜Ă¡Â»â€œ hay ma trĂ¡ÂºÂ­n nÄ‚Â o. |

---

## PART E Ă¢â‚¬â€ INCLUDE AUDIT (AUDIT CÄ‚ÂC QUAN HĂ¡Â»â€  INCLUDE)

Audit ngĂ¡Â»Â¯ nghĂ„Â©a UML cho cÄ‚Â¡c quan hĂ¡Â»â€¡ `<<include>>` (BĂ¡ÂºÂ¯t buĂ¡Â»â„¢c phĂ¡ÂºÂ£i thĂ¡Â»Â±c hiĂ¡Â»â€¡n trong mĂ¡Â»Âi kĂ¡Â»â€¹ch bĂ¡ÂºÂ£n cĂ¡Â»Â§a Base Use Case):

| Source Sub-UC | Relationship | Target Sub-UC | Valid / Invalid | Semantic Reason & Code Evidence |
| :--- | :---: | :--- | :---: | :--- |
| **UC5.2: TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n Web Order** | **`<<include>>`** | **UC5.1: TĂ¡ÂºÂ¡o & CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă†Â¡n hÄ‚Â ng POS** | `VALID` | ViĂ¡Â»â€¡c tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n Web Order (`AcceptWebOrderAsync`) bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c phĂ¡ÂºÂ£i thĂ¡Â»Â±c hiĂ¡Â»â€¡n viĂ¡Â»â€¡c chuyĂ¡Â»Æ’n Ă„â€˜Ă¡Â»â€¢i Ă„â€˜Ă†Â¡n chĂ¡Â»Â vÄ‚Â£ng lai thÄ‚Â nh mĂ¡Â»â„¢t Ă„â€˜Ă†Â¡n POS chÄ‚Â­nh thĂ¡Â»Â©c gĂ¡ÂºÂ¯n bÄ‚Â n/ngĂ†Â°Ă¡Â»Âi nhĂ¡ÂºÂ­n. |
| **UC5.3: GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n xuĂ¡Â»â€˜ng BĂ¡ÂºÂ¿p**| **`<<include>>`** | **UC5.1: TĂ¡ÂºÂ¡o & CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă†Â¡n hÄ‚Â ng POS** | `VALID` | MĂ¡Â»Âi Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Â­i mÄ‚Â³n xuĂ¡Â»â€˜ng KDS bĂ¡ÂºÂ¿p (`SendToKitchenAsync`) bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c phĂ¡ÂºÂ£i truy xuĂ¡ÂºÂ¥t vÄ‚Â  cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t lĂ¡ÂºÂ¡i sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i (`SentQuantity`) trÄ‚Âªn Ă„â€˜Ă†Â¡n hÄ‚Â ng POS. |
| *UC6.1: Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n hÄ‚Â ng* | *<<include>>* | *TÄ‚Â­nh VAT / Fee* | `INVALID (REMOVED)` | Ă„ÂÄ‚Â¢y lÄ‚Â  thuĂ¡ÂºÂ­t toÄ‚Â¡n tÄ‚Â­nh toÄ‚Â¡n nĂ¡Â»â„¢i bĂ¡Â»â„¢, khÄ‚Â´ng phĂ¡ÂºÂ£i Use Case Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p. |
| *UC12.1: Tra cĂ¡Â»Â©u qua AI* | *<<include>>* | *Check Permission* | `INVALID (REMOVED)` | KiĂ¡Â»Æ’m tra quyĂ¡Â»Ân lÄ‚Â  middleware kĂ¡Â»Â¹ thuĂ¡ÂºÂ­t, khÄ‚Â´ng phĂ¡ÂºÂ£i Use Case Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p. |

---

## PART F Ă¢â‚¬â€ EXTEND AUDIT (AUDIT CÄ‚ÂC QUAN HĂ¡Â»â€  EXTEND)

Audit ngĂ¡Â»Â¯ nghĂ„Â©a UML cho cÄ‚Â¡c quan hĂ¡Â»â€¡ `<<extend>>` (MĂ¡Â»Å¸ rĂ¡Â»â„¢ng cÄ‚Â³ Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n/tÄ‚Â¹y chĂ¡Â»Ân):

| Extension Sub-UC | Relationship | Base Sub-UC | Valid / Invalid | Extension Point & Condition |
| :--- | :---: | :--- | :---: | :--- |
| **UC6.2: Ă„ÂĂ¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m Loyalty giĂ¡ÂºÂ£m giÄ‚Â¡** | **`<<extend>>`** | **UC5.1: TĂ¡ÂºÂ¡o & CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă†Â¡n hÄ‚Â ng POS** | `VALID` | **Extension Point**: LĂ¡ÂºÂ­p tÄ‚Â i chÄ‚Â­nh Ă„â€˜Ă†Â¡n POS.<br>**Condition**: ChĂ¡Â»â€° thĂ¡Â»Â±c hiĂ¡Â»â€¡n khi khÄ‚Â¡ch hÄ‚Â ng yÄ‚Âªu cĂ¡ÂºÂ§u quy Ă„â€˜Ă¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m tÄ‚Â­ch lĂ…Â©y thÄ‚Â nh tiĂ¡Â»Ân giĂ¡ÂºÂ£m giÄ‚Â¡. |
| **UC6.3: CĂ¡ÂºÂ¥u hÄ‚Â¬nh & In hÄ‚Â³a Ă„â€˜Ă†Â¡n** | **`<<extend>>`** | **UC6.1: Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n hÄ‚Â ng** | `VALID` | **Extension Point**: HoÄ‚Â n tĂ¡ÂºÂ¥t giao dĂ¡Â»â€¹ch thanh toÄ‚Â¡n.<br>**Condition**: ChĂ¡Â»â€° thĂ¡Â»Â±c hiĂ¡Â»â€¡n khi thu ngÄ‚Â¢n hoĂ¡ÂºÂ·c khÄ‚Â¡ch hÄ‚Â ng cÄ‚Â³ nhu cĂ¡ÂºÂ§u xuĂ¡ÂºÂ¥t/in hÄ‚Â³a Ă„â€˜Ă†Â¡n giĂ¡ÂºÂ¥y. |

---

## PART G Ă¢â‚¬â€ ACTOR ASSOCIATION MATRIX (MA TRĂ¡ÂºÂ¬N KĂ¡ÂºÂ¾T NĂ¡Â»ÂI ACTOR CHÄ‚ÂNH THĂ¡Â»Â¨C)

BĂ¡ÂºÂ£ng ma trĂ¡ÂºÂ­n kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i Association giĂ¡Â»Â¯a 7 Actor vÄ‚Â  **29 Sub Use Cases thĂ¡Â»Â±c tĂ¡ÂºÂ¿** sau khi audit Ă„â€˜Ă¡ÂºÂ¿m lĂ¡ÂºÂ¡i chÄ‚Â­nh xÄ‚Â¡c:
- **`P`** = Primary Actor (NgĂ†Â°Ă¡Â»Âi khĂ¡Â»Å¸i tĂ¡ÂºÂ¡o Use Case Ă„â€˜Ă¡Â»Æ’ Ă„â€˜Ă¡ÂºÂ¡t mĂ¡Â»Â¥c tiÄ‚Âªu nghiĂ¡Â»â€¡p vĂ¡Â»Â¥)
- **`S`** = Supporting Actor (NgĂ†Â°Ă¡Â»Âi/HĂ¡Â»â€¡ thĂ¡Â»â€˜ng hĂ¡Â»â€” trĂ¡Â»Â£ hoĂ¡ÂºÂ·c tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n trong luĂ¡Â»â€œng)
- **`-`** = KhÄ‚Â´ng kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i Association

| Detailed Sub Use Case ID & Name | Admin | Manager | Cashier | Employee | Kitchen | Customer | Guest |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **UC1.1: QuÄ‚Â©t QR BÄ‚Â n & Chi nhÄ‚Â¡nh** | - | - | - | - | - | **P** | **P** |
| **UC1.2: Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n & Chi tiĂ¡ÂºÂ¿t mÄ‚Â³n** | - | - | - | - | - | **P** | **P** |
| **UC2.1: GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n Ă„â€˜Ă¡ÂºÂ·t mÄ‚Â³n QR / Web Order** | - | - | S | S | - | **P** | **P** |
| **UC2.2: Xem trĂ¡ÂºÂ¡ng thÄ‚Â¡i & LĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n cÄ‚Â¡ nhÄ‚Â¢n** | - | - | - | - | - | **P** | - |
| **UC3.1: TĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n trĂ¡Â»Â±c tuyĂ¡ÂºÂ¿n** | - | - | - | - | - | **P** | **P** |
| **UC3.2: TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n & QuĂ¡ÂºÂ£n lÄ‚Â½ lĂ¡Â»â€¹ch hĂ¡ÂºÂ¹n Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n** | S | **P** | **P** | **P** | - | - | - |
| **UC4.1: Xem & CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t HĂ¡Â»â€œ sĂ†Â¡ cÄ‚Â¡ nhÄ‚Â¢n** | - | - | - | - | - | **P** | - |
| **UC4.2: Tra cĂ¡Â»Â©u Ă„â€˜iĂ¡Â»Æ’m & HĂ¡ÂºÂ¡ng thÄ‚Â nh viÄ‚Âªn Loyalty** | - | - | S | - | - | **P** | - |
| **UC5.1: TĂ¡ÂºÂ¡o & CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă†Â¡n hÄ‚Â ng POS** | S | **P** | **P** | **P** | - | - | - |
| **UC5.2: TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n Ă„â€˜Ă¡ÂºÂ·t mÄ‚Â³n Web Order** | S | **P** | **P** | **P** | - | - | - |
| **UC5.3: GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n xuĂ¡Â»â€˜ng BĂ¡ÂºÂ¿p** | S | **P** | **P** | **P** | S | - | - |
| **UC6.1: Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n hÄ‚Â ng (Cash/Transfer)** | S | **P** | **P** | - | - | S | - |
| **UC6.2: Ă„ÂĂ¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m Loyalty giĂ¡ÂºÂ£m giÄ‚Â¡ Ă„â€˜Ă†Â¡n hÄ‚Â ng** | S | **P** | **P** | - | - | S | - |
| **UC6.3: CĂ¡ÂºÂ¥u hÄ‚Â¬nh & In hÄ‚Â³a Ă„â€˜Ă†Â¡n thanh toÄ‚Â¡n** | S | **P** | **P** | - | - | - | - |
| **UC7.1: XĂ¡Â»Â­ lÄ‚Â½ Ă„â€˜Ă¡Â»Â£t chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n KDS thĂ¡Â»Âi gian thĂ¡Â»Â±c** | S | S | - | - | **P** | - | - |
| **UC7.2: CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i tĂ¡ÂºÂ¡m hĂ¡ÂºÂ¿t mÄ‚Â³n** | **P** | - | - | - | **P** | - | - |
| **UC8.1: ChĂ¡ÂºÂ¥m cÄ‚Â´ng QR Check-in / Check-out** | S | **P** | **P** | **P** | **P** | - | - |
| **UC8.2: MĂ¡Â»Å¸ & ChĂ¡Â»â€˜t ca lÄ‚Â m viĂ¡Â»â€¡c (Shift)** | S | **P** | **P** | **P** | **P** | - | - |
| **UC8.3: Xem & Ă„ÂĂ„Æ’ng kÄ‚Â½ LĂ¡Â»â€¹ch lÄ‚Â m viĂ¡Â»â€¡c** | S | **P** | **P** | **P** | **P** | - | - |
| **UC9.1: QuĂ¡ÂºÂ£n lÄ‚Â½ SĂ†Â¡ Ă„â€˜Ă¡Â»â€œ bÄ‚Â n & Khu vĂ¡Â»Â±c chi nhÄ‚Â¡nh** | S | **P** | - | - | - | - | - |
| **UC9.2: QuĂ¡ÂºÂ£n lÄ‚Â½ HĂ¡Â»â€œ sĂ†Â¡ & PhÄ‚Â¢n lĂ¡Â»â€¹ch NhÄ‚Â¢n sĂ¡Â»Â±** | S | **P** | - | - | - | - | - |
| **UC9.3: QuĂ¡ÂºÂ£n lÄ‚Â½ Chi phÄ‚Â­ vĂ¡ÂºÂ­n hÄ‚Â nh chi nhÄ‚Â¡nh** | S | **P** | **P** | - | - | - | - |
| **UC10.1: QuĂ¡ÂºÂ£n lÄ‚Â½ Danh mĂ¡Â»Â¥c ThĂ¡Â»Â±c Ă„â€˜Ă†Â¡n & Topping**| **P** | - | - | - | - | - | - |
| **UC10.2: QuĂ¡ÂºÂ£n lÄ‚Â½ Chi nhÄ‚Â¡nh & Ă†Â¯u Ă„â€˜Ä‚Â£i toÄ‚Â n chuĂ¡Â»â€”i**| **P** | - | - | - | - | - | - |
| **UC10.3: CĂ¡ÂºÂ¥u hÄ‚Â¬nh ThiĂ¡ÂºÂ¿t lĂ¡ÂºÂ­p HĂ¡Â»â€¡ thĂ¡Â»â€˜ng (VAT/Fee)**| **P** | **P** | - | - | - | - | - |
| **UC11.1: Xem Dashboard thĂ¡Â»â€˜ng kÄ‚Âª doanh thu** | **P** | **P** | - | - | - | - | - |
| **UC11.2: Xem PhÄ‚Â¢n tÄ‚Â­ch & CĂ¡ÂºÂ£nh bÄ‚Â¡o Insights AI** | **P** | **P** | - | - | - | - | - |
| **UC12.1: Tra cĂ¡Â»Â©u thÄ‚Â´ng tin qua AI (Read Tools)** | **P** | **P** | **P** | **P** | **P** | **P** | **P** |
| **UC12.2: ThĂ¡Â»Â±c hiĂ¡Â»â€¡n tÄ‚Â¡c vĂ¡Â»Â¥ qua AI (Write Tools)**| **P** | **P** | **P** | **P** | **P** | **P** | - |

---

## PART H Ă¢â‚¬â€ FINAL DECOMPOSITION (CĂ¡ÂºÂ¤U TRÄ‚ÂC PHÄ‚â€N CĂ¡ÂºÂ¤P SĂ†Â  Ă„ÂĂ¡Â»â€™)

CĂ¡ÂºÂ¥u trÄ‚Âºc phÄ‚Â¢n cĂ¡ÂºÂ¥p biĂ¡Â»Æ’u Ă„â€˜Ă¡Â»â€œ tĂ¡Â»Â« **HÄ‚Â¬nh 3.1 Ă„â€˜Ă¡ÂºÂ¿n HÄ‚Â¬nh 3.7** Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»â€˜t nhĂ†Â° sau:

===================================================================================
                  HÄ‚Å’NH 3.1: BIĂ¡Â»â€U Ă„ÂĂ¡Â»â€™ USE CASE TĂ¡Â»â€NG QUÄ‚ÂT TOÄ‚â‚¬N HĂ¡Â»â€  THĂ¡Â»ÂNG
   (Bao gĂ¡Â»â€œm 7 Actors & 12 High-Level Business Groups HLUC1 -> HLUC12 theo Domain)
===================================================================================
                                       Ă¢â€â€
     Ă¢â€Å’Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€Â¼Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€Â
     Ă¢â€â€                                 Ă¢â€â€                                 Ă¢â€â€
     Ă¢â€“Â¼                                 Ă¢â€“Â¼                                 Ă¢â€“Â¼
`Domain KhÄ‚Â¡ch hÄ‚Â ng`           `Domain POS, KDS & Ca`            `Domain QuĂ¡ÂºÂ£n trĂ¡Â»â€¹ & AI`
     Ă¢â€â€                                 Ă¢â€â€                                 Ă¢â€â€
     Ă¢â€Å“Ă¢â€â‚¬Ă¢â€“Âº HÄ‚Å’NH 3.2                      Ă¢â€Å“Ă¢â€â‚¬Ă¢â€“Âº HÄ‚Å’NH 3.3                      Ă¢â€Å“Ă¢â€â‚¬Ă¢â€“Âº HÄ‚Å’NH 3.6
     Ă¢â€â€   PhÄ‚Â¢n rÄ‚Â£ DĂ¡Â»â€¹ch vĂ¡Â»Â¥ KhÄ‚Â¡ch hÄ‚Â ng    Ă¢â€â€   PhÄ‚Â¢n rÄ‚Â£ BÄ‚Â¡n hÄ‚Â ng POS,         Ă¢â€â€   PhÄ‚Â¢n rÄ‚Â£ QuĂ¡ÂºÂ£n trĂ¡Â»â€¹ VĂ¡ÂºÂ­n hÄ‚Â nh,
     Ă¢â€â€   & Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n TrĂ¡Â»Â±c tuyĂ¡ÂºÂ¿n          Ă¢â€â€   Thanh toÄ‚Â¡n & In HÄ‚Â³a Ă„â€˜Ă†Â¡n       Ă¢â€â€   HĂ¡Â»â€¡ thĂ¡Â»â€˜ng & Business Insights
     Ă¢â€â€   (HLUC1, HLUC2, HLUC3, HLUC4)  Ă¢â€â€   (HLUC5, HLUC6)                Ă¢â€â€   (HLUC9, HLUC10, HLUC11)
     Ă¢â€â€                                 Ă¢â€â€                                 Ă¢â€â€
     Ă¢â€â€Ă¢â€â‚¬Ă¢â€“Âº (ChĂ¡Â»Â©a UC1.1-1.2, UC2.1-2.2,   Ă¢â€Å“Ă¢â€â‚¬Ă¢â€“Âº HÄ‚Å’NH 3.4                      Ă¢â€â€Ă¢â€â‚¬Ă¢â€“Âº HÄ‚Å’NH 3.7
          UC3.1-3.2, UC4.1-4.2)        Ă¢â€â€   PhÄ‚Â¢n rÄ‚Â£ Ă„ÂiĂ¡Â»Âu phĂ¡Â»â€˜i ChĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n    Ă¢â€â€   PhÄ‚Â¢n rÄ‚Â£ TĂ†Â°Ă†Â¡ng tÄ‚Â¡c TrĂ¡Â»Â£ lÄ‚Â½ Ă¡ÂºÂ£o
                                       Ă¢â€â€   NhÄ‚Â  bĂ¡ÂºÂ¿p KDS (HLUC7)           Ă¢â€â€   AI Agent (HLUC12)
                                       Ă¢â€â€                                 Ă¢â€â€
                                       Ă¢â€â€Ă¢â€â‚¬Ă¢â€“Âº HÄ‚Å’NH 3.5                      Ă¢â€â€Ă¢â€â‚¬Ă¢â€“Âº (ChĂ¡Â»Â©a UC12.1-12.2)
                                           PhÄ‚Â¢n rÄ‚Â£ Ca lÄ‚Â m viĂ¡Â»â€¡c &
                                           ChĂ¡ÂºÂ¥m cÄ‚Â´ng NhÄ‚Â¢n sĂ¡Â»Â± (HLUC8)

---

## PART I Ă¢â‚¬â€ CONTRADICTIONS (CÄ‚ÂC MÄ‚â€U THUĂ¡ÂºÂªN VÄ‚â‚¬ KHÄ‚ÂC BIĂ¡Â»â€ T Ă„ÂÄ‚Æ’ GIĂ¡ÂºÂ¢I QUYĂ¡ÂºÂ¾T)

| Issue ID | MÄ‚Â´ tĂ¡ÂºÂ£ mÄ‚Â¢u thuĂ¡ÂºÂ«n / Discrepancy | MĂ¡Â»Â©c Ă„â€˜Ă¡Â»â„¢ Severity | CÄ‚Â¡ch xĂ¡Â»Â­ lÄ‚Â½ theo Single Source of Truth (Backend) |
| :-: | :--- | :---: | :--- |
| **CONTRAD-01** | **Manager hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Menu ThĂ¡Â»Â±c Ă„â€˜Ă†Â¡n trÄ‚Âªn Frontend** | `MAJOR` | On UI Admin-Web, Manager thĂ¡ÂºÂ¥y menu ThĂ¡Â»Â±c Ă„â€˜Ă†Â¡n. Tuy nhiÄ‚Âªn, Backend `ProductController` vÄ‚Â  `ToppingController` Ă„â€˜Ă¡Â»Âu cÄ‚Â³ `[Authorize(Roles="admin")]`. **ChĂ¡Â»â€˜t**: Manager KHÄ‚â€NG PHĂ¡ÂºÂ¢I Primary Actor cĂ¡Â»Â§a `UC10.1` (CRUD ThĂ¡Â»Â±c Ă„â€˜Ă†Â¡n toÄ‚Â n chuĂ¡Â»â€”i). Manager chĂ¡Â»â€° thĂ¡Â»Â±c hiĂ¡Â»â€¡n `UC7.2` (BÄ‚Â¡o tĂ¡ÂºÂ¡m hĂ¡ÂºÂ¿t mÄ‚Â³n tĂ¡ÂºÂ¡i chi nhÄ‚Â¡nh). |
| **CONTRAD-02** | **XÄ‚Â³a bĂ¡Â»Â hoÄ‚Â n toÄ‚Â n Module removed staff-finance module** | `CRITICAL` | File code `removed staff-finance moduleController.cs` vÄ‚Â  UI `removed staff-finance modulePage.tsx` chĂ¡Â»â€° cÄ‚Â²n comment obsolete. Migration `20260919075618` Ă„â€˜Ä‚Â£ xÄ‚Â³a cÄ‚Â¡c bĂ¡ÂºÂ£ng removed staff-finance module. **ChĂ¡Â»â€˜t**: LoĂ¡ÂºÂ¡i bĂ¡Â»Â hoÄ‚Â n toÄ‚Â n khĂ¡Â»Âi mÄ‚Â´ hÄ‚Â¬nh UML. |
| **CONTRAD-03** | **ChiĂ¡ÂºÂ¿t khĂ¡ÂºÂ¥u Discount do Client gĂ¡Â»Â­i lÄ‚Âªn** | `MINOR` | Client POS/Customer cÄ‚Â³ thĂ¡Â»Æ’ gĂ¡Â»Â­i field `Discount` trong body, nhĂ†Â°ng `OrderService` luÄ‚Â´n gÄ‚Â¡n `Discount = 0` khi tĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n mĂ¡Â»â€ºi Ă„â€˜Ă¡Â»Æ’ chĂ¡Â»â€˜ng gian lĂ¡ÂºÂ­n. **ChĂ¡Â»â€˜t**: GiĂ¡ÂºÂ£m giÄ‚Â¡ chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c sinh ra qua Use Case `UC6.2 (Ă„ÂĂ¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m Loyalty)`. |

---

## PART J Ă¢â‚¬â€ FINAL UML MODEL (TĂ¡Â»â€NG HĂ¡Â»Â¢P MÄ‚â€ HÄ‚Å’NH UML CHÄ‚ÂNH THĂ¡Â»Â¨C)

1. **Actors (7)**: `Admin` (Global), `Manager` (Branch), `Cashier` (Branch), `Employee` (Branch), `Kitchen` (Branch), `Customer` (Personal), `Guest` (Contextual).
2. **Actor Generalization (1)**: **`Customer --|> Guest`**.
3. **High-Level Business Groups (12)**: `HLUC1` Ă„â€˜Ă¡ÂºÂ¿n `HLUC12`.
4. **Detailed Sub Use Cases (29)**: `UC1.1` Ă„â€˜Ă¡ÂºÂ¿n `UC12.2`.
5. **Includes (2)**:
   - `UC5.2 (TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Web Order)` **`--<<include>>-->`** `UC5.1 (TĂ¡ÂºÂ¡o/CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă†Â¡n POS)`
   - `UC5.3 (GĂ¡Â»Â­i bĂ¡ÂºÂ¿p KDS)` **`--<<include>>-->`** `UC5.1 (TĂ¡ÂºÂ¡o/CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă†Â¡n POS)`
6. **Extends (2)**:
   - `UC6.2 (Ă„ÂĂ¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m Loyalty)` **`--<<extend>>-->`** `UC5.1 (TĂ¡ÂºÂ¡o/CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă†Â¡n POS)`
   - `UC6.3 (CĂ¡ÂºÂ¥u hÄ‚Â¬nh & In hÄ‚Â³a Ă„â€˜Ă†Â¡n)` **`--<<extend>>-->`** `UC6.1 (Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n hÄ‚Â ng)`
7. **Removed Elements**: Module removed staff-finance module, cÄ‚Â¡c thuĂ¡ÂºÂ­t toÄ‚Â¡n VAT/Fee calculation, Validation rules, Middleware checks, vÄ‚Â  cÄ‚Â¡c AI Tools rĂ¡Â»Âi rĂ¡ÂºÂ¡c.

---

## FINAL UML QUALITY GATE

| Quality Gate | Description | Status |
| :--- | :--- | :---: |
| **Gate 1** | MĂ¡Â»â€”i Actor cÄ‚Â³ Business Goal rÄ‚Âµ rÄ‚Â ng vÄ‚Â  phÄ‚Â¢n biĂ¡Â»â€¡t Ă„â€˜Ă†Â°Ă¡Â»Â£c Scope? | `PASS` |
| **Gate 2** | MĂ¡Â»â€”i Use Case Ă„â€˜Ă¡ÂºÂ¡i diĂ¡Â»â€¡n cho mĂ¡Â»â„¢t Actor Business Goal thĂ¡Â»Â±c sĂ¡Â»Â±? | `PASS` |
| **Gate 3** | KhÄ‚Â´ng chĂ¡Â»Â©a bĂ¡ÂºÂ¥t kĂ¡Â»Â³ Technical Use Case (CRUD Ă„â€˜Ă†Â¡n thuĂ¡ÂºÂ§n, Validation, API, DTO)? | `PASS` |
| **Gate 4** | KhÄ‚Â´ng chĂ¡Â»Â©a Actor `Staff` giĂ¡ÂºÂ£? | `PASS` |
| **Gate 5** | KhÄ‚Â´ng biĂ¡ÂºÂ¿n API Permission thÄ‚Â nh Actor Association mÄ‚Â¡y mÄ‚Â³c? | `PASS` |
| **Gate 6** | KhÄ‚Â´ng biĂ¡ÂºÂ¿n Service Call/Internal method thÄ‚Â nh Include? | `PASS` |
| **Gate 7** | KhÄ‚Â´ng dÄ‚Â¹ng Extend tÄ‚Â¹y tiĂ¡Â»â€¡n khi khÄ‚Â´ng cÄ‚Â³ Extension Point & Condition rÄ‚Âµ rÄ‚Â ng? | `PASS` |
| **Gate 8** | Quan hĂ¡Â»â€¡ `Customer --|> Guest` Ă„â€˜Ä‚Âºng bĂ¡ÂºÂ£n chĂ¡ÂºÂ¥t UML 2.5 Generalization? | `PASS` |
| **Gate 9** | High-Level Use Cases Ă„â€˜Ä‚Â³ng vai trÄ‚Â² Business Domain Groupings? | `PASS` |
| **Gate 10**| CÄ‚Â¡c AI Tools kĂ¡Â»Â¹ thuĂ¡ÂºÂ­t khÄ‚Â´ng bĂ¡Â»â€¹ biĂ¡ÂºÂ¿n thÄ‚Â nh Use Cases rĂ¡Â»Âi rĂ¡ÂºÂ¡c? | `PASS` |
| **Gate 11**| CÄ‚Â¡c chĂ¡Â»Â©c nĂ„Æ’ng Ă„â€˜Ä‚Â£ bĂ¡Â»â€¹ xÄ‚Â³a (removed staff-finance module) Ă„â€˜Ă†Â°Ă¡Â»Â£c loĂ¡ÂºÂ¡i bĂ¡Â»Â 100%? | `PASS` |
| **Gate 12**| Ma trĂ¡ÂºÂ­n Actor kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i chÄ‚Â­nh xÄ‚Â¡c theo quyĂ¡Â»Ân khĂ¡Â»Å¸i tĂ¡ÂºÂ¡o vÄ‚Â  tham gia nghiĂ¡Â»â€¡p vĂ¡Â»Â¥? | `PASS` |
| **Gate 13**| MĂ¡Â»Âi Association Ă„â€˜Ă¡Â»Âu cÄ‚Â³ giĂ¡ÂºÂ£i trÄ‚Â¬nh Business Flow rÄ‚Âµ rÄ‚Â ng? | `PASS` |
| **Gate 14**| KhÄ‚Â´ng cÄ‚Â³ Actor nÄ‚Â o Ă„â€˜Ă†Â°Ă¡Â»Â£c nĂ¡Â»â€˜i vÄ‚Â o Use Case chĂ¡Â»â€° vÄ‚Â¬ cÄ‚Â³ quyĂ¡Â»Ân Ă„â€˜Ă¡Â»Âc API? | `PASS` |
| **Gate 15**| KhÄ‚Â´ng cÄ‚Â³ Use Case nÄ‚Â o tĂ¡Â»â€œn tĂ¡ÂºÂ¡i chĂ¡Â»â€° Ă„â€˜Ă¡Â»Æ’ mÄ‚Â´ tĂ¡ÂºÂ£ implementation chi tiĂ¡ÂºÂ¿t? | `PASS` |

---

## FINAL STATUS

### **FINAL UML READY**

**KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n**: TĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ 15 Semantic Quality Gates Ă„â€˜Ă¡Â»Âu **PASS**. MÄ‚Â´ hÄ‚Â¬nh Use Case UML 2.5 Ă„â€˜Ä‚Â£ Ă„â€˜Ă¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ hoÄ‚Â n thiĂ¡Â»â€¡n tĂ¡Â»â€˜i Ă„â€˜a, trung thĂ¡Â»Â±c 100% vĂ¡Â»â€ºi mÄ‚Â£ nguĂ¡Â»â€œn dĂ¡Â»Â± Ä‚Â¡n hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i, loĂ¡ÂºÂ¡i bĂ¡Â»Â hoÄ‚Â n toÄ‚Â n nhiĂ¡Â»â€¦u kĂ¡Â»Â¹ thuĂ¡ÂºÂ­t vÄ‚Â  hoÄ‚Â n toÄ‚Â n sĂ¡ÂºÂµn sÄ‚Â ng Ă„â€˜Ă¡Â»Æ’ chuyĂ¡Â»Æ’n sang bĂ†Â°Ă¡Â»â€ºc xuĂ¡ÂºÂ¥t mÄ‚Â£ **Draw.io XML** cho tĂ¡Â»Â«ng hÄ‚Â¬nh sĂ†Â¡ Ă„â€˜Ă¡Â»â€œ (HÄ‚Â¬nh 3.1 $\rightarrow$ HÄ‚Â¬nh 3.7)!




