using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VetCare.Application.Authentication;
using VetCare.Application.Authentication.Models;
using VetCare.Application.Common.Exceptions;
using VetCare.Application.Common.Security;

namespace VetCare.Infrastructure.Authentication;

public sealed class AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtTokenGenerator jwtTokenGenerator, TimeProvider timeProvider, ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthenticatedUser> RegisterAsync(RegisterUserInput input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = input.Email.Trim();

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new ConflictException("EMAIL_ALREADY_EXISTS", "Ya existe un usuario registrado con ese correo.");
        }

        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        var user = new ApplicationUser(input.FirstName, input.LastName, email, createdAtUtc)
        {
            PhoneNumber = string.IsNullOrWhiteSpace(input.PhoneNumber) ? null : input.PhoneNumber.Trim()
        };

        var createResult = await userManager.CreateAsync(user, input.Password);

        if (!createResult.Succeeded)
        {
            ThrowIdentityErrors(createResult);
        }

        var roleResult = await userManager.AddToRoleAsync(user, RoleNames.Customer);

        if (!roleResult.Succeeded)
        {
            var deleteResult = await userManager.DeleteAsync(user);

            logger.LogError("Customer role assignment failed for user {UserId}. CleanupSucceeded: {CleanupSucceeded}", user.Id, deleteResult.Succeeded);

            throw new InvalidOperationException("No fue posible asignar el rol Customer al nuevo usuario.");
        }

        logger.LogInformation("User {UserId} registered with role {Role}", user.Id, RoleNames.Customer);

        return MapUser(user, [RoleNames.Customer]);
    }

    public async Task<LoginResult> LoginAsync(LoginInput input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = input.Email.Trim();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            logger.LogWarning("Login attempt failed for an unknown account");

            throw InvalidCredentials();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, input.Password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            logger.LogWarning("Login failed for user {UserId}. LockedOut: {LockedOut}", user.Id, signInResult.IsLockedOut);

            throw InvalidCredentials();
        }

        var roles = await userManager.GetRolesAsync(user);

        var token = jwtTokenGenerator.Generate(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                roles
            );

        logger.LogInformation("User {UserId} authenticated successfully", user.Id);

        return new LoginResult(token.Value, token.ExpiresAtUtc, MapUser(user, roles));
    }

    public async Task<AuthenticatedUser> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            throw new NotFoundException("USER_NOT_FOUND", "El usuario no existe.");
        }

        var roles = await userManager.GetRolesAsync(user);
        return MapUser(user, roles);
    }

    private static AuthenticatedUser MapUser(ApplicationUser user, IEnumerable<string> roles)
    {
        return new AuthenticatedUser(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            user.PhoneNumber,
            roles.ToArray()
        );
    }

    private static UnauthorizedException InvalidCredentials()
    {
        return new UnauthorizedException("INVALID_CREDENTIALS", "El correo o la contraseña son incorrectos.");
    }

    private static void ThrowIdentityErrors(IdentityResult result)
    {
        var hasDuplicateEmail = result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName");

        if (hasDuplicateEmail)
        {
            throw new ConflictException("EMAIL_ALREADY_EXISTS", "Ya existe un usuario registrado con ese correo.");
        }

        var errors = result.Errors
                .GroupBy(GetErrorField)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray(), StringComparer.OrdinalIgnoreCase);

        throw new AppValidationException("IDENTITY_VALIDATION_FAILED", "Los datos del usuario no cumplen las reglas de Identity.", errors);
    }

    private static string GetErrorField(IdentityError error)
    {
        if (error.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase))
        {
            return "password";
        }

        if (error.Code.Contains("Email", StringComparison.OrdinalIgnoreCase) || error.Code.Contains("UserName", StringComparison.OrdinalIgnoreCase))
        {
            return "email";
        }

        return "identity";
    }
}
