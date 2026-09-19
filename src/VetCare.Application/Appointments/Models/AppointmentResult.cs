using VetCare.Domain.Enums;

namespace VetCare.Application.Appointments.Models;

public sealed record AppointmentResult(
    Guid Id,
    Guid PetId,
    Guid VeterinaryServiceId,
    DateTime ScheduledStartUtc,
    DateTime ScheduledEndUtc,
    decimal Price,
    AppointmentStatus Status,
    string Reason,
    string? CancellationReason,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
