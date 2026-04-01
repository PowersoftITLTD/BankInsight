import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { AuthenticationService } from "../../../store/auth/authentication/authentication.service";

// error.interceptor.ts
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthenticationService);
  const router = inject(Router);
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      
      if (error.status === 401) {
        
        // Option A: Silent logout
        authService.logout();
        
        // Option B: Redirect to login
        router.navigate(['/login'], {
          queryParams: { sessionExpired: true }
        });
        
        // Option C: Refresh token (if you have refresh token logic)
        // return authService.refreshToken().pipe(
        //   switchMap(() => next(req))
        // );
      }
      
      return throwError(() => error);
    })
  );
};