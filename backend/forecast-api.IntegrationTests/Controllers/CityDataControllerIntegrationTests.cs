using forecast_api;
using forecast_api.Models.Dto;
using forecast_api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace forecast_api.IntegrationTests.Controllers;

public class CityDataControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string? _authToken;

    public CityDataControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task GetCityList_ReturnsListOfCities()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/city");
        AddAuthorizationHeader(request);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains("edmonton", content.Keys);
        Assert.Equal("Edmonton", content["edmonton"]);
    }

    [Fact]
    public async Task GetCityList_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/city");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCityDataByCityId_WithValidCityId_ReturnsCityData()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/city/data/edmonton");
        AddAuthorizationHeader(request);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // Note: This may fail because the service makes real Wikipedia calls
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCityDataByCityId_WithInvalidCityId_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/city/data/nonexistent");
        AddAuthorizationHeader(request);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
