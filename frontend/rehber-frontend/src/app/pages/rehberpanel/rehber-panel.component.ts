import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';


@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './rehber-panel.component.html',
  styleUrls: ['./rehber-panel.component.css'],
  imports: [CommonModule, TranslateModule],
})
export class DashboardComponent implements OnInit {
  toplamKisiSayisi = 0;
  sonAktiviteler: any[] = [];
  kullaniciAdi = '';
  hataliGirisler: any[] = [];

  constructor(
    private http: HttpClient,
    public translate: TranslateService
  ) {}
 
  ngOnInit(): void {
    const kullanici = JSON.parse(localStorage.getItem('kullanici') || '{}');
    
    if (!kullanici?.id) return;

    this.kullaniciAdi = kullanici.kullaniciAdi || '';

    this.http
      .get<any>(
        `http://localhost:5165/api/Rehber/kisiler?kullaniciId=${kullanici.id}`
      )
      .subscribe({
        next: (res) => {
          this.toplamKisiSayisi = res.totalCount;
        },
        error: (err) => {
          console.error('Dashboard verisi alınamadı:', err);
        },
      });
    this.http
      .get<any[]>(
        `http://localhost:5165/api/Kullanicilar/sonaktiviteler/${kullanici.id}`
      )
      .subscribe({
        next: (loglar) => {
          this.sonAktiviteler = loglar;
        },
        error: (err) => console.error('Loglar alınamadı:', err),
      });
    this.http
      .get<any[]>(
        `http://localhost:5165/api/Kullanicilar/GirisHata/${kullanici.id}`
      )
      .subscribe({
        next: (loglar) => {
          this.hataliGirisler = loglar;
        },
        error: (err) => console.error('Loglar alınamadı:', err),
      });
  }
}
