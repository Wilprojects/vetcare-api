namespace VetCare.Application.Authentication.Models;

public sealed record LoginInput(
    string Email,
    string Password
);
