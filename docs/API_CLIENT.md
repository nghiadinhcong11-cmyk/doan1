# OpenAPI và typed client

Swagger của API được sinh tại `/swagger/v1/swagger.json`. Các controller và model
được bật XML documentation trong `RestaurantPOS.api.csproj`; Swagger đọc file XML
này trong `Program.cs` để hiển thị mô tả endpoint và schema.

## Sinh type cho frontend

Từ thư mục gốc:

```bash
npm run codegen
```

Lệnh trên yêu cầu API đang chạy tại `http://localhost:5000`. Khi API chưa chạy,
có thể sinh từ snapshot đã lưu:

```bash
npm run codegen:file
```

Type được ghi vào `packages/shared/types/api.ts`. Không chỉnh sửa trực tiếp file
này; sau khi thay đổi contract hoặc XML comments, hãy cập nhật
`packages/shared/swagger.json` rồi chạy lại codegen. CI có thể dùng
`npm run codegen:check` để phát hiện type sinh ra bị stale.

`openapi-typescript` được chọn vì frontend hiện cần type-only client và package
đã có sẵn. Nếu cần sinh cả hooks/query client hoặc SDK gọi API, có thể chuyển sang
Orval trong một bước riêng; không nên chạy đồng thời hai generator cho cùng một
output.
