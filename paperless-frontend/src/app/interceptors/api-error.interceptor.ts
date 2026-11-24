import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      const message =
        typeof error.error === 'object' && error.error?.error
          ? error.error.error
          : 'Die Anfrage konnte nicht verarbeitet werden.';

      console.error('API request failed', {
        method: request.method,
        url: request.url,
        status: error.status,
        message
      });

      return throwError(() => ({ ...error, userMessage: message }));
    })
  );
