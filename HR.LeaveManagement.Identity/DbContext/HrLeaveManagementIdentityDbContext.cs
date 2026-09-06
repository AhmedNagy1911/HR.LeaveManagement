using HR.LeaveManagement.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HR.LeaveManagement.Identity.DbContext;

public class HrLeaveManagementIdentityDbContext(
    DbContextOptions<HrLeaveManagementIdentityDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(HrLeaveManagementIdentityDbContext).Assembly);
    }
}