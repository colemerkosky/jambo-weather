using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using forecast_api.Models;
using forecast_api.Services;
using Moq;
using Moq.Protected;

namespace forecast_api.UnitTests.Services;

public class OpenMeteoForecastServiceTests
{
    [Fact]
    public async Task GetForecastDataAsync_WithMockedHttpClient_ReturnsForecastData()
    {
        // Arrange
        var responseJson = JsonSerializer.Serialize(new
        {
            daily = new
            {
                time = new[] { "2025-01-01", "2025-01-02" },
                temperature_2m_max = new[] { 10.0, 12.1 },
                temperature_2m_min = new[] { 2.5, 3.2 }
            }
        });

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.open-meteo.com/")
        };

        var service = new OpenMeteoForecastService(httpClient);
        var geolocation = new Geolocation
        {
            CityName = "Test",
            CountryCode = "TT",
            Coordinates = new Coordinates { Latitude = 37.5, Longitude = -122.0 },
            Timezone = "UTC"
        };

        // Act
        var result = await service.GetForecastDataAsync(geolocation, DateOnly.Parse("2025-01-01"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Forecasts.Count);
        Assert.Equal(new DateOnly(2025, 1, 1), result.Forecasts[0].Date);
        Assert.Equal(10.0, result.Forecasts[0].HighTemp);
        Assert.Equal(2.5, result.Forecasts[0].LowTemp);

        handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
}
