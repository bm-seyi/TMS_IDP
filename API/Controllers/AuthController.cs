using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TMS_IDP.Models;
using TMS_IDP.DbContext;
using TMS_IDP.Utilities;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TMS_IDP.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController>  _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISecurityUtils _securityUtils;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, SignInManager<ApplicationUser> signInManager, ISecurityUtils securityUtils, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _securityUtils = securityUtils ?? throw new ArgumentNullException(nameof(securityUtils));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
            
            string clientId = _configuration["IdentityServer:ClientId"] ?? throw new ArgumentNullException(nameof(clientId));
            string scope = "openid profile api1.read offline_access";

            // Build Authorization URL
            string authorizationUrl = $"{_configuration["IdentityServer:Authority"]}/connect/authorize" +
                                    $"?response_type=code" +
                                    $"&client_id={clientId}" +
                                    $"&redirect_uri={Uri.EscapeDataString(_configuration["IdentityServer:RedirectUri"] ?? throw new ArgumentNullException())}" +
                                    $"&scope={Uri.EscapeDataString(scope)}" +
                                    $"&code_challenge={codeChallenger}" +
                                    $"&code_challenge_method=S256";

            _logger.LogInformation(authorizationUrl);

            return Redirect(authorizationUrl);
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