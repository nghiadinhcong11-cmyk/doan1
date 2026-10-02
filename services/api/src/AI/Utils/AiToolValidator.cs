using System;
using System.Text.Json;
using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Utils
{
    public static class AiToolValidator
    {
        private static void EnsureObject(JsonElement arguments)
        {
            if (arguments.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Cấu trúc tham số không hợp lệ. Phải là một JSON Object.");
            }
        }

        public static void ValidateRequired(JsonElement arguments, params string[] requiredProps)
        {
            if (arguments.ValueKind == JsonValueKind.Undefined || arguments.ValueKind == JsonValueKind.Null)
            {
                if (requiredProps.Length > 0)
                    throw new ArgumentException("Thiếu toàn bộ tham số bắt buộc.");
                return;
            }

            EnsureObject(arguments);

            foreach (var prop in requiredProps)
            {
                if (!arguments.TryGetProperty(prop, out _))
                {
                    throw new ArgumentException($"Thiếu tham số bắt buộc: {prop}");
                }
            }
        }

        public static string GetString(JsonElement arguments, string propertyName, bool required = true)
        {
            if (arguments.ValueKind != JsonValueKind.Object && arguments.ValueKind != JsonValueKind.Undefined && arguments.ValueKind != JsonValueKind.Null)
            {
                 if (required) throw new ArgumentException($"Tham số {propertyName} không thể được trích xuất vì cấu trúc dữ liệu sai.");
                 return "";
            }

            if (arguments.ValueKind == JsonValueKind.Object && arguments.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Null)
                {
                    if (required) throw new ArgumentException($"Tham số {propertyName} không được null.");
                    return "";
                }

                var val = prop.GetString();
                if (required && string.IsNullOrWhiteSpace(val))
                {
                    throw new ArgumentException($"Tham số {propertyName} không được để trống.");
                }
                return val ?? "";
            }

            if (required)
            {
                throw new ArgumentException($"Thiếu tham số: {propertyName}");
            }

            return "";
        }

        public static decimal GetDecimal(JsonElement arguments, string propertyName, decimal? min = null, decimal? max = null)
        {
            if (arguments.ValueKind == JsonValueKind.Object && arguments.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind != JsonValueKind.Number)
                {
                    throw new ArgumentException($"Tham số {propertyName} phải là kiểu số.");
                }

                decimal val = prop.GetDecimal();
                if (min.HasValue && val < min.Value)
                {
                    throw new ArgumentException($"Tham số {propertyName} phải lớn hơn hoặc bằng {min.Value}.");
                }
                if (max.HasValue && val > max.Value)
                {
                    throw new ArgumentException($"Tham số {propertyName} phải nhỏ hơn hoặc bằng {max.Value}.");
                }
                return val;
            }

            throw new ArgumentException($"Thiếu tham số: {propertyName}");
        }

        public static int GetInt32(JsonElement arguments, string propertyName, int? min = null, int? max = null)
        {
            if (arguments.ValueKind == JsonValueKind.Object && arguments.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind != JsonValueKind.Number)
                {
                    throw new ArgumentException($"Tham số {propertyName} phải là kiểu số nguyên.");
                }

                int val = prop.GetInt32();
                if (min.HasValue && val < min.Value)
                {
                    throw new ArgumentException($"Tham số {propertyName} phải lớn hơn hoặc bằng {min.Value}.");
                }
                if (max.HasValue && val > max.Value)
                {
                    throw new ArgumentException($"Tham số {propertyName} phải nhỏ hơn hoặc bằng {max.Value}.");
                }
                return val;
            }

            throw new ArgumentException($"Thiếu tham số: {propertyName}");
        }

        public static Guid GetGuid(JsonElement arguments, string propertyName)
        {
            var val = GetString(arguments, propertyName);
            if (!Guid.TryParse(val, out var guid))
            {
                throw new ArgumentException($"Tham số {propertyName} không phải là ID hợp lệ (Guid).");
            }
            return guid;
        }

        public static void ValidateBranchIsolation(Guid? argumentBranchId, AiUserContext userContext)
        {
            if (userContext.Role?.ToLower() == "admin") return;

            if (argumentBranchId.HasValue && argumentBranchId != userContext.BranchId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập dữ liệu của chi nhánh khác.");
            }
        }
    }
}
