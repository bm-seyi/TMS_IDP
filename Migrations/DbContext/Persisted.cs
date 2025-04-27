using IdServer = Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace TMS_MIGRATE.DbContext
{
    [ExcludeFromCodeCoverage]
    public class PersistedGrantDbContext : IdServer.PersistedGrantDbContext
    {
        public PersistedGrantDbContext(DbContextOptions<IdServer.PersistedGrantDbContext> options) : base(options)
        {

        }
    }
}
