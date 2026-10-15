# Dashboard, nghiệp vụ và branch scope

- Route: `/dashboard`, `/business-insights`, `/pos`, `/tables`, `/invoices`, `/reservations`, `/expenses`, `/customers`, `/products`, `/toppings`, `/promotions`, `/inventory/*`.
- Source: `ManagementRoutes.tsx`, manager navbar và các controllers/service branch-sensitive.
- Kiểm tra: dữ liệu chỉ Branch A, URL/query giả mạo Branch B, CRUD/idempotency expense, filter/report, payment dùng persisted total, loading/error/retry.
- Case: `MGR-01`–`MGR-05`, `MGR-08`, `MGR-12`–`MGR-16`.

