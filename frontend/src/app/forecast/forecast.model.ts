export interface Forecast {
  date: string; // ISO date string expected
  highTemp: number;
  lowTemp: number;
}

export interface ForecastData {
  forecasts: Forecast[];
}
