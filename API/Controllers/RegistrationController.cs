using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TMS_IDP.Models.ViewModel;
using TMS_MIGRATE.DbContext;

namespace TMS_IDP.Controllers
{

    [Route("auth")]
    public class RegistrationController : Controller
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public RegistrationController(ILogger<RegistrationController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet("register")]
        public IActionResult Register(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel registrationModel)
        {
            ViewData["ReturnUrl"] = registrationModel.ReturnUrl;

            if (registrationModel.Password != registrationModel.ConfirmPassword)
            {
                _logger.LogWarning("Passwords do not match for user: {Email}", registrationModel.Email.Replace("\r", "").Replace("\n", ""));
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid for user: {Email}", registrationModel.Email.Replace("\r", "").Replace("\n", ""));
                return View(registrationModel);
            }


            ApplicationUser user = new ApplicationUser
            {
                UserName = registrationModel.Email,
                Email = registrationModel.Email,
            };

            IdentityResult result = await _userManager.CreateAsync(user, registrationModel.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(registrationModel);
            }

            _logger.LogInformation("New user registered: {Email}", registrationModel.Email.Replace("\r", "").Replace("\n", ""));
            return RedirectToAction("Login", "Auth", new { registrationModel.ReturnUrl });
        }
    }
}
