using VetCare.Api.Contracts.Auth;
using VetCare.Application.Authentication;
using VetCare.Application.Authentication.Models;
using VetCare.Application.Common.Exceptions;
using VetCare.Application.Common.Security;

namespace VetCare.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Auth");

        group
            .MapPost("/register", RegisterAsync)
            .AllowAnonymous()
            .WithName("Register")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/me", GetMeAsync)
            .RequireAuthorization(PolicyNames.AuthenticatedUser)
            .WithName("GetCurrentUser")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, IAuthService authService, CancellationToken cancellationToken)
    {
        var user = await authService.RegisterAsync(
            new RegisterUserInput(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Password
            ),
            cancellationToken);

        return Results.Created("/api/v1/auth/me", ToResponse(user));
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, IAuthService authService, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(new LoginInput(request.Email, request.Password), cancellationToken);

        return Results.Ok(new LoginResponse(result.AccessToken, "Bearer", result.ExpiresAtUtc, ToResponse(result.User)));
    }

    private static async Task<IResult> GetMeAsync(ICurrentUser currentUser, IAuthService authService, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("INVALID_AUTHENTICATED_USER", "No fue posible determinar el usuario autenticado.");
        }

        var user = await authService.GetUserAsync(userId, cancellationToken);

        return Results.Ok(ToResponse(user));
    }

    private static UserResponse ToResponse(AuthenticatedUser user)
    {
        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.Roles
        );
    }
}
