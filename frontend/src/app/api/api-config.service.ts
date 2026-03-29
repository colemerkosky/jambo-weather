import { computed, Injectable, signal } from '@angular/core';

/**
 * Configuration service for authentication settings
 * Use this to configure the login URL and other auth settings
 */
@Injectable({
  providedIn: 'root'
})
export class ApiConfigService {
  /**
   * The base server URL for API data calls
   * Default: http://localhost:3000
   */
  baseUrl = signal('https://localhost:7241/api');

  /**
   * The URL for the login endpoint
   * Default: http://localhost:3000/auth/login
   */
  loginUrl = computed(() => this.baseUrl() + "/auth/login");

  /**
   * The URL for the login endpoint
   * Default: http://localhost:3000/auth/login
   */
  refreshUrl = computed(() => this.baseUrl() + "/auth/refresh");

  /**
   * Whether to use secure cookies (should be true in production)
   * Default: true
   */
  useSecureCookies = signal(true);

  /**
   * Set the base URL
   */
  setBaseUrl(url: string): void {
    this.baseUrl.set(url);
  }

  /**
   * Set whether to use secure cookies
   */
  setUseSecureCookies(value: boolean): void {
    this.useSecureCookies.set(value);
  }
}
