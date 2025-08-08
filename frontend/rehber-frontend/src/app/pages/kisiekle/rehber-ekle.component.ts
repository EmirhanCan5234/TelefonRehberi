import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgxMaskDirective } from 'ngx-mask';

@Component({
  selector: 'app-rehber-ekle',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, NgxMaskDirective],
  templateUrl: './rehber-ekle.component.html',
  styleUrls: ['./rehber-ekle.component.css'],
})
export class RehberEkleComponent implements OnInit {
  yeniKisi = {
    ad: '',
    soyad: '',
    telefon: '',
    email: '',
  };

  mesaj: string = '';
  mesajTipi: 'success' | 'error' | '' = '';

  tumKisiler: any[] = [];

  constructor(private http: HttpClient, private router: Router, private translate: TranslateService) {}

  ngOnInit() {
    const kullanici = JSON.parse(localStorage.getItem('kullanici') || '{}');
    if (!kullanici?.id) return;

    this.http
      .get<any>(`http://localhost:5165/api/Rehber/kisiler?kullaniciId=${kullanici.id}`)
      .subscribe({
        next: (data) => {
          this.tumKisiler = data?.data || []; 
        },
        error: (err) => console.error('Veriler alınamadı:', err),
      });
  }

  kisiKaydet() {
    const kullanici = JSON.parse(localStorage.getItem('kullanici') || '{}');
    if (!kullanici?.id) return;

    const { ad, soyad, telefon, email } = this.yeniKisi;

    if (!ad.trim() || !soyad.trim() || !telefon.trim()) {
      this.mesajTipi = 'error';
      this.translate.get('ZORUNLU_ALAN').subscribe(msg => this.mesaj = msg);
      return;
    }

    let temizTelefon = telefon.replace(/\D/g, '');

    if (!temizTelefon.startsWith('0')) {
      temizTelefon = '0' + temizTelefon;
    }

    

    if (temizTelefon.length !== 11 || !temizTelefon.startsWith('0')) {
      this.mesajTipi = 'error';
      this.translate.get('HATALI_TELEFON').subscribe(msg => this.mesaj = msg);
      return;
    }

    const formatliTelefon = `${temizTelefon.slice(0, 1)}(${temizTelefon.slice(1, 4)}) ${temizTelefon.slice(4, 7)}-${temizTelefon.slice(7)}`;

    const telefonVarMi = Array.isArray(this.tumKisiler) && this.tumKisiler.some((k) => k.telefon === formatliTelefon);
    if (telefonVarMi) {
      this.mesajTipi = 'error';
      this.translate.get('TELEFON_KAYITLI').subscribe(msg => this.mesaj = msg);
      return;
    }

    const isimVarMi = Array.isArray(this.tumKisiler) && this.tumKisiler.some(
      (k) =>
        k.ad.trim().toLowerCase() === ad.trim().toLowerCase() &&
        k.soyad.trim().toLowerCase() === soyad.trim().toLowerCase()
    );
    if (isimVarMi) {
      this.mesajTipi = 'error';
      this.translate.get('Ad_Hata').subscribe(msg => this.mesaj = msg);
      return;
    }

    const payload = {
      ...this.yeniKisi,
      telefon: formatliTelefon,
      kullaniciId: kullanici.id,
    };

    this.http.post('http://localhost:5165/api/Rehber/ekle', payload).subscribe({
      next: () => {
        this.mesajTipi = 'success';
        this.translate.get('KISI_EKLENDI').subscribe(msg => {
          this.mesaj = msg;
          setTimeout(() => this.router.navigate(['/rehber-panel']), 1500);
        });
      },
      error: () => {
        this.mesajTipi = 'error';
        this.translate.get('EKLEME_HATASI').subscribe(msg => this.mesaj = msg);
      },
    });
  }
}
