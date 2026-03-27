using System.Text.Json;
using forecast_api.Models;
using forecast_api.Models.IPAPI;

namespace forecast_api.Services
{
    /// <summary>
    /// An interface for fetching the Geolocation data for a given IP address
    /// </summary>
    public interface IIPGeolocationService
    {
        /// <summary>
        /// Fetches the Geolocation data for a given IP address
        /// </summary>
        /// <param name="ipAddress">The IP address to query for Geolocation data</param>
        /// <returns>The Geolocation data for the given IP address</returns>
        public Task<Geolocation?> GetGeolocationOfIpAddressAsync(String ipAddress);
    }

    /// <summary>
    /// An IPGeolocationService backed by the IPAPI service.
    /// See ipapi.co for more info
    /// </summary>
    public class IPAPI_IPGeolocationService() : IIPGeolocationService
    {
        private readonly HttpClient client = new();

        public async Task<Geolocation?> GetGeolocationOfIpAddressAsync(String ipAddress)
        {
            var url = $"https://ipapi.co/{ipAddress}/json";
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Test Forecast App");
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var ipapiGeolocationData = JsonSerializer.Deserialize<IPAPIGeolocationData>(responseBody);
            return ipapiGeolocationData?.ToGeolocation(); // Map to our BLL class
        }
    }
}