# Chủ quán (`admin`)

Route được cấp trong `ManagementRoutes.tsx`: toàn bộ management/operations và settings admin-only. Trang chủ đạo: dashboard, business insights, catalog, inventory, HR độc lập, POS, invoices, kitchen, settings/receipt, print templates, branches và profile.

Điều kiện: tài khoản `admin`, branch A/B, dữ liệu order/expense/menu/kho. Kiểm tra thêm quyền URL trực tiếp, branch switch, giá/tổng tiền backend, cấu hình và Mock AI confirmation. Case cũ `ADM-*` ở `00-LEGACY-CASES.md` là nguồn ID/kết quả; các file trang dưới đây là chỉ mục theo route.

Các trang đã bổ sung checklist riêng: [customers.md](customers.md), [support.md](support.md). Inventory/HR/settings subpage dùng case chung tại [../chung/inventory-hr-settings.md](../chung/inventory-hr-settings.md); bước chi tiết theo ID nằm trong file đó.
Câu hỏi test tay AI của chủ quán: [../ai-chat/01-chu-quan.md](../ai-chat/01-chu-quan.md) (`OWN-AI-01..54`). Các case kỹ thuật/API hoặc policy vẫn tham chiếu `../ky-thuat-runtime/README.md` và không được thay thế bằng chat UI.
