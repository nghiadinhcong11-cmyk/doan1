# 00. Tá»•ng quan há»‡ thá»‘ng Restaurant POS (DOAN POS)

## 1. Giá»›i thiá»‡u há»‡ thá»‘ng
DOAN POS lĂ  há»‡ thá»‘ng quáº£n lĂ½ nhĂ  hĂ ng vĂ  Ä‘iá»ƒm bĂ¡n hĂ ng (Point of Sale) hiá»‡n Ä‘áº¡i, há»— trá»£ váº­n hĂ nh Ä‘a chi nhĂ¡nh, tá»‘i Æ°u hĂ³a quy trĂ¬nh phá»¥c vá»¥ tá»« khĂ¢u Ä‘áº·t bĂ n, gá»i mĂ³n Ä‘áº¿n cháº¿ biáº¿n táº¡i báº¿p vĂ  thanh toĂ¡n. Há»‡ thá»‘ng tĂ­ch há»£p Trá»£ lĂ½ áº£o AI giĂºp quáº£n trá»‹ viĂªn theo dĂµi doanh thu vĂ  há»— trá»£ khĂ¡ch hĂ ng thá»±c hiá»‡n cĂ¡c nghiá»‡p vá»¥ qua ngĂ´n ngá»¯ tá»± nhiĂªn.

## 2. Má»¥c tiĂªu há»‡ thá»‘ng
- **Chuyá»ƒn Ä‘á»•i sá»‘ quy trĂ¬nh nhĂ  hĂ ng**: Thay tháº¿ ghi chĂ©p thá»§ cĂ´ng báº±ng há»‡ thá»‘ng quáº£n lĂ½ táº­p trung.
- **Tá»‘i Æ°u tráº£i nghiá»‡m khĂ¡ch hĂ ng**: Há»— trá»£ gá»i mĂ³n qua mĂ£ QR (Customer Web) vĂ  Ä‘áº·t bĂ n nhanh chĂ³ng.
- **Váº­n hĂ nh Ä‘a chi nhĂ¡nh**: Quáº£n lĂ½ táº­p trung nhiá»u cÆ¡ sá»Ÿ vá»›i cÆ¡ cháº¿ cĂ¡ch ly dá»¯ liá»‡u nghiĂªm ngáº·t.
- **á»¨ng dá»¥ng AI**: Trá»£ lĂ½ Mock AI deterministic vá»›i tool/function calling Ä‘Æ°á»£c kiá»ƒm soĂ¡t bá»Ÿi quyá»n server; khĂ´ng gá»i provider bĂªn ngoĂ i.

## 3. Äá»‘i tÆ°á»£ng sá»­ dá»¥ng
- **Quáº£n trá»‹ viĂªn (Admin)**: Quáº£n lĂ½ toĂ n há»‡ thá»‘ng, chi nhĂ¡nh, sáº£n pháº©m vĂ  thiáº¿t láº­p báº£o máº­t.
- **Quáº£n lĂ½ chi nhĂ¡nh (Manager)**: Theo dĂµi hoáº¡t Ä‘á»™ng, doanh thu vĂ  nhĂ¢n sá»± táº¡i cÆ¡ sá»Ÿ Ä‘Æ°á»£c gĂ¡n.
- **Thu ngĂ¢n (Cashier)**: Thá»±c hiá»‡n thanh toĂ¡n vĂ  quáº£n lĂ½ Ä‘Æ¡n hĂ ng táº¡i quáº§y.
- **NhĂ¢n viĂªn (Employee)**: Ghi mĂ³n, phá»¥c vá»¥ vĂ  kiá»ƒm tra lá»‹ch lĂ m viá»‡c.
- **NhĂ¢n viĂªn báº¿p (Kitchen)**: Tiáº¿p nháº­n yĂªu cáº§u cháº¿ biáº¿n qua há»‡ thá»‘ng KDS (Kitchen Display System).
- **KhĂ¡ch hĂ ng (Customer)**: Xem menu, gá»i mĂ³n qua QR vĂ  theo dĂµi tĂ­ch Ä‘iá»ƒm loyalty.

## 4. CĂ¡c module chĂ­nh
- **Admin/POS Web**: Quáº£n lĂ½ nghiá»‡p vá»¥ tá»•ng thá»ƒ vĂ  bĂ¡n hĂ ng táº¡i quáº§y.
- **Customer QR Ordering**: Giao diá»‡n cho khĂ¡ch hĂ ng gá»i mĂ³n táº¡i bĂ n.
- **Kitchen Display System (KDS)**: MĂ n hĂ¬nh hiá»ƒn thá»‹ cho báº¿p vá»›i cáº­p nháº­t thá»i gian thá»±c qua SignalR.
- **AI Assistant**: Trá»£ lĂ½ thĂ´ng minh há»— trá»£ truy váº¥n dá»¯ liá»‡u vĂ  thá»±c thi hĂ nh Ä‘á»™ng.

## 5. CĂ´ng nghá»‡ sá»­ dá»¥ng
- **Backend**: ASP.NET Core 8, C#, EF Core.
- **Database**: PostgreSQL (Supabase).
- **Frontend**: React, TypeScript, TailwindCSS, Vite.
- **Real-time**: SignalR.
- **AI hiá»‡n hĂ nh**: Mock AI local; cĂ¡c mĂ´ táº£ AI provider/external AI provider khĂ¡c trong tĂ i liá»‡u lĂ  thiáº¿t káº¿/lá»‹ch sá»­ cÅ©.
- **Security**: JWT, Npgsql (Identity), RBAC, IDOR Hardening.

## 6. Tráº¡ng thĂ¡i dá»± Ă¡n
Há»‡ thá»‘ng Ä‘Ă£ hoĂ n thĂ nh giai Ä‘oáº¡n **Production Hardening**, vÆ°á»£t qua 133 bĂ i kiá»ƒm thá»­ tá»± Ä‘á»™ng, Ä‘áº£m báº£o tĂ­nh toĂ n váº¹n dá»¯ liá»‡u tĂ i chĂ­nh vĂ  an toĂ n báº£o máº­t.


