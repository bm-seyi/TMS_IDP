using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TMS_IDP.DbContext
{
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

    public class ApplicationUser : IdentityUser<Guid> { } 
    public class ApplicationRole : IdentityRole<Guid> { }
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    { 
        public new Guid Id { get; set; }
    }
    public class ApplicationUserClaim : IdentityUserClaim<Guid>
    {
        public new Guid Id { get; set; }
    }
    public class ApplicationUserToken : IdentityUserToken<Guid> { }
    public class ApplicationUserRole : IdentityUserRole<Guid> { }
    public class ApplicationUserLogin : IdentityUserLogin<Guid> { }

}