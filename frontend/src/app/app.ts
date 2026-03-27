import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './auth/auth.service';
import { ApiConfigService } from './api/api-config.service';
import { environment } from '../environments/environment';

/**
 * Root application component
 * The AuthService is injected here to ensure it initializes on app startup.
 * This enables automatic token refresh scheduling if a valid token exists.
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = 'forecast-app';
  private authService = inject(AuthService);
  private apiConfigService = inject(ApiConfigService)

  constructor() {
    this.apiConfigService.setBaseUrl(environment.baseUrl)
  }
}

