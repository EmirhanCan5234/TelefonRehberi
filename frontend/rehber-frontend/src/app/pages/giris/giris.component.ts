import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-giris',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule],
  templateUrl: './giris.component.html',
  styleUrls: ['./giris.component.css'],
})
export class GirisComponent {
  kullaniciAdi = '';
  sifre = '';
  hataMesaji = '';
  mesaj = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private translate: TranslateService
  ) {}

  girisYap() {
    const data = {
      kullaniciAdi: this.kullaniciAdi,
      sifre: this.sifre,
    };

    this.http
      .post<any>('http://localhost:5165/api/Kullanicilar/giris', data)
      .subscribe({
        next: (res) => {
          this.hataMesaji = '';
          this.mesaj = this.translate.instant('GirisBasarili');
          localStorage.setItem('token', res.token);
          localStorage.setItem('kullanici', JSON.stringify(res.kullanici));
          setTimeout(() => {
            window.location.href = '/rehber-panel';
          }, 1500);
        },
        error: (err) => {
          const mesajKodu = err.error?.message || 'GENEL_HATA';
          this.translate.get(mesajKodu).subscribe((res) => {
            this.hataMesaji = res;
          });
        },
      });
  }
}
