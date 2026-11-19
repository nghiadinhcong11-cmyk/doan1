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
        private readonly INotificationService _notificationService;

        public ReservationService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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
                if (reservation.BranchId == Guid.Empty)
                {
                    reservation.BranchId = table.BranchId;
                    reservation.BranchName = table.BranchName;
                }
                else if (table.BranchId != reservation.BranchId)
                {
                    throw new InvalidOperationException("Bàn được chọn không thuộc chi nhánh này.");
                }
                reservation.TableName = table.Name;
            }

            if (reservation.BranchId == Guid.Empty)
                throw new InvalidOperationException("Đặt bàn phải thuộc một chi nhánh.");

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == reservation.BranchId && b.IsActive);
            if (branch == null)
                throw new InvalidOperationException("Chi nhánh không tồn tại hoặc đã ngừng hoạt động.");

            // Ensure BranchName is populated if BranchId is set
            if (string.IsNullOrEmpty(reservation.BranchName))
            {
                reservation.BranchName = branch.Name;
            }

            // Fix DateTime kind
            reservation.ReservationTime = DateTime.SpecifyKind(reservation.ReservationTime, DateTimeKind.Utc);
            ValidateReservationInput(reservation.NumberOfGuests, reservation.ReservationTime);

            if (reservation.TableId.HasValue)
            {
                var table = await _context.Tables.AsNoTracking().FirstAsync(t => t.Id == reservation.TableId.Value);
                if (!table.IsActive || table.Status == "Đã khóa" || table.Name == "Mang về")
                    throw new InvalidOperationException("Bàn đã ngừng hoạt động hoặc không nhận đặt trước.");
                if (reservation.NumberOfGuests > table.SeatCount)
                    throw new InvalidOperationException("Số khách vượt quá sức chứa của bàn.");
                await EnsureNoTableConflictAsync(reservation);
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            await NotifyReservationCreatedAsync(reservation);
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

            if (update.BranchId == Guid.Empty && res.BranchId == Guid.Empty)
                throw new InvalidOperationException("Đặt bàn phải thuộc một chi nhánh.");

            var isCustomerUpdate = !string.IsNullOrEmpty(authorizedCustomerPhone);
            if (isCustomerUpdate && update.BranchId != Guid.Empty && update.BranchId != res.BranchId)
                throw new InvalidOperationException("Khách hàng không được đổi chi nhánh của lịch đặt bàn.");

            // If branch is changing, validate it (only for staff who are NOT branch-restricted, i.e. Admins)
            if (!authorizedBranchId.HasValue && update.BranchId != Guid.Empty && update.BranchId != res.BranchId)
            {
                res.BranchId = update.BranchId;
                var branch = await _context.Branches.FindAsync(update.BranchId);
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

                if (table.BranchId != res.BranchId)
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
            res.CustomerPhone = isCustomerUpdate ? res.CustomerPhone : update.CustomerPhone;
            res.ReservationTime = DateTime.SpecifyKind(update.ReservationTime, DateTimeKind.Utc);
            res.NumberOfGuests = update.NumberOfGuests;
            res.Note = update.Note;
            if (!isCustomerUpdate)
            {
                ValidateStatus(update.Status);
                if (!IsAllowedStatusTransition(res.Status, update.Status))
                    throw new InvalidOperationException("Không thể chuyển trạng thái đặt bàn theo luồng hiện tại.");
                res.Status = update.Status;
            }
            else if (res.Status != "Pending")
            {
                throw new InvalidOperationException("Chỉ có thể chỉnh sửa lịch đang chờ xác nhận.");
            }

            ValidateReservationInput(res.NumberOfGuests, res.ReservationTime);
            if (res.TableId.HasValue)
            {
                var table = await _context.Tables.AsNoTracking().FirstAsync(t => t.Id == res.TableId.Value);
                if (!table.IsActive || table.Status == "Đã khóa" || table.Name == "Mang về")
                    throw new InvalidOperationException("Bàn đã ngừng hoạt động hoặc không nhận đặt trước.");
                if (res.NumberOfGuests > table.SeatCount)
                    throw new InvalidOperationException("Số khách vượt quá sức chứa của bàn.");
                await EnsureNoTableConflictAsync(res, res.Id);
            }

            await _context.SaveChangesAsync();
            return res;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid? authorizedBranchId)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return false;

            if (authorizedBranchId.HasValue && res.BranchId != authorizedBranchId.Value)
                return false;

            ValidateStatus(status);
            if (!IsAllowedStatusTransition(res.Status, status))
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

            // Keep reservation history; deletion is represented as cancellation.
            if (res.Status != "Cancelled" && res.Status != "Completed")
                res.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task EnsureNoTableConflictAsync(Reservation reservation, Guid? excludedId = null)
        {
            var windowStart = reservation.ReservationTime.AddHours(-2);
            var windowEnd = reservation.ReservationTime.AddHours(2);
            var conflict = await _context.Reservations.AnyAsync(existing =>
                existing.Id != excludedId &&
                existing.BranchId == reservation.BranchId &&
                existing.TableId == reservation.TableId &&
                (existing.Status == "Pending" || existing.Status == "Confirmed") &&
                existing.ReservationTime >= windowStart && existing.ReservationTime <= windowEnd);
            if (conflict)
                throw new InvalidOperationException("Bàn đã có lịch đặt trong khoảng thời gian này.");
        }

        private static void ValidateReservationInput(int numberOfGuests, DateTime reservationTime)
        {
            if (numberOfGuests <= 0) throw new InvalidOperationException("Số lượng khách phải lớn hơn 0.");
            if (reservationTime <= DateTime.UtcNow) throw new InvalidOperationException("Thời gian đặt bàn phải ở tương lai.");
        }

        private static void ValidateStatus(string status)
        {
            if (status is not ("Pending" or "Confirmed" or "Completed" or "Cancelled"))
                throw new InvalidOperationException("Trạng thái đặt bàn không hợp lệ.");
        }

        private static bool IsAllowedStatusTransition(string current, string next) =>
            current == next ||
            (current == "Pending" && (next == "Confirmed" || next == "Cancelled")) ||
            (current == "Confirmed" && (next == "Completed" || next == "Cancelled"));

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
            await NotifyReservationCreatedAsync(reservation);

            return (true, $"Đã ghi nhận yêu cầu đặt bàn của bạn tại {branch.Name}, bàn {tableName} cho {guestCount} người vào lúc {bookingTime:dd/MM/yyyy HH:mm}. Nhân viên sẽ sớm liên hệ xác nhận!", reservation);
        }

        private Task NotifyReservationCreatedAsync(Reservation reservation) => _notificationService.CreateAsync(new Notification
        {
            Type = "RESERVATION_CREATED",
            Title = "Có yêu cầu đặt bàn mới",
            Message = $"Khách {reservation.CustomerName} đặt bàn tại {reservation.BranchName} lúc {reservation.ReservationTime:dd/MM/yyyy HH:mm} cho {reservation.NumberOfGuests} người.",
            EntityType = "Reservation",
            EntityId = reservation.Id,
            Route = "/reservations",
            TargetRole = "admin",
            BranchId = reservation.BranchId
        });
    }
}
