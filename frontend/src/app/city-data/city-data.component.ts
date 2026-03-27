import { Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CityData } from './city-data.model';

@Component({
  selector: 'app-city-data',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './city-data.component.html',
  styleUrls: ['./city-data.component.css']
})
export class CityDataComponent {
    cityData = input.required<CityData>();

    countryEmoji = computed(() => {
        return this.getCountryEmojiFromCountryCode(this.cityData()?.geolocation.countryCode)
    })

    googleMapsUrl = computed(() => {
        return this.getGoogleMapsUrl(this.cityData().geolocation.coordinates)
    })

    getCountryEmojiFromCountryCode(countryCode: string) {
        const flagOffset = 0x1F1E6 // "REGIONAL INDICATOR SYMBOL LETTER A" in Unicode
        const asciiOffset = 0x41 // "A" in ASCII
        const flagToAsciiOffset = flagOffset - asciiOffset

        let codePoints = countryCode.toUpperCase().split('').map(char => flagToAsciiOffset + char.charCodeAt(0))
        let emoji = String.fromCodePoint(...codePoints)
        console.log(emoji)
        return emoji
    }

    getGoogleMapsUrl(coordinates: {latitude: number, longitude: number}) {
        return `https://www.google.com/maps/search/?api=1&query=${coordinates.latitude},${coordinates.longitude}`
    }
}
