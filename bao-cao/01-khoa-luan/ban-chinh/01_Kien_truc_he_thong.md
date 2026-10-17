# 01. Kiáº¿n trĂºc há»‡ thá»‘ng

Backend ASP.NET Core 8 Ä‘Æ°á»£c tá»• chá»©c theo Domain, Application, Infrastructure vĂ  WebAPI trong má»™t project. ÄĂ¢y lĂ  cáº¥u trĂºc phĂ¢n lá»›p theo nguyĂªn táº¯c Clean Architecture; repository khĂ´ng chá»©ng minh dependency enforcement nghiĂªm ngáº·t giá»¯a cĂ¡c project Ä‘á»™c láº­p.

Luá»“ng chĂ­nh: Web client â†’ WebAPI â†’ Application Service/AI Tool â†’ EF Core DbContext â†’ PostgreSQL.

Admin/POS Web vĂ  Customer Web lĂ  hai á»©ng dá»¥ng React/Vite Ä‘á»™c láº­p. SignalR `/kitchenHub` phá»¥c vá»¥ POS/KDS vĂ  cĂ³ polling phá»¥c há»“i. AI hiá»‡n lĂ  Mock AI Assistant deterministic vá»›i function/tool calling, khĂ´ng pháº£i autonomous agent.

Docker khĂ´ng thuá»™c runtime/deployment hiá»‡n táº¡i vĂ  Ä‘Ă£ Ä‘Æ°á»£c loáº¡i khá»i repository. PostgreSQL Ä‘Æ°á»£c cáº¥u hĂ¬nh bĂªn ngoĂ i qua connection string. removed staff-finance module khĂ´ng thuá»™c pháº¡m vi hiá»‡n táº¡i; Attendance, Shift vĂ  WorkSchedule váº«n Ä‘á»™c láº­p.


