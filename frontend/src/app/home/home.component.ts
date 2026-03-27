import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { CityDataService } from '../city-data/city-data.service';
import { CityDataComponent } from '../city-data/city-data.component';
import { CityData } from '../city-data/city-data.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, CityDataComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private cityDataService = inject(CityDataService);

  currentUser$ = this.authService.currentUser$;
  cityData = signal<CityData | null>(null);
  loading = signal(true);
  error = signal(true);

  ngOnInit(): void {
    this.loadCityData();
  }

  loadCityData(): void {
    console.log("Loading!")
    this.loading.set(true);
    this.error.set(false);

    this.cityDataService.getCurrentCityData().subscribe({
      next: (data) => {
        this.cityData.set(data);
        this.loading.set(false);
        this.error.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(true);
        this.cityData.set(null);
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }
}

