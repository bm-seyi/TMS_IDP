using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Moq;
using TMS_API.Controllers;
using TMS_API.Utilities;
using TMS_API.DbContext;
using TMS_API.Models;
using IdentityModel.Client;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;



namespace TMS_API.Tests
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<ILogger<AuthenticationController>> _mockLogger = null!;
        private Mock<IDatabaseActions> _mockActions = null!;
        private Mock<ITokenService> _mockTokenService = null!;
        private Mock<ISecurityUtils> _mockSecurityUtils = null!;
        private Mock<UserManager<ApplicationUser>> _mockUserManager = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private AuthenticationController _mockAuthenticationController = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<AuthenticationController>>();
            _mockActions = new Mock<IDatabaseActions>();
            _mockTokenService = new Mock<ITokenService>();
            _mockSecurityUtils = new Mock<ISecurityUtils>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), 
                null!, null!, null!, null!, null!, null!, null!, null!
            );

            _mockConfiguration.Setup(c => c["Encryption:Key"]).Returns("0123456789ABCDEF0123456789ABCDEF");

            _mockAuthenticationController = new AuthenticationController(
                _mockLogger.Object, _mockActions.Object, _mockTokenService.Object, _mockSecurityUtils.Object,
                _mockConfiguration.Object, _mockUserManager.Object);
            
        }

        [TestMethod]
        public async Task Post_InvalidModelState_ReturnsBadRequest()
        {
            _mockAuthenticationController.ModelState.AddModelError("Email", "Required");

            IActionResult result = await _mockAuthenticationController.Post(new AuthModel
            {
                Email = null!,
                Password = "DummyPassword",
                ClientId = "DummyClientID",
                ClientSecret = "DummyClientSecret"
            });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Post_UserNotFound_ReturnsBadRequest()
        {
            AuthModel authModel = new AuthModel
            {
                Email = "test@email.com",
                Password = "DummyPassword",
                ClientId = "DummyClientID",
                ClientSecret = "DummyClientSecret"
            };

            _mockUserManager.Setup(c => c.FindByEmailAsync(authModel.Email)).ReturnsAsync((ApplicationUser)null!);

            IActionResult result = await _mockAuthenticationController.Post(authModel);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Post_InvalidPassword_ReturnsBadRequest()
        {
            AuthModel authModel = new AuthModel
            {
                Email = "test@email.com",
                Password = "DummyPassword",
                ClientId = "DummyClientID",
                ClientSecret = "DummyClientSecret"
            };

            ApplicationUser user =  new ApplicationUser
            {
                Email = "test@email.com"
            };

            _mockUserManager.Setup(c => c.FindByEmailAsync(authModel.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(c => c.CheckPasswordAsync(user, authModel.Password)).ReturnsAsync(false);

            IActionResult result = await _mockAuthenticationController.Post(authModel);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Post_RefreshTokenStorageFails_ReturnsInternalServerError()
        {
            AuthModel authModel = new AuthModel
            {
                Email = "test@email.com",
                Password = "DummyPassword",
                ClientId = "DummyClientID",
                ClientSecret = "DummyClientSecret"
            };

            ApplicationUser user =  new ApplicationUser
            {
                Email = "test@email.com"
            };

            string refreshToken = "test_refresh_token";

            _mockUserManager.Setup(c => c.FindByEmailAsync(authModel.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(c => c.CheckPasswordAsync(user, authModel.Password)).ReturnsAsync(true);
            _mockSecurityUtils.Setup(c => c.GenerateRefreshToken(It.IsAny<int>())).Returns(refreshToken);
            _mockSecurityUtils.Setup(c => c.EncryptPlaintTextAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).ReturnsAsync([]);
            _mockActions.Setup(c => c.StoreRefreshTokenAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTime>(),It.IsAny<byte[]>())).ReturnsAsync(false);

            IActionResult result = await _mockAuthenticationController.Post(authModel);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }

        [TestMethod]
        public async Task Post_TokenServiceFails_ThrowsException()
        {
            AuthModel authModel = new AuthModel
            {
                Email = "test@email.com",
                Password = "DummyPassword",
                ClientId = "DummyClientID",
                ClientSecret = "DummyClientSecret"
            };

            ApplicationUser user =  new ApplicationUser
            {
                Email = "test@email.com"
            };

            string refreshToken = "test_refresh_token";

            _mockUserManager.Setup(c => c.FindByEmailAsync(authModel.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(c => c.CheckPasswordAsync(user, authModel.Password)).ReturnsAsync(true);
            _mockSecurityUtils.Setup(c => c.GenerateRefreshToken(It.IsAny<int>())).Returns(refreshToken);
            _mockSecurityUtils.Setup(c => c.EncryptPlaintTextAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).ReturnsAsync([]);
            _mockActions.Setup(c => c.StoreRefreshTokenAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTime>(),It.IsAny<byte[]>())).ReturnsAsync(true);
            _mockTokenService.Setup(c => c.ROPCAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).ThrowsAsync(new Exception("Token service failed"));

            IActionResult result = await _mockAuthenticationController.Post(authModel);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }
        [TestMethod]
        public async Task Post_ValidRequest_ReturnsOk()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream(); // Simulate response stream

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _mockAuthenticationController.ControllerContext = controllerContext;
            
            AuthModel authModel = new AuthModel
            {
                Email = "test@email.com",
                Password = "DummyPassword",
                ClientId = "DummyClientID",
                ClientSecret = "DummyClientSecret"
            };

            ApplicationUser user =  new ApplicationUser
            {
                Email = "test@email.com"
            };
            
            string refreshToken = "test_refresh_token";
            var json = @"{
            ""access_token"": ""access123"",
            ""expires_in"": 3600,
            ""token_type"": ""Bearer""
            }";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            TokenResponse tokenResponse = await ProtocolResponse.FromHttpResponseAsync<TokenResponse>(httpResponseMessage);
            
            _mockUserManager.Setup(c => c.FindByEmailAsync(authModel.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(c => c.CheckPasswordAsync(user, authModel.Password)).ReturnsAsync(true);
            _mockSecurityUtils.Setup(c => c.GenerateRefreshToken(It.IsAny<int>())).Returns(refreshToken);
            _mockSecurityUtils.Setup(c => c.EncryptPlaintTextAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).ReturnsAsync(new byte[] {});
            _mockActions.Setup(c => c.StoreRefreshTokenAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTime>(),It.IsAny<byte[]>())).ReturnsAsync(true);
            _mockTokenService.Setup(c => c.ROPCAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(tokenResponse);

            IActionResult result = await _mockAuthenticationController.Post(authModel);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }
    }
}