using VetCare.Application.Authentication.Models;

namespace VetCare.Application.Authentication;

public interface IJwtTokenGenerator
{
    GeneratedToken Generate(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        IEnumerable<string> roles
    );
}
