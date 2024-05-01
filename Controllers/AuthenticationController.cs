using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TMS_API.Utilities;
using System.Linq;

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

    [HttpPost]
    public IActionResult Post([FromBody] TMS_APP data)
    {

        try
        {
            (byte[] pwdhash, byte[] salt) = DatabaseActions.UserAuthetication(data.email);

            byte[] hashedpwd = Auth.passwordHasher(data.pwd, salt);

            return pwdhash.SequenceEqual(hashedpwd) ? Ok() : BadRequest("Password Don't Match");

        } catch (SqlException ex)
        {   
            Console.WriteLine(ex.Message);
            return BadRequest();
        }

    }
}
