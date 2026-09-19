using VetCare.Domain.Enums;

namespace VetCare.Application.Appointments.Models;

public sealed record AppointmentSearchInput(
    int PageNumber,
    int PageSize,
    AppointmentStatus? Status,
    DateTime? FromUtc,
    DateTime? ToUtc,
    string SortBy,
    string SortDirection
);
