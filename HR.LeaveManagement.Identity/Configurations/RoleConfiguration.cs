using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.LeaveManagement.Identity.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "cac43a6e-f7bb-4448-baaf-1add431ccbbf",
                ConcurrencyStamp = "f8a9981c-5384-4522-b425-bb23a77b6f6c",
                Name = "Employee",
                NormalizedName = "EMPLOYEE"
            },
            new IdentityRole
            {
                Id = "cbc43a8e-f7bb-4445-baaf-1add431ffbbf",
                ConcurrencyStamp = "774a1d9f-02a1-4db6-8b5c-438109a3cf19",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            }
        );
    }
}
