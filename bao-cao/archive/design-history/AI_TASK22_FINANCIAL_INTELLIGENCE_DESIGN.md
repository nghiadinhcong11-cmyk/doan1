# AI Financial Intelligence Design Documentation

## 1. Executive Summary
This document outlines the design for elevating the AI Assistant into an **AI Financial Intelligence Assistant**. The goal is to provide deep analysis of revenue, expenses, profit, and operational efficiency based on existing system data.

## 2. Current Financial Architecture
*   **Revenue**: Persisted in `Order.PaidAmount` for orders with `Status = 'HoĂ n thĂ nh'`.
*   **Expenses**: Persisted in `Expense` entity. Categories: "NguyĂªn liá»‡u", "Bao bĂ¬", "Äiá»‡n nÆ°á»›c", "Vá»‡ sinh", "Gas", "Váº­n hĂ nh", "KhĂ¡c".
*   **COGS (Cost of Goods Sold)**: Estimated based on current `Product.CostPrice` and `Topping.CostPrice`.
*   **Net Profit**: Calculated as `Revenue - TotalExpenses - EstimatedCOGS`.
*   **removed staff-finance module**: Managed in `removed staff-finance module` entity but currently **not** integrated into `DashboardService` totals.

## 3. Financial KPIs & Formulas

| KPI | Formula | Source |
| :--- | :--- | :--- |
| **Revenue** | `Sum(Order.PaidAmount)` where status is "HoĂ n thĂ nh" | `OrderService` / `DashboardService` |
| **Orders** | `Count(Orders)` where status is "HoĂ n thĂ nh" | `OrderService` / `DashboardService` |
| **AOV** | `Revenue / Orders` | `DashboardService` |
| **Expenses** | `Sum(Expense.Amount)` | `DashboardService` |
| **Estimated COGS** | `Sum(Item.Quantity * Product.CostPrice)` | `DashboardService` |
| **Estimated Profit** | `Revenue - Expenses - COGS` | `DashboardService` |
| **Profit Margin** | `(Estimated Profit / Revenue) * 100` | New Calculation |
| **Growth Rate** | `((Current - Previous) / Previous) * 100` | New Calculation |

## 4. Proposed AI Tools

### 4.1. `get_financial_analysis` (New or Extended)
*   **Purpose**: Comprehensive financial health check for a branch.
*   **Input**: `branchId`, `period` (today, yesterday, this_week, last_week, this_month, last_month).
*   **Output**: 
    *   Full KPI set (Revenue, Orders, AOV, Expenses, COGS, Profit, Margin).
    *   Comparison with the previous period (e.g., this month vs last month).
    *   Expense breakdown by Category.
    *   Top 5 best sellers and their contribution to revenue.
*   **Roles**: Admin, Manager.

### 4.2. `get_expense_breakdown`
*   **Purpose**: Deep dive into where the money is going.
*   **Input**: `branchId`, `period`.
*   **Output**: List of categories with total amount and percentage of total expenses.
*   **Roles**: Admin, Manager.

### 4.3. `detect_financial_anomalies` (Internal/Logic)
*   **Thresholds**: Significant Change = `Change >= 20%` (configurable).
*   **Rules**:
    *   **Revenue Anomaly**: Drop in revenue > 20% YoY/MoM.
    *   **Profit Anomaly**: Revenue stable/up but Profit down > 15%.
    *   **Expense Anomaly**: Specific category spikes > 50%.
    *   **AOV Anomaly**: Significant change in customer spending habits.

## 5. AI Reasoning Patterns (FACT/INFERENCE/RECOMMENDATION)

### Example A: Revenue Up, Profit Down
*   **FACT**: Revenue is up 15%, but Net Profit is down 8%. Marketing expenses increased by 150%.
*   **INFERENCE**: The marketing campaign is driving traffic and revenue but its cost is currently higher than the incremental profit generated.
*   **RECOMMENDATION**: Review the effectiveness of the marketing campaign and consider optimizing the spend-to-revenue ratio.

### Example B: AOV Increasing, Orders Decreasing
*   **FACT**: Total revenue is stable. Orders are down 10%, but AOV is up 12%.
*   **INFERENCE**: You are serving fewer customers, but each customer is spending more.
*   **RECOMMENDATION**: Investigate if this is due to recent price changes or successful upselling of combos/toppings.

## 6. Permission & Security
*   **Branch Isolation**: Strictly enforced via `userContext.BranchId`.
*   **RBAC**:
    *   `Admin`: Global view / Multi-branch comparison.
    *   `Manager`: Own branch analysis only.
    *   `Employee/Kitchen/Customer`: **DENIED** access to high-sensitivity financial data (Expenses, Profit, COGS).
*   **Data Masking**: AI should not expose raw transaction IDs or sensitive employee names linked to specific salary expenses unless required.

## 7. Limitations & Caveats
*   **Estimated COGS**: Based on current product prices; does not reflect historical cost changes.
*   **removed staff-finance module Gap**: Current `totalExpenses` may not include salaries unless manually entered as `Expenses`.
*   **Double-Counting Risk**: Estimated Profit is calculated as `Revenue - Expenses - EstimatedCOGS`. Since `Expenses` may contain categories like "NguyĂªn liá»‡u" (Ingredients) which are also the source of `EstimatedCOGS`, recording ingredient purchases in the Expense table can lead to double-counting costs in the net profit figure.
*   **Causality**: AI identifies correlations, not absolute proof of cause. Recommendations are advisory only.
*   **Statistical Significance**: Without a full year of data, seasonality analysis is limited.

## 8. Implementation Plan (Phase 2)
1.  **Backend**: Extend `DashboardService` to support multi-period comparison and expense category aggregation.
2.  **Backend**: Add `IExpenseService` to formalize expense logic.
3.  **AI**: Update `GetBusinessSummaryTool` or add `GetFinancialAnalysisTool`.
4.  **Prompt**: Update System Prompt to encourage strict adherence to the `FACT -> INFERENCE -> RECOMMENDATION` structure for financial queries.

---
**TASK 22 FINANCIAL INTELLIGENCE AUDIT/DESIGN COMPLETE â€” NO IMPLEMENTATION PERFORMED**


