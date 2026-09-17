using VetCare.Application.Common.Pagination;
using VetCare.Application.Pets.Models;
using VetCare.Domain.Entities;

namespace VetCare.Application.Pets.Repositories;

public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(
        Guid petId,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Pet>> GetPagedAsync(
        Guid ownerId,
        PetSearchInput input,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Pet pet,
        CancellationToken cancellationToken = default);

    Task<bool> HasFutureActiveAppointmentsAsync(
        Guid petId,
        DateTime referenceUtc,
        CancellationToken cancellationToken = default);
}
