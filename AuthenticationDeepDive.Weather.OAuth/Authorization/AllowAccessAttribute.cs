using Microsoft.AspNetCore.Authorization;

namespace AuthenticationDeepDive.Weather.OAuth.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AllowAccessAttribute : AuthorizeAttribute
    {
        private const string PolicyPrefix = "Access:";

        public AllowAccessAttribute(params AccessPrivilegesEnum[] accesses)
        {
            if (accesses == null || accesses.Length == 0)
            {
                throw new ArgumentException("At least one permission must be specified.", nameof(accesses));
            }

            Policy = $"{PolicyPrefix}{string.Join(",", accesses)}";
        }
    }
}
