using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Logistics.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;

namespace Logistics.Api.Security;

public sealed class JwtTokenBuilder
{
    public string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles, JwtOptions options)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
