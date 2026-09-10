using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services
{
    public interface ILoyaltyService
    {
        /// <summary>Cộng điểm cho khách hàng dựa trên hóa đơn.</summary>
        Task<LoyaltyTransaction?> EarnPointsAsync(Guid orderId);

        /// <summary>Sử dụng điểm để giảm giá cho hóa đơn.</summary>
        Task<LoyaltyTransaction?> RedeemPointsAsync(Guid customerId, Guid orderId, int points);

        /// <summary>Hoàn lại điểm nếu hóa đơn bị hủy/hoàn tiền.</summary>
        Task<LoyaltyTransaction?> RefundPointsAsync(Guid orderId);

        /// <summary>Lấy số dư điểm hiện tại.</summary>
        Task<int> GetBalanceAsync(Guid customerId);

        /// <summary>Lấy lịch sử giao dịch điểm.</summary>
        Task<List<LoyaltyTransaction>> GetHistoryAsync(Guid customerId);

        /// <summary>Lấy thông tin tích/đổi điểm của một hóa đơn.</summary>
        Task<OrderLoyaltySummaryDto?> GetOrderLoyaltyAsync(Guid orderId);
    }

    public class OrderLoyaltySummaryDto
    {
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public int BalanceAfter { get; set; }
    }
}
