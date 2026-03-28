using forecast_api.Models.OpenMeteo;
using System.Text.Json;

namespace forecast_api.IntegrationTests.Services;

public class OpenMeteoForecastServiceIntegrationTests
{
    [Fact]
    public void OpenMeteoForecastData_JsonDeserializationWorks()
    {
        // Arrange
        var jsonResponse = @"{
            ""daily"": {
                ""time"": [""2024-01-01"", ""2024-01-02""],
                ""temperature_2m_max"": [15.2, 14.5],
                ""temperature_2m_min"": [5.1, 4.8]
            }
        }";

        // Act
        var openMeteoData = JsonSerializer.Deserialize<OpenMeteoForecastData>(jsonResponse);

        // Assert
        Assert.NotNull(openMeteoData);
        Assert.NotNull(openMeteoData.Forecasts);
        Assert.Equal(2, openMeteoData.Forecasts.Time.Count);
        Assert.Equal(new DateOnly(2024, 1, 1), openMeteoData.Forecasts.Time[0]);
        Assert.Equal(new DateOnly(2024, 1, 2), openMeteoData.Forecasts.Time[1]);
        Assert.Equal(15.2, openMeteoData.Forecasts.Max[0]);
        Assert.Equal(14.5, openMeteoData.Forecasts.Max[1]);
        Assert.Equal(5.1, openMeteoData.Forecasts.Min[0]);
        Assert.Equal(4.8, openMeteoData.Forecasts.Min[1]);
    }

    [Fact]
    public void OpenMeteoForecastData_ConversionPreservesData()
    {
        // Arrange
        var jsonResponse = @"{
            ""daily"": {
                ""time"": [""2024-03-15"", ""2024-03-16"", ""2024-03-17""],
                ""temperature_2m_max"": [20.5, 22.3, 19.8],
                ""temperature_2m_min"": [8.2, 9.5, 7.1]
            }
        }";

        // Act
        var openMeteoData = JsonSerializer.Deserialize<OpenMeteoForecastData>(jsonResponse)!;
        var forecastData = openMeteoData.toForecastData();

        // Assert
        Assert.NotNull(forecastData);
        Assert.Equal(3, forecastData.Forecasts.Count);
        
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(openMeteoData.Forecasts.Time[i], forecastData.Forecasts[i].Date);
            Assert.Equal(openMeteoData.Forecasts.Max[i], forecastData.Forecasts[i].HighTemp);
            Assert.Equal(openMeteoData.Forecasts.Min[i], forecastData.Forecasts[i].LowTemp);
        }
    }

    [Fact]
    public void OpenMeteoForecastData_WithExtendedForecasts_HandlesMultipleDays()
    {
        // Arrange
        var jsonResponse = @"{
            ""daily"": {
                ""time"": [""2024-01-01"", ""2024-01-02"", ""2024-01-03"", ""2024-01-04"", ""2024-01-05"", ""2024-01-06"", ""2024-01-07""],
                ""temperature_2m_max"": [15.2, 14.5, 16.0, 13.8, 17.2, 12.5, 14.1],
                ""temperature_2m_min"": [5.1, 4.8, 6.2, 3.5, 7.0, 2.1, 4.3]
            }
        }";

        // Act
        var openMeteoData = JsonSerializer.Deserialize<OpenMeteoForecastData>(jsonResponse)!;
        var forecastData = openMeteoData.toForecastData();

        // Assert
        Assert.Equal(7, forecastData.Forecasts.Count);
        Assert.Equal(new DateOnly(2024, 1, 7), forecastData.Forecasts[6].Date);
        Assert.Equal(14.1, forecastData.Forecasts[6].HighTemp);
        Assert.Equal(4.3, forecastData.Forecasts[6].LowTemp);
    }
}
