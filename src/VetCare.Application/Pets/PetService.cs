using Microsoft.Extensions.Logging;
using VetCare.Application.Common.Exceptions;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Persistence;
using VetCare.Application.Common.Security;
using VetCare.Application.Pets.Models;
using VetCare.Application.Pets.Repositories;
using VetCare.Domain.Common;
using VetCare.Domain.Entities;
using VetCare.Domain.Enums;

namespace VetCare.Application.Pets;

public sealed class PetService(IPetRepository petRepository, IUnitOfWork unitOfWork, ICurrentUser currentUser, TimeProvider timeProvider, ILogger<PetService> logger) : IPetService
{
    public async Task<PagedResult<PetResult>> GetPagedAsync(PetSearchInput input, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var normalizedInput = NormalizeSearchInput(input);

        ValidateSearchInput(normalizedInput);

        var page = await petRepository.GetPagedAsync(ownerId, normalizedInput, cancellationToken);

        return new PagedResult<PetResult>(page.Items.Select(Map).ToArray(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<PetResult> GetByIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var pet = await petRepository.GetByIdAsync(petId, ownerId, cancellationToken);

        if (pet is null)
        {
            throw PetNotFound();
        }

        return Map(pet);
    }

    public async Task<PetResult> CreateAsync(CreatePetInput input, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        Pet pet;

        try
        {
            pet = Pet.Create(
                ownerId,
                input.Name,
                input.Species,
                input.Breed,
                input.Sex,
                input.BirthDate,
                input.WeightKg,
                nowUtc);
        }
        catch (DomainException exception)
        {
            throw new BusinessRuleException(exception.Code, exception.Message);
        }

        await petRepository.AddAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Pet {PetId} created by user {UserId}", pet.Id, ownerId);

        return Map(pet);
    }

    public async Task<PetResult> UpdateAsync(Guid petId, UpdatePetInput input, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var pet = await petRepository.GetByIdAsync(petId, ownerId, cancellationToken);

        if (pet is null)
        {
            throw PetNotFound();
        }

        if (!pet.IsActive)
        {
            throw new ConflictException("PET_INACTIVE", "No se puede modificar una mascota inactiva.");
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            pet.Update(
                input.Name,
                input.Species,
                input.Breed,
                input.Sex,
                input.BirthDate,
                input.WeightKg,
                nowUtc);
        }
        catch (DomainException exception)
        {
            throw new BusinessRuleException(exception.Code, exception.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pet {PetId} updated by user {UserId}", pet.Id, ownerId);

        return Map(pet);
    }

    public async Task DeactivateAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();
        var pet = await petRepository.GetByIdAsync(petId, ownerId, cancellationToken);

        if (pet is null)
        {
            throw PetNotFound();
        }

        if (!pet.IsActive)
        {
            return;
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var hasFutureAppointments = await petRepository.HasFutureActiveAppointmentsAsync(pet.Id, nowUtc, cancellationToken);

        if (hasFutureAppointments)
        {
            logger.LogWarning("Pet {PetId} could not be deactivated because it has future active appointments", pet.Id);
            throw new ConflictException("PET_HAS_FUTURE_APPOINTMENTS", "La mascota tiene citas futuras activas y no puede ser desactivada.");
        }

        try
        {
            pet.Deactivate(nowUtc);
        }
        catch (DomainException exception)
        {
            throw new BusinessRuleException(exception.Code, exception.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Pet {PetId} deactivated by user {UserId}", pet.Id, ownerId);
    }

    private Guid GetRequiredUserId()
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("INVALID_AUTHENTICATED_USER", "No fue posible determinar el usuario autenticado.");
        }

        return userId;
    }

    private static PetSearchInput NormalizeSearchInput(PetSearchInput input)
    {
        return input with
        {
            Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim(),
            SortBy = input.SortBy?.Trim().ToLowerInvariant() ?? string.Empty,
            SortDirection = input.SortDirection?.Trim().ToLowerInvariant() ?? string.Empty
        };
    }

    private static void ValidateSearchInput(PetSearchInput input)
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

        if (input.Search is { Length: > 100 })
        {
            errors["search"] =
            [
                "La búsqueda no puede superar los 100 caracteres."
            ];
        }

        if (input.Species.HasValue && (!Enum.IsDefined(input.Species.Value) || input.Species.Value == PetSpecies.Unknown))
        {
            errors["species"] =
            [
                "La especie indicada no es válida."
            ];
        }

        if (input.SortBy is not ("name" or "createdat" or "birthdate"))
        {
            errors["sortBy"] =
            [
                "sortBy debe ser name, createdAt o birthDate."
            ];
        }

        if (input.SortDirection is not ("asc" or "desc"))
        {
            errors["sortDirection"] =
            [
                "sortDirection debe ser asc o desc."
            ];
        }

        if (errors.Count > 0)
        {
            throw new AppValidationException("PET_QUERY_INVALID", "Los parámetros de consulta de mascotas no son válidos.", errors);
        }
    }

    private static PetResult Map(Pet pet)
    {
        return new PetResult(
            pet.Id,
            pet.Name,
            pet.Species,
            pet.Breed,
            pet.Sex,
            pet.BirthDate,
            pet.WeightKg,
            pet.IsActive,
            pet.CreatedAtUtc,
            pet.UpdatedAtUtc
        );
    }

    private static NotFoundException PetNotFound()
    {
        return new NotFoundException("PET_NOT_FOUND", "La mascota no existe.");
    }
}
