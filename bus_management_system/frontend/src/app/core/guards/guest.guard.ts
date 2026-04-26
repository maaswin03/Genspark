import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  const role = authService.getCurrentRole()?.toLowerCase();

  if (role === 'admin') {
    return router.parseUrl('/admin');
  }

  if (role === 'operator') {
    return router.parseUrl('/operator');
  }

  return router.parseUrl('/home');
};
