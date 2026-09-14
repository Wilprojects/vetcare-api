namespace VetCare.Domain.Constants;

public static class PetConstraints
{
    public const int NameMaxLength = 100;
    public const int BreedMaxLength = 100;

    public const decimal MinimumWeightKg = 0.01m;
    public const decimal MaximumWeightKg = 200m;
}
