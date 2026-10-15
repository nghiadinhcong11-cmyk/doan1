# Operations và settings admin

- Route: `/pos`, `/tables`, `/invoices`, `/reservations`, `/kitchen`, `/kitchen/history`, `/employees`, `/attendance`, `/schedule`, `/shifts`, `/settings`, `/settings/receipt`, `/print-templates`, `/branches`, `/profile`.
- Source: `ManagementRoutes.tsx`, các page tương ứng và controllers API.
- Kiểm tra: quyền admin, branch context, forms/modal/tab, validation/empty/error/retry, persisted order totals, cấu hình receipt/system, URL trực tiếp và reload.
- Case liên quan: `ADM-03`–`ADM-20`; không coi đọc code/build là PASS.

