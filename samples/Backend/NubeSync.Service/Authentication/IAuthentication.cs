using System.Security.Claims;

namespace NubeSync.Service
{
    public interface IAuthentication
    {
        string[] ScopeRequiredByApi { get; }

        string GetUserIdentifier(ClaimsPrincipal user);
    }
}
