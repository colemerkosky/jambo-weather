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
  selectedDate = signal<string>(this.getTodayIso());

  loading = signal(true);
  error = signal(false);

  ngOnInit(): void {
    this.resetDate();
    this.loadForecasts('current');
  }

  ngOnChanges({ selectedCityId }: SimpleChanges): void {
    const newCityId = selectedCityId.currentValue;
    const oldCityId = selectedCityId.previousValue;

    if (newCityId !== oldCityId) {
      this.resetDate();
      this.loadForecasts(newCityId);
    }
  }

  getTodayIso(): string {
    const now = new Date();
    return now.toISOString().split('T')[0];
  }

  getDatePlusDays(days: number): string {
    const dt = new Date();
    dt.setDate(dt.getDate() + days);
    return dt.toISOString().split('T')[0];
  }

  get minDate(): string {
    return this.getTodayIso();
  }

  get maxDate(): string {
    return this.getDatePlusDays(5);
  }

  resetDate(): void {
    this.selectedDate.set(this.getTodayIso());
  }

  onDateChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const dateValue = input.value;

    if (dateValue) {
      this.selectedDate.set(dateValue);
      this.loadForecasts(this.selectedCityId());
    }
  }

  loadForecasts(cityId: string): void {
    this.startLoading();

    this.forecastService
      .getForecasts(cityId, this.selectedDate())
      .subscribe({
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
