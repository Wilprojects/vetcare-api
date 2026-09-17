using VetCare.Domain.Enums;

namespace VetCare.Application.Pets.Models;

public sealed record PetResult(
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
