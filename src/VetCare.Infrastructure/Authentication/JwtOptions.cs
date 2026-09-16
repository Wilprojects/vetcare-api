using Microsoft.Extensions.Configuration;

namespace VetCare.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required byte[] SigningKeyBytes { get; init; }
    public required int AccessTokenExpirationMinutes { get; init; }

    public static JwtOptions FromConfiguration(IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = configuration["Jwt:Key"];
        var expirationValue = configuration["Jwt:AccessTokenExpirationMinutes"];

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("La configuración 'Jwt:Issuer' es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("La configuración 'Jwt:Audience' es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("La configuración secreta 'Jwt:Key' es obligatoria.");
        }

        if (!int.TryParse(expirationValue, out var expirationMinutes) || expirationMinutes <= 0)
        {
            throw new InvalidOperationException("'Jwt:AccessTokenExpirationMinutes' debe ser mayor que cero.");
        }

        byte[] signingKeyBytes;

        try
        {
            signingKeyBytes = Convert.FromBase64String(key);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("'Jwt:Key' debe ser una cadena Base64 válida.", exception);
        }

        if (signingKeyBytes.Length < 32)
        {
            throw new InvalidOperationException("'Jwt:Key' debe contener al menos 32 bytes.");
        }

        return new JwtOptions
        {
            Issuer = issuer.Trim(),
            Audience = audience.Trim(),
            SigningKeyBytes = signingKeyBytes,
            AccessTokenExpirationMinutes = expirationMinutes
        };
    }
}
