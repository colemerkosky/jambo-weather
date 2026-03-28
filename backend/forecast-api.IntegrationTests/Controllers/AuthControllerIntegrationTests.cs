using forecast_api;
using forecast_api.Models;
using forecast_api.Models.Dto;
using forecast_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace forecast_api.IntegrationTests.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        // Arrange
        var loginRequest = new { username = "testuser", password = "Sup3rSe_cr3tP@ssw0rd" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(content);
        Assert.NotNull(content.Token);
        Assert.NotNull(content.RefreshToken);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new { username = "testuser", password = "wrongpassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingUsername_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new { username = "", password = "Sup3rSe_cr3tP@ssw0rd" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewAccessToken()
    {
        // Arrange - First get an access token
        var loginRequest = new { username = "testuser", password = "Sup3rSe_cr3tP@ssw0rd" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var refreshToken = loginContent!.RefreshToken;

        // Act - Refresh the token
        var refreshRequest = new { RefreshToken = refreshToken };
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(content);
        Assert.NotNull(content.Token);
        Assert.NotNull(content.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new { RefreshToken = "invalid_token_12345" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
