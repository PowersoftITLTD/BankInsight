import { HttpInterceptorFn } from "@angular/common/http";

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {



  // Skip login
  if (req.url.includes('/login')) {
    return next(req);
  }

const auth = sessionStorage.getItem('auth');
const token = auth ? JSON.parse(auth).token : null;

  if (!token) {
    return next(req);
  }

  const clonedReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });


  return next(clonedReq);
};
