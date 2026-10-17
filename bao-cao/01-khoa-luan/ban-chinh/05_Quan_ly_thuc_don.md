# 05. Quản lý thực đơn (Catalog)

## 1. Mục tiêu module
Quản lý danh mục món ăn, giá bán, các tùy chọn đi kèm (Toppings/Sizes) và hình ảnh sản phẩm.

## 2. Chức năng chính
- Quản lý danh mục (Categories).
- Quản lý sản phẩm (Products): Tên, mô tả, giá gốc, hình ảnh.
- Quản lý tùy chọn: Thêm Topping, kích cỡ (Small/Medium/Large) với giá chênh lệch tương ứng.
- AI Assistant: Hỗ trợ khách hàng tra cứu món ăn và giá cả qua Tool `customer_get_menu`.

## 3. Business Logic
- **Giá bán**: Hệ thống hỗ trợ giá linh hoạt theo kích cỡ. Tổng giá món = `Giá cơ bản + Giá size + Tổng giá toppings`.
- **Validation**: Kiểm tra tính hợp lệ của hình ảnh sản phẩm (định dạng, kích thước) khi tải lên.

## 4. API chính
- `GET /api/Product`: Lấy danh sách món ăn (có hỗ trợ lọc theo danh mục).
- `POST /api/Product`: Thêm món mới (Chỉ dành cho Admin).
- `PUT /api/Product/update-price`: Cập nhật nhanh giá sản phẩm.

## 5. Dữ liệu (Entities)
- **Product**: `Id`, `Name`, `Price`, `Category`, `Image`, `ToppingsJson`, `SizesJson`.
- **Topping**: Danh sách các topping có sẵn trong hệ thống.

## 6. Bảo mật
- Quyền sửa thực đơn và giá cả chỉ dành cho role `admin`.
- AI Assistant (Admin role) có thể đổi giá món qua Tool `update_product_price` với cơ chế xác thực backend nghiêm ngặt.
