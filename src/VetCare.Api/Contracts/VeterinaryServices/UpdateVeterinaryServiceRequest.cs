using System.ComponentModel.DataAnnotations;
using VetCare.Domain.Constants;

namespace VetCare.Api.Contracts.VeterinaryServices;

public sealed class UpdateVeterinaryServiceRequest
{
    [Required]
    [StringLength(VeterinaryServiceConstraints.NameMaxLength)]
    public required string Name { get; init; }

    [Required]
    [StringLength(VeterinaryServiceConstraints.DescriptionMaxLength)]
    public required string Description { get; init; }

    [Range(VeterinaryServiceConstraints.MinimumDurationMinutes, VeterinaryServiceConstraints.MaximumDurationMinutes)]
    public int DurationMinutes { get; init; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal Price { get; init; }
}
