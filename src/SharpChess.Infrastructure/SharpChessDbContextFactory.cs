using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SharpChess.Infrastructure.Persistence;

public class SharpChessDbContextFactory : IDesignTimeDbContextFactory<SharpChessDbContext>
{
    public SharpChessDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<SharpChessDbContext> optionsBuilder = 
            new DbContextOptionsBuilder<SharpChessDbContext>();
        
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=sharpchess;Username=postgres;Password=devpassword");
        return new SharpChessDbContext(optionsBuilder.Options);
    }
}