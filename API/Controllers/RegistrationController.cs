using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using TMS_API.Utilities;
using TMS_API.Models;
using TMS_API.DbContext;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public RegistrationController(ILogger<RegistrationController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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
                ApplicationUser user = new ApplicationUser
                {
                    Email = data.Email,
                    UserName = data.Email,
                };

                var result = await _userManager.CreateAsync(user, data.Password);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.Any() ? string.Join(" ",result.Errors.Select(e => e.Description)) : "User Creation Failed");
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
