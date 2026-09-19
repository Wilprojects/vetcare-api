namespace VetCare.Application.VeterinaryServices.Models;

public sealed record VeterinaryServiceSearchInput(
    int PageNumber,
    int PageSize,
    string? Search,
    bool IncludeInactive,
    string SortBy,
    string SortDirection
);
