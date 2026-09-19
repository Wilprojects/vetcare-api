using VetCare.Application.Common.Pagination;
using VetCare.Application.VeterinaryServices.Models;

namespace VetCare.Application.VeterinaryServices;

public interface IVeterinaryServiceService
{
    Task<PagedResult<VeterinaryServiceResult>> GetPublicPagedAsync(VeterinaryServiceSearchInput input, CancellationToken cancellationToken = default);

    Task<VeterinaryServiceResult> GetPublicByIdAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<PagedResult<VeterinaryServiceResult>> GetAdminPagedAsync(VeterinaryServiceSearchInput input, CancellationToken cancellationToken = default);

    Task<VeterinaryServiceResult> CreateAsync(CreateVeterinaryServiceInput input, CancellationToken cancellationToken = default);

    Task<VeterinaryServiceResult> UpdateAsync(Guid serviceId, UpdateVeterinaryServiceInput input, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
