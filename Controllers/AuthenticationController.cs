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
    private readonly IDatabaseActions _databaseActions;

    public AuthenticationController(ILogger<AuthenticationController> logger, IDatabaseActions databaseActions)
    {
        _logger = logger;
        _databaseActions = databaseActions;
    }

    [EnableRateLimiting("TokenPolicy")]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TMS_APP data)
    {
        try
        {
            (byte[]? pwdhash, byte[]? salt) = await _databaseActions.UserAuthentication(data.email);

            if (pwdhash != null && salt != null)
            {
                Auth Authentication = new Auth();
                byte[] hashedpwd = Authentication.passwordHasher(data.pwd, salt);
                return pwdhash.SequenceEqual(hashedpwd) ? Ok(): Unauthorized();
            } 
            else 
            {
                _logger.LogWarning("The hashed password and/or salt returned null");
                return BadRequest();
            }

        } 
        catch (Exception ex)
        {   
            _logger.LogError("Error Message, {message}", ex.Message);

            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {message}", ex.InnerException.Message);
            }
            return StatusCode(500, "Internal Server Error");
        }
    }
}
