using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharpChess.Application.Auth.Models;
using SharpChess.Infrastructure.Identity;

namespace SharpChess.Infrastructure.Persistence;

public class SharpChessDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public SharpChessDbContext(DbContextOptions<SharpChessDbContext> options) : base(options) { }

    public DbSet<RefreshTokenRecord> RefreshTokens => Set<RefreshTokenRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshTokenRecord>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);

            entity.Property(token => token.UserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(token => token.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(token => token.TokenHash)
                .IsUnique();

            entity.HasIndex(token => new { token.UserId, token.ExpiresAtUtc });

            entity.HasOne<ApplicationUser>()
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
