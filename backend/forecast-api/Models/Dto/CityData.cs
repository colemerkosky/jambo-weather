namespace forecast_api.Models
{
    public record CityData
    {
        public required string Blurb {get;set;}
        public long? Population {get;set;}
        public required Geolocation Geolocation {get; set;}
        public required string LearnMoreUrl {get;set;}
    }
}