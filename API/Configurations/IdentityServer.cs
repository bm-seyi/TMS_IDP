using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.EntityFrameworkCore;

namespace TMS_IDP.Configuration
{
    public static class IdentityServerSeeder
    {
        public async static Task Seed (IServiceProvider serviceProvider)
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            ConfigurationDbContext configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

            await configurationDbContext.Database.EnsureCreatedAsync();
            if (configurationDbContext.Database.IsRelational()) 
            {
                await configurationDbContext.Database.MigrateAsync();
            }

            if (!configurationDbContext.ApiScopes.Any())
            {
                ApiScope apiScope = new ApiScope
                {
                    Name = "api1.read", 
                    DisplayName = "Read Access to API 1"
                };
                await configurationDbContext.ApiScopes.AddAsync(apiScope.ToEntity());
            }

            if (!configurationDbContext.ApiResources.Any())
            {
                ApiResource apiResource = new ApiResource
                {
                    Name = "api1",
                    DisplayName = "my API",
                    Scopes = { "api1.read", "api1.write" }
                };

                await configurationDbContext.ApiResources.AddAsync(apiResource.ToEntity());
            }

            if  (!configurationDbContext.Clients.Any())
            {
                Client client = new Client
                {
                    ClientId = "maui_client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false, // public client (.NET MAUI)
                    RedirectUris = {  "http://localhost:5000/callback" },
                    PostLogoutRedirectUris = { "http://localhost:5000/signout-callback" },
                    AllowedScopes = { "openid", "profile", "api1.read", "offline_access" },
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                };

                await configurationDbContext.Clients.AddAsync(client.ToEntity());
            }

            if (!configurationDbContext.IdentityResources.Any())
            {
                List<IdentityResource> identityResource = new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResource("custom", new[] 
                    { 
                        JwtClaimTypes.Name, 
                        JwtClaimTypes.Email 
                    })
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