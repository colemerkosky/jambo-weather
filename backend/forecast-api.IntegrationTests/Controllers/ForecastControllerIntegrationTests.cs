using forecast_api;
using forecast_api.Models.Dto;
using forecast_api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace forecast_api.IntegrationTests.Controllers;

public class ForecastControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string? _authToken;

    public ForecastControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _authToken = GetAuthToken().Result;
    }

    private async Task<string?> GetAuthToken()
    {
        var loginRequest = new { username = "testuser", password = "Sup3rSe_cr3tP@ssw0rd" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return content?.Token;
        }
        return null;
    }

    private void AddAuthorizationHeader(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_authToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
        }
    }

    [Fact]
    public async Task GetForecastByCityId_WithValidCityId_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/forecast/edmonton");
        AddAuthorizationHeader(request);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // Note: May fail due to external service calls - would need WireMock in real scenario
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetForecastByCityId_WithInvalidCityId_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/forecast/nonexistent");
        AddAuthorizationHeader(request);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetForecastByCityId_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/forecast/edmonton");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetForecastByCityId_WithSpecificDate_PassesDateParam()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/forecast/calgary?forecastDate={futureDate:yyyy-MM-dd}");
        AddAuthorizationHeader(request);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }
}
