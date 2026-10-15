# Kho tài liệu báo cáo

Thư mục này gom các tài liệu phục vụ viết báo cáo đồ án/khóa luận, nguồn kỹ thuật, hình ảnh và minh chứng. Việc gom chỉ thay đổi vị trí file; nội dung học thuật và trạng thái PASS/FAIL/NOT RUN được giữ nguyên.

## Cấu trúc

- [`do-an/`](do-an/) — bộ tài liệu hiện hành phục vụ báo cáo đồ án; bắt đầu tại [`do-an/README.md`](do-an/README.md). Các tài liệu Chương 4 nằm trong [`do-an/chapter-4/`](do-an/chapter-4/).
- [`khoa-luan/`](khoa-luan/) — các audit/source-of-truth phục vụ hiệu chỉnh khóa luận. Đây chưa được xác nhận là bản khóa luận cuối cùng.
- [`tai-lieu-ky-thuat/`](tai-lieu-ky-thuat/) — tài liệu kỹ thuật làm nguồn đối chiếu, đặc biệt [`system-audit/00-README.md`](tai-lieu-ky-thuat/system-audit/00-README.md), [`system-audit/15-THESIS-SOURCE-OF-TRUTH.md`](tai-lieu-ky-thuat/system-audit/15-THESIS-SOURCE-OF-TRUTH.md) và [`system-audit/10-AI-INTEGRATION.md`](tai-lieu-ky-thuat/system-audit/10-AI-INTEGRATION.md).
- [`hinh-anh/`](hinh-anh/) — ảnh, nguồn sơ đồ draw.io và tài sản hình ảnh phục vụ báo cáo. Nguồn sơ đồ được giữ cạnh nhóm hình ảnh.
- [`minh-chung/`](minh-chung/) — kết quả E2E, kiểm tra build/runtime, đánh giá Mock AI, branch isolation và các minh chứng liên quan. Đây là evidence, không tự động đồng nghĩa với kết quả hiện hành.
- [`archive/`](archive/) — tài liệu lịch sử, thiết kế cũ, output cũ và migration lịch sử đã được phân loại trước đó; không dùng làm source of truth hiện hành.

## Phạm vi chưa xác định

Các file trong `khoa-luan/` là audit và nguồn đối chiếu, không phải bản cuối của luận văn. Các kết quả trong `minh-chung/` giữ nguyên timestamp/trạng thái nguồn; cần đọc nhãn trong từng report trước khi trích dẫn. `evidence/ai-diagnostics/` và log TLS vẫn ở ngoài kho này vì script/runtime hiện tại còn ghi trực tiếp vào đó.
