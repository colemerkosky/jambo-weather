import { Injectable, signal, effect, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError, timer } from 'rxjs';
import { tap, catchError, switchMap } from 'rxjs/operators';
import { CookieService } from './cookie.service';
import { ApiConfigService } from '../api/api-config.service';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAtUtc: number; // in seconds
  refreshTokenExpiresAtUtc: number; // in seconds
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private refreshRefreshInterval: any;

  private http = inject(HttpClient);
  private cookieService = inject(CookieService);
  private router = inject(Router);
  private apiConfig = inject(ApiConfigService)
  // private authConfig = inject(AuthConfigService);
  
  isAuthenticated$ = new BehaviorSubject<boolean>(this.hasValidToken());
  currentUser$ = new BehaviorSubject<string | null>(this.getUserFromToken());

  /**
   * Login with username and password
   */
  login(username: string, password: string): Observable<AuthResponse> {
    const request: LoginRequest = {
      username, password
    }

    return this.http.post<AuthResponse>(this.apiConfig.loginUrl(), <LoginRequest>{
      username,
      password
    }).pipe(
      tap(response => {
        this.handleSuccessfulAuthRequest(response)
      }),
      catchError(error => {
        console.error('Login failed:', error);
        return throwError(() => error);
      })
    );
  }

  handleSuccessfulAuthRequest(response: AuthResponse): void {
    this.storeAuthToken(response.token, response.expiresAtUtc);
    this.storeRefreshToken(response.refreshToken, response.refreshTokenExpiresAtUtc);
    this.isAuthenticated$.next(true);
    this.currentUser$.next(this.getUserFromToken())
    this.scheduleTokenRefresh(response.expiresAtUtc);
  }

  /**
   * Logout and clear token
   */
  logout(): void {
    this.clearToken();
    this.isAuthenticated$.next(false);
    this.currentUser$.next(null);
    if (this.refreshRefreshInterval) {
      clearTimeout(this.refreshRefreshInterval);
    }
    this.router.navigate(['/login']);
  }

  /**
   * Get the stored JWT token
   */
  getToken(): string | null {
    return this.cookieService.get('auth_token');
  }

    /**
   * Get the stored JWT token
   */
  getRefreshToken(): string | null {
    return this.cookieService.get('refresh_token');
  }


  /**
   * Check if user has a valid token
   */
  hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) return false;
    return !this.isTokenExpired(token);
  }

  /**
   * Refresh the JWT token
   */
  refreshToken(): Observable<AuthResponse> {
    const token = this.getToken();
    if (!token) {
      return throwError(() => new Error('No token to refresh'));
    }

    const refresh_token = this.getRefreshToken();
    if (!token) {
      return throwError(() => new Error('No refresh token stored'));
    }

    return this.http.post<AuthResponse>(this.apiConfig.refreshUrl(), <RefreshRequest> {
      refreshToken: token
    }).pipe(
      tap(response => {
        this.handleSuccessfulAuthRequest(response)
      }),
      catchError(error => {
        console.error('Token refresh failed:', error);
        this.logout();
        return throwError(() => error);
      })
    );
  }


  private storeAuthToken(token: string, expiresIn?: number): void {
    this.storeExpiringCookie("auth_token", token, expiresIn)
  }

  private storeRefreshToken(refreshToken: string, expiresIn?: number): void {
    this.storeExpiringCookie("refresh_token", refreshToken, expiresIn)
  }

  /**
   * Store token in cookie with expiration
   */
  private storeExpiringCookie(cookie: string, value: string, expiresIn?: number): void {
    // Store in cookie (httpOnly would be ideal but requires backend support)
    // We're storing in a secure cookie with expiration
    const expirationDate = new Date();
    if (expiresIn) {
      expirationDate.setSeconds(expirationDate.getSeconds() + expiresIn);
    } else {
      // Default to 1 hour if not specified
      expirationDate.setHours(expirationDate.getHours() + 1);
    }
    
    this.cookieService.set(cookie, value, {
      expires: expirationDate,
      secure: this.apiConfig.useSecureCookies(),
      sameSite: 'Strict'
    });
  }

  /**
   * Clear stored token
   */
  private clearToken(): void {
    this.cookieService.delete('auth_token');
  }

  /**
   * Decode JWT and extract user info (basic decode, assumes 'sub' or 'username' claim)
   */
  private getUserFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = this.decodeToken(token);
      return payload.sub || payload.username || null;
    } catch {
      return null;
    }
  }

  /**
   * Decode JWT payload (basic implementation)
   */
  private decodeToken(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('Error decoding token:', error);
      return {};
    }
  }

  /**
   * Check if token is expired
   */
  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.decodeToken(token);
      if (!payload.exp) return false;

      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp < currentTime;
    } catch {
      return true;
    }
  }

  /**
   * Schedule token refresh before expiration
   */
  private scheduleTokenRefresh(expiresIn: number): void {
    // Clear any existing timeout
    if (this.refreshRefreshInterval) {
      clearTimeout(this.refreshRefreshInterval);
    }

    try {
      const currentTimeInSeconds = Math.floor(Date.now() / 1000);
      const timeUntilExpiry = (expiresIn - currentTimeInSeconds) * 1000; // convert to ms

      // Refresh at 80% of token lifetime or 1 minute before expiry, whichever is later
      const refreshBuffer = Math.max(60 * 1000, timeUntilExpiry * 0.2);
      const refreshTime = timeUntilExpiry - refreshBuffer;

      if (refreshTime > 0) {
        this.refreshRefreshInterval = setTimeout(() => {
          console.log('Auto-refreshing token...');
          this.refreshToken().subscribe({
            error: (err) => console.error('Failed to refresh token:', err)
          });
        }, refreshTime);
      }
    } catch (error) {
      console.error('Error scheduling token refresh:', error);
    }
  }
}
