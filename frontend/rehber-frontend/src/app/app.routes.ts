import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { GirisComponent } from './pages/giris/giris.component';
import { KayitComponent } from './pages/kayit/kayit.component';
import { authGuard } from './guards/auth.guard';
import { RehberComponent } from './pages/rehber/rehber.component';
import { RehberEkleComponent } from './pages/kisiekle/rehber-ekle.component';
import { ProfilComponent } from './pages/profil/profil.component';

import { DashboardComponent } from './pages/rehberpanel/rehber-panel.component';
export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  { path: 'giris', component: GirisComponent },
  { path: 'kayit', component: KayitComponent },
  { path: 'rehber', component: RehberComponent, canActivate: [authGuard] },
  { path: 'profil', component: ProfilComponent, canActivate: [authGuard] },
  {
    path: 'rehber-ekle',
    component: RehberEkleComponent,
    canActivate: [authGuard],
  },
  {
    path: 'rehber-panel',
    component: DashboardComponent,
    canActivate: [authGuard],
  },
];
