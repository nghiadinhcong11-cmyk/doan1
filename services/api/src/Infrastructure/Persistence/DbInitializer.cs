using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantPOS.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            if (!await context.Branches.AnyAsync())
            {
                // ...
                var mainBranch = new Branch
                {
                    Id = Guid.NewGuid(),
                    Name = "Chi nhánh Trung Tâm",
                    Address = "123 Đường Cầu Giấy, Hà Nội",
                    PhoneNumber = "0988888888",
                    IsMain = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Branches.Add(mainBranch);
                await context.SaveChangesAsync();

                if (!await context.Areas.AnyAsync())
                {
                    var area1 = new Area { Id = Guid.NewGuid(), Name = "Tầng 1", DisplayOrder = 1, CreatedAt = DateTime.UtcNow };
                    var area2 = new Area { Id = Guid.NewGuid(), Name = "Tầng 2", DisplayOrder = 2, CreatedAt = DateTime.UtcNow };
                    var areaOutdoor = new Area { Id = Guid.NewGuid(), Name = "Sân Vườn", DisplayOrder = 3, CreatedAt = DateTime.UtcNow };

                    context.Areas.AddRange(area1, area2, areaOutdoor);
                    await context.SaveChangesAsync();

                    if (!await context.Tables.AnyAsync())
                    {
                        context.Tables.AddRange(
                            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn 01", AreaName = "Tầng 1", SeatCount = 4, Status = "Trống", BranchId = mainBranch.Id, BranchName = mainBranch.Name, CreatedAt = DateTime.UtcNow },
                            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn 02", AreaName = "Tầng 1", SeatCount = 4, Status = "Trống", BranchId = mainBranch.Id, BranchName = mainBranch.Name, CreatedAt = DateTime.UtcNow },
                            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn 03", AreaName = "Tầng 1", SeatCount = 6, Status = "Trống", BranchId = mainBranch.Id, BranchName = mainBranch.Name, CreatedAt = DateTime.UtcNow },
                            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn 11", AreaName = "Tầng 2", SeatCount = 4, Status = "Trống", BranchId = mainBranch.Id, BranchName = mainBranch.Name, CreatedAt = DateTime.UtcNow },
                            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn 12", AreaName = "Tầng 2", SeatCount = 8, Status = "Trống", BranchId = mainBranch.Id, BranchName = mainBranch.Name, CreatedAt = DateTime.UtcNow },
                            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn VIP", AreaName = "Tầng 2", SeatCount = 10, Status = "Trống", BranchId = mainBranch.Id, BranchName = mainBranch.Name, CreatedAt = DateTime.UtcNow },
                            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn SV1", AreaName = "Sân Vườn", SeatCount = 4, Status = "Trống", BranchId = mainBranch.Id, BranchName = mainBranch.Name, CreatedAt = DateTime.UtcNow }
                        );
                        await context.SaveChangesAsync();
                    }
                }

                if (!await context.Employees.AnyAsync())
                {
                    var admin = new Employee
                    {
                        Id = Guid.NewGuid(),
                        EmployeeCode = "NV00001",
                        FullName = "Quản Trị Viên",
                        Username = "admin",
                        Position = "Quản lý",
                        Role = "admin",
                        Department = "Điều hành",
                        BranchId = mainBranch.Id,
                        BranchName = mainBranch.Name,
                        PhoneNumber = "0988888888",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var hasher = serviceProvider.GetRequiredService<IPasswordHasher<Employee>>();
                    admin.Password = hasher.HashPassword(admin, "password");

                    context.Employees.Add(admin);
                    await context.SaveChangesAsync();
                }

                if (!await context.Products.AnyAsync())
                {
                    context.Products.AddRange(
                        new Product { Id = Guid.NewGuid(), Code = "DU001", Name = "Cà phê sữa đá", Category = "Đồ uống", Group = "Cà phê", Price = 29000, CostPrice = 12000, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Id = Guid.NewGuid(), Code = "DU002", Name = "Bạc xỉu", Category = "Đồ uống", Group = "Cà phê", Price = 32000, CostPrice = 14000, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Id = Guid.NewGuid(), Code = "DU003", Name = "Trà đào cam sả", Category = "Đồ uống", Group = "Trà trái cây", Price = 39000, CostPrice = 15000, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Id = Guid.NewGuid(), Code = "DU004", Name = "Trà vải lài", Category = "Đồ uống", Group = "Trà trái cây", Price = 39000, CostPrice = 15000, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Id = Guid.NewGuid(), Code = "DA001", Name = "Mì Ý bò bằm", Category = "Đồ ăn", Group = "Món chính", Price = 65000, CostPrice = 30000, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Id = Guid.NewGuid(), Code = "DA002", Name = "Bò bít tết sốt tiêu", Category = "Đồ ăn", Group = "Món chính", Price = 129000, CostPrice = 65000, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Id = Guid.NewGuid(), Code = "DA003", Name = "Khoai tây chiên", Category = "Đồ ăn", Group = "Khai vị", Price = 35000, CostPrice = 12000, IsActive = true, CreatedAt = DateTime.UtcNow }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
