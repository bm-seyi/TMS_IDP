using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace TMS_API.Utilities
{

    public interface IJwtSecurity
    {
        string JwtTokenGenerator(string Email);
    }

    public class JwtSecurity : IJwtSecurity
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtSecurity> _logger;

        public JwtSecurity(IConfiguration configuration, ILogger<JwtSecurity> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string JwtTokenGenerator(string Email)
        {
            string issuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentNullException(nameof(issuer));
            string audience =  _configuration["Jwt:Audience"] ?? throw new ArgumentNullException(nameof(audience));
            byte[] key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new ArgumentNullException(nameof(key)));

            SigningCredentials loginCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[] 
            { 
                new Claim(JwtRegisteredClaimNames.Sub, Email),
                new Claim(JwtRegisteredClaimNames.Email, Email)
            });

            DateTime expiry = DateTime.UtcNow.AddMinutes(15);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = expiry,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = loginCredentials
            };

            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = securityTokenHandler.CreateToken(tokenDescriptor);
            
            _logger.LogInformation("Access Token has been generated");
            return securityTokenHandler.WriteToken(token);
        }
    }
}


