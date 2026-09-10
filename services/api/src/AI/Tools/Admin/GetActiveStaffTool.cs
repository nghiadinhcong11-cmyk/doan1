using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class GetActiveStaffTool : IAiTool
    {
        private readonly IEmployeeService _employeeService;
        public string Name => "get_active_staff";
        public string Description => "Lấy danh sách nhân viên đang đi làm trong ngày hôm nay.";
        public string[] AllowedRoles => new[] { "admin", "manager" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetActiveStaffTool(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public object GetSchema() => new
        {
            name = Name,
            description = Description,
            parameters = new
            {
                type = "object",
                properties = new { },
                required = Array.Empty<string>()
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                var activeStaff = await _employeeService.GetActiveStaffAsync(userContext.BranchId);
                if (activeStaff.Count == 0)
                {
                    return AiToolResult.CreateSuccess("Hôm nay chưa có nhân viên nào điểm danh vào ca.");
                }

                return AiToolResult.CreateSuccess(JsonSerializer.Serialize(activeStaff));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy thông tin nhân sự: {ex.Message}");
            }
        }
    }
}
