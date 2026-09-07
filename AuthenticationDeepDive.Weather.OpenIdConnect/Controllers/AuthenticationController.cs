using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace AuthenticationDeepDive.Weather.OpenIdConnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        [HttpGet("login")]
        [SwaggerOperation(Summary = "Initiates user login auth flow.")]
        [AllowAnonymous]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
                OpenIdConnectDefaults.AuthenticationScheme
            );
        }


        [HttpPost("logout")]
        [SwaggerOperation(Summary = "Logout the current user.")]
        public async Task<IActionResult> Logout([FromQuery] string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return SignOut(
                new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
                OpenIdConnectDefaults.AuthenticationScheme
            );
        }

        [HttpGet("claims")]
        [SwaggerOperation(Summary = "Retrieves the authenticated user's claims.")]
        public IActionResult GetClaims()
        {
            var claims = User.Claims
                .GroupBy(c => c.Type)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => c.Value)
                          .Distinct()
                          .OrderBy(v => v)
                          .ToArray());

            return Ok(claims);
        }

        [HttpGet("user-info")]
        [SwaggerOperation(Summary = "Retrieves user info for the currently logged in user.")]
        [Produces(typeof(UserContext))]
        public async Task<IActionResult> GetUserInfo()
        {
            var principal = HttpContext.User;
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                return NoContent();
            }
            var user = new UserContext(principal);
            var userId = user.UserId;

            return Ok(user);
        }
        public class UserContext
        {
            public string UserId { get; set; }
            public string Email { get; set; }
            public string[] AdGroups { get; set; }

            public UserContext(ClaimsPrincipal user)
            {
                UserId = user.FindFirstValue("userId");
                Email = user.FindFirstValue(ClaimTypes.Email);
                AdGroups = [.. user.FindAll("groups")
            .Select(g => g.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
            }
        }
    }
}
