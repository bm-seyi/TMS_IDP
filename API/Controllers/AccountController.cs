using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using  TMS_API.Models;
using TMS_API.DbContext;
using TMS_API.Utilities;

namespace TMS_API.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController>  _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(ILogger<AccountController> logger, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
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

            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, false, false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login Failed");
                ModelState.AddModelError("", "Invalid username or password");
                return View(login);
            }

            return LocalRedirect(login.ReturnUrl ?? "/");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("login");
        }
    }
}