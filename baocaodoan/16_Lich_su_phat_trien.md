# 16. Lịch sử phát triển và cải tiến hệ thống

Backend được tổ chức theo các lớp Domain/Application/Infrastructure/WebAPI trong một project. POS và Kitchen hỗ trợ gửi món theo batch bằng `SentQuantity`, SignalR và polling phục hồi. JWT, RBAC, branch checks và AI tool permissions được hardening theo từng luồng; không nên diễn giải là bảo đảm tuyệt đối cho mọi dữ liệu.

Docker đã bị loại khỏi current repository và không còn là deployment hiện tại. Payroll đã bị loại khỏi current system scope; migration/archive history có thể còn dấu vết lịch sử và không được xem là active feature. Kết quả build/test phải được ghi theo lần chạy thực tế.
