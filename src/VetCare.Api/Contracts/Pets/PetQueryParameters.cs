using System.ComponentModel.DataAnnotations;
using VetCare.Domain.Enums;

namespace VetCare.Api.Contracts.Pets;

public sealed class PetQueryParameters
{
    [Range(1, int.MaxValue)]
    public int? PageNumber { get; init; }

    [Range(1, 100)]
    public int? PageSize { get; init; }

    [StringLength(100)]
    public string? Search { get; init; }

    [EnumDataType(typeof(PetSpecies))]
    public PetSpecies? Species { get; init; }

    public bool? IncludeInactive { get; init; }

    [StringLength(20)]
    public string? SortBy { get; init; }

    [StringLength(4)]
    public string? SortDirection { get; init; }
}
