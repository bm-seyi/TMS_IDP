using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;

namespace TMS_API.Configuration
{
    public static class IdentityServerSeeder
    {
        public async static Task Seed (IServiceProvider serviceProvider)
        {
           await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
           ConfigurationDbContext configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

           await configurationDbContext.Database.MigrateAsync();

           if (!configurationDbContext.ApiScopes.Any())
           {
                ApiScope apiScope = new ApiScope
                {
                    Name = "default", 
                    DisplayName = "Default API scope"
                };
                await configurationDbContext.ApiScopes.AddAsync(apiScope.ToEntity());
           }

           if (!configurationDbContext.ApiResources.Any())
           {
                ApiResource apiResource = new ApiResource
                {
                    Name = "default_api",
                    DisplayName = "Default API",
                    Scopes = { "default" }
                };

                await configurationDbContext.ApiResources.AddAsync(apiResource.ToEntity());
           }

           if  (!configurationDbContext.Clients.Any())
           {
                Client client = new Client
                {
                    ClientId = "default_client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("default_secret".Sha256()) },
                    AllowedScopes = { "default" }
                };
           }

           if (!configurationDbContext.IdentityResources.Any())
           {
                List<IdentityResource> identityResource = new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile()
                };

                foreach (IdentityResource resource in identityResource)
                {
                    await configurationDbContext.IdentityResources.AddAsync(resource.ToEntity());
                }
           }

            await configurationDbContext.SaveChangesAsync();
        }
    }
}