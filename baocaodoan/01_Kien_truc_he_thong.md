# 01. Kiến trúc hệ thống

Backend ASP.NET Core 8 được tổ chức theo Domain, Application, Infrastructure và WebAPI trong một project. Đây là cấu trúc phân lớp theo nguyên tắc Clean Architecture; repository không chứng minh dependency enforcement nghiêm ngặt giữa các project độc lập.

Luồng chính: Web client → WebAPI → Application Service/AI Tool → EF Core DbContext → PostgreSQL.

Admin/POS Web và Customer Web là hai ứng dụng React/Vite độc lập. SignalR `/kitchenHub` phục vụ POS/KDS và có polling phục hồi. AI là Gemini-powered Assistant với function/tool calling, không phải autonomous agent.

Docker không thuộc runtime/deployment hiện tại và đã được loại khỏi repository. PostgreSQL được cấu hình bên ngoài qua connection string. Payroll không thuộc phạm vi hiện tại; Attendance, Shift và WorkSchedule vẫn độc lập.
