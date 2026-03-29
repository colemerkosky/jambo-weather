using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using forecast_api.Models;
using forecast_api.Models.OpenMeteo;

namespace forecast_api.Services
{
    /// <summary>
    /// An interface that can provide Forecast data for a given date
    /// </summary>
    public interface IForecastService
    {
        /// <summary>
        /// Fetches the forecast data from the data provider, for a given Geolocation, on a given date
        /// </summary>
        /// <param name="geolocation"><The Geolocation to fetch the forecast for/param>
        /// <param name="date">The date to fetch the forecast for</param>
        /// <returns></returns>
        public Task<ForecastData?> GetForecastDataAsync(Geolocation geolocation, DateOnly date);
    }

    /// <summary>
    /// A Forecast service backed by the OpenMeteo API
    /// See open-meteo.com for more info.
    /// </summary>
    public class OpenMeteoForecastService(HttpClient client) : IForecastService
    {
        public async Task<ForecastData?> GetForecastDataAsync(Geolocation geolocation, DateOnly date)
        {
            var coords = geolocation.Coordinates;

            client.Timeout = TimeSpan.FromSeconds(20);

            var url = new StringBuilder("https://api.open-meteo.com/v1/forecast")
                .Append($"?latitude={coords.Latitude}&longitude={coords.Longitude}")
                .Append($"&daily=temperature_2m_max,temperature_2m_min") // Get the daily high and low temperatures
                .Append($"&start_date={date.ToString("yyyy-MM-dd")}&end_date={date.AddDays(7).ToString("yyyy-MM-dd")}") // Get a week of forecast data
                .Append($"&timezone={geolocation.Timezone}") // Get in the specific timezone for this geolocation data
                .ToString();
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var openMeteoForecast = JsonSerializer.Deserialize<OpenMeteoForecastData>(responseString);

            return openMeteoForecast?.toForecastData();
        }
    }
}