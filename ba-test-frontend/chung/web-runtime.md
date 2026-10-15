# Web runtime dùng chung

**Source:** `scripts/start-dev.ps1`, `apps/admin-web/src/config.ts`, `apps/customer-web/src/config.ts`, `services/api/src/WebAPI/Program.cs`.

| ID | Phạm vi | Bước | Kết quả mong đợi | Trạng thái | Thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|
| NET-01 | localhost HTTPS | Chạy launcher local; mở API/Admin/Customer theo cổng cấu hình | HTTPS mở được, không mixed content, API không tự migration/seed | Chưa test | | |
| NET-02 | LAN/QR/SignalR | Mở customer và admin/kitchen từ thiết bị được phép; quét QR | API base URL, QR context và hub dùng cùng host/protocol đúng | Chưa test | | |
| NET-03 | certificate/host thay đổi | Dùng host đã có trong SAN; kiểm tra QR mới | Không hạ HTTPS hoặc validation để né lỗi | Chưa test | | |
| NET-04 | nhiều adapter | Chọn rõ host LAN rồi mở từ thiết bị test | Không chọn nhầm adapter/IP | Chưa test | | |
| NET-05 | lỗi trust/firewall | Tách từng nguyên nhân và quan sát lỗi | Phân loại đúng, không dùng CORS wildcard/HTTP workaround | Chưa test | | |

