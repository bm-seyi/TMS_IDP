using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TMS_SEED.Models;
using TMS_SEED.Utilities;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace TMS_SEED.Tests.Utilities
{
    [TestClass]
    public class SeederTests
    {
        private ConfigurationDbContext _dbContext;
        private IdentityServerModel _identityServerModel;
        private Seeder _seeder;
        private IServiceProvider _serviceProvider = null!;

        [TestInitialize]
        public void Initialize()
        {
           var options = new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var configurationStoreOptions = new ConfigurationStoreOptions();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<ConfigurationDbContext>(optionsAction: options => options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
            serviceCollection.AddSingleton(new ConfigurationStoreOptions());
            _serviceProvider = serviceCollection.BuildServiceProvider();

            _dbContext = _serviceProvider.GetRequiredService<ConfigurationDbContext>();

            _identityServerModel = new IdentityServerModel
            {
                ApiScopes = new List<ApiScopeModel>
                {
                    new ApiScopeModel
                    {
                        Name = "api1",
                        DisplayName = "API 1",
                        Description = "Description for API 1",
                    }
                },
                Clients = new List<ClientModel>
                {
                    new ClientModel
                    {
                        ClientId = "client1",
                        ClientName = "Client 1",
                        RequireClientSecret = true,
                        RedirectUris = new List<string> { "https://localhost/callback" },
                        PostLogoutRedirectUris = new List<string> { "https://localhost/signout-callback" },
                        AllowedCorsOrigins = new List<string> { "https://localhost" },
                        AllowedScopes = new List<string> { "api1" },
                        AllowOfflineAccess = true,
                        AllowedGrantTypes = new List<string> { "authorization_code" },
                        RequirePkce = true,
                        RefreshTokenUsage = 654,
                        RefreshTokenExpiration = 8685,
                        AccessTokenLifetime = 3600,
                        SlidingRefreshTokenLifetime = 1296000,
                        AllowAccessTokensViaBrowser = true,
                        IdentityTokenLifetime = 300
                    }
                },
                IdentityResources = new List<IdentityResourceModel>
                {
                    new IdentityResourceModel
                    {
                        Name = "openid",
                        DisplayName = "OpenID",
                        Description = "OpenID Connect scope",
                        UserClaims = new List<string> { "sub" }
                    }
                },
                ApiResources = new List<ApiResourceModel>
                {
                    new ApiResourceModel
                    {
                        Name = "api1",
                        DisplayName = "API 1",
                        Description = "Description for API 1",
                        Scopes = new List<string> { "api1" },
                        UserClaims = new List<string> { "scope1", "scope2" },
                    }
                }
            };

            _seeder = new Seeder(_dbContext, _identityServerModel);
        }

        [TestMethod]
        public async Task Seed_ShouldPopulateDatabase_WhenDatabaseIsEmpty()
        {
            // Act
            await _seeder.SeedAsync();

            // Assert
            Assert.IsTrue(_dbContext.ApiScopes.Any(), "ApiScopes should not be empty.");
            Assert.IsTrue(_dbContext.Clients.Any(), "Clients should not be empty.");
        }

        [TestMethod]
        public async Task Seed_ShouldNotDuplicateEntries_WhenCalledMultipleTimes()
        {
            // Act
            await _seeder.SeedAsync();
            await _seeder.SeedAsync();

            // Assert
            Assert.AreEqual(1, _dbContext.ApiScopes.Count());
            Assert.AreEqual(1, _dbContext.Clients.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext?.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}
