using VetCare.Domain.Common;
using VetCare.Domain.Constants;
using VetCare.Domain.Enums;

namespace VetCare.Domain.Entities;

public sealed class Pet : Entity
{
    private Pet()
    {
    }

    private Pet(Guid ownerId, string name, PetSpecies species, string? breed, PetSex sex, DateOnly? birthDate, decimal? weightKg, DateTime createdAtUtc)
        : base(createdAtUtc)
    {
        OwnerId = ValidateOwnerId(ownerId);
        Name = NormalizeName(name);
        Species = ValidateSpecies(species);
        Breed = NormalizeBreed(breed);
        Sex = ValidateSex(sex);
        BirthDate = ValidateBirthDate(birthDate, createdAtUtc);
        WeightKg = ValidateWeight(weightKg);
        IsActive = true;
    }

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PetSpecies Species { get; private set; }
    public string? Breed { get; private set; }
    public PetSex Sex { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public decimal? WeightKg { get; private set; }
    public bool IsActive { get; private set; }

    public static Pet Create(Guid ownerId, string name, PetSpecies species, string? breed, PetSex sex, DateOnly? birthDate, decimal? weightKg, DateTime createdAtUtc)
    {
        return new Pet(ownerId, name, species, breed, sex, birthDate, weightKg, createdAtUtc);
    }

    public void Update(string name, PetSpecies species, string? breed, PetSex sex, DateOnly? birthDate, decimal? weightKg, DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        var normalizedName = NormalizeName(name);
        var validatedSpecies = ValidateSpecies(species);
        var normalizedBreed = NormalizeBreed(breed);
        var validatedSex = ValidateSex(sex);
        var validatedBirthDate = ValidateBirthDate(birthDate, updatedAtUtc);
        var validatedWeight = ValidateWeight(weightKg);

        MarkAsUpdated(updatedAtUtc);

        Name = normalizedName;
        Species = validatedSpecies;
        Breed = normalizedBreed;
        Sex = validatedSex;
        BirthDate = validatedBirthDate;
        WeightKg = validatedWeight;
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        if (!IsActive)
        {
            return;
        }

        MarkAsUpdated(updatedAtUtc);
        IsActive = false;
    }

    private static Guid ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
        {
            throw new DomainException(DomainErrorCodes.PetOwnerRequired, "La mascota debe tener un propietario válido.");
        }

        return ownerId;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(DomainErrorCodes.PetNameRequired, "El nombre de la mascota es obligatorio.");
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > PetConstraints.NameMaxLength)
        {
            throw new DomainException(DomainErrorCodes.PetNameTooLong, $"El nombre de la mascota no puede superar " + $"{PetConstraints.NameMaxLength} caracteres.");
        }

        return normalizedName;
    }

    private static PetSpecies ValidateSpecies(PetSpecies species)
    {
        var isDefined = Enum.IsDefined(typeof(PetSpecies), species);

        if (!isDefined || species == PetSpecies.Unknown)
        {
            throw new DomainException(DomainErrorCodes.PetSpeciesInvalid, "La especie de la mascota no es válida.");
        }

        return species;
    }

    private static string? NormalizeBreed(string? breed)
    {
        if (string.IsNullOrWhiteSpace(breed))
        {
            return null;
        }

        var normalizedBreed = breed.Trim();

        if (normalizedBreed.Length > PetConstraints.BreedMaxLength)
        {
            throw new DomainException(DomainErrorCodes.PetBreedTooLong, $"La raza no puede superar " + $"{PetConstraints.BreedMaxLength} caracteres.");
        }

        return normalizedBreed;
    }

    private static PetSex ValidateSex(PetSex sex)
    {
        if (!Enum.IsDefined(typeof(PetSex), sex))
        {
            throw new DomainException(DomainErrorCodes.PetSexInvalid, "El sexo de la mascota no es válido.");
        }

        return sex;
    }

    private static DateOnly? ValidateBirthDate(DateOnly? birthDate, DateTime referenceUtc)
    {
        if (birthDate is null)
        {
            return null;
        }

        var currentDate = DateOnly.FromDateTime(referenceUtc);

        if (birthDate.Value > currentDate)
        {
            throw new DomainException(DomainErrorCodes.PetBirthDateInFuture, "La fecha de nacimiento no puede estar en el futuro.");
        }

        return birthDate;
    }

    private static decimal? ValidateWeight(decimal? weightKg)
    {
        if (weightKg is null)
        {
            return null;
        }

        if (weightKg.Value < PetConstraints.MinimumWeightKg || weightKg.Value > PetConstraints.MaximumWeightKg)
        {
            throw new DomainException(DomainErrorCodes.PetWeightOutOfRange, $"El peso debe encontrarse entre " + $"{PetConstraints.MinimumWeightKg} kg y " + $"{PetConstraints.MaximumWeightKg} kg.");
        }

        return weightKg;
    }
}
