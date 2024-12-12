using IdServer =Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace TMS_IDP.DbContext
{
    public class ConfigurationDbContext : IdServer.ConfigurationDbContext
    {
        public ConfigurationDbContext(DbContextOptions<IdServer.ConfigurationDbContext> options) : base(options)
        {

        }
    }
}