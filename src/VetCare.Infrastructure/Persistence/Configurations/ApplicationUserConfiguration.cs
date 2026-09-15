using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetCare.Infrastructure.Authentication;

namespace VetCare.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder
            .Property(user => user.Id)
            .ValueGeneratedNever();

        builder
            .Property(user => user.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(user => user.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(user => user.CreatedAtUtc)
            .HasPrecision(0)
            .IsRequired();
    }
}
