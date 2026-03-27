import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { lastValueFrom, Observable } from 'rxjs';
import { CityData } from './city-data.model';
import { CityList } from './city.model';
import { ApiConfigService } from '../api/api-config.service';

@Injectable({ providedIn: 'root' })
export class CityDataService {
  private http = inject(HttpClient)
  private apiConfig = inject(ApiConfigService)

  getCityData(cityId: string): Observable<CityData> {
    const baseUrl = this.apiConfig.baseUrl();
    return this.http.get<CityData>(`${baseUrl}/city/data/${cityId}`);
  }
  
  getCityList(): Observable<CityList> {
    const baseUrl = this.apiConfig.baseUrl();
    return this.http.get<CityList>(`${baseUrl}/city`);
  }
}
