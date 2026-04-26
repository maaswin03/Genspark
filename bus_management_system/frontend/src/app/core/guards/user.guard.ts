import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const userGuard: CanActivateFn = () => {
	const authService = inject(AuthService);
	const router = inject(Router);

	if (!authService.isAuthenticated()) {
		return router.parseUrl('/auth/login');
	}

	if (!authService.hasRole('user')) {
		return router.parseUrl('/unauthorized');
	}

	return true;
};
