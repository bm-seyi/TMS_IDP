using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Moq;
using TMS_IDP.Controllers;
using TMS_IDP.Models.Controllers;
using TMS_IDP.Utilities;
using TMS_IDP.DbContext;


namespace TMS_IDP.Tests
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<ILogger<AuthController>> _mockLogger = null!;
        private Mock<SignInManager<ApplicationUser>> _mockSignInManager =null!;
        private Mock<ISecurityUtils> _mockSecurityUtils = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private Mock<ITokenService> _mockTokenService = null!;
        private AuthController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockSecurityUtils = new Mock<ISecurityUtils>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockTokenService = new Mock<ITokenService>();

        // Mock dependencies for SignInManager
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        var mockContextAccessor = new Mock<IHttpContextAccessor>();
        var mockClaimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            mockContextAccessor.Object,
            mockClaimsFactory.Object,
            null!,
            null!,
            null!,
            null!
        );

        _controller = new AuthController(
            _mockLogger.Object,
            _mockSignInManager.Object,
            _mockSecurityUtils.Object,
            _mockConfiguration.Object
        );

        var context = new DefaultHttpContext();
        var sessionMock = new Mock<ISession>();
        context.Session = sessionMock.Object;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
    }


        [TestMethod]
        public async Task Login_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");
            var loginModel = new Login { Email = "", Password = "" };

            // Act
            var result = await _controller.Login(loginModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loginModel, result.Model);
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var loginModel = new Login { Email = "test@example.com", Password = "wrongpassword" };
            _mockSignInManager
                .Setup(x => x.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _controller.Login(loginModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loginModel, result.Model);
            Assert.IsTrue(_controller.ModelState.ContainsKey(""));
        }

        [TestMethod]
        public async Task Login_ValidCredentials_RedirectsToAuthorizationUrl()
        {
            // Arrange
            var loginModel = new Login { Email = "test@example.com", Password = "correctpassword" };
            _mockSignInManager
                .Setup(x => x.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, false))
                .ReturnsAsync(SignInResult.Success);
            _mockSecurityUtils.Setup(x => x.GenerateCodeVerifier(It.IsAny<int>())).Returns("codeVerifier");
            _mockSecurityUtils.Setup(x => x.GenerateCodeChallengerAsync("codeVerifier")).ReturnsAsync("codeChallenger");
            _mockConfiguration.Setup(x => x["IdentityServer:RedirectUri"]).Returns("https://localhost/callback");
            _mockConfiguration.Setup(x => x["IdentityServer:ClientId"]).Returns("client_id");
            _mockConfiguration.Setup(x => x["IdentityServer:Authority"]).Returns("https://identityserver");

            // Act
            var result = await _controller.Login(loginModel) as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Url.Contains("https://identityserver/connect/authorize"));
        }

        [TestMethod]
        public async Task Logout_ClearsSessionAndRedirectsToLogoutUrl()
        {
            // Arrange
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(x => x.TryGetValue("idToken",  out It.Ref<byte[]?>.IsAny));
            _controller.HttpContext.Session = sessionMock.Object;

            _mockConfiguration.Setup(x => x["IdentityServer:PostLogoutRedirectUri"]).Returns("https://localhost/logout");
            _mockConfiguration.Setup(x => x["IdentityServer:Authority"]).Returns("https://identityserver");

            // Act
            var result = await _controller.Logout() as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Url.Contains("https://identityserver/connect/endsession"));
            sessionMock.Verify(x => x.Clear(), Times.Once);
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenIloggerIsNull()
        {
            if (_mockLogger == null) throw new ArgumentNullException(nameof(_mockLogger));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new AuthController(null!, _mockSignInManager.Object, _mockSecurityUtils.Object, _mockConfiguration.Object));
        }


        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenSignInManagerIsNull()
        {
            if (_mockSignInManager == null) throw new ArgumentNullException(nameof(_mockSignInManager));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new AuthController(_mockLogger.Object, null!, _mockSecurityUtils.Object, _mockConfiguration.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenSecurityUtilsIsNull()
        {
            if (_mockSecurityUtils == null) throw new ArgumentNullException(nameof(_mockSecurityUtils));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new AuthController(_mockLogger.Object, _mockSignInManager.Object, null!, _mockConfiguration.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            if (_mockConfiguration == null) throw new ArgumentNullException(nameof(_mockConfiguration));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new AuthController(_mockLogger.Object, _mockSignInManager.Object, _mockSecurityUtils.Object, null!));
        }
    }
}

