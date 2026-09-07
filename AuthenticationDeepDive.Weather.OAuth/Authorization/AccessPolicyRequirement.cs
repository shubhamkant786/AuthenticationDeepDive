using Microsoft.AspNetCore.Authorization;

namespace AuthenticationDeepDive.Weather.OAuth.Authorization
{
    public class AccessPolicyRequirement : IAuthorizationRequirement
    {
        public IReadOnlyCollection<AccessPrivilegesEnum> Permissions { get; }

        public AccessPolicyRequirement(params AccessPrivilegesEnum[] permissions)
        {
            Permissions = permissions;
        }
    }
}
