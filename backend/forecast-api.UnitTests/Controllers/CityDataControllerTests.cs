using forecast_api.Controllers;
using forecast_api.Models;
using forecast_api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace forecast_api.UnitTests.Controllers;

public class CityDataControllerTests
{
    private readonly Mock<ICityDataService> _cityDataServiceMock;
    private readonly Mock<ICityService> _cityServiceMock;
    private readonly Mock<IIPGeolocationService> _geolocationServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly CityDataController _controller;

    public CityDataControllerTests()
    {
        _cityDataServiceMock = new Mock<ICityDataService>();
        _cityServiceMock = new Mock<ICityService>();
        _geolocationServiceMock = new Mock<IIPGeolocationService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(x => x["FallbackIPAddress"]).Returns("1.1.1.1");

        _controller = new CityDataController(
            _cityDataServiceMock.Object,
            _cityServiceMock.Object,
            _geolocationServiceMock.Object,
            _configurationMock.Object
        );
    }

    [Fact]
    public void GetCityList_ReturnsOkWithCityList()
    {
        // Arrange
        var cityList = new Dictionary<string, string>
        {
            { "edmonton", "Edmonton" },
            { "calgary", "Calgary" }
        };
        _cityServiceMock.Setup(x => x.GetCityList()).Returns(cityList);

        // Act
        var result = _controller.GetCityList();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var returnedList = okResult?.Value as Dictionary<string, string>;
        Assert.NotNull(returnedList);
        Assert.Equal(2, returnedList.Count);
    }

    [Fact]
    public async Task GetCityDataByCityId_WithValidCityId_ReturnsOkWithCityData()
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
        var cityData = new CityData
        {
            Blurb = "Edmonton is the capital of Alberta.",
            Geolocation = geolocation,
            Population = 1010000,
            LearnMoreUrl = "https://en.wikipedia.org/wiki/Edmonton"
        };

        _cityServiceMock.Setup(x => x.GetGeolocationByCityId(cityId)).Returns(geolocation);
        _cityDataServiceMock.Setup(x => x.GetCityDataAsync(geolocation)).ReturnsAsync(cityData);

        // Act
        var result = await _controller.GetCityDataByCityId(cityId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var returnedCityData = okResult?.Value as CityData;
        Assert.NotNull(returnedCityData);
        Assert.Equal("Edmonton is the capital of Alberta.", returnedCityData.Blurb);
    }

    [Fact]
    public async Task GetCityDataByCityId_WithInvalidCityId_ReturnsNotFound()
    {
        // Arrange
        var cityId = "nonexistent";
        _cityServiceMock.Setup(x => x.GetGeolocationByCityId(cityId)).Returns((Geolocation?)null);

        // Act
        var result = await _controller.GetCityDataByCityId(cityId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetCityDataByCityId_WhenServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var cityId = "calgary";
        var geolocation = new Geolocation
        {
            CityName = "Calgary",
            CountryCode = "CA",
            Timezone = "America/Edmonton",
            Coordinates = new Coordinates { Latitude = 51.0447, Longitude = -114.0719 }
        };

        _cityServiceMock.Setup(x => x.GetGeolocationByCityId(cityId)).Returns(geolocation);
        _cityDataServiceMock.Setup(x => x.GetCityDataAsync(geolocation)).ReturnsAsync((CityData?)null);

        // Act
        var result = await _controller.GetCityDataByCityId(cityId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetCityDataByIP_WithValidIp_ReturnsOkWithCityData()
    {
        // Arrange
        var geolocation = new Geolocation
        {
            CityName = "Toronto",
            CountryCode = "CA",
            Timezone = "America/Toronto",
            Coordinates = new Coordinates { Latitude = 43.6532, Longitude = -79.3832 }
        };
        var cityData = new CityData
        {
            Blurb = "Toronto is the capital of Ontario.",
            Geolocation = geolocation,
            Population = 2930000,
            LearnMoreUrl = "https://en.wikipedia.org/wiki/Toronto"
        };

        // Mock IP address context
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        _geolocationServiceMock.Setup(x => x.GetGeolocationOfIpAddressAsync("1.1.1.1"))
            .ReturnsAsync(geolocation);
        _cityDataServiceMock.Setup(x => x.GetCityDataAsync(geolocation)).ReturnsAsync(cityData);

        // Act
        var result = await _controller.GetCityDataByIP();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
