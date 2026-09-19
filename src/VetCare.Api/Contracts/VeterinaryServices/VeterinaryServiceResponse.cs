namespace VetCare.Api.Contracts.VeterinaryServices;

public sealed record VeterinaryServiceResponse(
    Guid Id,
    string Name,
    string Description,
    int DurationMinutes,
    decimal Price,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
