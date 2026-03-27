import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { CityDataService } from '../city-data/city-data.service';
import { CityDataComponent } from '../city-data/city-data.component';
import { CityDropdownComponent } from '../city-dropdown/city-dropdown.component';
import { ForecastComponent } from '../forecast/forecast.component';
import { CityData } from '../city-data/city-data.model';
import { CityOption } from '../city-data/city.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, CityDataComponent, CityDropdownComponent, ForecastComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private cityDataService = inject(CityDataService);

  currentUser$ = this.authService.currentUser$;
  cities = signal<CityOption[]>([]);
  selectedCityId = signal<string>('current');
  geolocationCityName = signal<string>('');

  ngOnInit(): void {
    this.loadCityList()
  }

  loadCityList(): void {
    this.cityDataService.getCityList().subscribe({
      next: (cityList) => {
        // Convert the object to array of CityOption
        const cityOptions: CityOption[] = Object.entries(cityList).map(([id, displayName]) => ({
          id,
          displayName
        }));
        this.cities.set(cityOptions);
      },
      error: (err) => {
        console.error('Failed to load city list:', err);
        // City list is optional, so we don't set error state
      }
    });
  }

  setGeolocationCityName(cityName: string){
    this.geolocationCityName.set(cityName);
  }

  onCitySelected(cityId: string): void {
    this.selectedCityId.set(cityId);
  }

  logout(): void {
    this.authService.logout();
  }
}

