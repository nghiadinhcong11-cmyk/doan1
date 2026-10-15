# Kiểm chứng kỹ thuật/runtime

Nhóm này không phải checklist UI. Dùng khi cần Network/HTTP client, log đã lọc secret, nhiều phiên, schema/migration hoặc cấu hình runtime. Lượt này không chạy các kiểm tra đó.

| ID | Phương pháp | Phạm vi | Điều kiện | Kết quả cần lưu | Trạng thái |
|---|---|---|---|---|---|
| TECH-01 | API/HTTP client | auth, branch isolation, payment total, idempotency | API test riêng và token test đã cấp | request đã che secret, status/body, timestamp | Chưa test |
| TECH-02 | hai phiên/Network | SignalR reconnect, order/payment đồng thời | hai browser, dữ liệu riêng | event/status trước-sau, log correlation | Chưa test |
| TECH-03 | database read-only | constraint, transaction, migration/schema | database test riêng, quyền read-only | schema/query/log; không sửa DB trong checklist | Chưa test |
| TECH-04 | log/config | error handling, secret redaction, backup/restore readiness | môi trường được phép | file log đã lọc và version/config fingerprint | Chưa test |
| TECH-05 | build/typecheck | backend/frontend contract | dependency hiện có | lệnh, commit, output tóm tắt | Chưa test |

