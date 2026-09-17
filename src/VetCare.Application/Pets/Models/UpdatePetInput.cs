using VetCare.Domain.Enums;

namespace VetCare.Application.Pets.Models;

public sealed record UpdatePetInput(
    string Name,
    PetSpecies Species,
    string? Breed,
    PetSex Sex,
    DateOnly? BirthDate,
    decimal? WeightKg
);
