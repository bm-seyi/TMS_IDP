using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using CertificateRequest = TMS_IDP.Models.CertificateRequest;

namespace TMS_IDP.Utilities
{
    public interface ICertificateService
    {
        Task<X509Certificate2> GetCertificateFromVault();
    }

    public class CertificateService : ICertificateService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CertificateService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<X509Certificate2> GetCertificateFromVault()
        {
            string token = _configuration["HashiCorp:Vault:Token"] ?? throw new ArgumentNullException(nameof(token));
            _logger.LogInformation("Retrieved Token from Configuration");

            HttpClient client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("http://localhost:8200");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Vault-Token", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

            Dictionary<string, string> payload = new Dictionary<string, string>
            {
                {"common_name", "localhost"},
                {"alt_names", "localhost"},
                {"ip_sans", "127.0.0.1"},
                {"ttl", "72h"}
            };

            StringContent content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Send the request to Vault to issue a certificate
            HttpResponseMessage response = await client.PostAsync("v1/pki/issue/my-role", content);
            _logger.LogInformation("Sent request to Vault to issue a certificate");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Status Code: {0}", response.StatusCode);
                _logger.LogError("Status Description: {0}", response.ReasonPhrase);
                _logger.LogError("Status Content: {0}", await response.Content.ReadAsStringAsync());

                throw new Exception($"Failed to get certificate from Vault: {response.ReasonPhrase}");
            }
            
            CertificateRequest responseContent = await response.Content.ReadFromJsonAsync<CertificateRequest>() ?? throw new ArgumentNullException(nameof(responseContent));
            string certificatePem = responseContent.Data.Certificate.ToString();
            _logger.LogInformation("Retrieved Certificate from Vault");
            string privateKeyPem = responseContent.Data.PrivateKey.ToString();
            _logger.LogInformation("Retrieved Private Key from Vault");

            X509Certificate2 certificate = X509Certificate2.CreateFromPem(certificatePem, privateKeyPem);

            return certificate;
        }

    }
}

