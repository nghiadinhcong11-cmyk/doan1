# 00. Tổng quan hệ thống Restaurant POS (DOAN POS)

## 1. Giới thiệu hệ thống
DOAN POS là hệ thống quản lý nhà hàng và điểm bán hàng (Point of Sale) hiện đại, hỗ trợ vận hành đa chi nhánh, tối ưu hóa quy trình phục vụ từ khâu đặt bàn, gọi món đến chế biến tại bếp và thanh toán. Hệ thống tích hợp Trợ lý ảo AI giúp quản trị viên theo dõi doanh thu và hỗ trợ khách hàng thực hiện các nghiệp vụ qua ngôn ngữ tự nhiên.

## 2. Mục tiêu hệ thống
- **Chuyển đổi số quy trình nhà hàng**: Thay thế ghi chép thủ công bằng hệ thống quản lý tập trung.
- **Tối ưu trải nghiệm khách hàng**: Hỗ trợ gọi món qua mã QR (Customer Web) và đặt bàn nhanh chóng.
- **Vận hành đa chi nhánh**: Quản lý tập trung nhiều cơ sở với cơ chế cách ly dữ liệu nghiêm ngặt.
- **Ứng dụng AI**: Tích hợp công nghệ AI (Google Gemini) để cung cấp thông tin thông minh và tự động hóa các tác vụ.

## 3. Đối tượng sử dụng
- **Quản trị viên (Admin)**: Quản lý toàn hệ thống, chi nhánh, sản phẩm và thiết lập bảo mật.
- **Quản lý chi nhánh (Manager)**: Theo dõi hoạt động, doanh thu và nhân sự tại cơ sở được gán.
- **Thu ngân (Cashier)**: Thực hiện thanh toán và quản lý đơn hàng tại quầy.
- **Nhân viên (Employee)**: Ghi món, phục vụ và kiểm tra lịch làm việc.
- **Nhân viên bếp (Kitchen)**: Tiếp nhận yêu cầu chế biến qua hệ thống KDS (Kitchen Display System).
- **Khách hàng (Customer)**: Xem menu, gọi món qua QR và theo dõi tích điểm loyalty.

## 4. Các module chính
- **Admin/POS Web**: Quản lý nghiệp vụ tổng thể và bán hàng tại quầy.
- **Customer QR Ordering**: Giao diện cho khách hàng gọi món tại bàn.
- **Kitchen Display System (KDS)**: Màn hình hiển thị cho bếp với cập nhật thời gian thực qua SignalR.
- **AI Assistant**: Trợ lý thông minh hỗ trợ truy vấn dữ liệu và thực thi hành động.

## 5. Công nghệ sử dụng
- **Backend**: ASP.NET Core 8, C#, EF Core.
- **Database**: PostgreSQL (Supabase).
- **Frontend**: React, TypeScript, TailwindCSS, Vite.
- **Real-time**: SignalR.
- **AI**: Google Gemini Pro (Gemini 1.5 Flash/Pro).
- **Security**: JWT, Npgsql (Identity), RBAC, IDOR Hardening.

## 6. Trạng thái dự án
Hệ thống đã hoàn thành giai đoạn **Production Hardening**, vượt qua 133 bài kiểm thử tự động, đảm bảo tính toàn vẹn dữ liệu tài chính và an toàn bảo mật.
