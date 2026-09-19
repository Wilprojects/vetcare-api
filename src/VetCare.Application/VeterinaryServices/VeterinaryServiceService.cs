using Microsoft.Extensions.Logging;
using VetCare.Application.Common.Exceptions;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Persistence;
using VetCare.Application.VeterinaryServices.Models;
using VetCare.Application.VeterinaryServices.Repositories;
using VetCare.Domain.Common;
using VetCare.Domain.Entities;

namespace VetCare.Application.VeterinaryServices;

public sealed class VeterinaryServiceService(IVeterinaryServiceRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider, ILogger<VeterinaryServiceService> logger) : IVeterinaryServiceService
{
    public async Task<PagedResult<VeterinaryServiceResult>> GetPublicPagedAsync(VeterinaryServiceSearchInput input, CancellationToken cancellationToken = default)
    {
        var normalizedInput = NormalizeSearchInput(input with { IncludeInactive = false });

        ValidateSearchInput(normalizedInput);

        var result = await repository.GetPagedAsync(normalizedInput, cancellationToken);

        return MapPage(result);
    }

    public async Task<VeterinaryServiceResult> GetPublicByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null || !service.IsActive)
        {
            throw ServiceNotFound();
        }

        return Map(service);
    }

    public async Task<PagedResult<VeterinaryServiceResult>> GetAdminPagedAsync(VeterinaryServiceSearchInput input, CancellationToken cancellationToken = default)
    {
        var normalizedInput = NormalizeSearchInput(input);

        ValidateSearchInput(normalizedInput);

        var result = await repository.GetPagedAsync(normalizedInput, cancellationToken);

        return MapPage(result);
    }

    public async Task<VeterinaryServiceResult> CreateAsync(CreateVeterinaryServiceInput input, CancellationToken cancellationToken = default)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        VeterinaryService service;

        try
        {
            service = VeterinaryService.Create(
                    input.Name,
                    input.Description,
                    input.DurationMinutes,
                    input.Price,
                    nowUtc
                );
        }
        catch (DomainException exception)
        {
            throw new BusinessRuleException(exception.Code, exception.Message);
        }

        var nameExists = await repository.ExistsByNameAsync(service.Name, cancellationToken: cancellationToken);

        if (nameExists)
        {
            throw new ConflictException("SERVICE_NAME_ALREADY_EXISTS", "Ya existe un servicio veterinario con ese nombre.");
        }

        await repository.AddAsync(service, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Veterinary service {ServiceId} created with name {ServiceName}", service.Id, service.Name);

        return Map(service);
    }

    public async Task<VeterinaryServiceResult> UpdateAsync(Guid serviceId, UpdateVeterinaryServiceInput input, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
        {
            throw ServiceNotFound();
        }

        if (!service.IsActive)
        {
            throw new ConflictException("SERVICE_INACTIVE", "No se puede modificar un servicio veterinario inactivo.");
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            var normalizedName = input.Name.Trim();

            var nameExists = await repository.ExistsByNameAsync(normalizedName, service.Id, cancellationToken);

            if (nameExists)
            {
                throw new ConflictException("SERVICE_NAME_ALREADY_EXISTS", "Ya existe un servicio veterinario con ese nombre.");
            }
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            service.Update(
                input.Name,
                input.Description,
                input.DurationMinutes,
                input.Price,
                nowUtc
            );
        }
        catch (DomainException exception)
        {
            throw new BusinessRuleException(exception.Code, exception.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Veterinary service {ServiceId} updated", service.Id);

        return Map(service);
    }

    public async Task DeactivateAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
        {
            throw ServiceNotFound();
        }

        if (!service.IsActive)
        {
            return;
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            service.Deactivate(nowUtc);
        }
        catch (DomainException exception)
        {
            throw new BusinessRuleException(exception.Code, exception.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Veterinary service {ServiceId} deactivated", service.Id);
    }

    private static VeterinaryServiceSearchInput NormalizeSearchInput(VeterinaryServiceSearchInput input)
    {
        return input with
        {
            Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim(),

            SortBy = input.SortBy.Trim().ToLowerInvariant(),

            SortDirection = input.SortDirection.Trim().ToLowerInvariant()
        };
    }

    private static void ValidateSearchInput(VeterinaryServiceSearchInput input)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (input.PageNumber < 1)
        {
            errors["pageNumber"] =
            [
                "El número de página debe ser mayor o igual a 1."
            ];
        }

        if (input.PageSize is < 1 or > 100)
        {
            errors["pageSize"] =
            [
                "El tamaño de página debe estar entre 1 y 100."
            ];
        }

        if (input.Search is { Length: > 150 })
        {
            errors["search"] =
            [
                "La búsqueda no puede superar los 150 caracteres."
            ];
        }

        if (input.SortBy is not
            ("name"
                or "price"
                or "duration"
                or "createdat"))
        {
            errors["sortBy"] =
            [
                "sortBy debe ser name, price, duration o createdAt."
            ];
        }

        if (input.SortDirection is not
            ("asc" or "desc"))
        {
            errors["sortDirection"] =
            [
                "sortDirection debe ser asc o desc."
            ];
        }

        if (errors.Count > 0)
        {
            throw new AppValidationException("VETERINARY_SERVICE_QUERY_INVALID", "Los parámetros de consulta no son válidos.", errors);
        }
    }

    private static VeterinaryServiceResult Map(VeterinaryService service)
    {
        return new VeterinaryServiceResult(
            service.Id,
            service.Name,
            service.Description,
            service.DurationMinutes,
            service.Price,
            service.IsActive,
            service.CreatedAtUtc,
            service.UpdatedAtUtc
        );
    }

    private static PagedResult<VeterinaryServiceResult> MapPage(PagedResult<VeterinaryService> page)
    {
        return new PagedResult<VeterinaryServiceResult>(
            page.Items.Select(Map).ToArray(),
            page.PageNumber,
            page.PageSize,
            page.TotalCount
        );
    }

    private static NotFoundException ServiceNotFound()
    {
        return new NotFoundException("VETERINARY_SERVICE_NOT_FOUND", "El servicio veterinario no existe.");
    }
}
