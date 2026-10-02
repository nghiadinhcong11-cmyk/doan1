# Thesis Source of Truth

## System

Restaurant POS and management system for multi-branch restaurant operations. The repository contains a .NET 8 API, two React/Vite web applications, PostgreSQL persistence through EF Core/Npgsql, SignalR kitchen communication, and a Gemini-backed assistant.

## Actors

`admin`, `manager`, `employee`, `cashier`, `kitchen`, `customer`, plus guest access that receives a `customer` JWT. Role is used for authorization; employee position is descriptive.

## Implemented scope

Authentication/JWT; branch and employee management; products, toppings/options and promotion CRUD; tables/areas/reservations; POS order lifecycle; authoritative financial calculation; kitchen batch requests and KDS history; manual cash/bank-transfer payment recording; customer profile, reservations and loyalty; attendance, shifts and schedules; expenses; dashboard/business insights; receipt/system settings; notifications; AI chat and permissioned tools; SignalR kitchen events. Payroll is explicitly outside the current system scope.

## Architecture

Browser -> WebAPI controller -> application service or AI tool -> EF Core `ApplicationDbContext` -> PostgreSQL. SignalR is hosted in the API. AI orchestration calls Gemini and can call registered tools; tools are the action boundary.

## Financial rule

OrderService recalculates authoritative prices from database product/topping/option data. The sequence is subtotal, service fee rounded to VND precision, VAT on subtotal plus service fee rounded independently, then legacy discount, then total. Payment compares against persisted `Order.TotalAmount`; client totals are not authoritative.

## Realtime

`/kitchenHub` is JWT-protected. Kitchen requests are emitted as `NewOrderRequest`; status changes as `RequestStatusUpdated`. KDS reconnects and polls as a recovery mechanism. Payment completion currently uses a global SignalR broadcast and should be treated as a security finding.

## AI

The implementation is an assistant with Gemini function calling and role/risk-gated tools. It is not a general autonomous agent. Some tools read business data; write tools include price update, order status update and booking. Persistent conversation storage is not established.

## Limitations and development status

Promotion CRUD exists but end-to-end discount application is not evidenced. Inventory/ingredient management is not evidenced. QR data is client supplied and must rely on server validation. Catalog branch ownership is not consistently modeled. Production hosting and external deployment cannot be proven from this repository. The migration tree contains current-looking, top-level, and backup variants requiring operational confirmation.

Primary evidence: `services/api/Program.cs`; `src/Domain/Entities`; `src/Application/Services/OrderService.cs`; `src/Infrastructure/Persistence/ApplicationDbContext.cs`; `src/WebAPI/Controllers`; `src/WebAPI/Hubs`; `src/AI`; `apps/admin-web/src`; `apps/customer-web/src`.
