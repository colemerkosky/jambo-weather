using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace forecast_api.Models.OpenMeteo
{
    public record OpenMeteoForecastData
    {
        [JsonPropertyName("daily")]
        public required OpenMeteoDaily Forecasts {get;init;}

        public ForecastData toForecastData()
        {
            var forecasts = new List<Forecast>();

            for(var i = 0; i < Forecasts.Time.Count; i++)
            {
                forecasts.Add(new Forecast
                {
                    Date = Forecasts.Time[i],
                    HighTemp = Forecasts.Max[i],
                    LowTemp = Forecasts.Min[i]
                });
            }

            return new ForecastData {
                Forecasts = forecasts
            };
        }
    }

    public record OpenMeteoDaily
    {
        [JsonPropertyName("time")]
        public required List<DateOnly> Time {get;init;}
        [JsonPropertyName("temperature_2m_max")]
        public required List<Double> Max {get;init;}
        [JsonPropertyName("temperature_2m_min")]
        public required List<Double> Min {get;init;}
    }

}