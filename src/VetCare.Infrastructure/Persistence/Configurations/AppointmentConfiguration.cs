using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetCare.Domain.Constants;
using VetCare.Domain.Entities;

namespace VetCare.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(appointment => appointment.Id);

        builder
            .Property(appointment => appointment.Id)
            .ValueGeneratedNever();

        builder
            .Property(appointment => appointment.PetId)
            .IsRequired();

        builder
            .Property(
                appointment => appointment.VeterinaryServiceId)
            .IsRequired();

        builder
            .Property(appointment => appointment.ScheduledStartUtc)
            .HasPrecision(0)
            .IsRequired();

        builder
            .Property(appointment => appointment.ScheduledEndUtc)
            .HasPrecision(0)
            .IsRequired();

        builder
            .Property(appointment => appointment.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        builder
            .Property(appointment => appointment.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(appointment => appointment.Reason)
            .HasMaxLength(
                AppointmentConstraints.ReasonMaxLength)
            .IsRequired();

        builder
            .Property(appointment => appointment.CancellationReason)
            .HasMaxLength(
                AppointmentConstraints.CancellationReasonMaxLength);

        builder
            .Property(appointment => appointment.CreatedAtUtc)
            .HasPrecision(0)
            .IsRequired();

        builder
            .Property(appointment => appointment.UpdatedAtUtc)
            .HasPrecision(0);

        builder
            .HasOne<Pet>()
            .WithMany()
            .HasForeignKey(appointment => appointment.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<VeterinaryService>()
            .WithMany()
            .HasForeignKey(
                appointment => appointment.VeterinaryServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(appointment => appointment.PetId);

        builder
            .HasIndex(
                appointment => appointment.VeterinaryServiceId);

        builder
            .HasIndex(
                appointment => appointment.ScheduledStartUtc);

        builder
            .HasIndex(appointment => new
            {
                appointment.Status,
                appointment.ScheduledStartUtc,
                appointment.ScheduledEndUtc
            });
    }
}
