using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingService _settingService;
        private readonly ILogger<LoyaltyService> _logger;

        public LoyaltyService(ApplicationDbContext context, ISystemSettingService settingService, ILogger<LoyaltyService>? logger = null)
        {
            _context = context;
            _settingService = settingService;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<LoyaltyService>.Instance;
        }

        public async Task<LoyaltyTransaction?> EarnPointsAsync(Guid orderId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = _context.Database.CurrentTransaction == null
                    ? await _context.Database.BeginTransactionAsync()
                    : null;

                var order = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                var customerIsActive = order?.CustomerId is Guid customerId
                    ? await _context.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => (bool?)c.IsActive).FirstOrDefaultAsync()
                    : null;
                var alreadyEarned = await _context.LoyaltyTransactions
                    .AnyAsync(t => t.OrderId == orderId && t.Type == "Earn");

                _logger.LogInformation(
                    "Loyalty earn trace: order {OrderId} hasCustomerId={HasCustomerId}, totalAmount={TotalAmount}, paidAmount={PaidAmount}, status={Status}, customerIsActive={CustomerIsActive}, alreadyEarned={AlreadyEarned}",
                    orderId, order?.CustomerId.HasValue ?? false, order?.TotalAmount, order?.PaidAmount,
                    order?.Status, customerIsActive, alreadyEarned);

                // Loyalty is earned only after the persisted order is fully paid.
                // A status transition alone must never award points for an unpaid order.
                if (order == null)
                {
                    _logger.LogInformation("Loyalty earn skipped for order {OrderId}: order not found", orderId);
                    return null;
                }
                if (order.CustomerId == null)
                {
                    _logger.LogInformation("Loyalty earn skipped for order {OrderId}: no customer linked", orderId);
                    return null;
                }
                if (order.TotalAmount <= 0)
                {
                    _logger.LogInformation("Loyalty earn skipped for order {OrderId}: total amount is not positive", orderId);
                    return null;
                }
                if (order.PaidAmount < order.TotalAmount)
                {
                    _logger.LogInformation("Loyalty earn skipped for order {OrderId}: payment is incomplete", orderId);
                    return null;
                }
                if (order.Status != "Hoàn thành" && order.Status != "Completed")
                {
                    _logger.LogInformation("Loyalty earn skipped for order {OrderId}: status is not completed", orderId);
                    return null;
                }

                // Idempotency check: Already earned for this order?
                if (alreadyEarned)
                {
                    _logger.LogInformation("Loyalty earn skipped for order {OrderId}: Earn transaction already exists", orderId);
                    return null;
                }

                var settings = await _settingService.GetSettingsAsync(order.BranchId);

                // Default: 1 point per 10,000 VND
                var pointsPerAmount = GetSettingDecimal(settings, "loyaltyPointsPerAmount", 10000);
                if (pointsPerAmount <= 0) pointsPerAmount = 10000;

                int pointsToEarn = (int)(order.TotalAmount / pointsPerAmount);
                _logger.LogInformation(
                    "Loyalty earn calculation: order {OrderId} pointsPerAmount={PointsPerAmount}, pointsToEarn={PointsToEarn}",
                    orderId, pointsPerAmount, pointsToEarn);
                if (pointsToEarn <= 0)
                {
                    _logger.LogInformation("Loyalty earn skipped for order {OrderId}: calculated points are zero", orderId);
                    return null;
                }

                // Atomic update to avoid race conditions
                var affected = await _context.Customers
                    .Where(c => c.Id == order.CustomerId.Value && c.IsActive)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.LoyaltyPoints, c => c.LoyaltyPoints + pointsToEarn)
                        .SetProperty(c => c.TotalSpending, c => c.TotalSpending + order.TotalAmount)
                        .SetProperty(c => c.TotalOrders, c => c.TotalOrders + 1)
                        .SetProperty(c => c.LastOrderDate, DateTime.UtcNow)
                        .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));

                if (affected == 0)
                {
                    _logger.LogInformation("Loyalty earn skipped for order {OrderId}: customer missing or inactive", orderId);
                    return null;
                }

                // ExecuteUpdateAsync bypasses EF's identity map. Detach a previously
                // tracked Customer so later reads in this transaction cannot return
                // a stale balance or overwrite the atomic database update.
                var trackedCustomer = _context.ChangeTracker.Entries<Customer>()
                    .FirstOrDefault(entry => entry.Entity.Id == order.CustomerId.Value);
                if (trackedCustomer != null)
                    trackedCustomer.State = EntityState.Detached;

                // Re-fetch for balance calculation in transaction record and group update
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == order.CustomerId.Value);
                if (customer != null)
                {
                    UpdateCustomerGroup(customer);
                    await _context.Customers
                        .Where(c => c.Id == customer.Id)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.CustomerGroup, customer.CustomerGroup));
                }

                var loyaltyTx = new LoyaltyTransaction
                {
                    Id = Guid.NewGuid(),
                    CustomerId = order.CustomerId.Value,
                    OrderId = orderId,
                    Type = "Earn",
                    Points = pointsToEarn,
                    BalanceAfter = customer?.LoyaltyPoints ?? 0,
                    Description = $"Tích điểm từ hóa đơn {order.InvoiceCode}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.LoyaltyTransactions.Add(loyaltyTx);
                order.Status = "Hoàn thành";
                order.PaymentMethod = "Đổi điểm";
                order.PaymentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Loyalty earn persisted for order {OrderId}: customerUpdateRows={AffectedRows}, transactionPoints={Points}, balanceAfter={BalanceAfter}",
                    orderId, affected, loyaltyTx.Points, loyaltyTx.BalanceAfter);

                if (transaction != null) await transaction.CommitAsync();

                return loyaltyTx;
            });
        }

        public async Task<LoyaltyTransaction?> RedeemPointsAsync(Guid customerId, Guid orderId, int points)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = _context.Database.CurrentTransaction == null
                    ? await _context.Database.BeginTransactionAsync()
                    : null;

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null || (order.Status != "Đang xử lý" && order.Status != "Processing") || order.PaidAmount > 0)
                    return null;

                // Idempotency check: Already redeemed for this order?
                var existing = await _context.LoyaltyTransactions
                    .AnyAsync(t => t.OrderId == orderId && t.Type == "Redeem");
                if (existing) return null;

                var settings = await _settingService.GetSettingsAsync(order.BranchId);

                // Default: 1 point = 1,000 VND
                var redemptionValue = GetSettingDecimal(settings, "loyaltyRedemptionValue", 1000);
                var minRedeemPoints = (int)GetSettingDecimal(settings, "loyaltyMinRedeemPoints", 0);

                if (points < minRedeemPoints) return null;

                decimal discountAmount = points * redemptionValue;

                // Cap discount to total amount
                if (discountAmount > order.TotalAmount)
                {
                    discountAmount = order.TotalAmount;
                    points = (int)Math.Ceiling(discountAmount / redemptionValue);
                }

                // Atomic update with point balance check to prevent negative balance
                var affected = await _context.Customers
                    .Where(c => c.Id == customerId && c.IsActive && c.LoyaltyPoints >= points)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.LoyaltyPoints, c => c.LoyaltyPoints - points)
                        .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));

                if (affected == 0) return null;

                var customer = await _context.Customers.FindAsync(customerId);
                var loyaltyTx = new LoyaltyTransaction
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    OrderId = orderId,
                    Type = "Redeem",
                    Points = -points,
                    BalanceAfter = customer?.LoyaltyPoints ?? 0,
                    Description = $"Sử dụng điểm cho hóa đơn {order.InvoiceCode}",
                    CreatedAt = DateTime.UtcNow
                };

                order.Discount += discountAmount;
                order.TotalAmount -= discountAmount;
                order.CustomerId = customerId;

                _context.LoyaltyTransactions.Add(loyaltyTx);
                await _context.SaveChangesAsync();

                if (transaction != null) await transaction.CommitAsync();

                return loyaltyTx;
            });
        }

        public async Task<LoyaltyTransaction?> RedeemGiftAsync(Guid customerId, Guid orderId, Guid promotionId, string? processedBy = null, Guid? processedByEmployeeId = null)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = _context.Database.CurrentTransaction == null
                    ? await _context.Database.BeginTransactionAsync()
                    : null;

                var order = await _context.Orders.FindAsync(orderId);
                var promotion = await _context.Promotions.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == promotionId && p.PromotionType == "Gift");
                var isLegacyGiftRequest = order?.Details?.Count == 1 &&
                    order.Details[0].ProductName?.StartsWith("[ĐỔI QUÀ]", StringComparison.OrdinalIgnoreCase) == true &&
                    order.Details[0].ProductId == null &&
                    order.Details[0].ToppingId == null &&
                    order.Details[0].UnitPrice == 0 &&
                    order.TotalAmount == 0;
                if (order == null || promotion == null || !promotion.IsActive ||
                    promotion.RequiredPoints <= 0 ||
                    (promotion.EndDate.HasValue && promotion.EndDate.Value <= DateTime.UtcNow) ||
                    (order.Status != "Đổi quà" && !(order.Status == "Đang xử lý" && isLegacyGiftRequest)) ||
                    order.PaidAmount > 0)
                    return null;

                var existing = await _context.LoyaltyTransactions
                    .AnyAsync(t => t.OrderId == orderId && t.Type == "RedeemGift");
                if (existing) return null;

                var affected = await _context.Customers
                    .Where(c => c.Id == customerId && c.IsActive && c.LoyaltyPoints >= promotion.RequiredPoints)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.LoyaltyPoints, c => c.LoyaltyPoints - promotion.RequiredPoints)
                        .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));
                if (affected == 0) return null;

                var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId);
                order.CustomerId = customerId;
                var loyaltyTx = new LoyaltyTransaction
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    OrderId = orderId,
                    Type = "RedeemGift",
                    Points = -promotion.RequiredPoints,
                    BalanceAfter = customer?.LoyaltyPoints ?? 0,
                    Description = $"Đổi quà: {promotion.Name}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.LoyaltyTransactions.Add(loyaltyTx);
                order.Status = "Hoàn thành";
                order.PaymentMethod = "Đổi điểm";
                order.PaymentAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(processedBy)) order.CreatedBy = processedBy.Trim();
                if (processedByEmployeeId.HasValue) order.CreatedByEmployeeId = processedByEmployeeId;
                await _context.SaveChangesAsync();
                if (transaction != null) await transaction.CommitAsync();
                return loyaltyTx;
            });
        }

        public async Task<LoyaltyTransaction?> RefundPointsAsync(Guid orderId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = _context.Database.CurrentTransaction == null
                    ? await _context.Database.BeginTransactionAsync()
                    : null;

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null || order.CustomerId == null) return null;

                var transactions = await _context.LoyaltyTransactions
                    .Where(t => t.OrderId == orderId && (t.Type == "Earn" || t.Type == "Redeem" || t.Type == "RedeemGift"))
                    .ToListAsync();

                if (!transactions.Any()) return null;

                // Idempotency check: Already reversed/refunded?
                var alreadyRefunded = await _context.LoyaltyTransactions
                    .AnyAsync(t => t.OrderId == orderId && (t.Type == "Refund" || t.Type == "Reversal"));
                if (alreadyRefunded) return null;

                int netPoints = transactions.Sum(t => t.Points);
                if (netPoints == 0) return null;

                // Atomic update for points and spending reversal
                var affected = await _context.Customers
                    .Where(c => c.Id == order.CustomerId.Value)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.LoyaltyPoints, c => c.LoyaltyPoints - netPoints)
                        .SetProperty(c => c.TotalSpending, c => c.TotalSpending - (transactions.Any(t => t.Type == "Earn") ? order.TotalAmount : 0))
                        .SetProperty(c => c.TotalOrders, c => c.TotalOrders - (transactions.Any(t => t.Type == "Earn") ? 1 : 0))
                        .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));

                if (affected == 0) return null;

                var customer = await _context.Customers.FindAsync(order.CustomerId.Value);
                if (customer != null)
                {
                    UpdateCustomerGroup(customer);
                    await _context.SaveChangesAsync();
                }

                var loyaltyTx = new LoyaltyTransaction
                {
                    Id = Guid.NewGuid(),
                    CustomerId = order.CustomerId.Value,
                    OrderId = orderId,
                    Type = netPoints > 0 ? "Reversal" : "Refund",
                    Points = -netPoints,
                    BalanceAfter = customer?.LoyaltyPoints ?? 0,
                    Description = $"Hoàn/Hủy điểm cho hóa đơn {order.InvoiceCode}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.LoyaltyTransactions.Add(loyaltyTx);
                await _context.SaveChangesAsync();

                if (transaction != null) await transaction.CommitAsync();

                return loyaltyTx;
            });
        }

        public async Task<int> GetBalanceAsync(Guid customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            return customer?.LoyaltyPoints ?? 0;
        }

        public async Task<List<LoyaltyTransaction>> GetHistoryAsync(Guid customerId)
        {
            return await _context.LoyaltyTransactions
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<OrderLoyaltySummaryDto?> GetOrderLoyaltyAsync(Guid orderId)
        {
            var transactions = await _context.LoyaltyTransactions
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            if (!transactions.Any()) return null;

            return new OrderLoyaltySummaryDto
            {
                PointsEarned = transactions.Where(t => t.Type == "Earn").Sum(t => t.Points),
                PointsRedeemed = Math.Abs(transactions.Where(t => t.Type == "Redeem" || t.Type == "RedeemGift").Sum(t => t.Points)),
                BalanceAfter = transactions.Last().BalanceAfter
            };
        }

        private static void UpdateCustomerGroup(Customer customer)
        {
            if (customer.TotalSpending > 10000000) customer.CustomerGroup = "Khách VIP";
            else if (customer.TotalSpending > 2000000) customer.CustomerGroup = "Khách quen";
            else customer.CustomerGroup = "Khách lẻ";
        }

        private static decimal GetSettingDecimal(IReadOnlyDictionary<string, string> settings, string key, decimal defaultValue)
        {
            if (settings.TryGetValue(key, out var value) &&
                decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
            return defaultValue;
        }
    }
}
