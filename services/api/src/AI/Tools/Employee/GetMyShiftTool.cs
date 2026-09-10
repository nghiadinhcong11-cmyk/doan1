using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Employee
{
    public class GetMyShiftTool : IAiTool
    {
        private readonly IEmployeeService _employeeService;
        public string Name => "get_my_shift";
        public string Description => "Xem lịch làm việc (ca làm) của bản thân nhân viên trong hôm nay.";
        public string[] AllowedRoles => new[] { "employee" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetMyShiftTool(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public object GetSchema() => new { name = Name, description = Description, parameters = new { type = "object", properties = new { } } };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);
                var today = nowVn.Date;

                var shifts = await _employeeService.GetShiftsByEmployeeIdAsync(userContext.UserId, today);

                if (!shifts.Any()) return AiToolResult.CreateSuccess("Hôm nay bạn không có ca làm việc nào được xếp lịch.");

                var result = string.Join("\n", shifts.Select(s =>
                    $"- Ca: {s.StartTime:HH:mm} đến {s.EndTime:HH:mm} ({s.Status})"));

                return AiToolResult.CreateSuccess($"Lịch làm việc của bạn hôm nay ({nowVn:dd/MM/yyyy}):\n{result}");
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy ca làm việc: {ex.Message}");
            }
        }
    }
}
