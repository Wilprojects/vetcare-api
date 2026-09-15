using VetCare.Domain.Common;
using VetCare.Domain.Entities;
using VetCare.Domain.Enums;

namespace VetCare.UnitTests.Domain;

public sealed class AppointmentTests
{
    private static readonly DateTime UtcNow = new(
        2026,
        9,
        14,
        15,
        0,
        0,
        DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidData_ShouldInitializePendingAppointment()
    {
        // Arrange
        var startUtc = UtcNow.AddDays(1);

        // Act
        var appointment = Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            startUtc,
            30,
            75m,
            "Control preventivo anual",
            UtcNow);

        // Assert
        Assert.NotEqual(Guid.Empty, appointment.Id);
        Assert.Equal(startUtc, appointment.ScheduledStartUtc);
        Assert.Equal(
            startUtc.AddMinutes(30),
            appointment.ScheduledEndUtc);
        Assert.Equal(75m, appointment.Price);
        Assert.Equal(
            AppointmentStatus.Pending,
            appointment.Status);
        Assert.Equal(
            "Control preventivo anual",
            appointment.Reason);
        Assert.Null(appointment.CancellationReason);
        Assert.Equal(UtcNow, appointment.CreatedAtUtc);
    }

    [Fact]
    public void Create_WhenStartIsNotInFuture_ShouldThrowDomainException()
    {
        // Act
        var exception = Assert.Throws<DomainException>(() =>
            Appointment.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                UtcNow,
                30,
                75m,
                "Control preventivo",
                UtcNow));

        // Assert
        Assert.Equal(
            DomainErrorCodes.AppointmentStartMustBeFuture,
            exception.Code);
    }

    [Fact]
    public void Confirm_WhenAppointmentIsPending_ShouldChangeStatus()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        var updatedAtUtc = UtcNow.AddMinutes(5);

        // Act
        appointment.Confirm(updatedAtUtc);

        // Assert
        Assert.Equal(
            AppointmentStatus.Confirmed,
            appointment.Status);
        Assert.Equal(updatedAtUtc, appointment.UpdatedAtUtc);
    }

    [Fact]
    public void Complete_WhenAppointmentIsConfirmed_ShouldChangeStatus()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        appointment.Confirm(UtcNow.AddMinutes(5));

        var completedAtUtc = UtcNow.AddMinutes(10);

        // Act
        appointment.Complete(completedAtUtc);

        // Assert
        Assert.Equal(
            AppointmentStatus.Completed,
            appointment.Status);
        Assert.Equal(completedAtUtc, appointment.UpdatedAtUtc);
    }

    [Fact]
    public void Complete_WhenAppointmentIsPending_ShouldThrowDomainException()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        var exception = Assert.Throws<DomainException>(() =>
            appointment.Complete(UtcNow.AddMinutes(5)));

        // Assert
        Assert.Equal(
            DomainErrorCodes.AppointmentInvalidStatusTransition,
            exception.Code);
    }

    [Fact]
    public void Reschedule_WhenAppointmentIsConfirmed_ShouldUpdateSchedule()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        appointment.Confirm(UtcNow.AddMinutes(5));

        var newStartUtc = UtcNow.AddDays(2);
        var updatedAtUtc = UtcNow.AddHours(1);

        // Act
        appointment.Reschedule(
            newStartUtc,
            45,
            "Control preventivo reprogramado",
            updatedAtUtc);

        // Assert
        Assert.Equal(
            AppointmentStatus.Confirmed,
            appointment.Status);
        Assert.Equal(newStartUtc, appointment.ScheduledStartUtc);
        Assert.Equal(
            newStartUtc.AddMinutes(45),
            appointment.ScheduledEndUtc);
        Assert.Equal(
            "Control preventivo reprogramado",
            appointment.Reason);
        Assert.Equal(updatedAtUtc, appointment.UpdatedAtUtc);
    }

    [Fact]
    public void Cancel_WhenAppointmentIsPending_ShouldStoreReason()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        var updatedAtUtc = UtcNow.AddMinutes(5);

        // Act
        appointment.Cancel(
            " No podré asistir. ",
            updatedAtUtc);

        // Assert
        Assert.Equal(
            AppointmentStatus.Cancelled,
            appointment.Status);
        Assert.Equal(
            "No podré asistir.",
            appointment.CancellationReason);
        Assert.Equal(updatedAtUtc, appointment.UpdatedAtUtc);
    }

    [Fact]
    public void Cancel_WhenAppointmentIsCompleted_ShouldThrowDomainException()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        appointment.Confirm(UtcNow.AddMinutes(5));
        appointment.Complete(UtcNow.AddMinutes(10));

        // Act
        var exception = Assert.Throws<DomainException>(() =>
            appointment.Cancel(
                "Intento de cancelación",
                UtcNow.AddMinutes(15)));

        // Assert
        Assert.Equal(
            DomainErrorCodes.AppointmentInvalidStatusTransition,
            exception.Code);
    }

    private static Appointment CreateValidAppointment()
    {
        return Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            UtcNow.AddDays(1),
            30,
            75m,
            "Control preventivo anual",
            UtcNow);
    }
}
