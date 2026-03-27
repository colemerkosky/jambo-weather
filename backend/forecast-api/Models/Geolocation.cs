namespace forecast_api.Models
{
    public record Geolocation
    {
        public required string CityName { get; init; }
        public required Coordinates Coordinates { get; init; }
        public required string CountryCode { get; set;}
        public required string Timezone {get;set;}
    }

    public record Coordinates
    {
        public required double Latitude { get;init ;}
        public required double Longitude {get; init;}
    }
}