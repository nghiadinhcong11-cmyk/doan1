# Realtime / SignalR

Hub: `/kitchenHub`, implemented by `KitchenHub` and registered in `Program.cs`.

| Event | Sender | Receiver | Trigger | Payload/purpose |
|---|---|---|---|---|
| `NewOrderRequest` | `KitchenNotifier` | `kitchen-admin` and `kitchen-branch:{branchId}` | New kitchen batch | Request/order/table/items payload |
| `RequestStatusUpdated` | `KitchenNotifier` | Admin and branch kitchen groups | Kitchen status update | `{ id, status }` |
| `PaymentCompleted` | `OrderController` | `Clients.All` | Successful payment | order/payment/branch summary |

Connections use JWT `access_token` query extraction for `/kitchenHub`. `KitchenHub.OnConnectedAsync` adds role, branch, branch+role and admin groups. The admin Kitchen page subscribes to the two kitchen events, uses automatic reconnect, refreshes after reconnect, and polls active requests every eight seconds as a recovery path. POS has kitchen status/realtime consumption evidence in `POSPage`.

The current notifier sends payment completion globally rather than branch-group scoped. The kitchen notifier is branch-aware for new/status events, but its overload without branch can fall back to `kitchen-admin`.
