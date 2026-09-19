namespace VetCare.Application.VeterinaryServices.Models;

public sealed record VeterinaryServiceResult(
    Guid Id,
    string Name,
    string Description,
    int DurationMinutes,
    decimal Price,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
