using AuthenticationDeepDive.Weather.OAuth.Authorization;

namespace AuthenticationDeepDive.Weather.OAuth.Services
{
    public interface IUserPriviligeVerificationService
    {
        Task<bool> HasPrivillegeAsync(string userId, string[] adGroups, AccessPrivilegesEnum privilege);
    }
}
