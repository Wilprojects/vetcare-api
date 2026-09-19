using Microsoft.EntityFrameworkCore;
using VetCare.Application.Common.Pagination;
using VetCare.Application.VeterinaryServices.Models;
using VetCare.Application.VeterinaryServices.Repositories;
using VetCare.Domain.Entities;

namespace VetCare.Infrastructure.Persistence.Repositories;

public sealed class VeterinaryServiceRepository(VetCareDbContext dbContext) : IVeterinaryServiceRepository
{
    public async Task<VeterinaryService?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.VeterinaryServices.SingleOrDefaultAsync(service => service.Id == serviceId, cancellationToken);
    }

    public async Task<PagedResult<VeterinaryService>> GetPagedAsync(VeterinaryServiceSearchInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.VeterinaryServices.AsNoTracking().AsQueryable();

        if (!input.IncludeInactive)
        {
            query = query.Where(service => service.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var search = input.Search;

            query = query.Where(service => service.Name.Contains(search) || service.Description.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var descending = input.SortDirection == "desc";

        IOrderedQueryable<VeterinaryService>
            orderedQuery = input.SortBy switch
            {
                "price" when descending => query.OrderByDescending(service => service.Price),

                "price" => query.OrderBy(service => service.Price),

                "duration" when descending => query.OrderByDescending(service => service.DurationMinutes),

                "duration" => query.OrderBy(service => service.DurationMinutes),

                "createdat" when descending => query.OrderByDescending(service => service.CreatedAtUtc),

                "createdat" => query.OrderBy(service => service.CreatedAtUtc),

                "name" when descending => query.OrderByDescending(service => service.Name),

                _ =>
                    query.OrderBy(service => service.Name)
            };

        orderedQuery = descending ? orderedQuery.ThenByDescending(service => service.Id) : orderedQuery.ThenBy(service => service.Id);

        var skip = (input.PageNumber - 1) * input.PageSize;

        var items = await orderedQuery
                .Skip(skip)
                .Take(input.PageSize)
                .ToArrayAsync(cancellationToken);

        return new PagedResult<VeterinaryService>(
            items,
            input.PageNumber,
            input.PageSize,
            totalCount);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludingServiceId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.VeterinaryServices
                .AsNoTracking()
                .Where(service => service.Name == name);

        if (excludingServiceId.HasValue)
        {
            query = query.Where(service => service.Id != excludingServiceId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(VeterinaryService service, CancellationToken cancellationToken = default)
    {
        await dbContext.VeterinaryServices.AddAsync(service, cancellationToken);
    }
}
