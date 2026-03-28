using System.Text.Json;
using forecast_api.Models;
using forecast_api.Models.IPAPI;
using Microsoft.VisualBasic;

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
    public class IPAPI_IPGeolocationService([FromKeyedServices("SingletonClient")] HttpClient client) : IIPGeolocationService
    {
        public async Task<Geolocation?> GetGeolocationOfIpAddressAsync(String ipAddress)
        {
            var geolocation = GetCached(ipAddress);
            if(geolocation != null)
            {
                return geolocation;
            }

            var url = $"https://ipapi.co/{ipAddress}/json";
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Test Forecast App");
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var ipapiGeolocationData = JsonSerializer.Deserialize<IPAPIGeolocationData>(responseBody);
            geolocation = ipapiGeolocationData?.ToGeolocation(); // Map to our BLL class

            if(geolocation != null)
            {
                StoreInCache(ipAddress, geolocation);
            }

            return geolocation; 
        }

        private void StoreInCache(string key, Geolocation value)
        {
            var expiry = DateTimeOffset.UtcNow.AddHours(1);
            IPCache[key] = new IPCacheValue {Geolocation = value, Expiry = expiry};
        }

        private Geolocation? GetCached(string key)
        {
            if(IPCache.TryGetValue(key, out var ipCacheValue)){
                if(DateTimeOffset.UtcNow < ipCacheValue.Expiry)
                {
                    return ipCacheValue.Geolocation;
                }
            }
            return null;
        }

        private Dictionary<string, IPCacheValue> IPCache = [];

        record IPCacheValue {
            public required Geolocation Geolocation {get;init;}
            public required DateTimeOffset Expiry {get;init;}
        };
    }
}