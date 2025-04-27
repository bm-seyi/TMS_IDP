using IdServer = Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace TMS_MIGRATE.DbContext
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationDbContext : IdServer.ConfigurationDbContext
    {
        public ConfigurationDbContext(DbContextOptions<IdServer.ConfigurationDbContext> options) : base(options)
        {

        }
    }
}