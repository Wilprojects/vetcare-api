using VetCare.Application.Authentication.Models;

namespace VetCare.Application.Authentication;

public interface IAuthService
{
    Task<AuthenticatedUser> RegisterAsync(RegisterUserInput input, CancellationToken cancellationToken = default);

    Task<LoginResult> LoginAsync(LoginInput input, CancellationToken cancellationToken = default);

    Task<AuthenticatedUser> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
