using System.ComponentModel.DataAnnotations;
using VetCare.Domain.Constants;

namespace VetCare.Api.Contracts.Appointments;

public sealed class CreateAppointmentRequest
{
    [Required]
    public Guid? PetId { get; init; }

    [Required]
    public Guid? VeterinaryServiceId { get; init; }

    [Required]
    public DateTime? ScheduledStartUtc { get; init; }

    [Required]
    [StringLength(AppointmentConstraints.ReasonMaxLength)]
    public required string Reason { get; init; }
}
