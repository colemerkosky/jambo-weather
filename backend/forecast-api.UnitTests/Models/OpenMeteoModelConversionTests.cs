using forecast_api.Models;
using forecast_api.Models.OpenMeteo;

namespace forecast_api.UnitTests.Models;

public class OpenMeteoModelConversionTests
{
    [Fact]
    public void OpenMeteoForecastData_toForecastData_ConvertsSuccessfully()
    {
        // Arrange
        var openMeteoData = new OpenMeteoForecastData
        {
            Forecasts = new OpenMeteoDaily
            {
                Time = new List<DateOnly>
                {
                    new DateOnly(2024, 1, 1),
                    new DateOnly(2024, 1, 2),
                    new DateOnly(2024, 1, 3)
                },
                Max = new List<double> { 15.2, 14.5, 16.0 },
                Min = new List<double> { 5.1, 4.8, 6.2 }
            }
        };

        // Act
        var result = openMeteoData.toForecastData();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Forecasts.Count);
        
        Assert.Equal(new DateOnly(2024, 1, 1), result.Forecasts[0].Date);
        Assert.Equal(15.2, result.Forecasts[0].HighTemp);
        Assert.Equal(5.1, result.Forecasts[0].LowTemp);
        
        Assert.Equal(new DateOnly(2024, 1, 2), result.Forecasts[1].Date);
        Assert.Equal(14.5, result.Forecasts[1].HighTemp);
        Assert.Equal(4.8, result.Forecasts[1].LowTemp);
        
        Assert.Equal(new DateOnly(2024, 1, 3), result.Forecasts[2].Date);
        Assert.Equal(16.0, result.Forecasts[2].HighTemp);
        Assert.Equal(6.2, result.Forecasts[2].LowTemp);
    }

    [Fact]
    public void OpenMeteoForecastData_toForecastData_WithEmptyData_ReturnsEmptyList()
    {
        // Arrange
        var openMeteoData = new OpenMeteoForecastData
        {
            Forecasts = new OpenMeteoDaily
            {
                Time = new List<DateOnly>(),
                Max = new List<double>(),
                Min = new List<double>()
            }
        };

        // Act
        var result = openMeteoData.toForecastData();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Forecasts);
    }

    [Fact]
    public void OpenMeteoForecastData_toForecastData_PreservesDataAccuracy()
    {
        // Arrange
        var openMeteoData = new OpenMeteoForecastData
        {
            Forecasts = new OpenMeteoDaily
            {
                Time = new List<DateOnly> { new DateOnly(2024, 12, 25) },
                Max = new List<double> { 25.123456789 },
                Min = new List<double> { -5.987654321 }
            }
        };

        // Act
        var result = openMeteoData.toForecastData();

        // Assert
        Assert.Equal(25.123456789, result.Forecasts[0].HighTemp);
        Assert.Equal(-5.987654321, result.Forecasts[0].LowTemp);
    }
}
