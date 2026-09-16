namespace VetCare.Api.Contracts.Auth;

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    IReadOnlyCollection<string> Roles
);
