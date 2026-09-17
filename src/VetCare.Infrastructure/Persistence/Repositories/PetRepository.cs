using Microsoft.EntityFrameworkCore;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Pets.Models;
using VetCare.Application.Pets.Repositories;
using VetCare.Domain.Entities;
using VetCare.Domain.Enums;

namespace VetCare.Infrastructure.Persistence.Repositories;

public sealed class PetRepository(VetCareDbContext dbContext) : IPetRepository
{
    public async Task<Pet?> GetByIdAsync(Guid petId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Pets.SingleOrDefaultAsync(pet => pet.Id == petId && pet.OwnerId == ownerId, cancellationToken);
    }

    public async Task<PagedResult<Pet>> GetPagedAsync(Guid ownerId, PetSearchInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Pets.AsNoTracking().Where(pet => pet.OwnerId == ownerId);

        if (!input.IncludeInactive)
        {
            query = query.Where(pet => pet.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var search = input.Search;
            query = query.Where(pet => pet.Name.Contains(search) || (pet.Breed != null && pet.Breed.Contains(search)));
        }

        if (input.Species.HasValue)
        {
            query = query.Where(pet => pet.Species == input.Species.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var descending = input.SortDirection == "desc";

        IOrderedQueryable<Pet> orderedQuery = input.SortBy switch
        {
            "createdat" when descending => query.OrderByDescending(pet => pet.CreatedAtUtc),
            "createdat" => query.OrderBy(pet => pet.CreatedAtUtc),
            "birthdate" when descending => query.OrderByDescending(pet => pet.BirthDate),
            "birthdate" => query.OrderBy(pet => pet.BirthDate),
            "name" when descending => query.OrderByDescending(pet => pet.Name),

            _ => query.OrderBy(pet => pet.Name)
        };

        orderedQuery = descending ? orderedQuery.ThenByDescending(pet => pet.Id) : orderedQuery.ThenBy(pet => pet.Id);

        var skip = (input.PageNumber - 1) * input.PageSize;
        var items = await orderedQuery.Skip(skip).Take(input.PageSize).ToArrayAsync(cancellationToken);
        return new PagedResult<Pet>(items, input.PageNumber, input.PageSize, totalCount);
    }

    public async Task AddAsync(Pet pet, CancellationToken cancellationToken = default)
    {
        await dbContext.Pets.AddAsync(pet, cancellationToken);
    }

    public async Task<bool> HasFutureActiveAppointmentsAsync(Guid petId, DateTime referenceUtc, CancellationToken cancellationToken = default)
    {
        return await dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(appointment => appointment.PetId == petId && appointment.ScheduledStartUtc > referenceUtc && (appointment.Status == AppointmentStatus.Pending || appointment.Status == AppointmentStatus.Confirmed), cancellationToken);
    }
}
