using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TMS_API.Utilities;
using System.Linq;
using Microsoft.AspNetCore.RateLimiting;

namespace TMS_API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
    }

    [EnableRateLimiting("TokenPolicy")]
    [HttpPost]
    public IActionResult Post([FromBody] TMS_APP data)
    {
        try
        {
            (byte[]? pwdhash, byte[]? salt) = DatabaseActions.UserAuthetication(data.email);

            if (pwdhash != null && salt != null)
            {
                Auth Authentication = new Auth();
                byte[] hashedpwd = Authentication.passwordHasher(data.pwd, salt);

                return pwdhash.SequenceEqual(hashedpwd) ? Ok() : Unauthorized();
            } else 
            {
                return BadRequest();
            }

        } catch (SqlException ex)
        {   
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }
}
