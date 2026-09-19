using System.ComponentModel.DataAnnotations;
using VetCare.Domain.Enums;

namespace VetCare.Api.Contracts.Appointments;

public sealed class AppointmentQueryParameters
{
    [Range(1, int.MaxValue)]
    public int? PageNumber { get; init; }

    [Range(1, 100)]
    public int? PageSize { get; init; }

    [EnumDataType(typeof(AppointmentStatus))]
    public AppointmentStatus? Status { get; init; }

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }

    [StringLength(30)]
    public string? SortBy { get; init; }

    [StringLength(4)]
    public string? SortDirection { get; init; }
}
