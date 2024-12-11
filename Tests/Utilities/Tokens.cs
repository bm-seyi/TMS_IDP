using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System.Net;
using Moq;
using Moq.Protected;
using TMS_IDP.Utilities;


namespace TMS_IDP.Tests
{

    [TestClass]
    public class TokenServiceTests
    {

        private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
        private Mock<IConfiguration> _configurationMock = null!;
        private TokenService _tokenService  = null!;

        [TestInitialize]
        public void Setup()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();

            // Mock configuration to return a fake API key
            _configurationMock.Setup(config => config["API:Key"]).Returns("fake-api-key");

            _tokenService = new TokenService(_httpClientFactoryMock.Object, _configurationMock.Object);
        }

        [TestMethod]
        public async Task PCKEAsync_ValidCredentials_ReturnsTokenResponse()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected() // Allows access to protected methods
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\": \"fake_access_token\", \"expires_in\": 3600}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _httpClientFactoryMock?.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            if (_tokenService == null) throw new ArgumentNullException(nameof(_tokenService));
            // Act
            var result = await _tokenService.PCKEAsync("dummyCodeVerifier", "dummyClientID", "dummyCode");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("fake_access_token", result.AccessToken);
        }


        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientFactoryIsNull()
        {
            if (_configurationMock == null) throw new ArgumentNullException(nameof(_configurationMock));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new TokenService(null!, _configurationMock.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            if (_httpClientFactoryMock == null) throw new  ArgumentNullException(nameof(_httpClientFactoryMock));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new TokenService(_httpClientFactoryMock.Object, null!));
        }

    }

}



