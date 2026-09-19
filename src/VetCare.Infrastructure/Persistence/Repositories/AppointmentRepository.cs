using Microsoft.EntityFrameworkCore;
using VetCare.Application.Appointments.Models;
using VetCare.Application.Appointments.Repositories;
using VetCare.Application.Common.Pagination;
using VetCare.Domain.Entities;
using VetCare.Domain.Enums;

namespace VetCare.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(VetCareDbContext dbContext) : IAppointmentRepository
{
    public async Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Appointments.SingleOrDefaultAsync(appointment => appointment.Id == appointmentId, cancellationToken);
    }

    public async Task<Appointment?> GetByIdForOwnerAsync(Guid appointmentId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var petIds = dbContext.Pets
                .Where(pet => pet.OwnerId == ownerId)
                .Select(pet => pet.Id);

        return await dbContext.Appointments
            .SingleOrDefaultAsync(appointment => appointment.Id == appointmentId && petIds.Contains(appointment.PetId), cancellationToken);
    }

    public async Task<PagedResult<Appointment>> GetPagedForOwnerAsync(Guid ownerId, AppointmentSearchInput input, CancellationToken cancellationToken = default)
    {
        var petIds = dbContext.Pets
                .Where(pet => pet.OwnerId == ownerId)
                .Select(pet => pet.Id);

        var query = dbContext.Appointments
                .AsNoTracking()
                .Where(appointment => petIds.Contains(appointment.PetId));

        return await GetPagedInternalAsync(query, input, cancellationToken);
    }

    public async Task<PagedResult<Appointment>> GetPagedAsync(AppointmentSearchInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Appointments
                .AsNoTracking()
                .AsQueryable();

        return await GetPagedInternalAsync(query, input, cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(DateTime scheduledStartUtc, DateTime scheduledEndUtc, Guid? excludingAppointmentId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Appointments
                .AsNoTracking()
                .Where(appointment =>
                    (
                        appointment.Status == AppointmentStatus.Pending || appointment.Status == AppointmentStatus.Confirmed
                    )
                    && appointment.ScheduledStartUtc < scheduledEndUtc
                    && scheduledStartUtc < appointment.ScheduledEndUtc);

        if (excludingAppointmentId.HasValue)
        {
            query = query.Where(appointment => appointment.Id != excludingAppointmentId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await dbContext.Appointments.AddAsync(appointment, cancellationToken);
    }

    private static async Task<PagedResult<Appointment>> GetPagedInternalAsync(IQueryable<Appointment> query, AppointmentSearchInput input, CancellationToken cancellationToken)
    {
        if (input.Status.HasValue)
        {
            query = query.Where(appointment => appointment.Status == input.Status.Value);
        }

        if (input.FromUtc.HasValue)
        {
            query = query.Where(appointment => appointment.ScheduledStartUtc >= input.FromUtc.Value);
        }

        if (input.ToUtc.HasValue)
        {
            query = query.Where(appointment => appointment.ScheduledStartUtc <= input.ToUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var descending = input.SortDirection == "desc";

        IOrderedQueryable<Appointment>
            orderedQuery = input.SortBy switch
            {
                "createdat" when descending =>
                    query.OrderByDescending(
                        appointment =>
                            appointment.CreatedAtUtc),

                "createdat" =>
                    query.OrderBy(
                        appointment =>
                            appointment.CreatedAtUtc),

                "scheduledstart" when descending =>
                    query.OrderByDescending(
                        appointment =>
                            appointment.ScheduledStartUtc),

                _ =>
                    query.OrderBy(
                        appointment =>
                            appointment.ScheduledStartUtc)
            };

        orderedQuery =
            descending
                ? orderedQuery.ThenByDescending(
                    appointment =>
                        appointment.Id)
                : orderedQuery.ThenBy(
                    appointment =>
                        appointment.Id);

        var skip = (input.PageNumber - 1) * input.PageSize;

        var items = await orderedQuery
                .Skip(skip)
                .Take(input.PageSize)
                .ToArrayAsync(cancellationToken);

        return new PagedResult<Appointment>(items, input.PageNumber, input.PageSize, totalCount);
    }
}
