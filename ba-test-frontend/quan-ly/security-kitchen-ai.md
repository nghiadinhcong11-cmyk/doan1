# Quyền, bếp/realtime và Mock AI

- Route: `/kitchen`, `/kitchen/history`, `/settings*`, `/branches`, `/print-templates`, `/profile`.
- Kiểm tra: manager không vượt branch/admin route; event SignalR đúng branch; Mock AI không gọi provider ngoài và tool bị giới hạn role/branch.
- Case: `MGR-07`, `MGR-09`–`MGR-11`; luồng chung `FLOW-05`, `FLOW-09`, `FLOW-14`.

