import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Forecast, ForecastData } from './forecast.model';
import { ApiConfigService } from '../api/api-config.service';

@Injectable({ providedIn: 'root' })
export class ForecastService {
  private http = inject(HttpClient);
  private apiConfig = inject(ApiConfigService);

  getForecasts(cityId: string): Observable<ForecastData> {
    const baseUrl = this.apiConfig.baseUrl();
    return this.http.get<ForecastData>(`${baseUrl}/forecast/${cityId}`);
  }
}
