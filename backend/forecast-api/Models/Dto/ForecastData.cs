using System.Collections.Immutable;

namespace forecast_api.Models
{
    public record ForecastData
    {
        public required List<Forecast> Forecasts {get; init;}
    }

    public record Forecast
    {
        public required DateOnly Date {get;init;}
        public required double HighTemp {get;init;}
        public required double LowTemp {get;init;}
    }
}