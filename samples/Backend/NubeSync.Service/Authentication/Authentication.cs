using System;
using System.Security.Claims;

namespace NubeSync.Service
{
    public class Authentication : IAuthentication
    {
        string[] IAuthentication.ScopeRequiredByApi => new string[] { "access_as_user" };

        public string GetUserIdentifier(ClaimsPrincipal user)
        {
            string owner = (user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            if (string.IsNullOrEmpty(owner))
            {
                throw new Exception("Unknown User");
            }

            return owner;
        }
    }
}