using System.ComponentModel.DataAnnotations;

namespace VetCare.Api.Contracts.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; init; }
}
