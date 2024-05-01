using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TMS_API.Utilities;

namespace TMS_API.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(ILogger<RegistrationController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Post([FromBody] TMS_APP data)
    {

        try
        {
            byte[] salt = Auth.GenerateSalt();
            byte[] hashedpwd = Auth.passwordHasher(data.pwd, salt);
            DatabaseActions.UserRegistration(data.email, hashedpwd, salt);
            return Ok();
        } catch (SqlException ex)
        {   
            Console.WriteLine(ex.Message);
            return BadRequest(ex.Message);
        }

    }
}
