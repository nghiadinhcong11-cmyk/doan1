# Quyền, bếp/realtime và Mock AI

- Route: `/kitchen`, `/kitchen/history`, `/settings*`, `/branches`, `/print-templates`, `/profile`.
- Kiểm tra: manager không vượt branch/admin route; event SignalR đúng branch; Mock AI không gọi provider ngoài và tool bị giới hạn role/branch.
- Bộ câu hỏi UI chính: [`../ai-chat/01-chu-quan.md`](../ai-chat/01-chu-quan.md); kiểm tra API/log/multi-session: [`../ky-thuat-runtime/README.md`](../ky-thuat-runtime/README.md). Không duy trì checklist AI thứ hai tại tài liệu này.
- Case: `MGR-07`, `MGR-09`–`MGR-11`; luồng chung `FLOW-05`, `FLOW-09`, `FLOW-14`.

