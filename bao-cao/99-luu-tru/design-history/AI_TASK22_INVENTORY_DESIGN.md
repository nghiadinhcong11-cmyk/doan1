# AI Inventory Intelligence Design Documentation

## 1. Executive Summary
This document outlines the design for integrating Inventory Intelligence into the AI Assistant. Currently, the system lacks a dedicated inventory management module. The proposed design introduces the necessary entities, business logic, and AI tools to enable stock tracking, low-stock alerts, and consumption analytics.

## 2. Current Inventory Architecture
*   **Inventory Entities**: NONE.
*   **Recipe System**: NONE.
*   **Stock Tracking**: NONE.
*   **Existing Hooks**:
    *   `Product.CostPrice` and `Topping.CostPrice`: Used for COGS estimation in `DashboardService`.
    *   `Expense`: Financial records for purchases, but not linked to specific items/quantities.
    *   `OrderDetails`: Record of what was sold, which is the driver for consumption.

## 3. Proposed Schema Changes (Target State)

### 3.1. Ingredient Entity
Represents a raw material or stock item.
*   `Id` (Guid)
*   `BranchId` (Guid): Scoped to a specific branch.
*   `Code` (string): Unique identifier (e.g., MAT001).
*   `Name` (string)
*   `Unit` (string): kg, g, l, ml, piece, box.
*   `CurrentStock` (decimal)
*   `MinimumStock` (decimal): Threshold for "Low Stock" alerts.
*   `CostPrice` (decimal): Average or last purchase price.
*   `IsActive` (bool)

### 3.2. ProductIngredient Entity (Recipe)
Maps a Product to its required Ingredients.
*   `ProductId` (Guid): Foreign key to Product.
*   `IngredientId` (Guid): Foreign key to Ingredient.
*   `Quantity` (decimal): Amount required for 1 unit of Product.
*   *Note: Consider adding ToppingIngredient for topping consumption.*

### 3.3. InventoryTransaction Entity
Logs all stock movements for audit and analytics.
*   `Id` (Guid)
*   `IngredientId` (Guid)
*   `BranchId` (Guid)
*   `Quantity` (decimal): Positive for intake, negative for deduction.
*   `Type` (Enum): `StockIn` (Purchase), `StockOut` (Sales/Waste), `Adjustment`.
*   `ReferenceId` (Guid?): Link to OrderId or ExpenseId.
*   `CreatedAt` (DateTime)
*   `CreatedBy` (string)

## 4. Proposed Business Logic

### 4.1. Stock Deduction (Sales)
When an Order reaches `Hoàn thành` (Completed) or an `OrderRequest` is `Completed` in the kitchen:
1.  Lookup `ProductIngredient` for all `OrderDetails`.
2.  Calculate total requirement: `Item.Quantity * Recipe.Quantity`.
3.  Decrease `Ingredient.CurrentStock`.
4.  Record `InventoryTransaction` with type `StockOut`.

### 4.2. Stock Replenishment (Purchase)
When creating an `Expense` categorized as "Raw Material":
1.  Allow linking to a `StockIn` transaction.
2.  Increase `Ingredient.CurrentStock`.
3.  Update `Ingredient.CostPrice` (Weighted Average or Last Price).

## 5. Proposed AI Tools

### 5.1. `get_inventory_summary`
*   **Purpose**: Overview of branch inventory health.
*   **Input**: `branchId`.
*   **Output**: Total items, count of low stock items, count of out of stock items, total estimated inventory value.
*   **Risk Level**: Read.
*   **Roles**: Admin, Manager.

### 5.2. `get_low_stock_items`
*   **Purpose**: List specific materials needing replenishment.
*   **Input**: `branchId`.
*   **Output**: List of `Ingredient` where `CurrentStock <= MinimumStock`.
*   **Risk Level**: Read.
*   **Roles**: Admin, Manager, Kitchen (optional).

### 5.3. `get_inventory_consumption`
*   **Purpose**: Analyze usage trends.
*   **Input**: `branchId`, `days` (default 30).
*   **Output**: Material usage totals, average daily consumption, "runway" (Days of Stock remaining based on recent sales).
*   **Risk Level**: Read.
*   **Roles**: Admin, Manager.

### 5.4. `get_inventory_sales_risk`
*   **Purpose**: Predictive warning for high-selling products.
*   **Input**: `branchId`.
*   **Output**: Products that are "Best Sellers" but have Ingredients at risk of running out.
*   **Risk Level**: Read.
*   **Roles**: Admin, Manager.

## 6. Permission Matrix

| Tool | Admin | Manager | Employee | Cashier | Kitchen | Customer |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| inventory_summary | ✓ | ✓ | X | X | X | X |
| low_stock_items | ✓ | ✓ | X | X | ✓ | X |
| inventory_consumption | ✓ | ✓ | X | X | X | X |
| sales_risk | ✓ | ✓ | X | X | X | X |

*Note: Kitchen staff need to know what's running out (`low_stock`) but don't need financial summaries.*

## 7. Branch Isolation & Security
*   **Mandatory Scoping**: Every inventory query must use the `BranchId` from the authenticated user context (JWT).
*   **Data Integrity**: AI is **NOT** allowed to modify `CurrentStock` directly. Changes must go through a formal `InventoryService`.
*   **Financial Sensitivity**: Inventory Value (Stock * Cost) is restricted to Manager/Admin.

## 8. AI Reasoning (FACT/INFERENCE/RECOMMENDATION)
*   **FACT**: "Bột cà phê hiện còn 2kg, ngưỡng tối thiểu là 5kg."
*   **INFERENCE**: "Với tốc độ bán hiện tại, lượng bột cà phê chỉ đủ dùng cho 1.5 ngày tới."
*   **RECOMMENDATION**: "Nên nhập thêm ít nhất 10kg bột cà phê trong hôm nay để đảm bảo không gián đoạn."

## 9. Implementation Plan (Phase 2)
1.  **Database**: Create migrations for `Ingredients`, `Recipes`, and `InventoryTransactions`.
2.  **Service**: Implement `InventoryService` for basic CRUD and stock adjustment logic.
    *   Implement `DeductStockOnOrderAsync`.
    *   Implement `ReplenishStockOnExpenseAsync`.
3.  **Analytics**: Update `DashboardService` to calculate "Days of Stock" and "Consumption Trends".
4.  **AI Integration**: Register new tools in `AiToolRegistry` and implement tool handlers using `InventoryService`.

## 10. Risks & Limitations
*   **Recipe Complexity**: Many restaurant items (e.g., drinks) have complex recipes with small units (ml/g). Accuracy depends heavily on staff data entry.
*   **Waste/Spoilage**: The system currently doesn't track waste, which will lead to "phantom stock" over time.
*   **Manual Adjustments**: Inventory reconciliation (Stocktake) will be necessary to fix drift.
*   **Initial Data**: The system is currently "Level C" (Not possible) until the schema is implemented.

---
**TASK 22 AUDIT/DESIGN COMPLETE — NO IMPLEMENTATION PERFORMED**
