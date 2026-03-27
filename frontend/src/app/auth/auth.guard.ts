import { Injectable, inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Guard to protect routes that require authentication
 * Usage in routes: canActivate: [authGuard]
 */
export const authGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.hasValidToken()) {
    return true;
  }

  // Store the attempted URL for redirecting after login
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
