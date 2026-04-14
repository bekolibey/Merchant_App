using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MerchantApp.WebAPI.Contracts;
using MerchantApp.WebAPI.Services;
using merchantapp.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MerchantApp.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ApplicationDbContext context, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        string password = request.Password.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest(new { message = "E-posta ve sifre zorunludur." });

        string hash = PasswordHasher.ComputeSha256(password);

        var user = await context.PortalUsers
            .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null || user.PasswordHash != hash)
            return Unauthorized(new { message = "Kullanici adi veya sifre gecersiz." });

        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenMinutes);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, email),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        user.AccessToken = accessToken;
        user.AccessTokenExpiresAtUtc = expiresAtUtc;
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new LoginResponse
        {
            Email = email,
            Message = "Giris basarili.",
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAtUtc
        });
    }
}
