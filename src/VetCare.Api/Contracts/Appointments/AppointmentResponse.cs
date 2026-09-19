using VetCare.Domain.Enums;

namespace VetCare.Api.Contracts.Appointments;

public sealed record AppointmentResponse(
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
