using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetCare.Domain.Constants;
using VetCare.Domain.Entities;
using VetCare.Infrastructure.Authentication;

namespace VetCare.Infrastructure.Persistence.Configurations;

public sealed class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");

        builder.HasKey(pet => pet.Id);

        builder
            .Property(pet => pet.Id)
            .ValueGeneratedNever();

        builder
            .Property(pet => pet.OwnerId)
            .IsRequired();

        builder
            .Property(pet => pet.Name)
            .HasMaxLength(PetConstraints.NameMaxLength)
            .IsRequired();

        builder
            .Property(pet => pet.Species)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(pet => pet.Breed)
            .HasMaxLength(PetConstraints.BreedMaxLength);

        builder
            .Property(pet => pet.Sex)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(pet => pet.BirthDate)
            .HasColumnType("date");

        builder
            .Property(pet => pet.WeightKg)
            .HasPrecision(5, 2);

        builder
            .Property(pet => pet.IsActive)
            .IsRequired();

        builder
            .Property(pet => pet.CreatedAtUtc)
            .HasPrecision(0)
            .IsRequired();

        builder
            .Property(pet => pet.UpdatedAtUtc)
            .HasPrecision(0);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(pet => pet.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(pet => pet.OwnerId);

        builder
            .HasIndex(pet => new
            {
                pet.OwnerId,
                pet.IsActive
            });
    }
}
