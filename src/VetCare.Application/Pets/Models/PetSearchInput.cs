using VetCare.Domain.Enums;

namespace VetCare.Application.Pets.Models;

public sealed record PetSearchInput(
    int PageNumber,
    int PageSize,
    string? Search,
    PetSpecies? Species,
    bool IncludeInactive,
    string SortBy,
    string SortDirection
);
