using forecast_api.Models;
using forecast_api.Models.IPAPI;

namespace forecast_api.UnitTests.Models;

public class IPAPIModelConversionTests
{
    [Fact]
    public void IPAPIGeolocationData_ToGeolocation_ConvertsSuccessfully()
    {
        // Arrange
        var ipapiData = new IPAPIGeolocationData
        {
            City = "Toronto",
            CountryCode = "CA",
            Timezone = "America/Toronto",
            Latitude = 43.6532,
            Longitude = -79.3832
        };

        // Act
        var result = ipapiData.ToGeolocation();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Toronto", result.CityName);
        Assert.Equal("CA", result.CountryCode);
        Assert.Equal("America/Toronto", result.Timezone);
        Assert.Equal(43.6532, result.Coordinates.Latitude);
        Assert.Equal(-79.3832, result.Coordinates.Longitude);
    }

    [Fact]
    public void IPAPIGeolocationData_ToGeolocation_WithDifferentCity_ConvertsCorrectly()
    {
        // Arrange
        var ipapiData = new IPAPIGeolocationData
        {
            City = "Paris",
            CountryCode = "FR",
            Timezone = "Europe/Paris",
            Latitude = 48.8566,
            Longitude = 2.3522
        };

        // Act
        var result = ipapiData.ToGeolocation();

        // Assert
        Assert.Equal("Paris", result.CityName);
        Assert.Equal("FR", result.CountryCode);
        Assert.Equal("Europe/Paris", result.Timezone);
        Assert.Equal(48.8566, result.Coordinates.Latitude);
        Assert.Equal(2.3522, result.Coordinates.Longitude);
    }

    [Fact]
    public void IPAPIGeolocationData_ToGeolocation_PreservesCoordinatePrecision()
    {
        // Arrange
        var ipapiData = new IPAPIGeolocationData
        {
            City = "Tokyo",
            CountryCode = "JP",
            Timezone = "Asia/Tokyo",
            Latitude = 35.6762123456789,
            Longitude = 139.6503987654321
        };

        // Act
        var result = ipapiData.ToGeolocation();

        // Assert
        Assert.Equal(35.6762123456789, result.Coordinates.Latitude);
        Assert.Equal(139.6503987654321, result.Coordinates.Longitude);
    }

    [Fact]
    public void IPAPIGeolocationData_ToGeolocation_WithNegativeCoordinates_ConvertsCorrectly()
    {
        // Arrange
        var ipapiData = new IPAPIGeolocationData
        {
            City = "Sydney",
            CountryCode = "AU",
            Timezone = "Australia/Sydney",
            Latitude = -33.8698,
            Longitude = 151.2083
        };

        // Act
        var result = ipapiData.ToGeolocation();

        // Assert
        Assert.Equal(-33.8698, result.Coordinates.Latitude);
        Assert.Equal(151.2083, result.Coordinates.Longitude);
    }
}
