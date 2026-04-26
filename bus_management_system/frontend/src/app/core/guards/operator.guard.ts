import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const operatorGuard: CanActivateFn = () => {
	const authService = inject(AuthService);
	const router = inject(Router);

	if (!authService.isAuthenticated()) {
		return router.parseUrl('/auth/login');
	}

	if (!authService.hasRole('operator')) {
		return router.parseUrl('/unauthorized');
	}

	return true;
};
