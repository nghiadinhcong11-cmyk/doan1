using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

using RestaurantPOS.AI.Utils;

namespace RestaurantPOS.AI.Tools.Customer
{
    public class CreateBookingTool : IAiTool
    {
        private readonly IReservationService _reservationService;
        public string Name => "create_booking";
        public string Description => "Đặt bàn trước tại nhà hàng. Cần thông tin thời gian, số khách, cơ sở và bàn (nếu có).";
        public string[] AllowedRoles => new[] { "customer" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Write;

        public CreateBookingTool(IReservationService reservationService) => _reservationService = reservationService;

        public object GetSchema() => new
        {
            name = Name,
            description = Description,
            parameters = new
            {
                type = "object",
                properties = new
                {
                    bookingTime = new { type = "string", description = "Thời gian đặt bàn (Định dạng: dd/MM/yyyy HH:mm)." },
                    guestCount = new { type = "integer", description = "Số lượng khách." },
                    branchName = new { type = "string", description = "Tên cơ sở/chi nhánh (Ví dụ: Cơ sở 1)." },
                    tableName = new { type = "string", description = "Tên bàn cụ thể nếu khách yêu cầu." },
                    note = new { type = "string", description = "Ghi chú thêm." }
                },
                required = new[] { "bookingTime", "guestCount", "branchName" }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                AiToolValidator.ValidateRequired(arguments, "bookingTime", "guestCount", "branchName");

                // 1. Phân tích thời gian từ AI (Hỗ trợ định dạng VN)
                string timeStr = AiToolValidator.GetString(arguments, "bookingTime");
                if (!DateTime.TryParseExact(timeStr, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime bookingTime))
                {
                    if (!DateTime.TryParse(timeStr, out bookingTime))
                        return AiToolResult.CreateError("Định dạng thời gian không hợp lệ. Vui lòng cung cấp dạng dd/MM/yyyy HH:mm.");
                }

                int guestCount = AiToolValidator.GetInt32(arguments, "guestCount", min: 1, max: 100);
                string branchName = AiToolValidator.GetString(arguments, "branchName");
                string tableName = AiToolValidator.GetString(arguments, "tableName", required: false);
                string note = AiToolValidator.GetString(arguments, "note", required: false);

                var result = await _reservationService.CreateBookingFromAiAsync(
                    userContext.UserName ?? "Khách hàng AI",
                    userContext.PhoneNumber,
                    bookingTime,
                    guestCount,
                    branchName,
                    tableName,
                    note
                );

                if (!result.Success)
                {
                    return AiToolResult.CreateError(result.Message);
                }

                return AiToolResult.CreateSuccess(result.Message);
            }
            catch (ArgumentException ex)
            {
                return AiToolResult.CreateError(ex.Message);
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
