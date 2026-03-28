using forecast_api.Controllers;
using forecast_api.Models;
using forecast_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;

namespace forecast_api.UnitTests.Controllers;

public class ForecastControllerTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IForecastService> _forecastServiceMock;
    private readonly Mock<IIPGeolocationService> _geolocationServiceMock;
    private readonly Mock<ICityService> _cityServiceMock;
    private readonly ForecastController _controller;

    public ForecastControllerTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _forecastServiceMock = new Mock<IForecastService>();
        _geolocationServiceMock = new Mock<IIPGeolocationService>();
        _cityServiceMock = new Mock<ICityService>();

        _configurationMock.Setup(x => x["FallbackIPAddress"]).Returns("1.1.1.1");

        _controller = new ForecastController(
            _configurationMock.Object,
            _forecastServiceMock.Object,
            _geolocationServiceMock.Object,
            _cityServiceMock.Object
        );
    }

    [Fact]
    public async Task GetForecastByCityId_WithValidCityId_ReturnsOkWithForecastData()
    {
        // Arrange
        var cityId = "edmonton";
        var geolocation = new Geolocation
        {
            CityName = "Edmonton",
            CountryCode = "CA",
            Timezone = "America/Edmonton",
            Coordinates = new Coordinates { Latitude = 53.5462, Longitude = -113.4937 }
        };
        var forecast = new ForecastData
        {
            Forecasts = new List<Forecast>
            {
                new Forecast { Date = DateOnly.FromDateTime(DateTime.Now), HighTemp = 20.0, LowTemp = 10.0 }
            }
        };

        _cityServiceMock.Setup(x => x.GetGeolocationByCityId(cityId)).Returns(geolocation);
        _forecastServiceMock.Setup(x => x.GetForecastDataAsync(geolocation, It.IsAny<DateOnly>())).ReturnsAsync(forecast);

        // Act
        var result = await _controller.GetForecastByCityId(cityId, null);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var returnedForecast = okResult?.Value as ForecastData;
        Assert.NotNull(returnedForecast);
        Assert.Single(returnedForecast.Forecasts);
    }

    [Fact]
    public async Task GetForecastByCityId_WithInvalidCityId_ReturnsNotFound()
    {
        // Arrange
        var cityId = "nonexistent";
        _cityServiceMock.Setup(x => x.GetGeolocationByCityId(cityId)).Returns((Geolocation?)null);

        // Act
        var result = await _controller.GetForecastByCityId(cityId, null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetForecastByCityId_WithSpecificDate_PassesDateToService()
    {
        // Arrange
        var cityId = "calgary";
        var forecastDate = new DateOnly(2024, 12, 25);
        var geolocation = new Geolocation
        {
            CityName = "Calgary",
            CountryCode = "CA",
            Timezone = "America/Edmonton",
            Coordinates = new Coordinates { Latitude = 51.0447, Longitude = -114.0719 }
        };
        var forecast = new ForecastData { Forecasts = new List<Forecast>() };

        _cityServiceMock.Setup(x => x.GetGeolocationByCityId(cityId)).Returns(geolocation);
        _forecastServiceMock.Setup(x => x.GetForecastDataAsync(geolocation, forecastDate)).ReturnsAsync(forecast);

        // Act
        var result = await _controller.GetForecastByCityId(cityId, forecastDate);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _forecastServiceMock.Verify(x => x.GetForecastDataAsync(geolocation, forecastDate), Times.Once);
    }

    [Fact]
    public async Task GetForecastByCityId_WhenServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var cityId = "edmonton";
        var geolocation = new Geolocation
        {
            CityName = "Edmonton",
            CountryCode = "CA",
            Timezone = "America/Edmonton",
            Coordinates = new Coordinates { Latitude = 53.5462, Longitude = -113.4937 }
        };

        _cityServiceMock.Setup(x => x.GetGeolocationByCityId(cityId)).Returns(geolocation);
        _forecastServiceMock.Setup(x => x.GetForecastDataAsync(geolocation, It.IsAny<DateOnly>())).ReturnsAsync((ForecastData?)null);

        // Act
        var result = await _controller.GetForecastByCityId(cityId, null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetForecastByIP_WithValidGeolocation_ReturnsOkWithForecastData()
    {
        // Arrange
        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var geolocation = new Geolocation
        {
            CityName = "Edmonton",
            CountryCode = "CA",
            Timezone = "America/Edmonton",
            Coordinates = new Coordinates { Latitude = 53.5462, Longitude = -113.4937 }
        };

        var forecast = new ForecastData
        {
            Forecasts = new List<Forecast> { new Forecast { Date = DateOnly.FromDateTime(DateTime.UtcNow), HighTemp = 15, LowTemp = 5 } }
        };

        _geolocationServiceMock.Setup(x => x.GetGeolocationOfIpAddressAsync("1.1.1.1")).ReturnsAsync(geolocation);
        _forecastServiceMock.Setup(x => x.GetForecastDataAsync(geolocation, It.IsAny<DateOnly>())).ReturnsAsync(forecast);

        // Act
        var result = await _controller.GetForecastByIP(null);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var returnedForecast = okResult?.Value as ForecastData;
        Assert.NotNull(returnedForecast);
        Assert.Single(returnedForecast!.Forecasts);
    }

    [Fact]
    public async Task GetForecastByIP_WhenGeolocationServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _geolocationServiceMock.Setup(x => x.GetGeolocationOfIpAddressAsync("1.1.1.1")).ReturnsAsync((Geolocation?)null);

        // Act
        var result = await _controller.GetForecastByIP(null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
