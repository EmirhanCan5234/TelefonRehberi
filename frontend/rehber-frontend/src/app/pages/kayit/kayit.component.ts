import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-kayit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule],
  templateUrl: './kayit.component.html',
  styleUrls: ['./kayit.component.css'],
})
export class KayitComponent {
  kullaniciAdi = '';
  sifre = '';
  ad = '';
  soyad = '';
  mesaj = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private translate: TranslateService
  ) {}

  kayitOl() {
    const yeniKullanici = {
      kullaniciAdi: this.kullaniciAdi,
      sifre: this.sifre,
      ad: this.ad,
      soyad: this.soyad,
      tema: 'light'
    };

   
    if (!this.kullaniciAdi || !this.sifre || !this.ad || !this.soyad) {
      this.translate.get('ZorunluAlanHatasi').subscribe(translated => {
        this.mesaj = '❌ ' + translated;
      });
      return;
    }

    this.http
      .post('http://localhost:5165/api/Kullanicilar/kayit', yeniKullanici)
      .subscribe({
        next: () => {
          this.translate.get('KayitBasarili').subscribe(translated => {
            this.mesaj = '✅ ' + translated;
          });
          setTimeout(() => this.router.navigate(['/giris']), 2000);
        },
        error: (err) => {
          const hataKodu = err.error?.message;
          if (hataKodu) {
            this.translate.get(hataKodu).subscribe(translated => {
              this.mesaj = '❌ ' + translated;
            });
          } else {
            this.mesaj = '❌ Kayıt sırasında hata oluştu.';
          }
        },
      });
  }
}
