using VetCare.Domain.Common;
using VetCare.Domain.Entities;

namespace VetCare.UnitTests.Domain;

public sealed class VeterinaryServiceTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 14, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidData_ShouldInitializeService()
    {
        // Act
        var service = VeterinaryService.Create(
            " Consulta general ",
            " Evaluación general de la mascota. ",
            30,
            75m,
            UtcNow);

        // Assert
        Assert.NotEqual(Guid.Empty, service.Id);
        Assert.Equal("Consulta general", service.Name);
        Assert.Equal(
            "Evaluación general de la mascota.",
            service.Description);
        Assert.Equal(30, service.DurationMinutes);
        Assert.Equal(75m, service.Price);
        Assert.True(service.IsActive);
        Assert.Equal(UtcNow, service.CreatedAtUtc);
        Assert.Null(service.UpdatedAtUtc);
    }

    [Fact]
    public void Create_WhenDurationIsTooShort_ShouldThrowDomainException()
    {
        // Act
        var exception = Assert.Throws<DomainException>(() =>
            VeterinaryService.Create(
                "Consulta general",
                "Evaluación general de la mascota.",
                10,
                75m,
                UtcNow));

        // Assert
        Assert.Equal(
            DomainErrorCodes.ServiceDurationOutOfRange,
            exception.Code);
    }

    [Fact]
    public void Create_WhenPriceIsNegative_ShouldThrowDomainException()
    {
        // Act
        var exception = Assert.Throws<DomainException>(() =>
            VeterinaryService.Create(
                "Consulta general",
                "Evaluación general de la mascota.",
                30,
                -1m,
                UtcNow));

        // Assert
        Assert.Equal(
            DomainErrorCodes.ServicePriceNegative,
            exception.Code);
    }

    [Fact]
    public void Update_WithValidData_ShouldModifyService()
    {
        // Arrange
        var service = CreateValidService();
        var updatedAtUtc = UtcNow.AddHours(1);

        // Act
        service.Update(
            "Consulta especializada",
            "Evaluación veterinaria especializada.",
            60,
            120m,
            updatedAtUtc);

        // Assert
        Assert.Equal("Consulta especializada", service.Name);
        Assert.Equal(
            "Evaluación veterinaria especializada.",
            service.Description);
        Assert.Equal(60, service.DurationMinutes);
        Assert.Equal(120m, service.Price);
        Assert.Equal(updatedAtUtc, service.UpdatedAtUtc);
    }

    [Fact]
    public void Deactivate_WhenServiceIsActive_ShouldMarkItAsInactive()
    {
        // Arrange
        var service = CreateValidService();
        var updatedAtUtc = UtcNow.AddHours(1);

        // Act
        service.Deactivate(updatedAtUtc);

        // Assert
        Assert.False(service.IsActive);
        Assert.Equal(updatedAtUtc, service.UpdatedAtUtc);
    }

    private static VeterinaryService CreateValidService()
    {
        return VeterinaryService.Create(
            "Consulta general",
            "Evaluación general de la mascota.",
            30,
            75m,
            UtcNow);
    }
}
