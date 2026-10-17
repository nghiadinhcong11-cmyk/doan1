# Kiểm kê cấu trúc `bao-cao/`

Ngày tái cấu trúc: 17/10/2026. Phạm vi thay đổi: chỉ `bao-cao/`.

## Before / After

| Chỉ số | Trước tái cấu trúc | Sau tái cấu trúc |
|---|---:|---:|
| File | 170 | 174 |
| Thư mục điều hướng cấp một có nội dung | 5 nhóm chính + root | 5 nhóm đánh số + root |
| Bộ khóa luận chính | `khoa-luan/` | `01-khoa-luan/ban-chinh/` |
| Tài liệu kỹ thuật | `tai-lieu-ky-thuat/` | `02-tai-lieu-ky-thuat/` |
| Hình ảnh | `hinh-anh/` | `04-hinh-anh-so-do/` |
| Audit học thuật | `audit-bao-cao-ver3/` | `05-audit-hoc-thuat/` |
| Lưu trữ | `archive/` | `99-luu-tru/` |

Bốn file tăng thêm là README điều hướng cho các vùng lớn. Không tạo thư mục `03-minh-chung/` vì checkout hiện tại không có artifact minh chứng độc lập; việc tạo thư mục rỗng sẽ làm sai trạng thái evidence.

## Cây điều hướng sau tái cấu trúc

```text
bao-cao/
├── 01-khoa-luan/
│   ├── ban-chinh/             # Word + chương Markdown 00–16
│   ├── chuong-4/              # verification/evidence/code map
│   ├── so-do-use-case/        # use-case source audit
│   └── phu-luc/               # diagram/source-of-truth audits
├── 02-tai-lieu-ky-thuat/
│   ├── ai-agent/              # 27 hồ sơ AI và baseline
│   └── system-audit/          # corpus audit kỹ thuật
├── 04-hinh-anh-so-do/         # 14 tài sản hình ảnh + README
├── 05-audit-hoc-thuat/        # 12 audit học thuật + README
├── 99-luu-tru/                # tài liệu lịch sử và migration archive
└── README.md
```

## Phân loại nội dung

| Nhóm | Nội dung | Quyết định |
|---|---|---|
| `01-khoa-luan/ban-chinh` | Word chính, chương 00–16, mục lục | KEEP |
| `01-khoa-luan/chuong-4` | E2E verification, evidence audit, module map | KEEP; 12 link evidence lịch sử thiếu = `MISSING_HISTORICAL_EVIDENCE` |
| `01-khoa-luan/so-do-use-case` | Use-case decomposition | KEEP |
| `01-khoa-luan/phu-luc` | Thesis/diagram source audit | KEEP |
| `02-tai-lieu-ky-thuat/ai-agent` | AI reports, E5 baseline, UAT, logging | KEEP; `AI-REPORTS-INDEX.md` là chỉ mục chính |
| `02-tai-lieu-ky-thuat/system-audit` | Architecture, API, DB, migration, security, frontend audits | KEEP; giữ corpus thống nhất để không mất provenance |
| `04-hinh-anh-so-do` | PNG/JPG/WEBP và tài sản hình ảnh | KEEP; không đổi tên |
| `05-audit-hoc-thuat` | Audit Word/source và ma trận sai lệch | KEEP; không trộn vào chương chính |
| `99-luu-tru` | Design history, generated output, legacy audits, migration history | ARCHIVE |

## Mapping chính

- `khoa-luan/00_*.md` … `16_*.md` → `01-khoa-luan/ban-chinh/`.
- `khoa-luan/README.md` → `01-khoa-luan/ban-chinh/README.md`.
- `khoa-luan/chapter-4/*` → `01-khoa-luan/chuong-4/`.
- `khoa-luan/use-case/*` → `01-khoa-luan/so-do-use-case/`.
- `khoa-luan/THESIS-*.md`, `UML_Activity_Sequence_Audit.md` → `01-khoa-luan/phu-luc/`.
- `hệ thống quản lý nhà hàng (ver3).docx` → `01-khoa-luan/ban-chinh/`.
- `tai-lieu-ky-thuat/AI-*` → `02-tai-lieu-ky-thuat/ai-agent/`.
- `tai-lieu-ky-thuat/system-audit/*` → `02-tai-lieu-ky-thuat/system-audit/`.
- `audit-bao-cao-ver3/*` → `05-audit-hoc-thuat/`.
- `hinh-anh/*` → `04-hinh-anh-so-do/`.
- `archive/*` → `99-luu-tru/`.

## Review còn lại

- 12 liên kết E2E lịch sử vẫn trỏ tới `minh-chung/e2e/` không có trong checkout; không tạo artifact thay thế.
- Các tham chiếu ngoài `bao-cao/` chưa được sửa theo phạm vi nhiệm vụ; cần xử lý riêng nếu muốn cập nhật toàn repository.
- Word/PDF chưa được render lại trong lượt này; cần review trực quan trước khi phát hành.
