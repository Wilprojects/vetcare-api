using VetCare.Application.Appointments.Models;
using VetCare.Application.Common.Pagination;
using VetCare.Domain.Enums;

namespace VetCare.Application.Appointments;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentResult>>
        GetMyAppointmentsAsync(
            AppointmentSearchInput input,
            CancellationToken cancellationToken = default);

    Task<AppointmentResult> GetMyAppointmentAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);

    Task<AppointmentResult> CreateAsync(
        CreateAppointmentInput input,
        CancellationToken cancellationToken = default);

    Task<AppointmentResult> RescheduleAsync(
        Guid appointmentId,
        RescheduleAppointmentInput input,
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        Guid appointmentId,
        string? cancellationReason,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentResult>>
        GetAdminAppointmentsAsync(
            AppointmentSearchInput input,
            CancellationToken cancellationToken = default);

    Task<AppointmentResult> UpdateStatusAsync(
        Guid appointmentId,
        AppointmentStatus status,
        string? cancellationReason,
        CancellationToken cancellationToken = default);
}
