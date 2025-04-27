using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TMS_MIGRATE.DbContext
{
    [ExcludeFromCodeCoverage]
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Id)
                .HasDefaultValueSql("NEWID()")
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Entity<ApplicationRole>()
                .Property(r => r.Id)
                .HasDefaultValueSql("NEWID()")
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Entity<ApplicationUserToken>()
                .Property(t => t.UserId)
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            // Configure IdentityRoleClaim to use Guid as Id
            builder.Entity<ApplicationRoleClaim>()
                .Property(t => t.Id)
                .HasColumnType("uniqueidentifier")
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            // Configure IdentityUserClaim to use Guid as Id
            builder.Entity<ApplicationUserClaim>()
                .Property(t => t.Id)
                .HasColumnType("uniqueidentifier")
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();
        }

    }

    [ExcludeFromCodeCoverage]
    public class ApplicationUser : IdentityUser<Guid> { } 

    [ExcludeFromCodeCoverage]
    public class ApplicationRole : IdentityRole<Guid> { }

    [ExcludeFromCodeCoverage]
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    { 
        public new Guid Id { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ApplicationUserClaim : IdentityUserClaim<Guid>
    {
        public new Guid Id { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ApplicationUserToken : IdentityUserToken<Guid> { }

    [ExcludeFromCodeCoverage]
    public class ApplicationUserRole : IdentityUserRole<Guid> { }

    [ExcludeFromCodeCoverage]
    public class ApplicationUserLogin : IdentityUserLogin<Guid> { }

}