# Bộ báo cáo khóa luận — Restaurant POS

Đây là bộ báo cáo chính duy nhất trong `bao-cao/`. Nội dung được hợp nhất từ bộ tài liệu trước đây ở `do-an/` và các tài liệu source-of-truth đã có trong `khoa-luan/`.

## Nội dung

- Các chương tổng quan đến lịch sử phát triển: `00_*.md` đến `16_*.md`.
- Audit triển khai và kiểm thử Chương 4: [`../chuong-4/`](../chuong-4/).
- Phân rã use case và đối chiếu source: [`../so-do-use-case/`](../so-do-use-case/).
- Audit nguồn sơ đồ và source-of-truth luận văn: `THESIS-*.md`.

## Nguyên tắc cập nhật

Source code hiện tại và [`../../02-tai-lieu-ky-thuat/`](../../02-tai-lieu-ky-thuat/) là căn cứ kỹ thuật. Các phần mô tả phải ghi rõ IMPLEMENTED, PARTIAL hoặc PLANNED khi chức năng chưa được xác minh đầy đủ.

AI hiện tại không phải một provider bên ngoài tổng quát. MockAiProvider vẫn tồn tại; Sentence Embedding E5 chỉ được triển khai giới hạn cho pilot manager với hai intent `REVENUE_QUERY` và `BUSINESS_SUMMARY`. Không mô tả pilot này là triển khai cho toàn bộ role hoặc toàn hệ thống.

Các kết quả lịch sử trong chương 4 và audit được giữ nguyên để truy vết. Liên kết tới bằng chứng đã thất lạc được ghi nhận trong báo cáo kiểm kê, không tự tạo lại dữ liệu.
