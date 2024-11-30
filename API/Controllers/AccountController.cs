using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TMS_API.Models;
using TMS_API.DbContext;
using TMS_API.Utilities;
using IdentityModel.Client;
using System.Net;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TMS_API.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController>  _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISecurityUtils _securityUtils;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AccountController(ILogger<AccountController> logger, SignInManager<ApplicationUser> signInManager, ISecurityUtils securityUtils, IConfiguration configuration, ITokenService tokenService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _securityUtils = securityUtils ?? throw new ArgumentNullException(nameof(securityUtils));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            ViewData["Title"] = "Login Page";
            var model = new Login
            {
                Email = string.Empty,
                Password = string.Empty,
            };
            return View(model);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm]Login login)
        {

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(ApiMessages.InvalidModelStateLog, ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
                return View(login);
            }

            SignInResult result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, true, false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login Failed");
                ModelState.AddModelError("", "Invalid username or password");
                return View(login);
            }

            string codeVerifier = _securityUtils.GenerateCodeVerifier();
            HttpContext.Session.SetString("codeVerifier", codeVerifier);

            string codeChallenger = await _securityUtils.GenerateCodeChallengerAsync(codeVerifier);
            
            string redirectUri = _configuration["IdentityServer:RedirectUri"] ?? throw new ArgumentNullException(nameof(redirectUri));
            string clientId = _configuration["IdentityServer:ClientId"] ?? throw new ArgumentNullException(nameof(clientId));
            string scope = "openid profile api1.read offline_access";

            // Build Authorization URL
            string authorizationUrl = $"{_configuration["IdentityServer:Authority"]}/connect/authorize" +
                                   $"?response_type=code" +
                                   $"&client_id={clientId}" +
                                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                                   $"&scope={Uri.EscapeDataString(scope)}" +
                                   $"&code_challenge={codeChallenger}" +
                                   $"&code_challenge_method=S256";

            _logger.LogInformation(authorizationUrl);

            return Redirect(authorizationUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            try
            {
                string? codeVerifier = HttpContext.Session.GetString("codeVerifier");
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(codeVerifier))
                {
                    return BadRequest();
                } 

                TokenResponse tokenResponse = await _tokenService.PCKEAsync(codeVerifier, "maui_client", code);
       
            
                if (tokenResponse.AccessToken == null || tokenResponse.IdentityToken == null ||tokenResponse.RefreshToken== null)
                {
                    throw new ArgumentNullException(nameof(tokenResponse));
                }

                HttpContext.Session.SetString("accessToken", tokenResponse.AccessToken);
                HttpContext.Session.SetString("idToken", tokenResponse.IdentityToken);
                HttpContext.Session.SetString("refreshToken", tokenResponse.RefreshToken);
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

            return Redirect(state);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            string? idToken = HttpContext.Session.GetString("idToken");
            string postLogoutRedirectUri = _configuration["IdentityServer:PostLogoutRedirectUri"] ?? throw new ArgumentNullException(nameof(postLogoutRedirectUri));

            HttpContext.Session.Clear();

            string logoutUrl = $"{_configuration["IdentityServer:Authority"]}/connect/endsession";

            if (!string.IsNullOrEmpty(idToken))
            {
                logoutUrl += $"?id_token_hint={Uri.EscapeDataString(idToken)}" +
                            $"&post_logout_redirect_uri={Uri.EscapeDataString(postLogoutRedirectUri)}";
            }
            else
            {
                logoutUrl += $"?post_logout_redirect_uri={Uri.EscapeDataString(postLogoutRedirectUri)}";
            }

            return Redirect(logoutUrl);
        }

    }
}