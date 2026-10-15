using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RestaurantPOS.Infrastructure.Persistence.ApplicationDbContext))]
[Migration("20260905050736_AddExpenseBranchAndAudit")]
public partial class AddExpenseBranchAndAudit
{
}
