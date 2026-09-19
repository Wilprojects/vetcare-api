namespace VetCare.Application.VeterinaryServices.Models;

public sealed record CreateVeterinaryServiceInput(
    string Name,
    string Description,
    int DurationMinutes,
    decimal Price
);
