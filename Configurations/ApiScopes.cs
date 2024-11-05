using Duende.IdentityServer.Models;

namespace TMS_API.Configuration
{
    public static class ApiScopes
    {
        public static IEnumerable<ApiScope> Get() =>
            new List<ApiScope> { new ApiScope("tms.read", "My API") };
    }

}
