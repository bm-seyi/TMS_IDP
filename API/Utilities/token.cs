using IdentityModel.Client;

namespace TMS_IDP.Utilities 
{
    public interface ITokenService
    {
    Task<TokenResponse> PCKEAsync(string codeVerifier, string ClientID, string code);
    }

    public class TokenService : ITokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<TokenResponse> PCKEAsync(string codeVerifier, string ClientID, string code)
        {
            HttpClient client =  _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", _configuration["API:Key"]);
            AuthorizationCodeTokenRequest tokenRequest = new AuthorizationCodeTokenRequest
            {
                Address = "https://localhost:5188/connect/token",
                ClientId = ClientID,
                Code = code,
                RedirectUri = "https://localhost:5188/account/callback",
                CodeVerifier = codeVerifier
            };

            return await client.RequestAuthorizationCodeTokenAsync(tokenRequest);
        }
    }

}
