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
            Auth Authenticate = new Auth();
            byte[] salt = Authenticate.GenerateSalt();
            byte[] hashedpwd = Authenticate.passwordHasher(data.pwd, salt);
            int status = DatabaseActions.UserRegistration(data.email, hashedpwd, salt);
            return (status != -1) ? Ok() : StatusCode(500, "Interal Server Error");
        } catch (Exception)
        {   
            return StatusCode(500, "Internal Server Error");
        }

    }
}
