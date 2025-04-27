
using Duende.IdentityServer.EntityFramework.Entities;

namespace TMS_SEED.Models
{
    public class IdentityServerModel
    {
        public List<ClientModel> Clients { get; set; } = new List<ClientModel>();
        public List<ApiScopeModel> ApiScopes { get; set; } = new List<ApiScopeModel>();
        public List<IdentityResourceModel> IdentityResources { get; set; } = new List<IdentityResourceModel>();
        public List<ApiResourceModel> ApiResources { get; set; } = new List<ApiResourceModel>();

    }

    public class ClientModel
    {
        public required string ClientId { get; set; }
        public required string ClientName { get; set; }
        public bool RequireClientSecret { get; set; }
        public required List<string> RedirectUris { get; set; }
        public required List<string> PostLogoutRedirectUris { get; set; }
        public required List<string> AllowedCorsOrigins { get; set; }
        public required List<string> AllowedScopes { get; set; }
        public bool AllowOfflineAccess { get; set; }
        public required List<string> AllowedGrantTypes { get; set; }
        public bool RequirePkce { get; set; }
        public int RefreshTokenUsage { get; set; }
        public int RefreshTokenExpiration { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int SlidingRefreshTokenLifetime { get; set; }
        public bool AllowAccessTokensViaBrowser { get; set; }
        public int IdentityTokenLifetime { get; set; }
    }

    public class ApiScopeModel
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Description { get; set; }
        public bool Required { get; set; }
        public bool Emphasize { get; set; }
    }

    public class IdentityResourceModel
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Description { get; set; }
        public bool Required { get; set; }
        public required List<string> UserClaims { get; set; }
    }

    public class ApiResourceModel
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Description { get; set; }
        public required List<string> Scopes { get; set; }
        public required List<string> UserClaims { get; set; }
    }
}

