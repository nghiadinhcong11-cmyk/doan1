# AI Proactive Business Alerts & Insights Design Documentation

## 1. Executive Summary
This document outlines the design for "AI Proactive Business Alerts & Insights" (Task 23). The system will transition from a reactive "ask-and-answer" assistant to a proactive partner that automatically detects business anomalies, generates insights, and alerts relevant stakeholders.

## 2. Current Architecture & Audit Findings
*   **AI Level**: Level 3 (Tool-Using Business Assistant).
*   **Gemini Model**: `gemini-3.7-flash`.
*   **Data Source**: `DashboardService` and `FinancialAnalysisService` provide validated KPIs and rule-based anomaly detection.
*   **Notifications**: A basic `Notification` entity exists in `Domain/Entities/Notification.cs`.
*   **Scheduler**: **MISSING**. No background execution framework (Hangfire, Quartz, or native BackgroundService) is currently configured.
*   **SignalR**: `KitchenHub` exists but is focused on order preparation.

## 3. Proposed Insights & Detection Rules
Insights will be detected using deterministic rules (reusing `FinancialAnalysisService` logic) before being passed to Gemini for interpretation.

| Insight Type | Threshold | Data Source | Severity |
| :--- | :--- | :--- | :--- |
| **Revenue Drop** | <= -20% | FinancialAnalysisService | Critical/Warning |
| **Revenue Spike** | >= +20% | FinancialAnalysisService | Info |
| **Expense Spike** | >= +50% (by category) | FinancialAnalysisService | Warning |
| **Profit Pressure** | Rev stable/up AND Profit -15% | FinancialAnalysisService | Warning |
| **AOV Anomaly** | +/- 20% | FinancialAnalysisService | Warning |
| **Operational Risk**| High volume + Low staff | OrderService + Attendance | Info |

## 4. Proactive System Architecture

### 4.1. Background Execution
*   **Technology**: ASP.NET Core `BackgroundService` (IHostedService).
*   **Schedule**: 
    *   **Daily Job**: Runs at 00:05 (VN Time) to analyze "Yesterday vs Day Before".
    *   **Hourly Job**: Runs to detect intra-day spikes if needed (Phase 2).
*   **Multi-Instance Safety**: In a multi-instance environment, a "Distributed Lock" or a database-backed "Job Execution Log" will be required to prevent duplicate alerts.

### 4.2. Insight Data Model (Proposed Entity)
`BusinessInsight`:
*   `Id` (Guid)
*   `BranchId` (Guid)
*   `Type` (e.g., `RevenueSignificantDrop`)
*   `Severity` (Info, Warning, Critical)
*   `Title` (string)
*   `Summary` (string) - *Deterministic base message*
*   `AiExplanation` (string) - *Populated by Gemini*
*   `AiRecommendation` (string) - *Populated by Gemini*
*   `EvidenceJson` (string) - *Stores the KPI snapshot at time of detection*
*   `DeduplicationKey` (string) - `BranchId:Type:Period` (e.g., `B1:RevDrop:2026-09-10`)
*   `IsRead` (bool)
*   `CreatedAt` (DateTime)

### 4.3. Gemini's Role
Gemini acts as an **Interpreter**, not a detector.
1.  System detects `RevenueSignificantDrop (-25%)`.
2.  System gathers context: `OrderCount (-30%)`, `AOV (+5%)`, `TopProducts`.
3.  System prompts Gemini: "Explain this 25% revenue drop given these metrics and suggest 3 recommendations."
4.  Gemini generates the `AiExplanation` and `AiRecommendation`.

## 5. Security & Isolation
*   **Branch Isolation**: Proactive jobs will iterate through active branches. Each insight is scoped to a specific `BranchId`.
*   **RBAC**: 
    *   `Admin`: Receives global/cross-branch insights.
    *   `Manager`: Receives insights for their own branch only.
    *   `Other Roles`: Denied financial insights.
*   **PII Protection**: Insights will never include customer names, phone numbers, or employee personal data.

