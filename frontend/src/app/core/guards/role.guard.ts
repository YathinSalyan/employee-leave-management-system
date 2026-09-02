import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Usage on a route: canActivate: [roleGuard(['Admin'])]
// or for multiple allowed roles: roleGuard(['Admin', 'Manager'])
export function roleGuard(allowedRoles: string[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      router.navigate(['/login']);
      return false;
    }

    const role = authService.role();
    if (role && allowedRoles.includes(role)) {
      return true;
    }

    // Logged in, just not permitted here — send them somewhere valid rather than blank.
    router.navigate(['/dashboard']);
    return false;
  };
}
