using System.ComponentModel.DataAnnotations;
using VetCare.Domain.Constants;
using VetCare.Domain.Enums;

namespace VetCare.Api.Contracts.Pets;

public sealed class UpdatePetRequest
{
    [Required]
    [StringLength(PetConstraints.NameMaxLength)]
    public required string Name { get; init; }

    [EnumDataType(typeof(PetSpecies))]
    public PetSpecies Species { get; init; }

    [StringLength(PetConstraints.BreedMaxLength)]
    public string? Breed { get; init; }

    [EnumDataType(typeof(PetSex))]
    public PetSex Sex { get; init; }

    public DateOnly? BirthDate { get; init; }

    [Range(typeof(decimal), "0.01", "200")]
    public decimal? WeightKg { get; init; }
}
