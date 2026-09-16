namespace VetCare.Application.Authentication.Models;

public sealed record GeneratedToken(
    string Value,
    DateTime ExpiresAtUtc
);
