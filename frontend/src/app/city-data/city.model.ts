export interface CityOption {
  id: string;
  displayName: string;
}

export interface CityList {
  [key: string]: string; // cityID -> displayName
}
