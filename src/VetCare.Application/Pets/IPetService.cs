using VetCare.Application.Common.Pagination;
using VetCare.Application.Pets.Models;

namespace VetCare.Application.Pets;

public interface IPetService
{
    Task<PagedResult<PetResult>> GetPagedAsync(
        PetSearchInput input,
        CancellationToken cancellationToken = default);

    Task<PetResult> GetByIdAsync(
        Guid petId,
        CancellationToken cancellationToken = default);

    Task<PetResult> CreateAsync(
        CreatePetInput input,
        CancellationToken cancellationToken = default);

    Task<PetResult> UpdateAsync(
        Guid petId,
        UpdatePetInput input,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid petId,
        CancellationToken cancellationToken = default);
}
