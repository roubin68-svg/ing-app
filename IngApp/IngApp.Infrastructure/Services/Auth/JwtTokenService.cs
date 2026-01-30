using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IngApp.Application.Common.Interfaces.Authentication;
using IngApp.Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IngApp.Infrastructure.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime Expiration) GenerateToken(User user, IEnumerable<string> permissions)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // ===========================
        // Base Claims (Minimum needed)
        // ===========================
        var claims = new List<Claim>
        {
            new Claim("uid", user.Id.ToString()),
            new Claim("phone", user.PhoneNumber),
            new Claim("type", user.UserType?.Code ?? string.Empty)
        };

        // ==================================
        // Add User Roles into JWT
        // (Admin → Role: Admin)
        // ==================================
        if (user.UserRoles != null && user.UserRoles.Any())
        {
            foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        // ==================================
        // Add Permission Claims
        // Used for menu / authorize attribute
        // ==================================
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        // ===========================
        // Standard Token Expiration
        // ===========================
        var expiration = DateTime.Now.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expiration);
    }
}
