namespace VetCare.Application.VeterinaryServices.Models;

public sealed record UpdateVeterinaryServiceInput(
    string Name,
    string Description,
    int DurationMinutes,
    decimal Price
);
