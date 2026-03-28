using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using forecast_api.Models;
using forecast_api.Services;
using Moq;
using Moq.Protected;

namespace forecast_api.UnitTests.Services;

public class IPGeolocationServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public IPGeolocationServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
    }

    [Fact]
    public async Task GetGeolocationOfIpAddressAsync_WithValidIp_ReturnsGeolocation()
    {
        // Arrange
        var ipAddress = "1.1.1.1";
        var jsonResponse = JsonSerializer.Serialize(new
        {
            ip = ipAddress,
            city = "Test City",
            region = "Test Region",
            country_code = "TT",
            latitude = 37.7749,
            longitude = -122.4194,
            timezone = "America/Los_Angeles"
        });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains($"/1.1.1.1/json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            })
            .Verifiable();

        var service = new IPAPI_IPGeolocationService(_httpClient);

        // Act
        var geolocation = await service.GetGeolocationOfIpAddressAsync(ipAddress);

        // Assert
        Assert.NotNull(geolocation);
        Assert.Equal("Test City", geolocation!.CityName);
        Assert.Equal("TT", geolocation.CountryCode);
        Assert.Equal(37.7749, geolocation.Coordinates.Latitude);
        Assert.Equal(-122.4194, geolocation.Coordinates.Longitude);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/1.1.1.1/json")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetGeolocationOfIpAddressAsync_CachesResults_AfterFirstCallSecondCallSkipsHttp()
    {
        // Arrange
        var ipAddress = "1.1.1.1";
        var jsonResponse = JsonSerializer.Serialize(new
        {
            ip = ipAddress,
            city = "Test City",
            region = "Test Region",
            country_code = "TT",
            latitude = 37.7749,
            longitude = -122.4194,
            timezone = "America/Los_Angeles"
        });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            })
            .Verifiable();

        var service = new IPAPI_IPGeolocationService(_httpClient);

        // Act
        var first = await service.GetGeolocationOfIpAddressAsync(ipAddress);
        var second = await service.GetGeolocationOfIpAddressAsync(ipAddress);

        // Assert
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Same(first, second);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }
}
