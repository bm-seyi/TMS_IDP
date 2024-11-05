using Duende.IdentityServer.Models;
using IdentityModel;

namespace TMS_API.Configuration
{
    public static class Clients
    {
        public static IEnumerable<Client> Get() =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("".ToSha256()) },
                    AllowedScopes = { "tms.read" }
                }
            };
    }

}
