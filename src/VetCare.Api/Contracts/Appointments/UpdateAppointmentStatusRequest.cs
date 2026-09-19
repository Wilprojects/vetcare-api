using System.ComponentModel.DataAnnotations;
using VetCare.Domain.Constants;
using VetCare.Domain.Enums;

namespace VetCare.Api.Contracts.Appointments;

public sealed class UpdateAppointmentStatusRequest
{
    [Required]
    [EnumDataType(typeof(AppointmentStatus))]
    public AppointmentStatus? Status { get; init; }

    [StringLength(AppointmentConstraints.CancellationReasonMaxLength)]
    public string? CancellationReason { get; init; }
}
