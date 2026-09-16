using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using VetCare.Application.Authentication;
using VetCare.Application.Authentication.Models;

namespace VetCare.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(JwtOptions jwtOptions, TimeProvider timeProvider) : IJwtTokenGenerator
{
    public GeneratedToken Generate(Guid userId, string email, string firstName, string lastName, IEnumerable<string> roles)
    {
        var now = timeProvider.GetUtcNow();

        var expiresAt = now.AddMinutes(jwtOptions.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),

            new(JwtRegisteredClaimNames.Email, email),

            new(JwtRegisteredClaimNames.GivenName, firstName),

            new(JwtRegisteredClaimNames.FamilyName, lastName),

            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64)
        };

        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim("role", role));
        }

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(jwtOptions.SigningKeyBytes), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expiresAt.UtcDateTime,
                signingCredentials: signingCredentials
            );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new GeneratedToken(tokenValue, expiresAt.UtcDateTime);
    }
}
