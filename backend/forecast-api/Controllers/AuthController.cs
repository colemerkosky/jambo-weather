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
        // In-memory store for refresh tokens: maps refresh token to (username, expiry)
        private static readonly Dictionary<string, (string Username, DateTime ExpiresAtUtc)> RefreshTokenStore = new();

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

            var accessTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            var accessToken = GenerateJwtToken(accessTokenExpiresAt, request.Username);
            var refreshToken = GenerateRefreshToken();

            // Store the refresh token
            RefreshTokenStore[refreshToken] = (request.Username, refreshTokenExpiresAt);

            return Ok(new AuthResponse
            {
                Token = accessToken,
                ExpiresAtUtc = accessTokenExpiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAtUtc = refreshTokenExpiresAt
            });
        }

        [HttpPost("refresh")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required");
            }

            // Check if refresh token exists and is valid
            if (!RefreshTokenStore.TryGetValue(request.RefreshToken, out var tokenData))
            {
                return Unauthorized("Invalid refresh token");
            }

            var (username, expiresAtUtc) = tokenData;

            // Check if refresh token has expired
            if (expiresAtUtc < DateTime.UtcNow)
            {
                RefreshTokenStore.Remove(request.RefreshToken);
                return Unauthorized("Refresh token has expired");
            }

            // Generate new access token and refresh token
            var newAccessTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            var newAccessToken = GenerateJwtToken(newAccessTokenExpiresAt, username);
            var newRefreshToken = GenerateRefreshToken();

            // Remove old refresh token and store new one
            RefreshTokenStore.Remove(request.RefreshToken);
            RefreshTokenStore[newRefreshToken] = (username, newRefreshTokenExpiresAt);

            return Ok(new AuthResponse
            {
                Token = newAccessToken,
                ExpiresAtUtc = newAccessTokenExpiresAt,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAtUtc = newRefreshTokenExpiresAt
            });
        }

        private string GenerateJwtToken(DateTime expiresAtUtc, string username)
        {
            var jwtKey = configuration["JWT_KEY"] ?? "my_secret_key";
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

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
