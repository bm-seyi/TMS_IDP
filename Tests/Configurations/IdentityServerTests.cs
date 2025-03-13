using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Duende.IdentityServer.EntityFramework.Options;
using Duende.IdentityServer.EntityFramework.DbContexts;
using TMS_IDP.Configuration;

namespace TMS_IDP.Tests.Configurations
{
    [TestClass]
    public class IdentityServerSeederTests
    {
        private IServiceProvider _serviceProvider = null!;
        private ConfigurationDbContext _dbContext = null!;

        [TestInitialize]
        public void Setup()
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
        }

        [TestMethod]
        public async Task Seed_ShouldPopulateDatabase_WhenDatabaseIsEmpty()
        {
            await IdentityServerSeeder.Seed(_serviceProvider);

            Assert.IsTrue(_dbContext.ApiScopes.Any(), "ApiScopes should not be empty.");
            Assert.IsTrue(_dbContext.ApiResources.Any(), "ApiResources should not be empty.");
            Assert.IsTrue(_dbContext.Clients.Any(), "Clients should not be empty.");
            Assert.IsTrue(_dbContext.IdentityResources.Any(), "IdentityResources should not be empty.");
        }

        [TestMethod]
        public async Task Seed_ShouldNotDuplicateEntries_WhenCalledMultipleTimes()
        {
            await IdentityServerSeeder.Seed(_serviceProvider);
            await IdentityServerSeeder.Seed(_serviceProvider);

            Assert.AreEqual(1, _dbContext.ApiScopes.Count());
            Assert.AreEqual(1, _dbContext.ApiResources.Count());
            Assert.AreEqual(1, _dbContext.Clients.Count());
            Assert.AreEqual(3, _dbContext.IdentityResources.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
        }
    }
}
