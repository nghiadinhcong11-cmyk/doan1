# Tool Boundaries Definition

## SET 1: Menu vs Popularity
| Tool | Use Cases | Similar Tool | Difference |
|------|-----------|--------------|------------|
| `customer_get_menu` | "What is on the menu?", "Price of Coffee", "What toppings for Tea?" | `get_best_sellers` | `customer_get_menu` is for catalog lookup. `get_best_sellers` is for historical popularity. |
| `get_best_sellers` | "What are the most popular items?", "Which dish sells best today?" | `customer_get_menu` | Focuses on sales quantity/revenue metrics. |

## SET 2: Financial Aggregations
| Tool | Use Cases | Similarity | Boundary |
|------|-----------|------------|----------|
| `get_revenue` | "Total sales today", "How much did we make in August?" | Direct revenue value. | Returns a single number for a period. |
| `get_revenue_comparison`| "Is revenue higher than yesterday?", "Growth this week vs last?" | Compares two periods. | Returns delta and percentage change. |
| `get_business_summary` | "How is the business today?", "Daily performance snapshot?" | KPI dashboard. | Returns Revenue, Order Count, AOV, and Profit. |
| `get_financial_analysis`| "Deep dive into profit/cost", "Analyze anomalies this month" | Deep analysis. | Includes Expense breakdown and Anomaly detection. |

## SET 3: My History vs Branch History
| Tool | Use Cases | Boundary |
|------|-----------|----------|
| `get_my_orders` | "Show my recent orders", "Status of my last purchase?" | Strictly for the authenticated Customer's OWN data. |
| `get_order_list` | "All orders today", "Which orders are still Processing?" | For staff/admin to view branch/global operations. |

## SET 4: Table Status vs Orders
| Tool | Use Cases | Boundary |
|------|-----------|----------|
| `employee_get_table_summary`| "How many tables are occupied?", "Are there free tables?" | Aggregated counts of table statuses. |
| `get_order_list` | "Which tables have active orders?" | Individual order details, even if associated with tables. |

## SET 5: Personal Shift vs Staffing
| Tool | Use Cases | Boundary |
|------|-----------|----------|
| `get_my_shift` | "When do I start today?", "My schedule?" | Personal employee schedule. |
| `get_active_staff` | "Who is on duty now?", "How many staff working today?" | Manager/Admin view of the whole branch team. |
