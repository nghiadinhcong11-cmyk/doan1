# Cost / Expense Management

This project intentionally implements simple cost recording rather than a physical inventory system.

## Scope

An expense records a purchase slip or operating cost such as ingredients, packaging, utilities, cleaning, gas, wages, or other operating costs. Every record belongs to a branch and contains a category, description, decimal amount, expense date, optional payment method/note, creator, and timestamps.

The module does not implement recipes, ingredient consumption, stock quantities, warehouse transfers, batches, or automatic stock deduction.

## API

- `GET /api/Expense`: branch/date/category filters with pagination and total amount.
- `POST /api/Expense`: validated branch-scoped creation.
- `PUT /api/Expense/{id}`: validated branch-scoped update.
- `DELETE /api/Expense/{id}`: branch-scoped deletion.

Admin may operate across branches. Employee and cashier requests are restricted to the `branchId` claim in the JWT. Client-supplied branch values cannot widen that scope.

## Reporting interpretation

Dashboard `totalExpenses` is the sum of recorded expense amounts for the selected day and branch. `netProfit` remains compatible with the existing dashboard calculation: revenue minus recorded expenses minus the existing estimated product/topping cost. That cost is not recipe-derived COGS; it is only an estimate based on the product/topping `CostPrice` captured by the current system.

