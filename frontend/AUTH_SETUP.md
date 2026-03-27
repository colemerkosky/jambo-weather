# Authentication System Documentation

## Overview

This authentication system provides complete JWT-based authentication with the following features:

- **Login Screen**: Username and password login form
- **JWT Storage**: Secure cookie-based storage
- **Automatic Token Refresh**: Automatically refreshes tokens before expiration
- **Route Guards**: Protects routes that require authentication
- **HTTP Interceptor**: Automatically adds JWT to all HTTP requests
- **Configurable Endpoints**: Easy to configure login and refresh URLs

## Features

### 1. Login Component
Located at: `src/app/auth/login.component.ts`

The login component provides:
- Username and password input fields with validation
- Form validation (minimum 3 chars for username, 6 chars for password)
- Error message display
- Loading state during login
- Automatic redirect to protected routes after successful login

### 2. Route Protection
Using the `authGuard` to protect routes:

```typescript
// In your routes
{
  path: 'protected-route',
  component: YourComponent,
  canActivate: [authGuard]
}
```

The guard will:
- Check if a valid JWT token exists
- Redirect to login if not authenticated
- Save the return URL for post-login redirect

### 3. JWT Token Management
The `AuthService` handles:
- Storing JWT in a secure cookie
- Token expiration checking
- Automatic token refresh before expiration
- User information extraction from token
- Logout and token cleanup

### 4. Automatic Token Refresh
The system automatically refreshes the token:
- Checks token expiration on service initialization
- Schedules refresh at 80% of token lifetime or 1 minute before expiry (whichever is later)
- Handles refresh failures gracefully by logging out the user
- Can handle concurrent requests during token refresh

### 5. HTTP Interceptor
The `AuthInterceptor` automatically:
- Adds the JWT token to all HTTP requests via Authorization header
- Handles 401 responses by attempting token refresh
- Prevents duplicate refresh requests during concurrent API calls

## Configuration

### Setting Login and Refresh URLs

You can configure the authentication endpoints in your app initialization:

```typescript
import { Component, inject, effect } from '@angular/core';
import { AuthService } from './auth/auth.service';

export class App {
  constructor() {
    const authService = inject(AuthService);
    
    // Configure custom endpoints
    authService.setLoginUrl('https://api.yourdomain.com/auth/login');
    authService.setRefreshUrl('https://api.yourdomain.com/auth/refresh');
  }
}
```

Or use the `AuthConfigService`:

```typescript
import { AuthConfigService } from './auth/auth-config.service';

export class App {
  constructor(authConfig: AuthConfigService) {
    authConfig.setLoginUrl('https://api.yourdomain.com/auth/login');
    authConfig.setRefreshUrl('https://api.yourdomain.com/auth/refresh');
    authConfig.setUseSecureCookies(true); // for production
  }
}
```

**Default URLs:**
- Login: `http://localhost:3000/auth/login`
- Refresh: `http://localhost:3000/auth/login/refresh`

### Cookie Configuration

Cookies are configured with:
- `secure`: true (only sent over HTTPS in production)
- `sameSite`: 'Strict' (CSRF protection)
- `expires`: Based on token expiration time (default 1 hour if not specified)

## API Endpoints Expected

### Login Endpoint
**POST** `/auth/login`

Request body:
```json
{
  "username": "string",
  "password": "string"
}
```

Response:
```json
{
  "token": "JWT_TOKEN_HERE",
  "expiresIn": 3600  // seconds (optional, defaults to 1 hour)
}
```

### Token Refresh Endpoint
**POST** `/auth/login/refresh` (or your configured refresh URL)

Request body:
```json
{
  "token": "JWT_TOKEN_HERE"
}
```

Response:
```json
{
  "token": "NEW_JWT_TOKEN_HERE",
  "expiresIn": 3600  // seconds (optional)
}
```

## JWT Token Format

The system expects standard JWT tokens with the following claims:
- `exp`: Token expiration timestamp (Unix timestamp in seconds) - **REQUIRED**
- `sub` or `username`: User identifier (optional, used to display current user)

Example JWT payload:
```json
{
  "sub": "user123",
  "username": "john_doe",
  "email": "john@example.com",
  "iat": 1234567890,
  "exp": 1234571490
}
```

