using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pedidos.Application;
using Pedidos.Domain;

namespace Pedidos.Infrastructure;

public sealed class JwtOptions
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "Pedidos.Api";
    public string Audience { get; set; } = "Pedidos.Web";
    public int ExpiresIn { get; set; } = 3600;
}

public sealed class AuthService(PedidosDbContext database, IPasswordHasher<Usuario> hasher, IOptions<JwtOptions> options) : IAuthService
{
    private static readonly Usuario DummyUser = new();
    private static readonly string DummyHash = new PasswordHasher<Usuario>().HashPassword(DummyUser, Guid.NewGuid().ToString());

    public async Task<LoginResult?> LoginAsync(LoginInput input, CancellationToken cancellationToken)
    {
        var email = input.Email.Trim().ToLowerInvariant();
        var user = await database.Usuarios.SingleOrDefaultAsync(entity => entity.Email == email, cancellationToken);
        var result = hasher.VerifyHashedPassword(user ?? DummyUser, user?.PasswordHash ?? DummyHash, input.Password);
        if (user is null || result == PasswordVerificationResult.Failed) return null;
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, input.Password);
            await database.SaveChangesAsync(cancellationToken);
        }
        var settings = options.Value;
        Claim[] claims = [new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Rol), new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())];
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            expires: DateTime.UtcNow.AddSeconds(settings.ExpiresIn),
            signingCredentials: new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), settings.ExpiresIn, user.Email, user.Rol);
    }
}