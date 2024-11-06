using IdServer = Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace TMS_API.DbContext
{
    public class PersistedGrantDbContext : IdServer.PersistedGrantDbContext
    {
        public PersistedGrantDbContext(DbContextOptions<IdServer.PersistedGrantDbContext> options) : base(options)
        {

        }
    }
}
