namespace VetCare.Application.Authentication.Models;

public sealed record LoginResult(
    string AccessToken,
    DateTime ExpiresAtUtc,
    AuthenticatedUser User
);