## 6. Notification Flow
1.  **Detection**: `InsightBackgroundService` detects anomaly via `IFinancialAnalysisService`.
2.  **Creation**: `BusinessInsight` record is created (if not a duplicate).
3.  **AI Interpretation**: System calls `IAiOrchestrator` to populate AI fields.
4.  **Distribution**:
    *   Create a record in the `Notifications` table.
    *   Broadcast via SignalR (Targeted to `Branch:{id}` or `Role:admin` groups).

## 7. Performance Considerations
*   **Aggregation**: Reuse `DashboardService` cache where possible.
*   **Batching**: Jobs will process branches in batches to avoid database spikes.
*   **API Quota**: Gemini calls will be rate-limited to stay within the 15 req/min `ai-limiter` or tier limits.

## 8. Testing Strategy
*   **Detection Tests**: Mock `IDashboardService` with various KPI scenarios to verify correct anomaly flags.
*   **Deduplication Tests**: Verify that multiple job runs within the same period do not create duplicate insights.
*   **RBAC Tests**: Ensure `manager_A` cannot access `branch_B` insights via API.
*   **Timezone Tests**: Verify Vietnam 00:00 boundary correctly maps to UTC queries.

## 9. Feasibility Matrix

| Feature | Data Exists | Infrastructure | Minimal Code | Risk | Recommendation |
| :--- | :---: | :---: | :---: | :---: | :--- |
| Detection Rules | **A** | **A** | Yes | Low | Reuse FinancialAnalysisService. |
| Background Jobs | **B** | **C** | No | Medium| Use native BackgroundService. |
| Persistence | **B** | **B** | Yes | Low | Create BusinessInsight entity. |
| SignalR Alert | **B** | **B** | Yes | Low | Extend KitchenHub or new ManagementHub. |
| AI Reasoning | **A** | **A** | Yes | Low | Use existing GeminiService. |

## 10. Implementation Plan (Proposed Phases)
*   **Phase 23.1**: Infrastructure (BackgroundService, BusinessInsight entity, Migration). **[DONE]**
*   **Phase 23.2**: Deterministic Detection Engine (Daily job + Rule logic). **[DONE]**
*   **Phase 23.3**: API Layer & Admin Web UI. **[DONE]**
*   **Phase 23.4**: SignalR Real-time Delivery. **[DONE]**

## 11. Implemented Features (Task 23.1, 23.2, 23.3 & 23.4)
*   **Entity**: `BusinessInsight` with deduplication key support.
*   **Service**: `ProactiveInsightService` for deterministic anomaly detection.
*   **AI Integration**: `InsightExplanationService` using Gemini `gemini-3.7-flash` with structured output (JSON schema) to generate explanations and recommendations.
*   **Scheduler**: `InsightBackgroundService` (native `BackgroundService`) running daily at 00:05 Vietnam Time.
*   **Rules**: Revenue Drop/Increase (+/- 20%), Expense Category Spike (+50%), Profit Margin Pressure (-15%), AOV Anomaly (+/- 20%).
*   **Multi-Instance Safety**: PostgreSQL advisory locks ensure only one instance processes insights. Explicitly maintaining session for session-level lock safety.
*   **Deduplication**: Database-level unique constraint on `DeduplicationKey`.
*   **Branch Isolation**: Insights are generated and filtered per-branch based on active branch list and JWT context.
*   **API**: `BusinessInsightController` for listing, detail view, unread count, and status management (read/resolve).
*   **Notification Integration**: New insights automatically create persistent notifications in the `Notifications` table.
*   **Admin UI**: Dedicated "CẢNH BÁO" page with list/detail view, severity badges, and AI-powered recommendations.
*   **Real-time Delivery**: Integrated with `KitchenHub` and `NotificationService` to push Business Insights to Admin/Manager browsers via SignalR with branch-scoped isolation.
*   **Toast System**: Real-time popups in the Admin Web navigation bar for immediate awareness of business anomalies.

## 12. Scope Boundaries
*   **NO** Automatic actions (e.g., "AI changed the price of Coffee because it's selling too fast").
*   **NO** External data (Weather, Competitors).
*   **NO** Training/Fine-tuning.
*   **NO** Inventory integration (until Task 24+).

---
**TASK 23.3 COMPLETE**
