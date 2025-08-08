import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('token');
  const kullanici = localStorage.getItem('kullanici');

  if (token && kullanici) {
    return true;
  } else {
    router.navigate(['/giris']);
    return false;
  }
};
