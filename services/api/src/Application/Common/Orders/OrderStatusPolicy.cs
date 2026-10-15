using System;
using System.Collections.Generic;

namespace RestaurantPOS.Application.Common.Orders;

/// <summary>
/// Canonical allow-list and transition rules for the persisted Order.Status field.
/// OrderRequest.Status is a separate KDS state machine and is intentionally not handled here.
/// </summary>
public static class OrderStatusPolicy
{
    public const string InProgress = "Đang xử lý";
    public const string LegacyPending = "Pending";
    public const string Completed = "Hoàn thành";
    public const string LegacyCompleted = "Completed";
    public const string Cancelled = "Đã hủy";
    public const string LegacyCancelled = "Cancelled";

    private static readonly IReadOnlyDictionary<string, string> KnownStatuses =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [InProgress] = InProgress,
            [LegacyPending] = LegacyPending,
            [Completed] = Completed,
            [LegacyCompleted] = LegacyCompleted,
            [Cancelled] = Cancelled,
            [LegacyCancelled] = LegacyCancelled
        };

    public static bool TryNormalize(string? status, out string normalizedStatus)
    {
        normalizedStatus = string.Empty;
        if (string.IsNullOrWhiteSpace(status)) return false;

        if (KnownStatuses.TryGetValue(status.Trim(), out var canonicalStatus))
        {
            normalizedStatus = canonicalStatus;
            return true;
        }

        return false;
    }

    public static bool IsTerminal(string? status) =>
        TryNormalize(status, out var normalizedStatus) &&
        (string.Equals(normalizedStatus, Completed, StringComparison.Ordinal) ||
         string.Equals(normalizedStatus, LegacyCompleted, StringComparison.Ordinal) ||
         string.Equals(normalizedStatus, Cancelled, StringComparison.Ordinal) ||
         string.Equals(normalizedStatus, LegacyCancelled, StringComparison.Ordinal));

    public static bool IsInProgress(string? status) =>
        TryNormalize(status, out var normalizedStatus) &&
        (string.Equals(normalizedStatus, InProgress, StringComparison.Ordinal) ||
         string.Equals(normalizedStatus, LegacyPending, StringComparison.Ordinal));

    public static bool IsCompleted(string? status) =>
        TryNormalize(status, out var normalizedStatus) &&
        (string.Equals(normalizedStatus, Completed, StringComparison.Ordinal) ||
         string.Equals(normalizedStatus, LegacyCompleted, StringComparison.Ordinal));

    public static bool IsCancelled(string? status) =>
        TryNormalize(status, out var normalizedStatus) &&
        (string.Equals(normalizedStatus, Cancelled, StringComparison.Ordinal) ||
         string.Equals(normalizedStatus, LegacyCancelled, StringComparison.Ordinal));

    public static bool CanTransition(
        string? currentStatus,
        string? requestedStatus,
        out string normalizedRequestedStatus,
        out string failureReason)
    {
        normalizedRequestedStatus = string.Empty;
        failureReason = string.Empty;

        if (!TryNormalize(requestedStatus, out normalizedRequestedStatus))
        {
            failureReason = $"Trạng thái đơn hàng không hợp lệ: '{requestedStatus}'.";
            return false;
        }

        if (!TryNormalize(currentStatus, out var normalizedCurrentStatus))
        {
            failureReason = $"Trạng thái hiện tại của đơn hàng không được hỗ trợ: '{currentStatus}'.";
            return false;
        }

        if (IsTerminal(normalizedCurrentStatus))
        {
            failureReason = "Đơn hàng đã hoàn thành hoặc đã hủy nên không thể thay đổi.";
            return false;
        }

        if (string.Equals(normalizedCurrentStatus, normalizedRequestedStatus, StringComparison.Ordinal))
        {
            return true;
        }

        if (IsInProgress(normalizedCurrentStatus) && IsTerminal(normalizedRequestedStatus))
        {
            return true;
        }

        failureReason = $"Không thể chuyển trạng thái đơn hàng từ '{normalizedCurrentStatus}' sang '{normalizedRequestedStatus}'.";
        return false;
    }
}
