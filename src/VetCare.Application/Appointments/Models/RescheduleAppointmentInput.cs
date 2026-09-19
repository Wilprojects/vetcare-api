namespace VetCare.Application.Appointments.Models;

public sealed record RescheduleAppointmentInput(
    DateTime ScheduledStartUtc,
    string Reason
);
