using AuthenticationDeepDive.Weather.OAuth.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthenticationDeepDive.Weather.OAuth.Authorization
{
    public class AccessPolicyRequirementAuthorizationHandler
        (IUserPriviligeVerificationService userPriviligeVerificationService)
        : AuthorizationHandler<AccessPolicyRequirement>
    {

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AccessPolicyRequirement requirement)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(requirement);
            var principal = context.User;
            if (principal == null || !principal.Identity.IsAuthenticated)
                context.Fail();
            var userId = context.User.FindFirstValue("userId");
            string[] adGroups = [.. principal.FindAll("groups")
                            .Select(g => g.Value)
                            .Distinct(StringComparer.OrdinalIgnoreCase)];

            foreach (var permission in requirement.Permissions)
            {
                var hasPermission = await userPriviligeVerificationService.HasPrivillegeAsync(userId, adGroups, permission);

                if (hasPermission)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
