# 12. Kiểm thử và đảm bảo chất lượng

Backend dùng xUnit; test bao phủ order/financial rules, authorization, branch isolation, customer IDOR, AI security và service flows. Test database dùng SQLite in-memory, không phụ thuộc Docker/PostgreSQL container.

Các lệnh kiểm tra: `dotnet build services/api/RestaurantPOS.api.csproj`, `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj`, và `npm run build` tại từng frontend. Kết quả phải ghi kèm ngày và command thực tế.

Payroll tests/residuals đã được loại bỏ vì Payroll không còn thuộc phạm vi. Attendance và Shift là các chức năng độc lập và không bị loại bỏ.
