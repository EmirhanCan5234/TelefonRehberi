import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AgGridModule } from 'ag-grid-angular';
import { ColDef, ModuleRegistry, AllCommunityModule } from 'ag-grid-community';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { saveAs } from 'file-saver';
import { NgxMaskDirective } from 'ngx-mask';
ModuleRegistry.registerModules([AllCommunityModule]);

@Component({
  selector: 'app-rehber',
  standalone: true,
  imports: [CommonModule, AgGridModule, FormsModule, TranslateModule,NgxMaskDirective],
  templateUrl: './rehber.component.html',
  styleUrls: ['./rehber.component.css'],
})
export class RehberComponent implements OnInit {
  columnDefs: ColDef[] = [];
  rowData: any[] = [];
  sayfaVerisi: any[] = [];
  aramaMetni: string = '';

  detayGoster = false;
  guncelleModu = false;
  silmeGoster = false;
  secilenKisi: any = null;

  mesaj: string = '';
  mesajTipi: 'success' | 'error' | '' = '';

  currentPage = 1;
  pageSize = 10;
  toplamSayfa = 0;

  constructor(
    private http: HttpClient,
    private router: Router,
    private translate: TranslateService
  ) {}

  ngOnInit() {
    const kullanici = localStorage.getItem('kullanici');
    if (!kullanici) {
      this.router.navigate(['/giris']);
      return;
    }

    this.loadColumnDefs();
    this.veriGetir();

    this.translate.onLangChange.subscribe(() => {
      this.loadColumnDefs();
    });
  }

  loadColumnDefs() {
    this.columnDefs = [
      { field: 'id', headerName: '', hide: true },
      { field: 'ad', headerName: this.translate.instant('Ad'), flex: 1, wrapText: true },
      { field: 'soyad', headerName: this.translate.instant('Soyad'), flex: 1, wrapText: true },
      {
        headerName: this.translate.instant('ISLEM'),
        cellRenderer: () => `
          <button class="detay-btn">${this.translate.instant('Detay')}</button>
          <button class="sil-btn">${this.translate.instant('KISI_SIL')}</button>
        `,
        onCellClicked: (params: any) => {
          this.secilenKisi = { ...params.data };
          const clickedText = (params.event.target as HTMLElement).innerText.trim();
          if (clickedText === this.translate.instant('Detay')) {
            this.detayGoster = true;
            this.guncelleModu = false;
          } else if (clickedText === this.translate.instant('KISI_SIL')) {
            this.silmeGoster = true;
          }
        },
        flex: 1,
        width: 150,
        wrapText: true,
      },
    ];
  }

  veriGetir() {
    const kullanici = JSON.parse(localStorage.getItem('kullanici') || '{}');
    const id = kullanici.id;

    this.http
      .get<any>(`http://localhost:5165/api/Rehber/kisiler?kullaniciId=${id}&page=${this.currentPage}&pageSize=${this.pageSize}`)
      .subscribe({
        next: (data) => {
          this.rowData = data.data || [];
          this.toplamSayfa = Math.ceil(data.totalCount / this.pageSize);
          this.filtrele();
        },
        error: (err) => console.error('Veri alınırken hata:', err),
      });
  }

  filtrele() {
    const arama = this.aramaMetni.toLowerCase();
    this.sayfaVerisi = this.rowData.filter((kisi) =>
      kisi.ad.toLowerCase().includes(arama)
    );
  }

  sonrakiSayfa() {
    if (this.currentPage < this.toplamSayfa) {
      this.currentPage++;
      this.veriGetir();
    }
  }

  oncekiSayfa() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.veriGetir();
    }
  }

  guncelleKaydet() {
  const kullanici = JSON.parse(localStorage.getItem('kullanici') || '{}');
  if (!this.secilenKisi?.id || !kullanici?.id) return;
  this.secilenKisi.kullaniciId = kullanici.id;

  if (
    !this.secilenKisi.ad ||
    !this.secilenKisi.soyad ||
    !this.secilenKisi.telefon ||
    !this.secilenKisi.email
  ) {
    this.mesajTipi = "error";
    this.translate.get('ZORUNLU_ALAN').subscribe((msg) => {
      this.mesaj = msg;
      setTimeout(() => {
        this.mesaj = '';
      }, 1500);
    });
    return;
  }

  const duzgunTelefon = this.secilenKisi.telefon.replace(/\D/g, ''); 
  if (duzgunTelefon.length === 10) {
    this.secilenKisi.telefon = `0(${duzgunTelefon.slice(0, 3)})${duzgunTelefon.slice(3, 6)}-${duzgunTelefon.slice(6, 10)}`;
  } else if (duzgunTelefon.length === 11 && duzgunTelefon.startsWith('0')) {
    this.secilenKisi.telefon = `0(${duzgunTelefon.slice(1, 4)})${duzgunTelefon.slice(4, 7)}-${duzgunTelefon.slice(7, 11)}`;
  } else {
    this.mesajTipi = 'error';
    this.mesaj = 'Telefon formatı hatalı.';
    return;
  }

  this.http
    .put(`http://localhost:5165/api/Rehber/guncelle/${this.secilenKisi.id}`, this.secilenKisi)
    .subscribe({
      next: () => {
        this.mesajTipi = 'success';
        this.translate.get('KAYIT_BASARILI').subscribe((msg) => {
          this.mesaj = msg;
          setTimeout(() => {
            this.mesaj = '';
            this.detayGoster = false;
            this.guncelleModu = false;
            this.veriGetir();
          }, 1500);
        });
      },
      error: (err) => {
        this.mesajTipi = 'error';
        this.translate.get('GUNCELLEME_HATA').subscribe((msg) => {
          this.mesaj = msg;
        });
        console.error('Güncelleme hatası:', err);
      },
    });
}


  kisiSil() {
    if (!this.secilenKisi?.id) return;

    this.http
      .delete(`http://localhost:5165/api/Rehber/sil/${this.secilenKisi.id}`)
      .subscribe({
        next: () => {
          this.mesajTipi = 'success';
          this.translate.get('SILME_BASARILI').subscribe((msg) => {
            this.mesaj = msg;
            setTimeout(() => {
              this.mesaj = '';
              this.silmeGoster = false;
              this.veriGetir();
            }, 1500);
          });
        },
        error: (err) => {
          this.mesajTipi = 'error';
          this.translate.get('SILME_HATA').subscribe((msg) => {
            this.mesaj = msg;
          });
          console.error(this.translate.instant('SILME_HATASI'), err);
        },
      });
  }

  iptalEt() {
    this.silmeGoster = false;
  }

  excelIndir() {
    const token = localStorage.getItem('token');
    
    const headers = new HttpHeaders({
      Authorization: 'Bearer ' + token,
      Accept: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    });

    this.http
      .get(`http://localhost:5165/api/Rehber/export`, {
        headers,
        responseType: 'blob',
      })
      .subscribe({
        next: (response: Blob) => {
          const blob = new Blob([response], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
          });
          const fileName = 'Rehber_' + new Date().toISOString() + '.xlsx';
          saveAs(blob, fileName);
        },
        error: (err) => {
          console.error('Excel indirme hatası:', err);
          alert('Excel dosyası indirilemedi.');
        },
      });
  }
}
