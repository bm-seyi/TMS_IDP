using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using TMS_IDP.Utilities;
using TMS_IDP.Models.DataProtection;


namespace TMS_IDP.Tests.Utilities
{

    [TestClass]
    public class CertificateServiceTests
    {
        private Mock<HttpMessageHandler> _handlerMock = null!;
        private HttpClient _httpClient = null!;
        private CertificateService _certificateService = null!;

        [TestInitialize]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("HashiCorp__Vault__Token", "test-token");
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8200")
            };
            _certificateService = new CertificateService(_httpClient);
        }

        [TestMethod]
        public async Task GenerateAsync_Success_ReturnsCertificate()
        {
            var mockResponse = new GenerateRequestModel
            {
                RequestId = "test-request-id",
                LeaseId = "test-lease-id",
                WrapInfo = null!,  // Assuming it's nullable
                Warnings = null!,  // Assuming it's nullable
                Auth = null!,      // Assuming it's nullable
                MountType = "pki",
                Data = new GenerateDataModel
                {
                    CaChain = new List<string> { "-----BEGIN CERTIFICATE-----\n...\n-----END CERTIFICATE-----" },
                    IssuingCa = "-----BEGIN CERTIFICATE-----\n...\n-----END CERTIFICATE-----",
                    SerialNumber = "test-serial-number",
                    Expiration = 1234567890,
                    PrivateKeyType = "rsa-2048",
                    Certificate = "-----BEGIN CERTIFICATE-----\nMIIDtzCCAp+gAwIBAgIUJkciWqqgDacXl+3/ugv2op4JFbkwDQYJKoZIhvcNAQEL\nBQAwFjEUMBIGA1UEAxMLZXhhbXBsZS5jb20wHhcNMjUwMjEzMTk1ODU4WhcNMjUw\nMjE2MTk1OTI4WjAUMRIwEAYDVQQDEwlsb2NhbGhvc3QwggEiMA0GCSqGSIb3DQEB\nAQUAA4IBDwAwggEKAoIBAQC4L/pp/G6n2AOJ8oPZAqwrNV48+vidw42uF75Df2vg\nFeJSLOvNlgARuw+H0WnBkeHPXeCLuEb8Yucl0UcXRkEX9C1J8nOl27A2TjWlQOy8\nTZ+389sx3CG5YlvqNhY1ZjlmcIfaXiCzA/c8h6jICg6cTVtBYc681BOc9Uw2ePzE\n3TYLk+kQQmfC0ltH0jxM8toR6T2iVcJuJbDXRhzvG6gE2AWQsnZ8lc7IluJe0qeA\nSJVKc1sCmQMfLlYelOaQk4ncyRKdWbVc7zCeGGGjNsbWxoOJZADDZTFxi/aCHf7I\nV1IU91OxAzIFgoepW3/5E9C7LR1zRVxYP3pDX+kr580RAgMBAAGjgf4wgfswDgYD\nVR0PAQH/BAQDAgOoMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAdBgNV\nHQ4EFgQU0K0Tyf8hlSbKeae8cPGpBJCtTmAwHwYDVR0jBBgwFoAU2AdhMZeLwUJ8\nMu0XlLzfZ41OD+kwOwYIKwYBBQUHAQEELzAtMCsGCCsGAQUFBzAChh9odHRwOi8v\nbG9jYWxob3N0OjgyMDAvdjEvcGtpL2NhMBoGA1UdEQQTMBGCCWxvY2FsaG9zdIcE\nfwAAATAxBgNVHR8EKjAoMCagJKAihiBodHRwOi8vbG9jYWxob3N0OjgyMDAvdjEv\ncGtpL2NybDANBgkqhkiG9w0BAQsFAAOCAQEAYN1kNPh4gnHnkWjYbe/YLGnACkTT\nvt9eRyEOHEz8PwQZhim3js4AINg/x4CDrg5fiM7zBKmb2+1JJPmGHNn26v9HbTX9\ndUFZEhBzdk3XnoWqXnmcHh2zqCOEngFXASTbh1v5McwZtloqOnJB2lJEvfeaYu5C\nCrp40k4Lj4zxf4doefcpRXcpfrXLGroDKZ7hwTc791rb7Bg3K3TzVsmwnqplxVQi\nG7jMFp1b7DBBecWXGUEIVovjC3td5XPB32AswPBi88mqZM5uH4F6fCwKI8Dd7+gG\nm9ZG9+lj0YiJB8Rb8h41fjo3S3Wm8v0PJO8GUTnaksqzLX8cekMTxnBISg==\n-----END CERTIFICATE-----",
                    PrivateKey = "-----BEGIN RSA PRIVATE KEY-----\nMIIEpAIBAAKCAQEAuC/6afxup9gDifKD2QKsKzVePPr4ncONrhe+Q39r4BXiUizr\nzZYAEbsPh9FpwZHhz13gi7hG/GLnJdFHF0ZBF/QtSfJzpduwNk41pUDsvE2ft/Pb\nMdwhuWJb6jYWNWY5ZnCH2l4gswP3PIeoyAoOnE1bQWHOvNQTnPVMNnj8xN02C5Pp\nEEJnwtJbR9I8TPLaEek9olXCbiWw10Yc7xuoBNgFkLJ2fJXOyJbiXtKngEiVSnNb\nApkDHy5WHpTmkJOJ3MkSnVm1XO8wnhhhozbG1saDiWQAw2UxcYv2gh3+yFdSFPdT\nsQMyBYKHqVt/+RPQuy0dc0VcWD96Q1/pK+fNEQIDAQABAoIBAQCDopZ/ZM/Y2QM+\ndbpRQB24C748KsnARuBdCc8LAmggkMNdI4mrNob6JQymbr38f7w4rckrUho5ip3U\nY0tBkQ44hmRPsx1/7hBz31Vrs4j90yRRHdEMQ10+1tMGAn3A0Dw5wDb3k8oe5rit\n4+52eOmUP5z2j2ZngdI6nP1I+RL9xjqOiMsHRsNhY9ypYgO66exxj6WxTixN+Aq0\nlhLjMl0LxKsix9iruAwmivD3HnsdN83CkokXlRyiXC36TRhgyZgoPyVuHmQsaAiQ\nZpatq4otp30CIVLY/N34sqTaSCWQWa/PRKF9fRY9mPXxe1lPEKm8yhjtEfTw6Wxo\n3aQWiy2hAoGBAPUZE+5Ly2K0BUrnTpAGjOJ+9LXcNEfG9uqws6tx/Zyxjz31F+wD\nVUFvE3lLWuqMmXcWXe/BzrxS50wf3rAXFWsQHIqX5pyaZRm1NCvrgokLbL2QGQ70\nyxf8GiZDD0D5bC2Voae1Uv24b5pyK1Pl+o7L7MGalnlt2MutZkis6849AoGBAMBh\nUGMYaFBGdNZTNQ9XGmymqfB1L23406ANOqKVzefUqGFADHhJTbbR3GTVmYDrOqlb\nVhnscXKaQF59wqYd1Xph/QJhGTqcGpXBDS9z2jIXYTErDNl2Go8MkP3x68BPDyvW\nh/wCsS29EcU5+sHt8X79JV73ZTACYeMIWVYNixtlAoGAHWUoksfcWLYmfFlJftSK\nSQ/Y4YbLbmBadMNEiSdet1BEUbX3bILp0rMzrrRu7vp13WZ9Vaf013lJ7ENWPeBG\n3VRNWAHn0phhz7d/zlSsjysjm4iQuM57HSFLMZORXMWNR9pOTQLeNTfNisRuld1b\nM40ZlA6qRV37RlJBli3HCjECgYBwbK5IquvS9cmzsn6Qj2uO0TsAncrw7nfl0bVR\nbFAfSgR4iLCA3v2+eBffCYCieVUXwZuonKeTvJcfYUkOQOMPmRH9gPb4bF+Q4net\nInwBx+3xiOICd2V/8W0OKoGGKe2Ixd9EI+KdAx/ObVqgWEhH2PIs9FC65LmFrsxe\nYJ3JjQKBgQCRbEjJuHtzjAQjQuL1wKIs/Hu8NxON0qDmPna84SyL8LEPNV4DdcA0\nrVJSI82XwQQnJN24T9ezAyRjtZzLzqnMOcpurhzLvZaQJfINsn+Bm/YoYyxUcmKX\nVDJWSf3vTWa7bQoi13Nhnacxxl699OcdlAzPmCaHQ9iVPyn2s3Afwg==\n-----END RSA PRIVATE KEY-----"
                }
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(mockResponse)
                });

            var result = await _certificateService.GenerateAsync();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RetrieveAsync_CertificateExists_ReturnsCertificate()
        {
            var mockResponse = new ReceiveCertificateModel
            {
                RequestId = "test-request-id",
                LeaseId = "test-lease-id",
                WrapInfo = null!,  // Assuming it's nullable
                Warnings = null!,  // Assuming it's nullable
                Auth = null!,      // Assuming it's nullable
                MountType = "pki",
                Data = new RetrieveCertificateDataWrapper
                {
                    Metadata = new Metadata
                    {
                        CreatedTime = DateTime.Now,
                        CustomMetadata = null!,  // Assuming it's nullable
                        DeletionTime = "test-deletion-time"
                    },
                    Data = new RetrieveCertificateData
                    {
                        Certificate = "-----BEGIN CERTIFICATE-----\nMIIDtzCCAp+gAwIBAgIUJkciWqqgDacXl+3/ugv2op4JFbkwDQYJKoZIhvcNAQEL\nBQAwFjEUMBIGA1UEAxMLZXhhbXBsZS5jb20wHhcNMjUwMjEzMTk1ODU4WhcNMjUw\nMjE2MTk1OTI4WjAUMRIwEAYDVQQDEwlsb2NhbGhvc3QwggEiMA0GCSqGSIb3DQEB\nAQUAA4IBDwAwggEKAoIBAQC4L/pp/G6n2AOJ8oPZAqwrNV48+vidw42uF75Df2vg\nFeJSLOvNlgARuw+H0WnBkeHPXeCLuEb8Yucl0UcXRkEX9C1J8nOl27A2TjWlQOy8\nTZ+389sx3CG5YlvqNhY1ZjlmcIfaXiCzA/c8h6jICg6cTVtBYc681BOc9Uw2ePzE\n3TYLk+kQQmfC0ltH0jxM8toR6T2iVcJuJbDXRhzvG6gE2AWQsnZ8lc7IluJe0qeA\nSJVKc1sCmQMfLlYelOaQk4ncyRKdWbVc7zCeGGGjNsbWxoOJZADDZTFxi/aCHf7I\nV1IU91OxAzIFgoepW3/5E9C7LR1zRVxYP3pDX+kr580RAgMBAAGjgf4wgfswDgYD\nVR0PAQH/BAQDAgOoMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAdBgNV\nHQ4EFgQU0K0Tyf8hlSbKeae8cPGpBJCtTmAwHwYDVR0jBBgwFoAU2AdhMZeLwUJ8\nMu0XlLzfZ41OD+kwOwYIKwYBBQUHAQEELzAtMCsGCCsGAQUFBzAChh9odHRwOi8v\nbG9jYWxob3N0OjgyMDAvdjEvcGtpL2NhMBoGA1UdEQQTMBGCCWxvY2FsaG9zdIcE\nfwAAATAxBgNVHR8EKjAoMCagJKAihiBodHRwOi8vbG9jYWxob3N0OjgyMDAvdjEv\ncGtpL2NybDANBgkqhkiG9w0BAQsFAAOCAQEAYN1kNPh4gnHnkWjYbe/YLGnACkTT\nvt9eRyEOHEz8PwQZhim3js4AINg/x4CDrg5fiM7zBKmb2+1JJPmGHNn26v9HbTX9\ndUFZEhBzdk3XnoWqXnmcHh2zqCOEngFXASTbh1v5McwZtloqOnJB2lJEvfeaYu5C\nCrp40k4Lj4zxf4doefcpRXcpfrXLGroDKZ7hwTc791rb7Bg3K3TzVsmwnqplxVQi\nG7jMFp1b7DBBecWXGUEIVovjC3td5XPB32AswPBi88mqZM5uH4F6fCwKI8Dd7+gG\nm9ZG9+lj0YiJB8Rb8h41fjo3S3Wm8v0PJO8GUTnaksqzLX8cekMTxnBISg==\n-----END CERTIFICATE-----",
                        PrivateKey = "-----BEGIN RSA PRIVATE KEY-----\nMIIEpAIBAAKCAQEAuC/6afxup9gDifKD2QKsKzVePPr4ncONrhe+Q39r4BXiUizr\nzZYAEbsPh9FpwZHhz13gi7hG/GLnJdFHF0ZBF/QtSfJzpduwNk41pUDsvE2ft/Pb\nMdwhuWJb6jYWNWY5ZnCH2l4gswP3PIeoyAoOnE1bQWHOvNQTnPVMNnj8xN02C5Pp\nEEJnwtJbR9I8TPLaEek9olXCbiWw10Yc7xuoBNgFkLJ2fJXOyJbiXtKngEiVSnNb\nApkDHy5WHpTmkJOJ3MkSnVm1XO8wnhhhozbG1saDiWQAw2UxcYv2gh3+yFdSFPdT\nsQMyBYKHqVt/+RPQuy0dc0VcWD96Q1/pK+fNEQIDAQABAoIBAQCDopZ/ZM/Y2QM+\ndbpRQB24C748KsnARuBdCc8LAmggkMNdI4mrNob6JQymbr38f7w4rckrUho5ip3U\nY0tBkQ44hmRPsx1/7hBz31Vrs4j90yRRHdEMQ10+1tMGAn3A0Dw5wDb3k8oe5rit\n4+52eOmUP5z2j2ZngdI6nP1I+RL9xjqOiMsHRsNhY9ypYgO66exxj6WxTixN+Aq0\nlhLjMl0LxKsix9iruAwmivD3HnsdN83CkokXlRyiXC36TRhgyZgoPyVuHmQsaAiQ\nZpatq4otp30CIVLY/N34sqTaSCWQWa/PRKF9fRY9mPXxe1lPEKm8yhjtEfTw6Wxo\n3aQWiy2hAoGBAPUZE+5Ly2K0BUrnTpAGjOJ+9LXcNEfG9uqws6tx/Zyxjz31F+wD\nVUFvE3lLWuqMmXcWXe/BzrxS50wf3rAXFWsQHIqX5pyaZRm1NCvrgokLbL2QGQ70\nyxf8GiZDD0D5bC2Voae1Uv24b5pyK1Pl+o7L7MGalnlt2MutZkis6849AoGBAMBh\nUGMYaFBGdNZTNQ9XGmymqfB1L23406ANOqKVzefUqGFADHhJTbbR3GTVmYDrOqlb\nVhnscXKaQF59wqYd1Xph/QJhGTqcGpXBDS9z2jIXYTErDNl2Go8MkP3x68BPDyvW\nh/wCsS29EcU5+sHt8X79JV73ZTACYeMIWVYNixtlAoGAHWUoksfcWLYmfFlJftSK\nSQ/Y4YbLbmBadMNEiSdet1BEUbX3bILp0rMzrrRu7vp13WZ9Vaf013lJ7ENWPeBG\n3VRNWAHn0phhz7d/zlSsjysjm4iQuM57HSFLMZORXMWNR9pOTQLeNTfNisRuld1b\nM40ZlA6qRV37RlJBli3HCjECgYBwbK5IquvS9cmzsn6Qj2uO0TsAncrw7nfl0bVR\nbFAfSgR4iLCA3v2+eBffCYCieVUXwZuonKeTvJcfYUkOQOMPmRH9gPb4bF+Q4net\nInwBx+3xiOICd2V/8W0OKoGGKe2Ixd9EI+KdAx/ObVqgWEhH2PIs9FC65LmFrsxe\nYJ3JjQKBgQCRbEjJuHtzjAQjQuL1wKIs/Hu8NxON0qDmPna84SyL8LEPNV4DdcA0\nrVJSI82XwQQnJN24T9ezAyRjtZzLzqnMOcpurhzLvZaQJfINsn+Bm/YoYyxUcmKX\nVDJWSf3vTWa7bQoi13Nhnacxxl699OcdlAzPmCaHQ9iVPyn2s3Afwg==\n-----END RSA PRIVATE KEY-----"
                    }
                }
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(mockResponse)
                });

            var result = await _certificateService.RetrieveAsync("some/path");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RetrieveAsync_CertificateNotFound_ReturnsNull()
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            var result = await _certificateService.RetrieveAsync("invalid/path");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task DeleteAsync_SuccessfulDeletion()
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

            await _certificateService.DeleteAsync("some/path");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task DeleteAsync_FailedDeletion_ThrowsException()
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Bad Request"
                });

            await _certificateService.DeleteAsync("some/path");
        }

        [TestMethod]
        public async Task ListAsync_Success_ReturnsList()
        {
            var mockResponse = new ListCertificateModel
            {
                RequestId = "test-request-id",
                LeaseId = "test-lease-id",
                WrapInfo = null!,  // Assuming it's nullable
                Warnings = null!,  // Assuming it's nullable
                Auth = null!,      // Assuming it's nullable
                MountType = "pki",
                Data = new ListDataContent
                {
                    Keys = new List<string> { "cert1", "cert2" }
                }
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(mockResponse)
                });

            var result = await _certificateService.ListAsync("some/path");

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task ListAsync_NotFound_ReturnsNull()
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            var result = await _certificateService.ListAsync("invalid/path");

            Assert.IsNull(result);
        }
    }
}