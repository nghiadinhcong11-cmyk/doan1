# Bộ checklist kiểm thử thủ công

Khi test giao diện, chỉ cần mở file đúng role bên dưới. Mỗi dòng là một checklist manual; ghi `PASS`, `FAIL` hoặc `BLOCKED` và thêm ghi chú ngắn nếu có lỗi.

- [01 — Chủ quán / admin](01-chu-quan.md)
- [02 — Quản lý / manager](02-quan-ly.md)
- [03 — Bếp / kitchen](03-bep.md)
- [04 — Nhân viên / employee](04-nhan-vien.md)
- [05 — Thu ngân / cashier](05-thu-ngan.md)
- [06 — Khách đăng nhập / customer](06-khach-dang-nhap.md)
- [07 — Khách QR / guest](07-khach-chua-dang-nhap.md)

Phần AI được tách riêng tại [ai-chat](ai-chat/README.md). Các thư mục và ma trận cũ giữ lại làm tài liệu tham chiếu, không phải luồng test chính.

## Quy ước ghi kết quả

- `PASS`: đã chạy trên web/API thực tế và đúng kết quả mong đợi.
- `FAIL`: đã chạy nhưng kết quả sai; ghi bước tái hiện và ảnh/log nếu có.
- `BLOCKED`: chưa thể chạy do thiếu tài khoản, dữ liệu, API hoặc môi trường.

