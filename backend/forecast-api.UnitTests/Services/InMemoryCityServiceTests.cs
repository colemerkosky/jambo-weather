using forecast_api.Models;
using forecast_api.Services;

namespace forecast_api.UnitTests.Services;

public class InMemoryCityServiceTests
{
    private readonly InMemoryCityService _service;

    public InMemoryCityServiceTests()
    {
        _service = new InMemoryCityService();
    }

    [Fact]
    public void GetCityList_ReturnsAllCities()
    {
        // Act
        var cities = _service.GetCityList();

        // Assert
        Assert.NotEmpty(cities);
        Assert.Contains("edmonton", cities.Keys);
        Assert.Contains("calgary", cities.Keys);
        Assert.Contains("london", cities.Keys);
        Assert.Contains("beijing", cities.Keys);
        Assert.Contains("rio_de_janeiro", cities.Keys);
        Assert.Contains("sydney", cities.Keys);
    }

    [Fact]
    public void GetCityList_ReturnsCityNames()
    {
        // Act
        var cities = _service.GetCityList();

        // Assert
        Assert.Equal("Edmonton", cities["edmonton"]);
        Assert.Equal("Calgary", cities["calgary"]);
        Assert.Equal("London", cities["london"]);
        Assert.Equal("Beijing", cities["beijing"]);
        Assert.Equal("Rio de Janeiro", cities["rio_de_janeiro"]);
        Assert.Equal("Sydney", cities["sydney"]);
    }

    [Fact]
    public void GetGeolocationByCityId_WithValidCityId_ReturnsGeolocation()
    {
        // Act
        var geolocation = _service.GetGeolocationByCityId("edmonton");

        // Assert
        Assert.NotNull(geolocation);
        Assert.Equal("Edmonton", geolocation.CityName);
        Assert.Equal("CA", geolocation.CountryCode);
        Assert.Equal("America/Edmonton", geolocation.Timezone);
        Assert.Equal(53.5462, geolocation.Coordinates.Latitude);
        Assert.Equal(-113.4937, geolocation.Coordinates.Longitude);
    }

    [Fact]
    public void GetGeolocationByCityId_WithInvalidCityId_ReturnsNull()
    {
        // Act
        var geolocation = _service.GetGeolocationByCityId("nonexistent");

        // Assert
        Assert.Null(geolocation);
    }

    [Fact]
    public void GetGeolocationByCityId_WithCalgary_ReturnsCorrectData()
    {
        // Act
        var geolocation = _service.GetGeolocationByCityId("calgary");

        // Assert
        Assert.NotNull(geolocation);
        Assert.Equal("Calgary", geolocation.CityName);
        Assert.Equal("CA", geolocation.CountryCode);
        Assert.Equal("America/Edmonton", geolocation.Timezone);
        Assert.Equal(51.0447, geolocation.Coordinates.Latitude);
        Assert.Equal(-114.0719, geolocation.Coordinates.Longitude);
    }

    [Fact]
    public void GetGeolocationByCityId_WithSydney_ReturnsCorrectData()
    {
        // Act
        var geolocation = _service.GetGeolocationByCityId("sydney");

        // Assert
        Assert.NotNull(geolocation);
        Assert.Equal("Sydney", geolocation.CityName);
        Assert.Equal("AU", geolocation.CountryCode);
        Assert.Equal("Australia/Sydney", geolocation.Timezone);
        Assert.Equal(-33.8698, geolocation.Coordinates.Latitude);
        Assert.Equal(151.2083, geolocation.Coordinates.Longitude);
    }
}
