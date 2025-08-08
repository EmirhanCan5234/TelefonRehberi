import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class LanguageInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const selectedLang = localStorage.getItem('seciliDil') || 'tr';

    const clonedReq = req.clone({
      setHeaders: {
        'Accept-Language': selectedLang
      }
    });

    return next.handle(clonedReq);
  }
}
