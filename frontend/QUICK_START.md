# Authentication System - Quick Start Guide

## 🚀 Getting Started

### 1. Configure Your Backend Endpoints

Edit `src/app/app.ts` and uncomment the configuration lines to point to your backend API:

```typescript
export class App {
  private authService = inject(AuthService);

  constructor() {
    this.authService.setLoginUrl('https://api.yourdomain.com/auth/login');
    this.authService.setRefreshUrl('https://api.yourdomain.com/auth/refresh');
  }
}
```

**Default endpoints** (if not configured):
- Login: `http://localhost:3000/auth/login`
- Refresh: `http://localhost:3000/auth/login/refresh`

### 2. Backend Requirements

Your backend should expose these endpoints:

#### POST `/auth/login`
```json
// Request
{
  "username": "user@example.com",
  "password": "password123"
}

// Response
{
  "token": "eyJhbGc...",
  "expiresIn": 3600
}
```

#### POST `/auth/refresh` 
```json
// Request
{
  "token": "eyJhbGc..."
}

// Response
{
  "token": "eyJhbGc...",
  "expiresIn": 3600
}
```

### 3. JWT Token Format

Your JWT should include:
- `exp`: Token expiration (Unix timestamp in seconds)
- `sub` or `username`: User identifier (for display)

Example payload:
```json
{
  "sub": "user123",
  "username": "john_doe",
  "exp": 1234571490
}
```

### 4. Run the Application

```bash
npm start
```

Visit `http://localhost:4200`
- You'll be redirected to `/login`
- Login form will appear with username and password fields
- After successful login, you'll be redirected to the home page

### 5. Protect Your Routes

To protect a route, add the `authGuard` to your route definition in `app.routes.ts`:

```typescript
import { authGuard } from './auth/auth.guard';

export const routes: Routes = [
  {
    path: 'my-protected-page',
    component: MyComponent,
    canActivate: [authGuard]
  }
];
```

### 6. Add Logout Button

In any component, inject the `AuthService` and call `logout()`:

```typescript
import { AuthService } from './auth/auth.service';

export class MyComponent {
  constructor(private authService: AuthService) {}
  
  logout() {
    this.authService.logout();
  }
}
```

The `HomeComponent` already includes a logout button as an example.

## 📋 What's Included

### Components
- **LoginComponent** - Login form with validation
- **HomeComponent** - Protected example component with logout button

### Services
- **AuthService** - Main authentication logic
- **CookieService** - Cookie management
- **AuthConfigService** - Configuration management
- **AuthInterceptor** - Automatic JWT injection into requests
- **authGuard** - Route protection

### Features
✅ Login form with username/password  
✅ Form validation  
✅ JWT stored in secure cookies  
✅ Automatic token refresh before expiration  
✅ Route guards for protected pages  
✅ HTTP interceptor for automatic JWT injection  
✅ User information extraction from JWT  
✅ Logout functionality  
✅ Error handling and user feedback  

## 🔧 Configuration Options

### Setting Custom URLs

```typescript
const authService = inject(AuthService);

// Set login URL
authService.setLoginUrl('https://api.example.com/auth/login');

// Set refresh URL  
authService.setRefreshUrl('https://api.example.com/auth/refresh');
```

### Using AuthConfigService

```typescript
const authConfig = inject(AuthConfigService);

authConfig.setLoginUrl('https://api.example.com/auth/login');
authConfig.setRefreshUrl('https://api.example.com/auth/refresh');
authConfig.setUseSecureCookies(true); // for production
```

## 📚 File Reference

| File | Purpose |
|------|---------|
| `auth/auth.service.ts` | Core auth logic, login, logout, token refresh |
| `auth/auth.guard.ts` | Route protection via `canActivate` guard |
| `auth/auth.interceptor.ts` | Automatic JWT injection into HTTP requests |
| `auth/auth-config.service.ts` | Manages auth configuration |
| `auth/cookie.service.ts` | Cookie storage and retrieval |
| `auth/login.component.ts` | Login form component |
| `home/home.component.ts` | Example protected component |
| `app.config.ts` | App configuration with interceptor registration |
| `app.routes.ts` | Route definitions with guards |

For detailed documentation, see [AUTH_SETUP.md](./AUTH_SETUP.md)

## 🐛 Common Issues

**"Login failed" error:**
- Check your backend endpoint URLs in `app.ts`
- Verify backend is running and accessible
- Check browser console for CORS errors

**Token not being sent in requests:**
- Ensure HTTP calls use `HttpClient` (injected)
- Check AuthInterceptor is registered in `app.config.ts`

**Not redirected to login after logout:**
- Router should navigate to `/login` automatically
- Check that `/login` route exists in `app.routes.ts`

**Token not refreshing:**
- Verify refresh endpoint returns `token` and optionally `expiresIn`
- Check console for any error messages
- Ensure token has `exp` claim

## 💡 Next Steps

1. Update your backend API endpoints in `app.ts`
2. Test the login with valid credentials
3. Add more protected routes with `canActivate: [authGuard]`
4. Customize the login form styling in `login.component.css`
5. Replace the home component with your actual application content
6. In production, ensure HTTPS and set `useSecureCookies: true`

Need help? Check [AUTH_SETUP.md](./AUTH_SETUP.md) for comprehensive documentation.
