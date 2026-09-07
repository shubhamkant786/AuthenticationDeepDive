using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AuthenticationDeepDive.Weather.OAuth.Authorization
{
    /// <summary>
    /// This class provides the authorization policy to be used for the AccessRequirement. 
    /// It checks if the policy name starts with "Access:" and extracts the permissions from the policy name.
    /// If valid permissions are found, it creates an authorization 
    /// policy that requires the user to be authenticated and have the specified permissions.
    /// </summary>
    public class AccessPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private const string PolicyPrefix = "Access:";

        public AccessPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            ArgumentNullException.ThrowIfNull(policyName);

            if (policyName.StartsWith(PolicyPrefix))
            {
                var value = policyName[PolicyPrefix.Length..];

                var permissions = value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => Enum.TryParse<AccessPrivilegesEnum>(p, out var permission)
                        ? permission
                        : (AccessPrivilegesEnum?)null)
                    .Where(p => p.HasValue)
                    .Select(p => p!.Value)
                    .ToArray();

                if (permissions.Length > 0)
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddRequirements(new AccessPolicyRequirement(permissions))
                        .Build();

                    return Task.FromResult<AuthorizationPolicy?>(policy);
                }
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}
