using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TMS_API.Utilities;

namespace TMS_API.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly ILogger<RegistrationController> _logger;
    private readonly IDatabaseActions _databaseActions;

    public RegistrationController(ILogger<RegistrationController> logger, IDatabaseActions databaseActions)
    {
        _logger = logger;
        _databaseActions = databaseActions;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TMS_APP data)
    {
        try
        {
            Auth Authenticate = new Auth();
            byte[] salt = Authenticate.GenerateSalt();
            byte[] hashedpwd = Authenticate.passwordHasher(data.pwd, salt);
            int status = await _databaseActions.UserRegistration(data.email, hashedpwd, salt);
            return (status != -1) ? Ok() : StatusCode(500, "Internal Server Error");

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
