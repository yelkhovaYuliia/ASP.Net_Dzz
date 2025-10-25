using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebATB.Data.Entities;
using WebATB.Data.Entities.Idenity;
using WebATB.Data.Entities.Identity;

namespace WebATB.Data;

public class AppATBDbContext : IdentityDbContext<UserEntity, RoleEntity, int, IdentityUserClaim<int>, UserRoleEntity, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public AppATBDbContext(DbContextOptions<AppATBDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // identity
        builder.Entity<UserRoleEntity>()
            .HasOne(u => u.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(u => u.UserId);

        builder.Entity<UserRoleEntity>()
            .HasOne(r => r.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(r => r.RoleId);

        builder.Entity<UserEntity>()
            .HasMany(u => u.Logins)
            .WithOne()
            .HasForeignKey(l => l.UserId)
            .IsRequired();
    }

    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ProductImageEntity> ProductImages { get; set; }
}