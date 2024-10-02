using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TMS_API.Utilities;
using TMS_API.Models;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly IDatabaseActions _databaseActions;
        private readonly ISecurityUtils _securityUtils;
        private readonly byte[] encryptionKey;
        public RegistrationController(ILogger<RegistrationController> logger, IDatabaseActions databaseActions, ISecurityUtils securityUtils, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseActions = databaseActions ?? throw new ArgumentNullException(nameof(databaseActions));
            _securityUtils = securityUtils ?? throw new ArgumentNullException(nameof(securityUtils));
            encryptionKey = Convert.FromHexString(configuration["Encryption:Key"] ?? throw new ArgumentNullException(nameof(configuration)));
        }

        [EnableRateLimiting("TokenPolicy")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegModel data)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(ApiMessages.InvalidModelStateLog, ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new {Message = ApiMessages.InvalidModelStateMessage, Details = ModelState});
            }
            try
            {
                var authCredentials = await _databaseActions.CredentialsAuthenticationAsync(data.ClientId);
                if (!authCredentials.HasValue)
                {
                    _logger.LogWarning(ApiMessages.CredentialsAuthenticationFailedLog, data.ClientId.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                    return Unauthorized(new { Message = ApiMessages.ClientCredentialsAuthenticationFailedMessage });
                }

                string decrypted = _securityUtils.DecryptPlainText(authCredentials.Value.secret, encryptionKey, authCredentials.Value.iv);
                if (decrypted.Trim() != data.ClientSecret)
                {
                    _logger.LogWarning(ApiMessages.CredentialsAuthenticationFailedLog, data.ClientId.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                    return Unauthorized(new { Message = ApiMessages.ClientCredentialsAuthenticationFailedMessage });
                }

                byte[] salt = _securityUtils.GenerateSalt();
                byte[] hashedpwd = _securityUtils.GenerateHash(data.Password, salt);

                if (!await _databaseActions.UserRegistrationAsync(data.Email, hashedpwd, salt))
                {
                   _logger.LogError("User Registration Failed");
                    return StatusCode(500, new {Message = ApiMessages.InternalServerErrorMessage}); 
                }

                _logger.LogInformation("User Registered Successfully");
                return Ok(new {Message = "User Registered Successfully"});

            } 
            catch (Exception ex)
            {   
                _logger.LogError(ApiMessages.ErrorMessageLog, ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ApiMessages.InternalErrorMessageLog, ex.InnerException.Message);
                }
                return StatusCode(500, new {Message = ApiMessages.InternalServerErrorMessage});
            }
        }
    }
}
