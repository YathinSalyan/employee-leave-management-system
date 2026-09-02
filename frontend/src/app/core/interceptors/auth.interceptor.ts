import { HttpInterceptorFn } from '@angular/common/http';

const TOKEN_KEY = 'elms_token';

// Reads the token directly from localStorage rather than injecting AuthService,
// to avoid a circular-dependency risk between the interceptor and the service
// it would otherwise depend on.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(TOKEN_KEY);

  if (!token) {
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });

  return next(cloned);
};
