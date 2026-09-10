using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.DTOs.Orders;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingService _settingService;
        private readonly ILoyaltyService _loyaltyService;

        public OrderService(ApplicationDbContext context, ISystemSettingService settingService, ILoyaltyService loyaltyService)
        {
            _context = context;
            _settingService = settingService;
            _loyaltyService = loyaltyService;
        }

        // Keeps existing direct service consumers and tests compatible while the
        // application itself continues to receive ISystemSettingService through DI.
        public OrderService(ApplicationDbContext context)
            : this(context, new SystemSettingService(context), new LoyaltyService(context, new SystemSettingService(context)))
        {
        }

        public async Task<List<Order>> GetOrdersAsync(OrderQueryFilter filter)
        {
            var query = _context.Orders.Include(o => o.Details).AsQueryable();

            if (filter.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == filter.BranchId.Value);
            }

            if (!string.IsNullOrEmpty(filter.TableName))
            {
                query = query.Where(o => o.TableName == filter.TableName);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(o => o.InvoiceCode.Contains(filter.Search) ||
                    (o.CustomerName != null && o.CustomerName.Contains(filter.Search)));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                if (filter.Status.Contains(","))
                {
                    var statuses = filter.Status.Split(',').Select(s => s.Trim()).ToList();
                    query = query.Where(o => statuses.Contains(o.Status));
                }
                else
                {
                    query = query.Where(o => o.Status == filter.Status);
                }
            }

            if (!string.IsNullOrEmpty(filter.CustomerPhone))
            {
                query = query.Where(o => o.CustomerPhone == filter.CustomerPhone ||
                    (o.CustomerName != null && o.CustomerName.Contains(filter.CustomerPhone)));
            }

            if (!string.IsNullOrEmpty(filter.FromDate) && DateTime.TryParse(filter.FromDate, out var fDate))
            {
                var from = DateTime.SpecifyKind(fDate.Date, DateTimeKind.Utc);
                query = query.Where(o => o.CreatedAt >= from);
            }

            if (!string.IsNullOrEmpty(filter.ToDate) && DateTime.TryParse(filter.ToDate, out var tDate))
            {
                var to = DateTime.SpecifyKind(tDate.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(o => o.CreatedAt < to);
            }

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

            // Enrich customer info from Customers table
            var phones = orders.Where(o => !string.IsNullOrEmpty(o.CustomerPhone))
                               .Select(o => o.CustomerPhone!)
                               .Distinct()
                               .ToList();

            if (phones.Any())
            {
                var customers = await _context.Customers
                    .Where(c => phones.Contains(c.PhoneNumber))
                    .ToListAsync();

                foreach (var order in orders)
                {
                    if (!string.IsNullOrEmpty(order.CustomerPhone))
                    {
                        var customer = customers.FirstOrDefault(c => c.PhoneNumber == order.CustomerPhone);
                        if (customer != null)
                        {
                            order.CustomerName = customer.FullName;
                            order.CustomerEmail = customer.Email;
                        }
                    }
                }
            }

            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _context.Orders.Include(o => o.Details).FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateOrUpdateOrderAsync(Order order)
        {
            const int MAX_QUANTITY = 500;

            if (order.Details != null && order.Details.Any(d => d.Quantity > MAX_QUANTITY))
            {
                throw new ArgumentException($"Số lượng món không được vượt quá {MAX_QUANTITY}.");
            }

            // Logic: Nếu bàn đã có đơn "Đang xử lý", ta cập nhật đơn đó thay vì tạo mới
            if (order.Id != Guid.Empty || (order.TableName != "Mang về" && !string.IsNullOrEmpty(order.TableName)))
            {
                // Do not let a previously materialized request graph participate in the
                // merge. The order payload is client input, while the database graph below
                // is the source of truth for keys and sent quantities.
                _context.ChangeTracker.Clear();
                var existingOrder = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Details)
                    .FirstOrDefaultAsync(o =>
                        (order.Id != Guid.Empty ? o.Id == order.Id :
                            o.TableName == order.TableName && o.BranchId == order.BranchId)
                        && o.Status == "Đang xử lý");

                if (existingOrder != null)
                {
                    // 1. Cập nhật thông tin đơn hàng chính (Chỉ những trường vận hành an toàn)
                    // Tuyệt đối không cập nhật Status, PaidAmount, PaymentMethod, Discount từ client payload
                    // để tránh gian lận tài chính. Những trường này phải đi qua flow thanh toán riêng.
                    existingOrder.CustomerName = order.CustomerName;
                    existingOrder.CustomerPhone = order.CustomerPhone;
                    existingOrder.CustomerEmail = order.CustomerEmail;
                    existingOrder.CustomerId = order.CustomerId;
                    existingOrder.CreatedBy = order.CreatedBy;
                    existingOrder.BranchName = order.BranchName;
                    existingOrder.Note = order.Note;

                    // 2. Cập nhật chi tiết món (Merge logic)
                    if (order.Details != null)
                    {
                        // Tìm các món bị xóa (Chỉ món chưa gửi bếp mới được xóa thực sự)
                        var removedItems = existingOrder.Details
                            .Where(old => !order.Details.Any(newD =>
                                (newD.ProductId == old.ProductId && newD.ToppingId == old.ToppingId && newD.Options == old.Options)))
                            .ToList();

                        foreach (var item in removedItems)
                        {
                            if (item.SentQuantity == 0)
                            {
                                existingOrder.Details.Remove(item);
                                _context.OrderDetails.Remove(item);
                            }
                            else
                            {
                                item.Quantity = item.SentQuantity; // Giữ lại phần đã gửi
                            }
                        }

                        foreach (var newDetail in order.Details)
                        {
                            var existingDetail = existingOrder.Details
                                .FirstOrDefault(d => d.ProductId == newDetail.ProductId
                                                  && d.ToppingId == newDetail.ToppingId
                                                  && (d.Options ?? "").Trim() == (newDetail.Options ?? "").Trim());

                            if (existingDetail != null)
                            {
                                existingDetail.Quantity = Math.Max(newDetail.Quantity, existingDetail.SentQuantity);
                            }
                            else
                            {
                                var addedDetail = new OrderDetail
                                {
                                    Id = Guid.NewGuid(),
                                    ProductId = newDetail.ProductId,
                                    ToppingId = newDetail.ToppingId,
                                    ProductName = newDetail.ProductName,
                                    Quantity = newDetail.Quantity,
                                    SentQuantity = 0,
                                    Options = newDetail.Options
                                };
                                existingOrder.Details.Add(addedDetail);
                                _context.OrderDetails.Add(addedDetail);
                            }
                        }
                    }

                    // Recalculate totals based on reliable DB prices
                    await RecalculateOrderFinancials(existingOrder);

                    _context.Orders.Update(existingOrder);
                    await _context.SaveChangesAsync();
                    return existingOrder;
                }
            }

            // Tạo đơn mới hoàn toàn
            order.Id = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;

            // Financial safety: Initial orders must be "Đang xử lý" and unpaid.
            order.Status = "Đang xử lý";
            order.PaidAmount = 0;
            order.PaymentMethod = null;
            order.PaymentAt = null;

            // There is no authoritative discount/promotion reference in the create
            // contract yet. A client-supplied discount must therefore be ignored.
            order.Discount = 0;
            if (string.IsNullOrEmpty(order.InvoiceCode))
                order.InvoiceCode = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");

            if (order.Details != null)
            {
                foreach (var detail in order.Details)
                {
                    detail.Id = Guid.NewGuid();
                    detail.SentQuantity = 0;
                }
            }

            // Recalculate totals based on reliable DB prices
            await RecalculateOrderFinancials(order);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        private async Task RecalculateOrderFinancials(Order order)
        {
            var settings = await _settingService.GetSettingsAsync(order.BranchId);

            var vatPercent = GetNonNegativeDecimalSetting(settings, "vatPercent");
            // P0.1 persisted percentage-only keys. Respect an explicit enabled
            // flag when present, otherwise preserve that configuration by treating
            // a positive configured rate as enabled.
            var vatEnabled = IsEnabled(settings, "vatEnabled", vatPercent);
            var serviceFeePercent = GetNonNegativeDecimalSetting(settings, "serviceFeePercent", "serviceFee");
            var serviceFeeEnabled = IsEnabled(settings, "serviceFeeEnabled", serviceFeePercent);

            decimal subTotal = 0;

            if (order.Details != null)
            {
                foreach (var detail in order.Details)
                {
                    decimal unitPrice = 0;
                    if (detail.ProductId.HasValue)
                    {
                        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == detail.ProductId.Value);
                        if (product != null)
                        {
                            unitPrice = product.Price;

                            // Match Size from ProductName "Product (Size)"
                            if (!string.IsNullOrEmpty(product.Name) &&
                                !string.IsNullOrEmpty(detail.ProductName) &&
                                detail.ProductName.Contains("(") && detail.ProductName.Contains(")"))
                            {
                                int start = detail.ProductName.LastIndexOf("(") + 1;
                                int end = detail.ProductName.LastIndexOf(")");
                                if (start > 0 && end > start)
                                {
                                    string sizeName = detail.ProductName.Substring(start, end - start);
                                    if (!string.IsNullOrEmpty(product.SizesJson))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(product.SizesJson);
                                            foreach (var s in doc.RootElement.EnumerateArray())
                                            {
                                                if (s.GetProperty("name").GetString()?.Equals(sizeName, StringComparison.OrdinalIgnoreCase) == true)
                                                {
                                                    unitPrice += s.GetProperty("price").GetDecimal();
                                                    break;
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }

                            // Match Toppings from Options (comma separated names)
                            if (!string.IsNullOrEmpty(detail.Options))
                            {
                                var optionNames = detail.Options.Split(',').Select(s => s.Trim()).ToList();
                                if (!string.IsNullOrEmpty(product.ToppingsJson))
                                {
                                    try
                                    {
                                        using var doc = JsonDocument.Parse(product.ToppingsJson);
                                        var availableToppings = doc.RootElement.EnumerateArray().ToList();
                                        foreach (var optName in optionNames)
                                        {
                                            var match = availableToppings.FirstOrDefault(t => t.GetProperty("name").GetString()?.Equals(optName, StringComparison.OrdinalIgnoreCase) == true);
                                            if (match.ValueKind != JsonValueKind.Undefined)
                                            {
                                                unitPrice += match.GetProperty("price").GetDecimal();
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    else if (detail.ToppingId.HasValue)
                    {
                        var topping = await _context.Toppings.AsNoTracking().FirstOrDefaultAsync(t => t.Id == detail.ToppingId.Value);
                        if (topping != null)
                        {
                            unitPrice = topping.Price;
                        }
                    }

                    detail.UnitPrice = unitPrice;
                    subTotal += detail.Quantity * detail.UnitPrice;
                }
            }

            order.SubTotal = subTotal;
            order.VatPercent = vatEnabled ? vatPercent : 0;
            order.ServiceFeePercent = serviceFeeEnabled ? serviceFeePercent : 0;

            // Amounts are rounded once per charge to the nearest VND, with halves
            // away from zero. Decimal arithmetic is used throughout.
            var serviceFeeAmount = RoundMoney(subTotal * order.ServiceFeePercent.Value / 100);
            var taxableBase = subTotal + serviceFeeAmount;
            var vatAmount = RoundMoney(taxableBase * order.VatPercent.Value / 100);

            order.ServiceFeeAmount = serviceFeeAmount;
            order.VatAmount = vatAmount;
            // Existing discounts are retained as legacy snapshots. New client
            // discounts are set to zero above until a server-side promotion flow is
            // introduced; the established order remains fee -> VAT -> discount.
            order.TotalAmount = taxableBase + vatAmount - order.Discount;
        }

        private static decimal GetNonNegativeDecimalSetting(
            IReadOnlyDictionary<string, string> settings,
            params string[] keys)
        {
            foreach (var key in keys)
            {
                if (settings.TryGetValue(key, out var value) &&
                    decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) &&
                    parsed >= 0)
                {
                    return parsed;
                }
            }

            return 0;
        }

        private static bool IsEnabled(IReadOnlyDictionary<string, string> settings, string key, decimal rate) =>
            settings.TryGetValue(key, out var value)
                ? bool.TryParse(value, out var enabled) && enabled
                : rate > 0;

        private static decimal RoundMoney(decimal amount) =>
            Math.Round(amount, 0, MidpointRounding.AwayFromZero);

        public async Task<(WebOrderAcceptResult Result, Order? Order)> AcceptWebOrderAsync(Guid id, Guid? tableId, Guid? allowedBranchId, string acceptedBy)
        {
            // Supabase uses a retrying Npgsql execution strategy. The complete
            // transaction must run inside that strategy so a transient retry
            // cannot leave a user-initiated transaction unsupported.
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var current = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
                if (current == null)
                    return (WebOrderAcceptResult.NotFound, (Order?)null);

                if (allowedBranchId.HasValue && current.BranchId != allowedBranchId.Value)
                    return (WebOrderAcceptResult.Forbidden, (Order?)null);

                // A Web Order is pending and has not yet been claimed by a staff user.
                if (current.Status != "Đang xử lý" || !string.IsNullOrEmpty(current.CreatedBy))
                    return (WebOrderAcceptResult.Conflict, current);

                RestaurantTable? targetTable = null;
                if (tableId.HasValue)
                {
                    targetTable = await _context.Tables.AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == tableId.Value && t.IsActive);
                    if (targetTable == null)
                        return (WebOrderAcceptResult.InvalidTable, (Order?)null);
                    if (allowedBranchId.HasValue && targetTable.BranchId != allowedBranchId.Value)
                        return (WebOrderAcceptResult.Forbidden, (Order?)null);
                    if (targetTable.BranchId != current.BranchId)
                        return (WebOrderAcceptResult.Forbidden, (Order?)null);

                    // Claim the table in the same transaction. A concurrent acceptance
                    // cannot assign another order to an occupied table.
                    var tableClaimed = await _context.Tables
                        .Where(t => t.Id == targetTable.Id && t.IsActive && t.Status != "Có khách")
                        .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Status, "Có khách"));
                    if (tableClaimed != 1)
                        return (WebOrderAcceptResult.Conflict, current);
                }

            // This conditional update is the concurrency-safe claim. Only one
            // concurrent request can change the still-unclaimed Web Order.
            var claimed = await _context.Orders
                .Where(o => o.Id == id
                    && o.Status == "Đang xử lý"
                    && string.IsNullOrEmpty(o.CreatedBy)
                    && (!allowedBranchId.HasValue || o.BranchId == allowedBranchId.Value))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.TableName, targetTable == null ? "Mang về" : targetTable.Name)
                    .SetProperty(o => o.CreatedBy, acceptedBy));

            if (claimed != 1)
                return (WebOrderAcceptResult.Conflict, current);

            _context.ChangeTracker.Clear();
            var accepted = await _context.Orders
                .Include(o => o.Details)
                .FirstAsync(o => o.Id == id);

            await transaction.CommitAsync();
            return (WebOrderAcceptResult.Accepted, accepted);
            });
        }

        public async Task<(PaymentAttemptResult Result, Order? Order)> PayOrderAsync(Guid id, decimal amount, string paymentMethod)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var affected = await _context.Orders
                    .Where(o => o.Id == id && o.Status == "Đang xử lý" && o.PaidAmount <= 0 && o.TotalAmount == amount)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(o => o.PaidAmount, amount)
                        .SetProperty(o => o.PaymentMethod, paymentMethod)
                        .SetProperty(o => o.PaymentAt, DateTime.UtcNow)
                        .SetProperty(o => o.Status, "Hoàn thành"));

                if (affected == 1)
                {
                    await _loyaltyService.EarnPointsAsync(id);
                    await transaction.CommitAsync();
                    return (PaymentAttemptResult.Paid, await GetOrderByIdAsync(id));
                }

                var current = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
                if (current == null)
                    return (PaymentAttemptResult.NotFound, null);
                if (current.PaidAmount > 0 || string.Equals(current.Status, "Hoàn thành", StringComparison.OrdinalIgnoreCase))
                    return (PaymentAttemptResult.AlreadyPaid, current);
                return (PaymentAttemptResult.NotPayable, current);
            });
        }

        public async Task<Order?> UpdateOrderStatusAsync(Guid id, OrderUpdateDto update)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var order = await _context.Orders.Include(o => o.Details).FirstOrDefaultAsync(o => o.Id == id);
                if (order == null) return null;

                if (IsTerminal(order.Status))
                    throw new InvalidOperationException("Completed or cancelled orders cannot be changed.");

                var oldStatus = order.Status;
                order.Status = update.Status;
                if (!string.IsNullOrEmpty(update.CreatedBy)) order.CreatedBy = update.CreatedBy;
                if (!string.IsNullOrEmpty(update.BranchName)) order.BranchName = update.BranchName;

                if (!IsTerminal(oldStatus) && (order.Status == "Hoàn thành" || order.Status == "Completed"))
                {
                    order.PaymentAt ??= DateTime.UtcNow;
                    // Save changes first to ensure order is marked as completed for EarnPointsAsync check
                    await _context.SaveChangesAsync();
                    await _loyaltyService.EarnPointsAsync(id);
                }
                else if (!IsTerminal(oldStatus) && (order.Status == "Đã hủy" || order.Status == "Cancelled"))
                {
                    // Save changes first to ensure order is marked as cancelled for RefundPointsAsync check
                    await _context.SaveChangesAsync();
                    await _loyaltyService.RefundPointsAsync(id);
                }

                if (update.BranchId.HasValue && update.BranchId != order.BranchId)
                {
                    order.BranchId = update.BranchId;
                    await RecalculateOrderFinancials(order);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            });
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;

            if (IsTerminal(order.Status)) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> UpdateOrderStatusByTableOrInvoiceAsync(string? invoiceCode, string? tableName, string newStatus)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(invoiceCode))
            {
                query = query.Where(o => o.InvoiceCode == invoiceCode);
            }
            else if (!string.IsNullOrEmpty(tableName))
            {
                query = query.Where(o => o.TableName == tableName && o.Status == "Đang xử lý");
            }
            else
            {
                return (false, "Cần cung cấp mã hóa đơn hoặc tên bàn.");
            }

            var order = await query.FirstOrDefaultAsync();
            if (order == null)
            {
                return (false, "Không tìm thấy đơn hàng phù hợp để cập nhật.");
            }

            var oldStatus = order.Status;
            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return (true, $"Đã cập nhật trạng thái đơn '{order.InvoiceCode}' (Bàn: {order.TableName}) từ '{oldStatus}' thành '{newStatus}'.");
        }

        public async Task<(bool Success, string Message)> UpdateOrderStatusByCodeAsync(string orderCode, string newStatus, Guid? userBranchId, string role)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.InvoiceCode == orderCode);
            if (order == null)
            {
                return (false, $"Không tìm thấy đơn hàng mã '{orderCode}'.");
            }

            // Kiểm tra quyền theo chi nhánh (trừ Admin)
            if (!role.Equals("admin", StringComparison.OrdinalIgnoreCase) && order.BranchId != userBranchId)
            {
                return (false, "Bạn không có quyền cập nhật đơn hàng của chi nhánh khác.");
            }

            var oldStatus = order.Status;
            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return (true, $"Đã cập nhật đơn hàng {orderCode} từ '{oldStatus}' sang '{newStatus}' thành công.");
        }

        public async Task<List<Order>> GetCustomerOrdersAsync(string customerPhone)
        {
            return await _context.Orders
                .Include(o => o.Details)
                .Where(o => o.CustomerPhone == customerPhone)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByCustomerAsync(string? email, string? phone)
        {
            var query = _context.Orders.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(phone))
            {
                query = query.Where(o => o.CustomerEmail == email || o.CustomerPhone == phone);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(o => o.CustomerEmail == email);
            }
            else if (!string.IsNullOrEmpty(phone))
            {
                query = query.Where(o => o.CustomerPhone == phone);
            }
            else
            {
                return new List<Order>();
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();
        }

        private static bool IsTerminal(string? status) =>
            string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Hoàn thành", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Đã hủy", StringComparison.OrdinalIgnoreCase);
    }
}
