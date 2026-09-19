using System.ComponentModel.DataAnnotations;
using VetCare.Domain.Constants;

namespace VetCare.Api.Contracts.Appointments;

public sealed class RescheduleAppointmentRequest
{
    [Required]
    public DateTime? ScheduledStartUtc { get; init; }

    [Required]
    [StringLength(AppointmentConstraints.ReasonMaxLength)]
    public required string Reason { get; init; }
}
