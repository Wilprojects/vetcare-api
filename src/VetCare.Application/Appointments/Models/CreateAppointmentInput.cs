namespace VetCare.Application.Appointments.Models;

public sealed record CreateAppointmentInput(
    Guid PetId,
    Guid VeterinaryServiceId,
    DateTime ScheduledStartUtc,
    string Reason
);
