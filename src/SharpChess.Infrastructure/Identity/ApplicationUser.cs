using Microsoft.AspNetCore.Identity;
using SharpChess.Application.Auth.Models;

namespace SharpChess.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<RefreshTokenRecord> RefreshTokens { get; private set; } = [];
}
