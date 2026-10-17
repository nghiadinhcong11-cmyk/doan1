# Catalog, inventory và Mock AI

- Route: `/products`, `/toppings`, `/promotions`, `/inventory/*` và chatbot trên layout.
- Source: catalog/inventory pages, `AiController`, `MockAiProvider`, `AiOrchestrator`, registry, permission và confirmation store.
- Kiểm tra: CRUD được phép, dữ liệu rỗng/lỗi, giá authoritative từ backend, tool đúng role/branch, write tool dừng ở confirmation.
- Case chức năng hiện hành: [`../ai-chat/01-chu-quan.md`](../ai-chat/01-chu-quan.md). ID lịch sử `ADM-12` được ánh xạ trong README bộ chính; `FLOW-09` và `FLOW-14` vẫn ở checklist liên vai trò/kỹ thuật.

