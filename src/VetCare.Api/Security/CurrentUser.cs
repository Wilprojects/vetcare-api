using VetCare.Application.Common.Security;

namespace VetCare.Api.Security;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string SubjectClaim = "sub";
    private const string EmailClaim = "email";
    private const string RoleClaim = "role";

    private System.Security.Claims.ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirst(SubjectClaim)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Email => Principal?.FindFirst(EmailClaim)?.Value;

    public IReadOnlyCollection<string> Roles => Principal?
            .FindAll(RoleClaim)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? [];

    public bool IsInRole(string role)
    {
        return Principal?.IsInRole(role) ?? false;
    }
}
