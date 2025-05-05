using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using TMS_SEED.Models;

namespace TMS_SEED.Utilities
{
    public interface ISeeder
    {
        Task SeedAsync();
    }

    public class Seeder : ISeeder
    {
        private readonly ConfigurationDbContext _context;
        private readonly IdentityServerModel _identityServer;

        public Seeder(ConfigurationDbContext context, IdentityServerModel identityServer)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _identityServer = identityServer ?? throw new ArgumentNullException(nameof(identityServer));
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            if (_context.Database.IsRelational()) 
            {
                await _context.Database.MigrateAsync();
            }
            if (!_context.Clients.Any())
            {
                foreach (ClientModel client in _identityServer.Clients)
                {
                    Client clientEntity = new Client
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName,
                        RequireClientSecret = client.RequireClientSecret,
                        RedirectUris = client.RedirectUris.Select(r => new ClientRedirectUri { RedirectUri = r }).ToList(),
                        PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(r => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = r }).ToList(),
                        AllowedCorsOrigins = client.AllowedCorsOrigins.Select(o => new ClientCorsOrigin { Origin = o }).ToList(),
                        AllowedScopes = client.AllowedScopes.Select(s => new ClientScope { Scope = s }).ToList(),
                        AllowOfflineAccess = client.AllowOfflineAccess,
                        AllowedGrantTypes = client.AllowedGrantTypes.Select(g => new ClientGrantType { GrantType = g }).ToList(),
                        RequirePkce = client.RequirePkce,
                        RefreshTokenUsage = client.RefreshTokenUsage,
                        RefreshTokenExpiration = client.RefreshTokenExpiration,
                        AccessTokenLifetime = client.AccessTokenLifetime,
                        SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
                        AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                        IdentityTokenLifetime = client.IdentityTokenLifetime
                    };
                    await _context.Clients.AddAsync(clientEntity);
                }
            }

            if (!_context.ApiScopes.Any())
            {
                foreach (ApiScopeModel apiScope in _identityServer.ApiScopes)
                {
                    var apiScopeEntity = new ApiScope
                    {
                        Name = apiScope.Name,
                        DisplayName = apiScope.DisplayName,
                        Description = apiScope.Description,
                        Required = apiScope.Required,
                        Emphasize = apiScope.Emphasize
                    };

                    await _context.ApiScopes.AddAsync(apiScopeEntity);
                }
            }

            if (!_context.IdentityResources.Any())
            {
                foreach (IdentityResourceModel identityResource in _identityServer.IdentityResources)
                {
                    IdentityResource identityResourceEntity = new IdentityResource
                    {
                        Name = identityResource.Name,
                        DisplayName = identityResource.DisplayName,
                        Description = identityResource.Description,
                        Required = identityResource.Required,
                        UserClaims = identityResource?.UserClaims?.Select(c => new IdentityResourceClaim { Type = c }).ToList()
                    };

                    await _context.IdentityResources.AddAsync(identityResourceEntity);
                }
            }

            if (!_context.ApiResources.Any())
            {
                foreach (ApiResourceModel apiResource in _identityServer.ApiResources)
                {
                    var apiResourceEntity = new ApiResource
                    {
                        Name = apiResource.Name,
                        DisplayName = apiResource.DisplayName,
                        Description = apiResource.Description,
                        Scopes = apiResource.Scopes.Select(s => new ApiResourceScope { Scope = s }).ToList(),
                        UserClaims = apiResource.UserClaims.Select(c => new ApiResourceClaim { Type = c }).ToList()
                    };

                    await _context.ApiResources.AddAsync(apiResourceEntity);
                }
            }

            await _context.SaveChangesAsync();
            
        }
    }
}