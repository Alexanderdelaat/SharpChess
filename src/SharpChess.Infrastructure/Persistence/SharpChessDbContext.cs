using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharpChess.Domain.Entities;

namespace SharpChess.Infrastructure.Persistence;

public class SharpChessDbContext : IdentityDbContext<IdentityUser>
{
    public SharpChessDbContext(DbContextOptions<SharpChessDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}