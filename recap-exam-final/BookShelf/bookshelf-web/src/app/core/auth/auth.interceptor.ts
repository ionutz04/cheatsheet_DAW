import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.token();
  const isAuthCall =
    req.url.startsWith('/api/auth/login') ||
    req.url.startsWith('/api/auth/register');

  const outgoingReq = !token ? req : withAuthorizationHeader(req, token);

  return next(outgoingReq).pipe(
    catchError((err) => {
      if (err.status === 401 && !isAuthCall) {
        auth.logout();
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      }
      return throwError(() => err);
    })
  );
};

function withAuthorizationHeader(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });
}
