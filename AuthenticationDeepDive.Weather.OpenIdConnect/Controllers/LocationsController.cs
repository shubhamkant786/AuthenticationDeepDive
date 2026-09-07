using AuthenticationDeepDive.Weather.OpenIdConnect.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationDeepDive.Weather.OpenIdConnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        [HttpGet("locations")]
        [Authorize(Policy = AuthPolicies.ReadAccess)]
        public IActionResult GetLocations()
        {
            var locations = new List<string>
            {
                "New York",
                "Los Angeles",
                "Chicago",
                "Houston",
                "Phoenix"
            };
            return Ok(locations);
        }

        [HttpPost("locations")]
        [Authorize(Policy = AuthPolicies.ReadWriteAccess)]
        public IActionResult AddLocation([FromBody] string location)
        {
            // In a real application, you would add the location to a database or data store.
            // For this example, we'll just return a success message.
            return Ok($"Location '{location}' added successfully.");
        }
    }
}
