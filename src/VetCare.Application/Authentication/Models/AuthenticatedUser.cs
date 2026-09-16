namespace VetCare.Application.Authentication.Models;

public sealed record AuthenticatedUser(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    IReadOnlyCollection<string> Roles

);
