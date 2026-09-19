using VetCare.Application.Appointments.Models;
using VetCare.Application.Common.Pagination;
using VetCare.Domain.Entities;

namespace VetCare.Application.Appointments.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdForOwnerAsync(
        Guid appointmentId,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Appointment>> GetPagedForOwnerAsync(
        Guid ownerId,
        AppointmentSearchInput input,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Appointment>> GetPagedAsync(
        AppointmentSearchInput input,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        DateTime scheduledStartUtc,
        DateTime scheduledEndUtc,
        Guid? excludingAppointmentId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default);
}
