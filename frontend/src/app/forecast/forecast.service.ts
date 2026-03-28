import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ForecastData } from './forecast.model';
import { ApiConfigService } from '../api/api-config.service';

@Injectable({ providedIn: 'root' })
export class ForecastService {
  private http = inject(HttpClient);
  private apiConfig = inject(ApiConfigService);

  getForecasts(cityId: string, forecastDate?: string): Observable<ForecastData> {
    const baseUrl = this.apiConfig.baseUrl();
    let params = new HttpParams();

    if (forecastDate) {
      params = params.set('forecastDate', forecastDate);
    }

    return this.http.get<ForecastData>(`${baseUrl}/forecast/${cityId}`, { params });
  }
}
