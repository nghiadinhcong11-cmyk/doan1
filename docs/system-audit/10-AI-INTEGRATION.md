# AI Integration

The system is an AI assistant with tool/function calling, not merely a static chatbot. `AiController` accepts `POST /api/Ai/chat`; `AiOrchestrator` builds role-specific prompts, sends conversation history and tool declarations to Gemini, executes requested tools sequentially, feeds results back, and limits iterations. Conversation history is supplied in the request model; no persistent conversation repository/table was verified.

`AiPermissionService` checks tool allowed role and risk level through `AiAuthorization`. Tools declare `Name`, schema, `AllowedRoles`, `RiskLevel` and `ExecuteAsync`. Registered tools include revenue, active staff, order list, best sellers, revenue comparison, business summary, financial analysis, product price update, table summary, shift lookup, order status update, menu, customer orders and booking.

Read tools dominate. Write tools evidenced include product price update, order status update and customer booking. Tools validate arguments and are intended to call application services. Branch filtering is implemented variably in tool/service code; the permission gate itself primarily checks role/risk, so branch isolation requires tool-by-tool verification.

Provider evidence is `GeminiService`, which reads `Gemini:ApiKey` and `Gemini:Model` and calls the Google Generative Language API. The repository currently contains a plaintext-looking API key in `services/api/appsettings.json`; this document intentionally redacts it. Rotate/remove it outside the audit if it is real.
