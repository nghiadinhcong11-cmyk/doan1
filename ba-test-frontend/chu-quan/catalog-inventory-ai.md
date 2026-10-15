# Catalog, inventory và Mock AI

- Route: `/products`, `/toppings`, `/promotions`, `/inventory/*` và chatbot trên layout.
- Source: catalog/inventory pages, `AiController`, `MockAiProvider`, `AiOrchestrator`, registry, permission và confirmation store.
- Kiểm tra: CRUD được phép, dữ liệu rỗng/lỗi, giá authoritative từ backend, tool đúng role/branch, write tool dừng ở confirmation.
- Case liên quan: `ADM-04`, `ADM-09`, `ADM-12`, `FLOW-09`, `FLOW-14`.

