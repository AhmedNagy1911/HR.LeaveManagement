using HR.LeaveManagement.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.LeaveManagement.Identity.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasData(
             new ApplicationUser
             {
                 Id = "8e445865-a24d-4543-a6c6-9443d048cdb9",
                 AccessFailedCount = 0,
                 ConcurrencyStamp = "369c1b56-dc44-4f7e-842a-9dca5e8bdfa5",
                 Email = "admin@localhost.com",
                 NormalizedEmail = "ADMIN@LOCALHOST.COM",
                 EmailConfirmed = true,
                 FirstName = "System",
                 LastName = "Admin",
                 LockoutEnabled = false,
                 UserName = "admin@localhost.com",
                 NormalizedUserName = "ADMIN@LOCALHOST.COM",
                 PasswordHash = "AQAAAAIAAYagAAAAEKHyL9S10i910Fb4vkskNISfwA4KXSbvy6/hDrKiCkKQqm+10Sf01KqMAZAgnn7Y1A==",   //Ahmed~nage2004
                 PhoneNumberConfirmed = false,
                 SecurityStamp = "e92e49cc-0601-43c5-bfcf-b9e8455c4874",
                 TwoFactorEnabled = false
             },
             new ApplicationUser
             {
                 Id = "9e224968-33e4-4652-b7b7-8574d048cdb9",
                 AccessFailedCount = 0,
                 ConcurrencyStamp = "5c81d164-a6ad-48c8-82fc-069989c43eee",
                 Email = "user@localhost.com",
                 NormalizedEmail = "USER@LOCALHOST.COM",
                 EmailConfirmed = true,
                 FirstName = "System",
                 LastName = "User",
                 LockoutEnabled = false,
                 UserName = "user@localhost.com",
                 NormalizedUserName = "USER@LOCALHOST.COM",
                 PasswordHash = "AQAAAAIAAYagAAAAEDUMqJSLZdjh1P+nKPgF+kyN7L6cA/blhq6wiVKIrLQmBaL4Exfdyzw02smB9kK1fQ==",//Ahmed~nage2004
                 PhoneNumberConfirmed = false,
                 SecurityStamp = "70e2c99f-fb94-49cb-8bb4-f1d1e34a4ff4",
                 TwoFactorEnabled = false
             }
        );
    }
}
