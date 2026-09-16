namespace VetCare.Application.Authentication.Models;

public sealed record RegisterUserInput(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Password
);
