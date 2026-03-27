import { Component, computed, input, model, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CityOption } from '../city-data/city.model';

@Component({
  selector: 'app-city-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './city-dropdown.component.html',
  styleUrls: ['./city-dropdown.component.css']
})
export class CityDropdownComponent {
  cities = input<CityOption[]>([]);
  geolocationCityName = input<string>('')
  selectedCityId = model<string>('current')
  citySelected = output<string>();

  geolocationCityOptionText = computed(() => {
    const formattedCurrentCity = this.geolocationCityName() ? ` (${this.geolocationCityName()})` : ""
    return `Current Location${formattedCurrentCity}`
  })

  isOpen = false;

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  selectCity(cityId: string): void {
    this.selectedCityId.set(cityId);
    this.citySelected.emit(cityId);
    this.isOpen = false;
  }

  getSelectedCityName(): string {
    if (this.selectedCityId() === 'current') {
      return this.geolocationCityOptionText();
    }
    const selectedCity = this.cities().find(city => city.id === this.selectedCityId());
    return selectedCity ? selectedCity.displayName : 'Select City';
  }
}