using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Reservation>> GetReservationsAsync(ReservationQueryFilter filter)
        {
            var query = _context.Reservations.AsNoTracking().AsQueryable();

            if (filter.BranchId.HasValue)
                query = query.Where(r => r.BranchId == filter.BranchId.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(r => r.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.CustomerPhone))
                query = query.Where(r => r.CustomerPhone == filter.CustomerPhone);

            if (!string.IsNullOrEmpty(filter.FromDate) && DateTime.TryParse(filter.FromDate, out var fDate))
            {
                var from = DateTime.SpecifyKind(fDate.Date, DateTimeKind.Utc);
                query = query.Where(r => r.ReservationTime >= from);
            }

            if (!string.IsNullOrEmpty(filter.ToDate) && DateTime.TryParse(filter.ToDate, out var tDate))
            {
                var to = DateTime.SpecifyKind(tDate.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(r => r.ReservationTime < to);
            }

            return await query.OrderBy(r => r.ReservationTime).ToListAsync();
        }

        public async Task<Reservation?> GetByIdAsync(Guid id, Guid? authorizedBranchId, string? authorizedCustomerPhone)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return null;

            if (authorizedBranchId.HasValue && res.BranchId != authorizedBranchId.Value)
                return null;

            if (!string.IsNullOrEmpty(authorizedCustomerPhone) && !string.Equals(res.CustomerPhone?.Trim(), authorizedCustomerPhone.Trim(), StringComparison.OrdinalIgnoreCase))
                return null;

            return res;
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            if (reservation.Id == Guid.Empty)
                reservation.Id = Guid.NewGuid();

            if (reservation.CreatedAt == default)
                reservation.CreatedAt = DateTime.UtcNow;

            // Validate Table and Branch match
            if (reservation.TableId.HasValue)
            {
                var table = await _context.Tables.FindAsync(reservation.TableId.Value);
                if (table == null)
                {
                    throw new InvalidOperationException("Bàn được chọn không tồn tại.");
                }

                // If reservation.BranchId is not set, take it from the table
                if (!reservation.BranchId.HasValue)
                {
                    reservation.BranchId = table.BranchId;
                    reservation.BranchName = table.BranchName;
                }
                else if (table.BranchId != reservation.BranchId.Value)
                {
                    throw new InvalidOperationException("Bàn được chọn không thuộc chi nhánh này.");
                }
                reservation.TableName = table.Name;
            }

            // Ensure BranchName is populated if BranchId is set
            if (reservation.BranchId.HasValue && string.IsNullOrEmpty(reservation.BranchName))
            {
                var branch = await _context.Branches.FindAsync(reservation.BranchId.Value);
                if (branch != null) reservation.BranchName = branch.Name;
            }

            // Fix DateTime kind
            reservation.ReservationTime = DateTime.SpecifyKind(reservation.ReservationTime, DateTimeKind.Utc);

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<Reservation?> UpdateReservationAsync(Guid id, Reservation update, Guid? authorizedBranchId, string? authorizedCustomerPhone)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return null;

            if (authorizedBranchId.HasValue && res.BranchId != authorizedBranchId.Value)
                return null;

            if (!string.IsNullOrEmpty(authorizedCustomerPhone) && !string.Equals(res.CustomerPhone?.Trim(), authorizedCustomerPhone.Trim(), StringComparison.OrdinalIgnoreCase))
                return null;

            // If branch is changing, validate it (only for staff who are NOT branch-restricted, i.e. Admins)
            if (!authorizedBranchId.HasValue && update.BranchId.HasValue && update.BranchId != res.BranchId)
            {
                res.BranchId = update.BranchId;
                var branch = await _context.Branches.FindAsync(update.BranchId.Value);
                res.BranchName = branch?.Name;
            }

            // Validate Table and Branch match if table is updated
            if (update.TableId.HasValue)
            {
                var table = await _context.Tables.FindAsync(update.TableId.Value);
                if (table == null)
                {
                    throw new InvalidOperationException("Bàn được chọn không tồn tại.");
                }

                if (res.BranchId.HasValue && table.BranchId != res.BranchId.Value)
                {
                    throw new InvalidOperationException("Bàn được chọn không thuộc chi nhánh này.");
                }
                res.TableId = update.TableId;
                res.TableName = table.Name;
            }
            else if (update.TableId == null)
            {
                res.TableId = null;
                res.TableName = null;
            }

            res.CustomerName = update.CustomerName;
            res.CustomerPhone = update.CustomerPhone;
            res.ReservationTime = DateTime.SpecifyKind(update.ReservationTime, DateTimeKind.Utc);
            res.NumberOfGuests = update.NumberOfGuests;
            res.Note = update.Note;
            res.Status = update.Status;

            await _context.SaveChangesAsync();
            return res;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid? authorizedBranchId)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return false;

            if (authorizedBranchId.HasValue && res.BranchId != authorizedBranchId.Value)
                return false;

            res.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReservationAsync(Guid id, Guid? authorizedBranchId, string? authorizedCustomerPhone)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return false;

            if (authorizedBranchId.HasValue && res.BranchId != authorizedBranchId.Value)
                return false;

            if (!string.IsNullOrEmpty(authorizedCustomerPhone) && !string.Equals(res.CustomerPhone?.Trim(), authorizedCustomerPhone.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;

            _context.Reservations.Remove(res);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message, Reservation? Reservation)> CreateBookingFromAiAsync(
            string customerName,
            string? customerPhone,
            DateTime bookingTime,
            int guestCount,
            string branchName,
            string? tableName,
            string? note)
        {
            if (guestCount <= 0)
            {
                return (false, "Số lượng khách phải lớn hơn 0.", null);
            }

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Name.Contains(branchName));
            if (branch == null)
            {
                return (false, $"Không tìm thấy cơ sở nào tên là '{branchName}'.", null);
            }

            Guid? tableId = null;
            if (!string.IsNullOrEmpty(tableName))
            {
                var table = await _context.Tables.FirstOrDefaultAsync(t => t.BranchId == branch.Id && t.Name.Contains(tableName));
                tableId = table?.Id;
            }

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                CustomerName = string.IsNullOrWhiteSpace(customerName) ? "Khách hàng AI" : customerName,
                CustomerPhone = customerPhone,
                ReservationTime = DateTime.SpecifyKind(bookingTime, DateTimeKind.Utc),
                NumberOfGuests = guestCount,
                BranchId = branch.Id,
                BranchName = branch.Name,
                TableId = tableId,
                TableName = tableName,
                Note = note,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return (true, $"Đã ghi nhận yêu cầu đặt bàn của bạn tại {branch.Name}, bàn {tableName} cho {guestCount} người vào lúc {bookingTime:dd/MM/yyyy HH:mm}. Nhân viên sẽ sớm liên hệ xác nhận!", reservation);
        }
    }
}
