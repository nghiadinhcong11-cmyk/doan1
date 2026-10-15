using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

var connectionString = $"Host=127.0.0.1;Port=55432;Database=restaurant_e2e;Username=e2e_admin;Password={Environment.GetEnvironmentVariable("E2E_DB_PASSWORD")};SSL Mode=Disable";
var branchA = Guid.Parse("c298cf4f-9810-4c22-b95b-fb94e5b2f375");
var customer = Guid.Parse("b0a4958f-7162-45a0-bde7-f3d253b65a9c");
var product = Guid.Parse("008d4b14-79b6-4609-b805-0fecc95c4efb");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var db = new ApplicationDbContext(options);
var settings = new SystemSettingService(db);
var loyalty = new LoyaltyService(db, settings);
var orders = new OrderService(db, settings, loyalty);
var kitchen = new KitchenService(db, new RecordingKitchenNotifier());

var order = new Order
{
    Id = Guid.NewGuid(),
    BranchId = branchA,
    BranchName = "E2E Branch A",
    TableName = "E2E Table A",
    CustomerId = customer,
    CustomerName = "E2E Customer",
    CustomerPhone = "0900000201",
    Status = "Đang xử lý",
    Details = [new OrderDetail
    {
        Id = Guid.NewGuid(), ProductId = product, ProductName = "E2E Product",
        Quantity = 2, SentQuantity = 0, UnitPrice = 1m, Options = null
    }]
};

var created = await orders.CreateOrUpdateOrderAsync(order);
Assert(created.SubTotal == 100000m, $"subtotal expected 100000, got {created.SubTotal}");
Assert(created.ServiceFeeAmount == 5000m, $"service fee expected 5000, got {created.ServiceFeeAmount}");
Assert(created.VatAmount == 10500m, $"VAT expected 10500, got {created.VatAmount}");
Assert(created.TotalAmount == 115500m, $"total expected 115500, got {created.TotalAmount}");

var accepted = await orders.AcceptWebOrderAsync(created.Id, null, branchA, "e2e-manager-a");
Assert(accepted.Result == WebOrderAcceptResult.Accepted, $"accept expected Accepted, got {accepted.Result}");

var kitchenRequest = await kitchen.SendToKitchenAsync(created.Id);
Assert(kitchenRequest.Items.Count == 1 && kitchenRequest.Items[0].Quantity == 2, "first kitchen request quantity mismatch");

created.Details[0].Quantity = 3;
var updated = await orders.CreateOrUpdateOrderAsync(created);
var reloaded = await orders.GetOrderByIdAsync(created.Id);
Assert(reloaded?.Details.Single().Quantity == 3, "order update quantity mismatch");
Assert(reloaded?.Details.Single().SentQuantity == 2, $"SentQuantity should remain 2, got {reloaded?.Details.Single().SentQuantity}");

var secondKitchenRequest = await kitchen.SendToKitchenAsync(created.Id);
Assert(secondKitchenRequest.Items.Count == 1 && secondKitchenRequest.Items[0].Quantity == 1, "incremental kitchen request should contain only one new item");

var wrongPayment = await orders.PayOrderAsync(created.Id, 1m, "Cash");
Assert(wrongPayment.Result == PaymentAttemptResult.NotPayable, $"wrong payment expected NotPayable, got {wrongPayment.Result}");

var paymentResults = await Task.WhenAll(
    PayWithFreshContextAsync(created.Id, reloaded!.TotalAmount),
    PayWithFreshContextAsync(created.Id, reloaded.TotalAmount));
Assert(paymentResults.Count(x => x == PaymentAttemptResult.Paid) == 1, "exactly one concurrent payment should succeed");
Assert(paymentResults.Count(x => x == PaymentAttemptResult.AlreadyPaid) == 1, "exactly one concurrent payment should be rejected as already paid");

var completed = await orders.UpdateOrderStatusAsync(created.Id, new RestaurantPOS.Application.DTOs.Orders.OrderUpdateDto { Status = "Hoàn thành" });
Assert(completed?.Status == "Hoàn thành", "order completion failed");
var earned = await loyalty.EarnPointsAsync(created.Id);
var earnedAgain = await loyalty.EarnPointsAsync(created.Id);
var balance = await loyalty.GetBalanceAsync(customer);
Assert(earned is null && earnedAgain is null && balance > 0, $"payment should earn points once and repeated earn should be idempotent; balance={balance}");

Console.WriteLine("PASS postgres-service-order-kitchen-payment-loyalty");
Console.WriteLine($"order={created.Id} subtotal={reloaded.SubTotal} serviceFee={reloaded.ServiceFeeAmount} vat={reloaded.VatAmount} total={reloaded.TotalAmount} firstKitchenQty={kitchenRequest.Items[0].Quantity} secondKitchenQty={secondKitchenRequest.Items[0].Quantity} loyaltyBalance={balance}");

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

async Task<PaymentAttemptResult> PayWithFreshContextAsync(Guid orderId, decimal amount)
{
    await using var paymentDb = new ApplicationDbContext(options);
    var paymentSettings = new SystemSettingService(paymentDb);
    var paymentLoyalty = new LoyaltyService(paymentDb, paymentSettings);
    var paymentOrders = new OrderService(paymentDb, paymentSettings, paymentLoyalty);
    return (await paymentOrders.PayOrderAsync(orderId, amount, "Cash")).Result;
}

sealed class RecordingKitchenNotifier : IKitchenNotifier
{
    public Task NotifyNewOrderRequestAsync(object payload) => Task.CompletedTask;
    public Task NotifyRequestStatusUpdatedAsync(Guid requestId, string status) => Task.CompletedTask;
    public Task NotifyRequestStatusUpdatedAsync(Guid requestId, string status, Guid? branchId) => Task.CompletedTask;
}
