using System.ComponentModel.DataAnnotations;
using VetCare.Domain.Constants;

namespace VetCare.Api.Contracts.Appointments;

public sealed class CancelAppointmentRequest
{
    [StringLength(AppointmentConstraints.CancellationReasonMaxLength)]
    public string? CancellationReason { get; init; }
}
