import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { TokenService } from '../services/token.service';
import { SKIP_AUTH_REDIRECT } from './skip-auth-redirect.token';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
	const router = inject(Router);
	const tokenService = inject(TokenService);
	const isAuthRequest = request.url.includes('/api/auth/login') || request.url.includes('/api/auth/register') || request.url.includes('/api/auth/operator/register');
	const skipRedirect = request.context.get(SKIP_AUTH_REDIRECT);

	return next(request).pipe(
		catchError((error) => {
			if (error?.status === 401 && !isAuthRequest && !skipRedirect) {
				tokenService.clearAuth();
				router.navigateByUrl('/auth/login');
			} else if (error?.status === 403 && !skipRedirect) {
				router.navigateByUrl('/unauthorized');
			}

			return throwError(() => error);
		})
	);
};
