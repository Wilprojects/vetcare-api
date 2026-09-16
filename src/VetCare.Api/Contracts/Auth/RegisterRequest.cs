using System.ComponentModel.DataAnnotations;

namespace VetCare.Api.Contracts.Auth;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string LastName { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; init; }

    [Phone]
    [StringLength(30)]
    public string? PhoneNumber { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; init; }
}
