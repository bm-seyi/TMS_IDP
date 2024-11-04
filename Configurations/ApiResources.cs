using Duende.IdentityServer.Models;

namespace TMS_API.Configuration
{
    public static class ApiResources
    {
        public static IEnumerable<ApiResource> Get() =>
            new List<ApiResource> { new ApiResource("tms.read", "TMS read only") { Scopes = { "tms.read" } } };
    }
}

