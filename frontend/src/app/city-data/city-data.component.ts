import { Component, computed, input, inject, signal, output, model, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CityData } from './city-data.model';
import { CityDataService } from './city-data.service';

@Component({
  selector: 'app-city-data',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './city-data.component.html',
  styleUrls: ['./city-data.component.css']
})
export class CityDataComponent {
    private cityDataService = inject(CityDataService);
    private emptyCityData = <CityData>{
      blurb: "",
      population: 0,
      learnMoreUrl: "",
      geolocation: {
        cityName: "",
        countryCode: "",
        timezone: "",
        coordinates: {
          latitude: 0,
          longitude: 0
        }
      }
    }

    cityData = signal<CityData>(this.emptyCityData)
    selectedCityId = input.required<string>();
    geolocationCityName = output<string>();

    loading = signal(true)
    error = signal(false);

    countryEmoji = computed(() => {
        return this.getCountryEmojiFromCountryCode(this.cityData().geolocation.countryCode)
    })

    googleMapsUrl = computed(() => {
        return this.getGoogleMapsUrl(this.cityData().geolocation.coordinates)
    })

    getCountryEmojiFromCountryCode(countryCode: string) {
        if(!countryCode) {
          return null
        }
        const flagOffset = 0x1F1E6 // "REGIONAL INDICATOR SYMBOL LETTER A" in Unicode
        const asciiOffset = 0x41 // "A" in ASCII
        const flagToAsciiOffset = flagOffset - asciiOffset

        let codePoints = countryCode.toUpperCase().split('').map(char => flagToAsciiOffset + char.charCodeAt(0))
        let emoji = String.fromCodePoint(...codePoints)

        return emoji
    }

    getGoogleMapsUrl(coordinates: {latitude: number, longitude: number}) {
        return `https://www.google.com/maps/search/?api=1&query=${coordinates.latitude},${coordinates.longitude}`
    }

    ngOnInit(): void {
      this.loadCityData('current')
    }

    ngOnChanges({selectedCityId}: SimpleChanges){
      if(selectedCityId.currentValue != selectedCityId.previousValue && !selectedCityId.isFirstChange()){
        this.loadCityData(selectedCityId.currentValue)
      }
    }

    loadCityData(cityId: string){
        this.startLoading();

        this.cityDataService.getCityData(cityId).subscribe({
        next: (data) => {
          this.handleCityDataLoaded(data)
          if(cityId === 'current'){
            this.geolocationCityName.emit(data.geolocation.cityName)
          }
        },
        error: () => {
          this.onErrorLoading()
        }
      })
    }

    startLoading() : void {
      this.loading.set(true);
      this.error.set(false);
    }

    onErrorLoading() : void {
      this.loading.set(false);
      this.error.set(true);
      this.cityData.set(this.emptyCityData);
    }

    handleCityDataLoaded(data: CityData) : void {
      this.cityData.set(data);
      this.loading.set(false);
      this.error.set(false);
    } 
}
