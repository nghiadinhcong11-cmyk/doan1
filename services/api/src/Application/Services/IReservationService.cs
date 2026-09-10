using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services
{
    public class ReservationQueryFilter
    {
        public Guid? BranchId { get; set; }
        public string? Status { get; set; }
        public string? CustomerPhone { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

    public interface IReservationService
    {
        Task<List<Reservation>> GetReservationsAsync(ReservationQueryFilter filter);
        Task<Reservation?> GetByIdAsync(Guid id, Guid? authorizedBranchId, string? authorizedCustomerPhone);
        Task<Reservation> CreateReservationAsync(Reservation reservation);
        Task<Reservation?> UpdateReservationAsync(Guid id, Reservation update, Guid? authorizedBranchId, string? authorizedCustomerPhone);
        Task<bool> UpdateStatusAsync(Guid id, string status, Guid? authorizedBranchId);
        Task<bool> DeleteReservationAsync(Guid id, Guid? authorizedBranchId, string? authorizedCustomerPhone);

        Task<(bool Success, string Message, Reservation? Reservation)> CreateBookingFromAiAsync(
            string customerName,
            string? customerPhone,
            DateTime bookingTime,
            int guestCount,
            string branchName,
            string? tableName,
            string? note);
    }
}