## Usage Examples

### In Components

#### Checking Authentication Status
```typescript
import { Component, inject } from '@angular/core';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-example',
  template: `
    <div *ngIf="(authService.isAuthenticated$ | async)">
      User is logged in
    </div>
  `
})
export class ExampleComponent {
  authService = inject(AuthService);
}
```

#### Getting Current User
```typescript
<span *ngIf="(authService.currentUser$ | async) as user">
  Welcome, {{ user }}!
</span>
```

#### Logging Out
```typescript
import { AuthService } from './auth/auth.service';

export class ExampleComponent {
  constructor(private authService: AuthService) {}

  logout() {
    this.authService.logout();
    // User will be redirected to /login
  }
}
```

#### Making Protected API Calls
```typescript
import { HttpClient } from '@angular/common/http';

export class DataService {
  constructor(private http: HttpClient) {}

  fetchData() {
    // The AuthInterceptor will automatically add Authorization header
    return this.http.get('/api/data');
  }
}
```

### In Routes

```typescript
import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { LoginComponent } from './auth/login.component';
import { HomeComponent } from './home/home.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { SettingsComponent } from './settings/settings.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '',
    component: HomeComponent,
    canActivate: [authGuard]
  },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'settings',
    component: SettingsComponent,
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
```

## File Structure

```
src/app/
├── auth/
│   ├── auth.service.ts          # Main authentication service
│   ├── auth-config.service.ts   # Configuration management
│   ├── auth.guard.ts            # Route protection guard
│   ├── auth.interceptor.ts       # HTTP interceptor for JWT
│   ├── cookie.service.ts        # Cookie management utility
│   ├── login.component.ts       # Login UI component
│   ├── login.component.html     # Login template
│   └── login.component.css      # Login styles
├── home/
│   ├── home.component.ts        # Protected home/dashboard
│   ├── home.component.html
│   └── home.component.css
├── app.config.ts                # App configuration with interceptor
├── app.routes.ts                # Route definitions with guards
└── app.ts                        # Root component
```

## Security Considerations

1. **HTTPS Only**: Always use HTTPS in production with `secure: true` for cookies
2. **SameSite Cookies**: Uses `SameSite: Strict` for CSRF protection
3. **Token Expiration**: Tokens are validated against expiration time
4. **Automatic Refresh**: Tokens are refreshed before expiration to prevent mid-request failures
5. **Secure Refresh Logic**: The interceptor handles concurrent requests properly during token refresh

## Troubleshooting

### Token Not Being Sent in Requests
- Check that the AuthInterceptor is properly registered in `app.config.ts`
- Verify the HTTP client is being used (not XMLHttpRequest directly)

### Token Refresh Not Working
- Verify the refresh endpoint URL is correct using `authService.getRefreshUrl()`
- Check the API response includes both `token` and `expiresIn` fields
- Ensure the backend is returning valid JWT tokens

### Infinite Redirect Loop
- Check that the login route is excluded from the auth guard
- Verify `hasValidToken()` correctly identifies valid tokens

### User Not Being Extracted from Token
- Ensure the JWT token includes `sub` or `username` claim
- Check token is properly formatted (3 parts separated by dots)

## Extending the System

### Adding Remember Me
```typescript
// In your login component
this.authService.login(username, password).subscribe({
  next: () => {
    // Save preference
    localStorage.setItem('rememberMe', JSON.stringify({
      username,
      timestamp: Date.now()
    }));
  }
});
```

### Adding Additional Claims
Modify the `getUserFromToken()` method in `auth.service.ts` to extract additional info:

```typescript
private getUserFromToken(): any {
  const token = this.getToken();
  if (!token) return null;
  
  try {
    const payload = this.decodeToken(token);
    return {
      username: payload.sub || payload.username,
      email: payload.email,
      roles: payload.roles
    };
  } catch {
    return null;
  }
}
```

### Handling Role-Based Access
```typescript
export const roleGuard = (requiredRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const token = authService.getToken();
    
    if (!token) return false;
    
    const payload = authService.decodeToken(token);
    const userRoles = payload.roles || [];
    
    return requiredRoles.some(role => userRoles.includes(role));
  };
};
```
