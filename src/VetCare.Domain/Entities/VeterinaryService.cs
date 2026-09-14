using VetCare.Domain.Common;
using VetCare.Domain.Constants;

namespace VetCare.Domain.Entities;

public sealed class VeterinaryService : Entity
{
    private VeterinaryService()
    {
    }

    private VeterinaryService(string name, string description, int durationMinutes, decimal price, DateTime createdAtUtc)
        : base(createdAtUtc)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        DurationMinutes = ValidateDuration(durationMinutes);
        Price = ValidatePrice(price);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int DurationMinutes { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    public static VeterinaryService Create(string name, string description, int durationMinutes, decimal price, DateTime createdAtUtc)
    {
        return new VeterinaryService(name, description, durationMinutes, price, createdAtUtc);
    }

    public void Update(string name, string description, int durationMinutes, decimal price, DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        var normalizedName = NormalizeName(name);
        var normalizedDescription = NormalizeDescription(description);
        var validatedDuration = ValidateDuration(durationMinutes);
        var validatedPrice = ValidatePrice(price);

        MarkAsUpdated(updatedAtUtc);

        Name = normalizedName;
        Description = normalizedDescription;
        DurationMinutes = validatedDuration;
        Price = validatedPrice;
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

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(DomainErrorCodes.ServiceNameRequired, "El nombre del servicio veterinario es obligatorio.");
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length >
            VeterinaryServiceConstraints.NameMaxLength)
        {
            throw new DomainException(DomainErrorCodes.ServiceNameTooLong, $"El nombre del servicio no puede superar " + $"{VeterinaryServiceConstraints.NameMaxLength} caracteres.");
        }

        return normalizedName;
    }

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException(DomainErrorCodes.ServiceDescriptionRequired, "La descripción del servicio es obligatoria.");
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length >
            VeterinaryServiceConstraints.DescriptionMaxLength)
        {
            throw new DomainException(DomainErrorCodes.ServiceDescriptionTooLong, $"La descripción no puede superar " + $"{VeterinaryServiceConstraints.DescriptionMaxLength} caracteres.");
        }

        return normalizedDescription;
    }

    private static int ValidateDuration(int durationMinutes)
    {
        if (durationMinutes < VeterinaryServiceConstraints.MinimumDurationMinutes ||
            durationMinutes > VeterinaryServiceConstraints.MaximumDurationMinutes)
        {
            throw new DomainException(DomainErrorCodes.ServiceDurationOutOfRange, $"La duración debe estar entre " +
                $"{VeterinaryServiceConstraints.MinimumDurationMinutes} y " +
                $"{VeterinaryServiceConstraints.MaximumDurationMinutes} minutos.");
        }

        return durationMinutes;
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price < VeterinaryServiceConstraints.MinimumPrice)
        {
            throw new DomainException(DomainErrorCodes.ServicePriceNegative, "El precio del servicio no puede ser negativo.");
        }

        return price;
    }
}
