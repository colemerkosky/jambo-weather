using System.Collections.ObjectModel;

namespace forecast_api.Models
{
    public record City {
        public required string Id {get; init;}
        public required Geolocation Geolocation {get; init;}
    };
}