import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, RouterLink } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';

import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule,RouterLink, TranslateModule, HttpClientModule],
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
})
export class AppComponent {
  kullanici: any = null;
  menuAcik = false;
  rehberMenuAcik = false;
  tema: 'light' | 'dark' = 'light';

  constructor(private http: HttpClient, private router: Router, private translate: TranslateService) 
 {
    this.translate.addLangs(['tr', 'en']);
    const savedLang =
      localStorage.getItem('seciliDil') ||
      this.translate.getBrowserLang() ||
      'tr';
    this.translate.setDefaultLang('tr');
    this.translate.use(savedLang);
  }

  ngOnInit() {
    const data = localStorage.getItem('kullanici');
    if (data) {
      this.kullanici = JSON.parse(data);
      this.tema = this.kullanici.tema || 'light';
      document.body.classList.add('logged-in');
    } else {
      document.body.classList.remove('logged-in');
      this.tema = 'light';
    }
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(this.tema);
  }

  toggleMenu() {
    this.menuAcik = !this.menuAcik;
  }

  toggleRehberMenu() {
    this.rehberMenuAcik = !this.rehberMenuAcik;
  }


ikonTiklandi() {
  const kullanici = JSON.parse(localStorage.getItem('kullanici') || '{}');
  if (kullanici && kullanici.id) {
    this.router.navigate(['/rehber-panel']);
  } else {
    this.router.navigate(['/home']);
  }
}

  cikisYap() {
    const kullanici = JSON.parse(localStorage.getItem('kullanici') || 'null');

    if (kullanici || kullanici.id) {
      this.http
        .post('http://localhost:5165/api/kullanicilar/cikis', {
          id: kullanici.id,
        })
        .subscribe({
          next: () => console.log('Çıkış logu gönderildi.'),
          error: (err) => console.error('Log gönderilemedi:', err),
        });
    }
  localStorage.removeItem('token');
    localStorage.removeItem('kullanici');
    this.kullanici = null;
    this.tema = 'light';

    document.body.classList.remove('dark', 'light');
    document.body.classList.add('light');

    this.router.navigate(['/home']).then(() => {
        location.reload();
  });
  }
sidebarAcik = false;

toggleSidebar() {
  this.sidebarAcik = !this.sidebarAcik;
  const sidebar = document.querySelector('.left-bar');
  if (sidebar) {
    sidebar.classList.toggle('active', this.sidebarAcik);
  }
}

  dilDegistir(dil: string) {
    localStorage.setItem('seciliDil', dil);
    this.translate.use(dil);
  }

  applyTheme(tema: string) {
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(tema);
  }

  temaDegistir() {
    this.tema = this.tema === 'light' ? 'dark' : 'light';

    document.body.classList.remove('light', 'dark');
    document.body.classList.add(this.tema);

    if (this.kullanici && this.kullanici.id) {
      this.http
        .put(`http://localhost:5165/api/kullanicilar/${this.kullanici.id}/tema`, {
          tema: this.tema,
        })
        .subscribe({
          next: () => {
            this.kullanici.tema = this.tema;
            localStorage.setItem('kullanici', JSON.stringify(this.kullanici));
          },
          error: (err) => console.error('Tema güncelleme hatası:', err),
        });
    } else {
      const data = localStorage.getItem('kullanici');
      if (data) {
        const user = JSON.parse(data);
        user.tema = this.tema;
        localStorage.setItem('kullanici', JSON.stringify(user));
      }
    }
  }
}
