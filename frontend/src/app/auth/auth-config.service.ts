import { Injectable, signal } from '@angular/core';

/**
 * Configuration service for authentication settings
 * Use this to configure the login URL and other auth settings
 */
@Injectable({
  providedIn: 'root'
})
export class AuthConfigService {
  /**
   * The URL for the login endpoint
   * Default: http://localhost:3000/auth/login
   */
  loginUrl = signal('http://localhost:3000/auth/login');

  /**
   * The URL for the token refresh endpoint
   * Default: http://localhost:3000/auth/login/refresh
   */
  refreshUrl = signal('http://localhost:3000/auth/login/refresh');

  /**
   * Whether to use secure cookies (should be true in production)
   * Default: true
   */
  useSecureCookies = signal(true);

  /**
   * Set the login URL
   */
  setLoginUrl(url: string): void {
    this.loginUrl.set(url);
  }

  /**
   * Set the refresh URL
   */
  setRefreshUrl(url: string): void {
    this.refreshUrl.set(url);
  }

  /**
   * Set whether to use secure cookies
   */
  setUseSecureCookies(value: boolean): void {
    this.useSecureCookies.set(value);
  }
}
