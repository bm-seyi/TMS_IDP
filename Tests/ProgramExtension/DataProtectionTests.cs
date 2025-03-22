using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using StackExchange.Redis;
using Moq;
using TMS_IDP.Utilities;


namespace TMS_IDP.Tests.ProgramExtensions
{
    [TestClass]
    public class ProgramExtensionsTests
    {
        private Mock<ICertificateService> _mockCertificateService = null!;
        private Mock<IConnectionMultiplexer> _mockRedis = null!;
        private IServiceCollection _serviceCollection = null!;

        [TestInitialize]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("HashiCorp__Vault__Token", "testtoken");
            _mockCertificateService = new Mock<ICertificateService>();
            _serviceCollection = new ServiceCollection();
            _mockRedis = new Mock<IConnectionMultiplexer>();
        }

        [TestMethod]
        public async Task AddDataProtectionAsync_UsesExistingValidCertificate()
        {
            // Arrange
            X509Certificate2 existingCert = CreateTestCertificate();
            _mockCertificateService.Setup(c => c.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(existingCert);

            // Act
            await _serviceCollection.AddDataProtectionAsync(_mockCertificateService.Object, _mockRedis.Object);

            // Assert
            Assert.IsTrue(_serviceCollection.Count > 0);
        }

        [TestMethod]
        public async Task AddDataProtectionAsync_GeneratesNewCertificate_WhenNoneExists()
        {
            // Arrange
            _mockCertificateService.Setup(c => c.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync((X509Certificate2?)null);

            var newCert = CreateTestCertificate();
            _mockCertificateService.Setup(c => c.GenerateAsync())
                .ReturnsAsync(newCert);


            // Act
            await _serviceCollection.AddDataProtectionAsync( _mockCertificateService.Object, _mockRedis.Object);

            // Assert
            _mockCertificateService.Verify(c => c.GenerateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task AddDataProtectionAsync_ReplacesExpiredCertificate()
        {
            // Arrange
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest(
                "CN=TestCertificate",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            X509Certificate2 expiredCert = req.CreateSelfSigned(DateTimeOffset.Now.AddYears(-2), DateTimeOffset.Now.AddYears(-1));

            _mockCertificateService.Setup(c => c.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(expiredCert);

            _mockCertificateService.Setup(c => c.GenerateAsync())
                .ReturnsAsync(CreateTestCertificate());


            // Act
            await _serviceCollection.AddDataProtectionAsync(_mockCertificateService.Object, _mockRedis.Object);

            // Assert
            _mockCertificateService.Verify(c => c.DeleteAsync(It.IsAny<string>()), Times.Once);
            _mockCertificateService.Verify(c => c.GenerateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task AddDataProtectionAsync_RetrieveArchivedCertificate()
        {
            // Arrange
            _mockCertificateService.Setup(c => c.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(CreateTestCertificate());

            _mockCertificateService.Setup(c => c.ListAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "test", "test2" });


            // Act
            await _serviceCollection.AddDataProtectionAsync(_mockCertificateService.Object, _mockRedis.Object);

            // Assert
            _mockCertificateService.Verify(c => c.RetrieveAsync(It.IsAny<string>()), Times.Exactly(3));
        }

        public X509Certificate2 CreateTestCertificate()
        {
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest(
                "CN=TestCertificate",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            return req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        }

    }
}

