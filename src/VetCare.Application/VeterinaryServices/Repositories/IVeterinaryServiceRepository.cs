using VetCare.Application.Common.Pagination;
using VetCare.Application.VeterinaryServices.Models;
using VetCare.Domain.Entities;

namespace VetCare.Application.VeterinaryServices.Repositories;

public interface IVeterinaryServiceRepository
{
    Task<VeterinaryService?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<PagedResult<VeterinaryService>> GetPagedAsync(VeterinaryServiceSearchInput input, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, Guid? excludingServiceId = null, CancellationToken cancellationToken = default);

    Task AddAsync(VeterinaryService service, CancellationToken cancellationToken = default);
}
