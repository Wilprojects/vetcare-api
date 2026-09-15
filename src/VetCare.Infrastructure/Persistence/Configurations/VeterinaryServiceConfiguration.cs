using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetCare.Domain.Constants;
using VetCare.Domain.Entities;

namespace VetCare.Infrastructure.Persistence.Configurations;

public sealed class VeterinaryServiceConfiguration : IEntityTypeConfiguration<VeterinaryService>
{
    public void Configure(EntityTypeBuilder<VeterinaryService> builder)
    {
        builder.ToTable("VeterinaryServices");

        builder.HasKey(service => service.Id);

        builder
            .Property(service => service.Id)
            .ValueGeneratedNever();

        builder
            .Property(service => service.Name)
            .HasMaxLength(
                VeterinaryServiceConstraints.NameMaxLength)
            .IsRequired();

        builder
            .Property(service => service.Description)
            .HasMaxLength(
                VeterinaryServiceConstraints.DescriptionMaxLength)
            .IsRequired();

        builder
            .Property(service => service.DurationMinutes)
            .IsRequired();

        builder
            .Property(service => service.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        builder
            .Property(service => service.IsActive)
            .IsRequired();

        builder
            .Property(service => service.CreatedAtUtc)
            .HasPrecision(0)
            .IsRequired();

        builder
            .Property(service => service.UpdatedAtUtc)
            .HasPrecision(0);

        builder
            .HasIndex(service => service.Name)
            .IsUnique();

        builder
            .HasIndex(service => service.IsActive);
    }
}
