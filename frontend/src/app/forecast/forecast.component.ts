import { Component, computed, inject, input, signal, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Forecast, ForecastData } from './forecast.model';
import { ForecastService } from './forecast.service';

@Component({
  selector: 'app-forecast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './forecast.component.html',
  styleUrls: ['./forecast.component.css']
})
export class ForecastComponent {
  private forecastService = inject(ForecastService);

  forecasts = signal<Forecast[]>([]);
  selectedCityId = input.required<string>();

  loading = signal(true);
  error = signal(false);

  ngOnInit(): void {
    this.loadForecasts('current');
  }

  ngOnChanges({ selectedCityId }: SimpleChanges): void {
    if(selectedCityId.currentValue != selectedCityId.previousValue && !selectedCityId.isFirstChange()){
      this.loadForecasts(selectedCityId.currentValue)
    }
  }

  loadForecasts(cityId: string): void {
    this.startLoading();

    this.forecastService.getForecasts(cityId).subscribe({
      next: (data) => this.handleForecastsLoaded(data),
      error: () => this.onErrorLoading()
    });
  }

  startLoading(): void {
    this.loading.set(true);
    this.error.set(false);
  }

  onErrorLoading(): void {
    this.loading.set(false);
    this.error.set(true);
    this.forecasts.set([]);
  }

  handleForecastsLoaded(data: ForecastData): void {
    this.forecasts.set(data.forecasts);
    console.log(this.forecasts())
    this.loading.set(false);
    this.error.set(false);
  }

  formatDate(rawDate: string): string {
    const date = new Date(rawDate);
    if (Number.isNaN(date.getTime())) {
      return rawDate;
    }

    return date.toLocaleDateString(undefined, {
      weekday: 'long',
      month: 'long',
      day: 'numeric'
    });
  }
}
