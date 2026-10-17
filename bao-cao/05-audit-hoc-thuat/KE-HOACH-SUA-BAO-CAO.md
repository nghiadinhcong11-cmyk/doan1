# Kế hoạch audit và bàn giao dữ kiện báo cáo

## Phân công hiện hành (thay thế kế hoạch Prompt 0/1 trước đây)

Theo yêu cầu ngày 16/10/2026, công cụ này **chỉ audit kỹ thuật và tài liệu**. ChatGPT/người viết sẽ viết nội dung chính. Các chỉ dẫn cũ về viết lại toàn văn chương, tạo/cập nhật Word, render/QA trang, xử lý hình/caption/định dạng **không còn áp dụng cho công cụ audit**. Không tạo `CHUONG-N-DA-SUA.md`, không tạo sơ đồ/ảnh/biểu đồ/bìa, không sửa code, DB, dependency, migration. Những hiện vật Word và `CHUONG-1-DA-SUA.md` còn từ phân công trước được để nguyên; không coi là sản phẩm của lượt audit này.

Nguồn duy nhất cần đối chiếu: `D:/Dev/doan/bao-cao/hệ thống quản lý nhà hàng (ver3).docx` (SHA-256 `D26DCE6C0437D9E54AB66862E40888C3A2EF21CE565A0016379294A7AE11ABF9`). Thư mục đầu ra được phép: `D:/Dev/doan/bao-cao/05-audit-hoc-thuat/`. Prompt 0 đã hoàn thành; trước mỗi lượt chỉ kiểm lại HEAD/working tree và nguồn có thể đổi, không lặp khảo sát toàn repository. Giữ nguyên file gốc, không chạm bản Word sửa có sẵn.

## Quy ước hồ sơ

Mỗi `CHUONG-N-AUDIT.md` phải **tự đủ thông tin để gửi cho ChatGPT**: phiên bản/ngày/HEAD/working tree; đề cương nguyên gốc từ thân Word/numbering; ma trận từng mục với trích đoạn gốc, nhãn, dữ kiện cần sửa, bằng chứng `file:symbol:line` giải thích được nếu không mở repository, mức kiểm chứng; gói dữ kiện; AI tách hiện trạng/mục tiêu/chưa chốt; phát biểu không được giữ; câu hỏi thực sự còn thiếu; handoff nêu rõ mục đủ/chưa đủ. Đọc **mọi mục** trong chương, đánh dấu đã audit/chưa audit. Không chỉ đưa đường dẫn. Nội dung báo cáo chỉ dùng tên đề tài hoặc “hệ thống”, không dùng tên tự đặt trong code. Giữ cấu trúc và độ sâu gốc; chỉ đề nghị gộp/đổi/thêm mục khi có lý do cụ thể.

Mã checkout/DI/đường gọi chỉ chứng minh **hiện trạng tĩnh**; không chứng minh runtime/schema DB đã triển khai. Phân biệt entity CLR, EF mapping, snapshot, migration, DB; branch/role theo từng endpoint/service/tool/Hub; build, test, checklist và audit tĩnh. Không bịa số liệu, nguồn, log. Lượt audit này không chạy build/test/runtime, migration/seed, DB dùng chung hoặc AI trả phí. Trường `BasicSalary`/`EmployeeType` cần kiểm độc lập; tiền lương chỉ là định hướng tương lai, không kể lịch sử Payroll. Với AI, provider hiện hành là Mock theo DI; local sentence embedding là **thiết kế mục tiêu người dùng đã chọn**, thư mục spike không là bằng chứng tích hợp. Không tự chốt model/metric/ngưỡng hoặc gọi cosine là xác suất.

## Lượt audit tài liệu

1. **Chương 1:** `CHUONG-1-AUDIT.md` đã cập nhật theo phân công mới. Đủ dữ kiện về mục tiêu/nhiệm vụ/phạm vi/phương pháp/ý nghĩa, hai heading trùng, kho, AI, test; không viết lại chương.
2. **Chương 2:** `CHUONG-2-AUDIT.md` đã audit toàn bộ B267–558, đối chiếu manifest/lockfile/imports, mã nghiệp vụ, auth/SignalR/DB/AI, test và danh mục [1]–[17]; phân biệt LLM lý thuyết–Mock hiện trạng–embedding mục tiêu và test lịch sử. Không viết lại chương hoặc sửa Word.
3. **Chương 3 lượt 1:** yêu cầu, Use Case và QR/POS/KDS/thanh toán; quyền/chi nhánh theo đường cụ thể.
4. **Chương 3 lượt 2:** bảng dữ liệu đối chiếu entity/EF mapping/snapshot/migration, không suy DB thật; kho và nhân sự.
5. **Chương 3 lượt 3:** kiến trúc, SignalR producer/consumer, AI tool/quyền/xác nhận, UI; chỉ ghi mâu thuẫn caption/thuyết minh nếu ảnh hưởng nội dung, không sửa hình/đánh số.
6. **Chương 4:** cài đặt kho/AI/nhân sự, test và giới hạn đánh giá; không gọi 375 test lịch sử là hiện tại.
7. **Mở đầu, Kết luận, phụ lục:** đối chiếu tuyên bố nghiệp vụ/kỹ thuật với các chương; không audit định dạng/bìa/hình.

Mỗi lượt cập nhật `MA-TRAN-SAI-LECH.md` theo trạng thái **đã có dữ kiện audit** hay **còn chờ**, không nói “đã sửa Word/chương” nếu công cụ audit không thực hiện. Mọi quyết định cần tác giả chốt được ghi riêng, không cản các phần có đủ nguồn.
