using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TMS_API.Utilities;
using TMS_API.Models;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IDatabaseActions _databaseActions;
        private readonly IJwtSecurity _jwtSecurity;
        private readonly ISecurityUtils _securityUtils;
        private readonly byte[] encryptionKey;

        public AuthenticationController(ILogger<AuthenticationController> logger, IDatabaseActions databaseActions, IJwtSecurity jwtSecurity, ISecurityUtils securityUtils,  IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseActions = databaseActions ?? throw new ArgumentNullException(nameof(databaseActions));
            _jwtSecurity = jwtSecurity ?? throw new ArgumentNullException(nameof(jwtSecurity));
            _securityUtils = securityUtils ?? throw new ArgumentNullException(nameof(securityUtils));
            encryptionKey = Convert.FromHexString(configuration["Encryption:Key"] ?? throw new ArgumentNullException(nameof(configuration)));
        }
        
        [EnableRateLimiting("TokenPolicy")]
        [HttpPost("token")]
        public async Task<IActionResult> Post([FromBody] AuthModel data)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(ApiMessages.InvalidModelStateLog, ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new {Message = ApiMessages.InvalidModelStateMessage, Details = ModelState});
            }

            try
            {
                return data.GrantType switch
                {
                    "password" => await HandlePasswordGrant(data),
                    "refresh_token" => await HandleRefreshTokenGrant(data),
                    _ => BadRequest(new {error = "unsupported_grant_type", Message = ApiMessages.UnsupportedGrantTypeMessage}),
                };

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

        private async Task<IActionResult> HandlePasswordGrant(AuthModel authModel)
        {
            if (string.IsNullOrWhiteSpace(authModel.Email) || string.IsNullOrWhiteSpace(authModel.Password) || !Misc.IsValidEmail(authModel.Email))
            {
                _logger.LogWarning(ApiMessages.EmailOrPasswordNotProvidedMessage);
                return BadRequest(new {Message = ApiMessages.EmailOrPasswordNotProvidedMessage});
            };

            (byte[]? pwdhash, byte[]? salt) = await _databaseActions.UserAuthenticationAsync(authModel.Email);
            if (pwdhash == null || salt == null)
            {
                _logger.LogWarning(ApiMessages.UserAuthenticationFailedLog);
                return Unauthorized(new {Message = ApiMessages.AuthenticationFailedMessage});
            } 

            byte[] hashedpwd = _securityUtils.GenerateHash(authModel.Password, salt);

            if (!pwdhash.SequenceEqual(hashedpwd))
            {
                _logger.LogWarning(ApiMessages.PasswordMismatchLog);
                return Unauthorized(new {Message = ApiMessages.AuthenticationFailedMessage});
            }

            if (!await VerifyClientCredentials(authModel))
            {
                _logger.LogWarning(ApiMessages.CredentialsAuthenticationFailedLog, authModel.ClientId.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                return Unauthorized(new { Message = ApiMessages.ClientCredentialsAuthenticationFailedMessage });
            }

            string refreshToken = _securityUtils.GenerateRefreshToken();

            (byte[] EncryptedText, byte[] IV) encryptRefreshToken = _securityUtils.EncryptPlaintText(refreshToken, encryptionKey);

            byte[] hashedToken = _securityUtils.GenerateHashToken(refreshToken);

            DateTime expiry = DateTime.UtcNow.AddDays(3);
            
            bool storedRefreshToken = await _databaseActions.StoreRefreshTokenAsync(
                authModel.ClientId, encryptRefreshToken.EncryptedText, expiry, 
                encryptRefreshToken.IV, hashedToken);

            if (!storedRefreshToken)
            {
                _logger.LogError(ApiMessages.StoreRefreshTokenFailedLog);
                StatusCode(500, new {Message = ApiMessages.InternalServerErrorMessage});
            }

            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expiry
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
           
            string jwtToken = _jwtSecurity.JwtTokenGenerator(authModel.Email);
            return Ok(new {AccessToken = jwtToken});
        }

        private async Task<IActionResult> HandleRefreshTokenGrant([FromBody] AuthModel authModel)
        {
            if (!await VerifyClientCredentials(authModel))
            {
                _logger.LogWarning(ApiMessages.CredentialsAuthenticationFailedLog, authModel.ClientId.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                return Unauthorized(new { Message = ApiMessages.ClientCredentialsAuthenticationFailedMessage});
            }
            
            if (string.IsNullOrWhiteSpace(authModel.RefreshToken))
            {
                _logger.LogWarning(ApiMessages.RefreshTokenNotProvidedLog);
                return BadRequest(new { Message = ApiMessages.RefreshTokenNotProvidedMessage});
            } 
            
            byte[] hashedRefreshToken = _securityUtils.GenerateHashToken(authModel.RefreshToken);
            (DateTime expiry, byte[] Key, byte[] IV, byte[] refreshToken)? tokenData = await _databaseActions.RefreshTokenAuthenticationAsync(hashedRefreshToken);

            if (tokenData == null)
            {
                _logger.LogWarning(ApiMessages.RefreshTokenAuthenticationFailedLog);
                return Unauthorized(new { Message = ApiMessages.RefreshTokenAuthenticationFailedMessage});
            }

            if (tokenData.Value.expiry < DateTime.UtcNow)
            {
                _logger.LogWarning(ApiMessages.RefreshTokenExpiredLog);
                return Unauthorized(new {Message = ApiMessages.RefreshTokenExpiredMessage});
            }

            string decryptKey = _securityUtils.DecryptPlainText(tokenData.Value.refreshToken, tokenData.Value.Key, tokenData.Value.IV);

            if (!decryptKey.Equals(authModel.RefreshToken))
            {
                _logger.LogWarning(ApiMessages.InvalidRefreshTokenProvidedLog);
                return Unauthorized(new {Message = ApiMessages.InvalidRefreshTokenProvidedMessage});
            }

            string accessToken =  _jwtSecurity.JwtTokenGenerator(authModel.Email);

            return Ok(new {AccessToken = accessToken});
        }

        private async Task<bool> VerifyClientCredentials(AuthModel authModel)
        {
            var authCredentials = await _databaseActions.CredentialsAuthenticationAsync(authModel.ClientId);
            if (!authCredentials.HasValue) return false;

            string decrypted = _securityUtils.DecryptPlainText(authCredentials.Value.secret, encryptionKey, authCredentials.Value.iv);

            if (decrypted.Trim() != authModel.ClientSecret)
            {
                return false; 
            }

            return true;
        }
    }
}

