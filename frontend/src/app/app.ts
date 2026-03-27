import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './auth/auth.service';

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

  constructor() {
    // CONFIGURE YOUR AUTHENTICATION ENDPOINTS HERE
    // Uncomment and modify these URLs to point to your backend

    // Example for production:
    this.authService.setLoginUrl('https://localhost:7241/api/auth/login');
    // this.authService.setRefreshUrl('https://api.yourdomain.com/auth/refresh');
    
    // For local development, the defaults are:
    // Login URL: http://localhost:3000/auth/login
    // Refresh URL: http://localhost:3000/auth/login/refresh
  }
}

