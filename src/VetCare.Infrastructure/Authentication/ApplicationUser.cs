using Microsoft.AspNetCore.Identity;

namespace VetCare.Infrastructure.Authentication;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    private ApplicationUser()
    {
    }

    public ApplicationUser(string firstName, string lastName, string email, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "El nombre del usuario es obligatorio.",
                nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException(
                "El apellido del usuario es obligatorio.",
                nameof(lastName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "El correo del usuario es obligatorio.",
                nameof(email));
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("La fecha de creación debe estar expresada en UTC.", nameof(createdAtUtc));
        }

        var normalizedEmail = email.Trim();

        Id = Guid.NewGuid();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = normalizedEmail;
        UserName = normalizedEmail;
        CreatedAtUtc = createdAtUtc;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
}
