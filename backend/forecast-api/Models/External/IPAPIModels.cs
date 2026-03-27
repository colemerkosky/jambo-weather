using System.Text.Json.Serialization;

namespace forecast_api.Models.IPAPI
{
    public record IPAPIGeolocationData
    {
        [JsonPropertyName("city")]
        public required string City { get; init; }

        [JsonPropertyName("timezone")]
        public required string Timezone { get; init; }

        [JsonPropertyName("country_code")]
        public required string CountryCode { get; init; }

        [JsonPropertyName("latitude")]
        public required double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public required double Longitude {get; init;}

        public Geolocation ToGeolocation()
        {
            return new Geolocation {
                CityName = City,
                CountryCode = CountryCode,
                Timezone = Timezone,
                Coordinates = new Coordinates
                {
                    Latitude = Latitude,
                    Longitude = Longitude
                }
            };
        }
    }
}