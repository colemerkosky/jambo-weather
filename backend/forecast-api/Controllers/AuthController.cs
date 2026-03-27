using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using forecast_api.Models.Dto;
using forecast_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace forecast_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IConfiguration configuration, IAuthenticationService authenticationService) : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Token([FromBody] AuthRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Unauthorized();
            }

            if (!authenticationService.IsAuthenticated(request))
            {
                return Unauthorized();
            }

            var expiresAt = DateTime.UtcNow.AddHours(1);
            var token = GenerateJwtToken(expiresAt, request.Username);

            return Ok(new AuthResponse
            {
                Token = token,
                ExpiresAtUtc = expiresAt
            });
        }

        private string GenerateJwtToken(DateTime expiresAtUtc, string username)
        {
            var jwtKey = configuration["Jwt:Key"] ?? "my_secret_key";
            var jwtIssuer = configuration["Jwt:Issuer"] ?? "forecast-api";
            var jwtAudience = configuration["Jwt:Audience"] ?? "forecast-api";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAtUtc,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
