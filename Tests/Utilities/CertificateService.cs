using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System.Text;
using Moq;
using TMS_IDP.Utilities;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Moq.Protected;
using Azure.Core;
using StackExchange.Redis;


namespace TMS_IDP.Tests
{
    [TestClass]
    public class CertificateServiceTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private Mock<ILogger<CertificateService>> _mockLogger = null!;
        private CertificateService _certificateService = null!;


        [TestInitialize]
        public void Initialize()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<CertificateService>>();

            _mockConfiguration.Setup(c => c["HashiCorp:Vault:Token"]).Returns("mock-token");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            
            var responseContent = new
            {
                request_id = "123456789",
                lease_id = "123456789",
                renewable = true,
                lease_duration = 2764800,
                data = new
                {
                    certificate = "-----BEGIN CERTIFICATE-----\nMIID5jCCAs6gAwIBAgIUPKUIBTyDnXrF1MJTboiQA63B840wDQYJKoZIhvcNAQEL\nBQAwFDESMBAGA1UEAxMJbG9jYWxob3N0MB4XDTI1MDIwMTE3MjIxN1oXDTI1MDIw\nNDE3MjI0NlowFDESMBAGA1UEAxMJbG9jYWxob3N0MIIBIjANBgkqhkiG9w0BAQEF\nAAOCAQ8AMIIBCgKCAQEA93v1VXWbbztrTAYTRUkkvYVu60XR6V0xrosJxpymPL8E\nq76weQbYUErOTprUAQab73gxijL1Iminibd1Uk2BctcvIkcYeZQUp5uZ65ltUIaA\ndUMXyFan7FSvTjwmGaoJIbKcn62VfF8XZk0jQbzNfS87YAux/KPUGwEWXV3heE8E\nuP/nNIwHWjDyWj1jBWvZljRhwz/LrarpFUXnB5dD6kPUcBVQK01P1DOT0ypNdP1I\nMZT39mDbaWPg7u4ePUguw44SY+uqSYzuNJefi1XiUFSWjyyYvkA54bg8b76Qy05S\nrAyEMHv/vV9ag3a6ci2SxAk4YvJHBvuWiyHcTAIOkQIDAQABo4IBLjCCASowDgYD\nVR0PAQH/BAQDAgOoMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAdBgNV\nHQ4EFgQUALCUm4nZT2uuRsnUmmyq18iGtCMwHwYDVR0jBBgwFoAUogGWeckkcy07\n4QYd24aP/MoABIAwagYIKwYBBQUHAQEEXjBcMC0GCCsGAQUFBzABhiFodHRwOi8v\nbG9jYWxob3N0OjgyMDAvdjEvcGtpL29jc3AwKwYIKwYBBQUHMAKGH2h0dHA6Ly9s\nb2NhbGhvc3Q6ODIwMC92MS9wa2kvY2EwGgYDVR0RBBMwEYIJbG9jYWxob3N0hwR/\nAAABMDEGA1UdHwQqMCgwJqAkoCKGIGh0dHA6Ly9sb2NhbGhvc3Q6ODIwMC92MS9w\na2kvY3JsMA0GCSqGSIb3DQEBCwUAA4IBAQCXPPLWe89sz+t7wOLrwf1nuqiVd/RR\n6sjkHZbpSMT5IqTOedZYMcxNTAvFK5M5ofgUzm3s79FAVA7yKn8bOUAC78wAsGhk\nqfU34nHAlfGrc/wmmqYWRsDmZOvdMshJyC4PJPv9oiFNxETe3lnwufSjtxCDX9/9\nLX6uDInLKMg/mHwWMYAsqfv4j2WgDV0Hni2/lAVKmeYcCa3+EJqErDODPY7/ztZu\nW5Ki2MvfX3LMKsjE3jt5bQzroIGJhzVVOi5iJcbVp/vXU40+HE8j9+AX7wYSWJzJ\nEcUOhxHwFcTwaN074H0R+R6UC5fc8zyqqDbnMartwksb26UsWkGz68G2\n-----END CERTIFICATE-----",
                    ca_chain = new[] { "-----BEGIN CERTIFICATE-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEB\n-----END CERTIFICATE-----" },
                    expiration = 1716239023,
                    issuing_ca = "-----BEGIN CERTIFICATE-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEC\n-----END CERTIFICATE-----",
                    private_key = "-----BEGIN RSA PRIVATE KEY-----\nMIIEpAIBAAKCAQEA93v1VXWbbztrTAYTRUkkvYVu60XR6V0xrosJxpymPL8Eq76w\neQbYUErOTprUAQab73gxijL1Iminibd1Uk2BctcvIkcYeZQUp5uZ65ltUIaAdUMX\nyFan7FSvTjwmGaoJIbKcn62VfF8XZk0jQbzNfS87YAux/KPUGwEWXV3heE8EuP/n\nNIwHWjDyWj1jBWvZljRhwz/LrarpFUXnB5dD6kPUcBVQK01P1DOT0ypNdP1IMZT3\n9mDbaWPg7u4ePUguw44SY+uqSYzuNJefi1XiUFSWjyyYvkA54bg8b76Qy05SrAyE\nMHv/vV9ag3a6ci2SxAk4YvJHBvuWiyHcTAIOkQIDAQABAoIBAQC3S6Th/a/4iz6l\n2N4O8+R1Rc1oDZcxyudQCgRciIsK9MMl3J7hlHNDzJPOXhflBpsZmqC+ZY1vRanI\ncws+wM6+Wqe7MILtEQLXPIScnU78VBHyR5XmuF+4xuPsAtqJKYmH3rzn+u17ZwZy\nq3EQcCCVthS4xxC1ODWRJpYE4tJqgk736Jtd5B/bBcqxFXmad97xbyQ6WNc4je9l\ngo42dpcexbOEHfGMFGkf1P+b6gw9bOzzWTBToGWVKndHsEHQTgG1liYRR4INNcW7\nne5dSQNyiQrqrW2gaghRnWH7T4V5hy/PWte6n68DpJrWVrqNOtclnlczJ9o7MXCg\nCuTHiJ/9AoGBAPrgdhKE3qBaM4gjUtZK6d1sv4kicMbhzN5DzfX1rldc/QBSWzAk\nknUAzbknXculxwWbB1I08uHNQ3/CKA6+BPBfd9anePBNW1r6UvnZEZXYIxQ+E/Gd\nPAC50nurRAOZdK/uHeIHpbqCvtk4jyjqDxZggoD0dl+Sa2JXmi5A3s6DAoGBAPyJ\nwuJ5tG3FaYfGzF4dw5UL1NDQi1VFm9o5u46mUwfgB0TNv370cd2FAhTfEKR5QO7U\nGfeFZVcYhB8GtSTRP+kQM/BfQxwvbWJBZS7uzRAkL06vSkJvvrOMdzx1IIi/lp2m\nSO5XcS74dcLZnnWYUFW0vrUaY1cui8OaDlLTVeJbAoGADrcDtepdNIKV6zJHNZKH\nTRmH0n9WphOwdIj9l6OlajJmFJLADn7WqE43wthwQ/WhSs7hCw1YAa6Mev3kY5j5\nqS+wU8LW8SFYbmmoXEdDJMrco99QRCe40UIU+nP9NUjW80rALfXM3re0ggEzRG8W\nG3XlsbKlDs4Dxmzk+jmL2AkCgYEA5pzK2dP3/zICT5or8FpPy2DVg6adRk5dp2eH\nLhoWwp9DJAKbN8zz2i1nHDYjVX7g2/fWiqFHTMS3ijmu26M2MJe6RmxHtYpd4hcD\n1lr96hqRFNKgBpFS3VWNYSk4f4gte2NpQDWbxx/fMgNWX96qpcl7SZiCVQ/NU97v\n65TP3fcCgYB4JE4xOhsMDpFuGzYGHwxUhSAWTjb7laq9FHfFjDdsLpB4xe4hV7w/\nmdkQcV6pFJSjrzRxCUCQF+oUho1404SfiBq77EjAgKo1Ut4raO0FYq46OZh5Jz+i\nE5zb1CbizjAYWYKog6G32WbFYMLLL9tmp98ltjXj9smkqnIgkmqmag==\n-----END RSA PRIVATE KEY-----",
                    private_key_type = "rsa",
                    serial_number = "123456789ABCDEF"
                },
                wrap_info = new { },
                warnings = new { },
                auth = new { },
                mount_type = "pki"
            };


            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost:8200")
            };

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            _certificateService = new CertificateService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetCertificateFromVault_ShouldReturnCertificate()
        {
            var certificate = await _certificateService.GetCertificateFromVault();
            Assert.IsNotNull(certificate);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetCertificateFromVault_ShouldThrowException_OnFailure()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad Request")
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            _certificateService = new CertificateService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);
            
            await _certificateService.GetCertificateFromVault();
        }
  

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDependenciesAreNull()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new CertificateService(null!, _mockConfiguration.Object, _mockLogger.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new CertificateService(_mockHttpClientFactory.Object, null!, _mockLogger.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new CertificateService(_mockHttpClientFactory.Object, _mockConfiguration.Object, null!));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockHttpClientFactory.Reset();
            _mockConfiguration.Reset();
            _mockLogger.Reset();
        }
    }
}