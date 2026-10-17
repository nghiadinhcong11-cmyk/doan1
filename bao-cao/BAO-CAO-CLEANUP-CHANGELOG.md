# Changelog tái cấu trúc information architecture

Ngày: 17/10/2026.

## Mapping old path → new path

| Old path | New path | Lý do |
|---|---|---|
| `bao-cao/khoa-luan/00_*.md` … `16_*.md` | `bao-cao/01-khoa-luan/ban-chinh/` | Một nơi duy nhất cho nội dung chương đang dùng |
| `bao-cao/khoa-luan/README.md` | `bao-cao/01-khoa-luan/ban-chinh/README.md` | Giữ README gắn với bộ chương |
| `bao-cao/khoa-luan/chapter-4/` | `bao-cao/01-khoa-luan/chuong-4/` | Điều hướng tiếng Việt, đúng nhóm phụ lục kiểm thử |
| `bao-cao/khoa-luan/use-case/` | `bao-cao/01-khoa-luan/so-do-use-case/` | Tách use case khỏi chương chính |
| `bao-cao/khoa-luan/THESIS-*.md`, `UML_Activity_Sequence_Audit.md` | `bao-cao/01-khoa-luan/phu-luc/` | Audit nguồn/sơ đồ không phải chương chính |
| `bao-cao/hệ thống quản lý nhà hàng (ver3).docx` | `bao-cao/01-khoa-luan/ban-chinh/` | Đặt bản Word cạnh bộ báo cáo chính |
| `bao-cao/tai-lieu-ky-thuat/AI-*` | `bao-cao/02-tai-lieu-ky-thuat/ai-agent/` | Tập trung AI Agent và baseline |
| `bao-cao/tai-lieu-ky-thuat/system-audit/` | `bao-cao/02-tai-lieu-ky-thuat/system-audit/` | Giữ corpus audit kỹ thuật nhất quán |
| `bao-cao/audit-bao-cao-ver3/` | `bao-cao/05-audit-hoc-thuat/` | Tách audit học thuật khỏi source chương |
| `bao-cao/hinh-anh/` | `bao-cao/04-hinh-anh-so-do/` | Tên nhóm thể hiện cả ảnh và sơ đồ |
| `bao-cao/archive/` | `bao-cao/99-luu-tru/` | Đặt archive ở cuối danh sách điều hướng |

## Nội dung merge/delete

- Không merge nội dung học thuật giữa các chương; chỉ di chuyển theo nhóm để giữ nguyên provenance.
- Không xóa file báo cáo, baseline, dataset, model reference hoặc evidence.
- Các thư mục cũ sau khi đã rỗng được xóa: `khoa-luan/`, `tai-lieu-ky-thuat/`, `audit-bao-cao-ver3/`, `hinh-anh/`, `archive/`.
- Không tạo `03-minh-chung/` vì không có artifact minh chứng hiện hữu.
- Tạo README điều hướng cho root, khóa luận, kỹ thuật, hình ảnh, audit học thuật và archive.

## Link/reference handling

- Đã cập nhật liên kết Markdown trong phạm vi `bao-cao/` theo mapping trên.
- Đã cập nhật đường dẫn source trong hồ sơ HTTPS và các tham chiếu AI/chương 4 bị ảnh hưởng.
- 12 liên kết E2E lịch sử tới `minh-chung/e2e/` vẫn thiếu từ trước; giữ nguyên và ghi `MISSING_HISTORICAL_EVIDENCE`.
- Tham chiếu ngoài `bao-cao/` không được sửa theo ràng buộc; cần xử lý riêng nếu muốn đồng bộ toàn repository.

## Git/evidence safety

Tất cả thay đổi của lượt này nằm trong `bao-cao/`. Nội dung file được giữ nguyên ngoại trừ các path reference và README điều hướng. Không chạy database, migration, API, browser hoặc build backend/frontend.
