using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using System.Net;
using TMS_API.Utilities;
using TMS_API.Models;
using TMS_API.DbContext;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IDatabaseActions _databaseActions;
        private readonly ITokenService _tokenService;
        private readonly ISecurityUtils _securityUtils;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly byte[] encryptionKey;

        public AuthenticationController(ILogger<AuthenticationController> logger, IDatabaseActions databaseActions, ITokenService tokenService, ISecurityUtils securityUtils,  IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseActions = databaseActions ?? throw new ArgumentNullException(nameof(databaseActions));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _securityUtils = securityUtils ?? throw new ArgumentNullException(nameof(securityUtils));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            encryptionKey = Convert.FromHexString(configuration["Encryption:Key"] ?? throw new ArgumentNullException(nameof(configuration)));
        }
        
        [EnableRateLimiting("TokenPolicy")]
        [HttpPost("password")]
        public async Task<IActionResult> Post([FromBody] AuthModel authModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(ApiMessages.InvalidModelStateLog, ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new {Message = ApiMessages.InvalidModelStateMessage, Details = ModelState});
            }

            try
            {
                ApplicationUser? user = await _userManager.FindByEmailAsync(authModel.Email);

                if (user == null || authModel.Password == null || !await _userManager.CheckPasswordAsync(user, authModel.Password))
                {
                    _logger.LogWarning(ApiMessages.AuthenticationFailedLog);
                    return BadRequest(new { Message = ApiMessages.AuthenticationFailedMessage });
                }

                string refreshToken = _securityUtils.GenerateRefreshToken();

                byte[] encryptRefreshToken = await _securityUtils.EncryptPlaintTextAsync(refreshToken, encryptionKey);

                ValueTask<byte[]> hashedToken = _securityUtils.GenerateHashToken(refreshToken);

                DateTime expiry = DateTime.UtcNow.AddDays(3);
                
                bool storedRefreshToken = await _databaseActions.StoreRefreshTokenAsync(
                    authModel.ClientId, encryptRefreshToken, expiry, hashedToken.Result);

                if (!storedRefreshToken)
                {
                    _logger.LogError(ApiMessages.StoreRefreshTokenFailedLog);
                    StatusCode((int)HttpStatusCode.InternalServerError, new {Message = ApiMessages.InternalServerErrorMessage});
                }

                IdentityModel.Client.TokenResponse tokenResponse = await _tokenService.ROPCAsync(authModel.Password, authModel.Email, authModel.ClientId, authModel.ClientSecret);

                if (tokenResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(ApiMessages.InternalServerErrorMessage);
                } 

                CookieOptions cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = expiry
                };

                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
                return Ok(new {AccessToken = tokenResponse.AccessToken});

            } 
            catch (Exception ex)
            {   
                _logger.LogError(ApiMessages.ErrorMessageLog, ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ApiMessages.InternalErrorMessageLog, ex.InnerException.Message);
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, new {Message = ApiMessages.InternalServerErrorMessage});
            }
        }

   
    }
}
