using VetCare.Domain.Enums;

namespace VetCare.Api.Contracts.Pets;

public sealed record PetResponse(
    Guid Id,
    string Name,
    PetSpecies Species,
    string? Breed,
    PetSex Sex,
    DateOnly? BirthDate,
    decimal? WeightKg,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
