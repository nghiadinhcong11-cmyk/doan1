# 11. Frontend, UX, Mobile và Môi trường LAN

## 1. Công nghệ Frontend
- **Framework**: React 18 với TypeScript.
- **Styling**: TailwindCSS cho giao diện hiện đại và tùy biến nhanh.
- **Build Tool**: Vite cho tốc độ phát triển và tối ưu hóa bundle.

## 2. Thiết kế đáp ứng (Responsive Design)
Giao diện được thiết kế để hoạt động tốt trên đa thiết bị:
- **Desktop (1920x1080, 1366x768)**: Dành cho Admin và POS quản lý.
- **Tablet (768x1024)**: Tối ưu cho nhân viên phục vụ cầm tay.
- **Mobile (360x800, 390x844)**: Dành cho khách hàng gọi món qua QR và nhân viên kiểm tra lịch làm việc.

## 3. Trải nghiệm người dùng (UX)
- **Loading States**: Hiển thị spinner hoặc skeleton khi đang tải dữ liệu.
- **Real-time Feedback**: Hiển thị thông báo (toast) ngay lập tức khi bếp hoàn thành món hoặc có đơn hàng mới.
- **SignalR Connection Indicator**: Đèn báo trạng thái kết nối thời gian thực trên màn hình KDS và POS.
- **Empty States**: Giao diện thân thiện khi không có dữ liệu (Ví dụ: "Chưa có đơn hàng nào").

## 4. Hỗ trợ môi trường LAN (Local Area Network)
Hệ thống được tối ưu để triển khai trong mạng nội bộ nhà hàng:
- **Dynamic IP**: Hỗ trợ cấu hình linh hoạt theo địa chỉ IP của máy chủ LAN.
- **HTTPS nội bộ**: Đảm bảo các tính năng như Camera (để quét QR) hoạt động ổn định trên trình duyệt di động (yêu cầu HTTPS).
- **Offline Resilience**: Các thông báo lỗi rõ ràng khi mất kết nối mạng hoặc mất kết nối tới Server.

## 5. Đồng bộ hóa Types (Codegen)
Để đảm bảo tính nhất quán giữa Backend và Frontend, hệ thống sử dụng `openapi-typescript` để tự động sinh mã (Codegen):
- **Source**: `swagger.json` sinh ra từ XML Documentation của ASP.NET Core.
- **Output**: Các interface TypeScript được sinh tự động tại `packages/shared/types/api.ts`.
- **Lợi ích**: Giảm thiểu lỗi sai lệch kiểu dữ liệu (Type mismatch) và tăng tốc độ phát triển khi API thay đổi contract.

## 6. Các luồng Demo tiêu biểu
- **Admin**: Thống kê doanh thu, quản lý menu, xem log AI.
- **POS**: Quy trình tạo đơn, gửi bếp, thanh toán và in hóa đơn nhiệt.
- **Customer**: Quét mã QR, chọn món, thêm topping, gửi yêu cầu phục vụ.
- **Kitchen**: Tiếp nhận và xử lý món ăn trên KDS.
