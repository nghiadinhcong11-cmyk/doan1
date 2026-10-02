# 15. Quản lý chi phí (Expense Management)

Expense ghi nhận chi phí vận hành theo branch, ngày, danh mục, mô tả và người tạo. Module không phải Payroll và không tự động tính lương nhân viên.

Các endpoint chính: `GET/POST/PUT/DELETE /api/Expense` với kiểm tra quyền và branch ở backend. Business Insight có thể đưa ra ước tính từ doanh thu, Expense và CostPrice; đây không phải báo cáo kế toán COGS đầy đủ vì inventory/recipe accounting chưa được chứng minh.
