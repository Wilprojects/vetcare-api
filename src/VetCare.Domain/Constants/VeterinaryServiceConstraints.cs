namespace VetCare.Domain.Constants;

public static class VeterinaryServiceConstraints
{
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 1000;

    public const int MinimumDurationMinutes = 15;
    public const int MaximumDurationMinutes = 240;

    public const decimal MinimumPrice = 0m;
}
