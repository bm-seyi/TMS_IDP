using IdentityModel.Client;

public interface ITokenService
{
    Task<TokenResponse> ROPCAsync(string password, string username, string ClientID, string ClientSecret);
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

    public async Task<TokenResponse> ROPCAsync(string password, string username, string ClientID, string ClientSecret)
    {
        HttpClient client =  _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", _configuration["API:Key"]);
        var tokenRequest = new PasswordTokenRequest
        {
            Address = "http://localhost:5188/connect/token",
            Password = password,
            UserName = username,
            ClientId = ClientID,
            ClientSecret = ClientSecret,
            Scope = "tms.read"
        };

        return await client.RequestPasswordTokenAsync(tokenRequest);
    }
}
