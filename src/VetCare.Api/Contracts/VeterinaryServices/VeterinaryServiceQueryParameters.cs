using System.ComponentModel.DataAnnotations;

namespace VetCare.Api.Contracts.VeterinaryServices;

public sealed class VeterinaryServiceQueryParameters
{
    [Range(1, int.MaxValue)]
    public int? PageNumber { get; init; }

    [Range(1, 100)]
    public int? PageSize { get; init; }

    [StringLength(150)]
    public string? Search { get; init; }

    [StringLength(20)]
    public string? SortBy { get; init; }

    [StringLength(4)]
    public string? SortDirection { get; init; }
}
