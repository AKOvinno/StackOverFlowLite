using StackOverflowLite.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StackOverflowLite.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ApplicationUser (the Identity user).
/// Lives in Infrastructure — the only layer that owns Identity.
/// This is NOT picked up by ApplyConfigurationsFromAssembly in Persistence
/// because it lives in a different assembly; it is applied manually in DbContext.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.Reputation)
            .HasDefaultValue(0);
    }
}
