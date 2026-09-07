using AuthenticationDeepDive.Weather.OAuth.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationDeepDive.Weather.OAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationController : ControllerBase
    {
        [HttpGet]
        [AllowAccessAttribute(AccessPrivilegesEnum.ReadLocationApi)]
        public IActionResult GetLocation()
        {
            var location = new List<string> { "New York", "Los Angeles", "Chicago" };
            return Ok(location);
        }

        [HttpPost]
        [AllowAccessAttribute(AccessPrivilegesEnum.ReadWriteLocationApi)]
        public IActionResult AddLocation([FromBody] string location)
        {
            // Logic to add the location
            return Ok($"Location '{location}' added successfully.");
        }
    }
}
