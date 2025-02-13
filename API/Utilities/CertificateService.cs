using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using TMS_IDP.Models.DataProtection;

namespace TMS_IDP.Utilities
{
    public class CertificateService
    {
        private readonly HttpClient _client;
        
        public CertificateService(HttpClient client)
        {
            string vaultToken = Environment.GetEnvironmentVariable("HashiCorp__Vault__Token") ?? throw new ArgumentNullException(nameof(vaultToken));
            Console.WriteLine("Vault Token Received From Environment Variables");

            _client = client;
            _client.BaseAddress = new Uri("http://localhost:8200");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("X-Vault-Token", vaultToken);
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            Console.WriteLine("HttpClient Configured for Vault API");
        }
        

        public async Task<X509Certificate2> GenerateAsync()
        {

            Dictionary<string, string> payload = new Dictionary<string, string>
            {
                {"common_name", "localhost"},
                {"alt_names", "localhost"},
                {"ip_sans", "127.0.0.1"},
                {"ttl", "72h"}
            };

            StringContent content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Send the request to Vault to issue a certificate
            HttpResponseMessage response = await _client.PostAsync("v1/pki/issue/my-role", content);
            Console.WriteLine("Request Sent to Vault to Generate Certificate");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Status Code: {0}", response.StatusCode);
                Console.WriteLine("Status Description: {0}", response.ReasonPhrase);
                Console.WriteLine("Status Content: {0}", await response.Content.ReadAsStringAsync());

                throw new Exception($"Failed to get certificate from Vault: {response.ReasonPhrase}");
            }
            
            GenerateRequestModel responseContent = await response.Content.ReadFromJsonAsync<GenerateRequestModel>() ?? throw new ArgumentNullException(nameof(responseContent));
            string certificatePem = responseContent.Data.Certificate.ToString();
            Console.WriteLine("Retrieved Certificate from Vault");
            string privateKeyPem = responseContent.Data.PrivateKey.ToString();
            Console.WriteLine("Retrieved Private Key from Vault");

            X509Certificate2 certificate = X509Certificate2.CreateFromPem(certificatePem, privateKeyPem);

            return certificate;
        }

        public async Task<X509Certificate2?> RetrieveAsync(string path)
        {
            try
            {
                ReceiveCertificateModel responseMessage = await _client.GetFromJsonAsync<ReceiveCertificateModel>(path) ?? throw new ArgumentNullException(nameof(responseMessage)); 
                Console.WriteLine("Request Sent to Vault to Retrieve Certificate");

                string certificatePem = responseMessage.Data.Data.Certificate.ToString();
                Console.WriteLine("Retrieved Certificate from Vault");

                string privateKeyPem = responseMessage.Data.Data.PrivateKey.ToString();
                Console.WriteLine("Retrieved Private Key from Vault");

                X509Certificate2 certificate = X509Certificate2.CreateFromPem(certificatePem, privateKeyPem);
                return certificate;    
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Certificate not found in Vault");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get certificate from Vault: {0}", ex.Message);
                return null;
            }
        }

        public async Task DeleteAsync(string path)
        {
            HttpResponseMessage responseMessage = await _client.DeleteAsync(path);
            Console.WriteLine("Request Sent to Vault to Delete Certificate");

            if (!responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("Status Code: {0}", responseMessage.StatusCode);
                Console.WriteLine("Status Description: {0}", responseMessage.ReasonPhrase);
                Console.WriteLine("Status Content: {0}", await responseMessage.Content.ReadAsStringAsync());

                throw new Exception($"Failed to get certificate from Vault: {responseMessage.ReasonPhrase}");
            }
        }

        public async Task StoreAsync(string path, X509Certificate2 certificate)
        {
            RSA rSA = certificate.GetRSAPrivateKey() ?? throw new ArgumentNullException(nameof(rSA));
            string privateKey = $"-----BEGIN PRIVATE KEY-----\n{Convert.ToBase64String(rSA.ExportPkcs8PrivateKey(), Base64FormattingOptions.InsertLineBreaks)}\n-----END PRIVATE KEY-----"; 

            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                { "data", new Dictionary<string, string>
                    {
                        { "certificate", certificate.ExportCertificatePem() },
                        { "private_key", privateKey }
                    }
                }
            };

            StringContent content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = await _client.PostAsync(path, content);
            Console.WriteLine("Request Sent to Vault to Store Certificate");

            if (!responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("Status Code: {0}", responseMessage.StatusCode);
                Console.WriteLine("Status Description: {0}", responseMessage.ReasonPhrase);
                Console.WriteLine("Status Content: {0}", await responseMessage.Content.ReadAsStringAsync());

                throw new Exception($"Failed to get certificate from Vault: {responseMessage.ReasonPhrase}");
            }
        }

        public async Task<List<string>?> ListAsync(string path)
        {
            try
            {
                ListCertificateModel response = await _client.GetFromJsonAsync<ListCertificateModel>(path)  ?? throw new ArgumentNullException(nameof(response));
                Console.WriteLine("Request sent to Vault to List Certificates");
                return [.. response.Data.Keys];
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Requested location within Vault not found");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get certificate from Vault: {0}", ex.Message);
                return null;
            }           
        }
    }
}

