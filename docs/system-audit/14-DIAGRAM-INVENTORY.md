# Diagram Inventory

Recommended diagrams grounded in current implementation:

1. System context/use-case overview: admin, manager, cashier, employee, kitchen, customer and guest around the two web apps/API.
2. POS order sequence: POS -> OrderController -> OrderService -> DbContext -> response.
3. POS-to-kitchen sequence: send-to-kitchen -> SentQuantity delta -> OrderRequest -> KitchenNotifier -> KDS.
4. Payment sequence: POS -> payment endpoint -> persisted total check -> conditional update -> completion event.
5. QR ordering activity/sequence: scan -> table lookup -> menu -> cart -> order -> kitchen.
6. AI tool-calling sequence: client -> AiController -> orchestrator -> Gemini -> permission -> tool -> application service.
7. ERD: Branch/order/customer/kitchen/HRM/settings/loyalty relationships from `07-DATABASE.md`.
8. Backend architecture/dependency diagram from `03-ARCHITECTURE.md`.
9. Deployment diagram: browser apps, API HTTPS 5000, PostgreSQL/Supabase, Gemini, certificate/config.
10. SignalR group/event diagram from `09-REALTIME-SIGNALR.md`.

Do not draw inventory, external automated payment settlement, or persistent AI conversation diagrams as implemented because current source does not establish those flows.
