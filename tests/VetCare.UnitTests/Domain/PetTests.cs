using VetCare.Domain.Common;
using VetCare.Domain.Entities;
using VetCare.Domain.Enums;

namespace VetCare.UnitTests.Domain;

public sealed class PetTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 14, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidData_ShouldInitializePet()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var pet = Pet.Create(
            ownerId,
            " Luna ",
            PetSpecies.Dog,
            " Labrador ",
            PetSex.Female,
            new DateOnly(2022, 5, 15),
            18.50m,
            UtcNow);

        // Assert
        Assert.NotEqual(Guid.Empty, pet.Id);
        Assert.Equal(ownerId, pet.OwnerId);
        Assert.Equal("Luna", pet.Name);
        Assert.Equal(PetSpecies.Dog, pet.Species);
        Assert.Equal("Labrador", pet.Breed);
        Assert.Equal(PetSex.Female, pet.Sex);
        Assert.Equal(new DateOnly(2022, 5, 15), pet.BirthDate);

        Assert.True(pet.WeightKg.HasValue);
        Assert.Equal(18.50m, pet.WeightKg.Value);

        Assert.True(pet.IsActive);
        Assert.Equal(UtcNow, pet.CreatedAtUtc);
        Assert.Null(pet.UpdatedAtUtc);
    }

    [Fact]
    public void Create_WhenBirthDateIsInFuture_ShouldThrowDomainException()
    {
        // Arrange
        var futureBirthDate =
            DateOnly.FromDateTime(UtcNow).AddDays(1);

        // Act
        var exception = Assert.Throws<DomainException>(() =>
            Pet.Create(
                Guid.NewGuid(),
                "Luna",
                PetSpecies.Dog,
                null,
                PetSex.Female,
                futureBirthDate,
                18.50m,
                UtcNow));

        // Assert
        Assert.Equal(
            DomainErrorCodes.PetBirthDateInFuture,
            exception.Code);
    }

    [Fact]
    public void Create_WhenSpeciesIsUnknown_ShouldThrowDomainException()
    {
        // Act
        var exception = Assert.Throws<DomainException>(() =>
            Pet.Create(
                Guid.NewGuid(),
                "Luna",
                PetSpecies.Unknown,
                null,
                PetSex.Unknown,
                null,
                null,
                UtcNow));

        // Assert
        Assert.Equal(
            DomainErrorCodes.PetSpeciesInvalid,
            exception.Code);
    }

    [Fact]
    public void Create_WhenWeightIsZero_ShouldThrowDomainException()
    {
        // Act
        var exception = Assert.Throws<DomainException>(() =>
            Pet.Create(
                Guid.NewGuid(),
                "Luna",
                PetSpecies.Dog,
                null,
                PetSex.Female,
                null,
                0m,
                UtcNow));

        // Assert
        Assert.Equal(
            DomainErrorCodes.PetWeightOutOfRange,
            exception.Code);
    }

    [Fact]
    public void Update_WithValidData_ShouldModifyPetDetails()
    {
        // Arrange
        var pet = CreateValidPet();
        var updatedAtUtc = UtcNow.AddHours(1);

        // Act
        pet.Update(
            "Luna Actualizada",
            PetSpecies.Dog,
            "Golden Retriever",
            PetSex.Female,
            new DateOnly(2022, 4, 10),
            20m,
            updatedAtUtc);

        // Assert
        Assert.Equal("Luna Actualizada", pet.Name);
        Assert.Equal("Golden Retriever", pet.Breed);
        Assert.Equal(20m, pet.WeightKg);
        Assert.Equal(updatedAtUtc, pet.UpdatedAtUtc);
    }

    [Fact]
    public void Deactivate_WhenPetIsActive_ShouldMarkItAsInactive()
    {
        // Arrange
        var pet = CreateValidPet();
        var updatedAtUtc = UtcNow.AddHours(1);

        // Act
        pet.Deactivate(updatedAtUtc);

        // Assert
        Assert.False(pet.IsActive);
        Assert.Equal(updatedAtUtc, pet.UpdatedAtUtc);
    }

    private static Pet CreateValidPet()
    {
        return Pet.Create(
            Guid.NewGuid(),
            "Luna",
            PetSpecies.Dog,
            "Labrador",
            PetSex.Female,
            new DateOnly(2022, 5, 15),
            18.50m,
            UtcNow);
    }
}
