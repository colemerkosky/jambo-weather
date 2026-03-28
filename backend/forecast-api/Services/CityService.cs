using System.Collections.Immutable;
using forecast_api.Models;

namespace forecast_api.Services
{
    /// <summary>
    /// Provides data for a saved set of cities. This data could be saved in various places - in memory, in a database, etc
    /// </summary>
    public interface ICityService
    {
        /// <summary>
        /// Gets a mapping of city IDs to names (for displaying in a select list)
        /// </summary>
        /// <returns>A mapping of city IDs to names</returns>
        public Dictionary<string, string> GetCityList();

        /// <summary>
        /// Gets the Geolocation data of the City with the given ID
        /// </summary>
        /// <param name="cityId">The ID of the city for which to get Geolocation data</param>
        /// <returns>The saved Geolocation data of the city, or null if it doesn't exist</returns>
        public Geolocation? GetGeolocationByCityId(string cityId);
    }

    /// <summary>
    /// An in-memory implementation of the city service, which simply returns a hardcoded list of cities.
    /// </summary>
    public class InMemoryCityService() : ICityService
    {
        public Dictionary<string, string> GetCityList()
        {
            return standardCities.ToDictionary(city => city.Id, city => city.Geolocation.CityName);
        }

        public Geolocation? GetGeolocationByCityId(string cityId)
        {
            return standardCities.FirstOrDefault(it => it.Id == cityId)?.Geolocation;
        }

        // The "saved" list of cities
        private readonly List<City> standardCities = [
            new City {
                Id = "edmonton",
                Geolocation = new Geolocation {
                    CityName = "Edmonton",
                    CountryCode = "CA",
                    Timezone = "America/Edmonton",
                    Coordinates = new Coordinates {
                        Latitude = 53.5462,
                        Longitude = -113.4937,
                    },
                }
            },
            new City {
                Id = "calgary",
                Geolocation = new Geolocation {
                    CityName = "Calgary",
                    CountryCode = "CA",
                    Timezone = "America/Edmonton",
                    Coordinates = new Coordinates {
                        Latitude = 51.0447,
                        Longitude = -114.0719,
                    },
                },
            },
            new City {
                Id = "london",
                Geolocation = new Geolocation {
                    CityName = "London",
                    CountryCode = "GB",
                    Timezone = "Europe/London",
                    Coordinates = new Coordinates {
                        Latitude = 51.5072,
                        Longitude = -0.1276,
                    },
                },
            },
            new City {
                Id = "beijing",
                Geolocation = new Geolocation {
                    CityName = "Beijing",
                    CountryCode = "CN",
                    Timezone = "Asia/Shanghai",
                    Coordinates = new Coordinates {
                        Latitude = 39.9042,
                        Longitude = 116.4074,
                    },
                }
            },
            new City {
                Id = "rio_de_janeiro",
                Geolocation = new Geolocation {
                    CityName = "Rio de Janeiro",
                    CountryCode = "BR",
                    Timezone = "America/Sao_Paulo",
                    Coordinates = new Coordinates { 
                        Latitude = -22.9068,
                        Longitude = -43.1729,
                    },
                },
            },
            new City {
                Id = "sydney",
                Geolocation = new Geolocation {
                    CityName = "Sydney",
                    CountryCode = "AU",
                    Timezone = "Australia/Sydney",
                    Coordinates = new Coordinates { 
                        Latitude = -33.8698,
                        Longitude = 151.2083,
                    },
                },
            }
        ];
    }
}