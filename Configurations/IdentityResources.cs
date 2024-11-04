using IdServer = Duende.IdentityServer.Models;

namespace TMS_API.Configuration
{
    public static class IdentityResources
    {
        public static IEnumerable<IdServer.IdentityResource> Get() =>
            new List<IdServer.IdentityResource> { new IdServer.IdentityResources.OpenId(), new IdServer.IdentityResources.Profile() };
    }

}
