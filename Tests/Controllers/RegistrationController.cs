using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using TMS_IDP.Controllers;
using TMS_IDP.DbContext;
using TMS_IDP.Models;

namespace  TMS_IDP.Tests
{
    [TestClass]
    public class RegControllerTests
    {
        public Mock<ILogger<RegistrationController>> _mockLogger = null!;
        public Mock<UserManager<ApplicationUser>> _mockUserManager = null!;
        public RegistrationController _mockRegController = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<RegistrationController>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), 
                null!, null!, null!, null!, null!, null!, null!, null!
            );

            _mockRegController = new RegistrationController(_mockLogger.Object, _mockUserManager.Object);
        }

        [TestMethod]
        public async Task Post_InvalidModelState_ReturnsBadRequest()
        {
            _mockRegController.ModelState.AddModelError("Email", "Missing Email");

            IActionResult result = await _mockRegController.Post(new RegModel
            {
                Email = null!,
                Password = "DummyPassword",
                ClientId = "DummyClientID",
                ClientSecret = "DummyClientSecret"
            });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Post_UserCreationFails_ReturnsInternalServerError()
        {
            RegModel model = new RegModel
            {
                Email = "dummy@email.com",
                Password = "DummyPassword",
                ClientId = "DummyClientId",
                ClientSecret = "DummyClientSecret"
            };

            _mockUserManager.Setup(c => c.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Description = "Creation Failed"})
                );
            
            IActionResult result = await _mockRegController.Post(model);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }

        [TestMethod]
        public async Task Post_SuccessfulRegistration_ReturnsOk()
        {
            RegModel model = new RegModel
            {
                Email = "dummy@email.com",
                Password = "DummyPassword",
                ClientId = "DummyClientId",
                ClientSecret = "DummyClientSecret"
            };

            _mockUserManager.Setup(c => c.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(
                IdentityResult.Success
                );
            
            IActionResult result = await _mockRegController.Post(model);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }
    }
}