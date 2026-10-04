using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StyleCart.UserService.Models;

namespace StyleCart.UserService.Data;

public sealed class UserDbContext : IdentityDbContext<AppUser>
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .Property(user => user.FirstName)
            .HasMaxLength(100);

        builder.Entity<AppUser>()
            .Property(user => user.LastName)
            .HasMaxLength(100);
    }
}
