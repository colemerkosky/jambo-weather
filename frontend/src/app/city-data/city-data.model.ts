export interface CityData {
  blurb: string;
  population: number;
  learnMoreUrl: string;
  geolocation: {
    cityName: string;
    countryCode: string;
    timezone: string;
    coordinates: {
      latitude: number;
      longitude: number;
    };
  };
}
