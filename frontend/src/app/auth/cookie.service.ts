import { Injectable } from '@angular/core';

export interface CookieOptions {
  expires?: Date | number; // Date or days from now
  path?: string;
  domain?: string;
  secure?: boolean;
  sameSite?: 'Strict' | 'Lax' | 'None';
}

@Injectable({
  providedIn: 'root'
})
export class CookieService {
  /**
   * Get a cookie value by name
   */
  get(name: string): string | null {
    const nameEQ = name + '=';
    const cookies = document.cookie.split(';');
    
    for (let cookie of cookies) {
      cookie = cookie.trim();
      if (cookie.indexOf(nameEQ) === 0) {
        return decodeURIComponent(cookie.substring(nameEQ.length));
      }
    }
    return null;
  }

  /**
   * Set a cookie
   */
  set(name: string, value: string, options?: CookieOptions): void {
    let cookieString = `${name}=${encodeURIComponent(value)}`;

    if (options?.expires) {
      let expiresDate: Date;
      if (options.expires instanceof Date) {
        expiresDate = options.expires;
      } else {
        expiresDate = new Date();
        expiresDate.setDate(expiresDate.getDate() + options.expires);
      }
      cookieString += `; expires=${expiresDate.toUTCString()}`;
    }

    if (options?.path) {
      cookieString += `; path=${options.path}`;
    } else {
      cookieString += '; path=/';
    }

    if (options?.domain) {
      cookieString += `; domain=${options.domain}`;
    }

    if (options?.secure) {
      cookieString += '; secure';
    }

    if (options?.sameSite) {
      cookieString += `; samesite=${options.sameSite}`;
    }

    document.cookie = cookieString;
  }

  /**
   * Delete a cookie
   */
  delete(name: string): void {
    this.set(name, '', { expires: new Date(0) });
  }

  /**
   * Check if a cookie exists
   */
  exists(name: string): boolean {
    return this.get(name) !== null;
  }
}
