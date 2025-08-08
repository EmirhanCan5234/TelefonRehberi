import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { HttpClient } from '@angular/common/http';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgxMaskDirective } from 'ngx-mask';
@Component({
  selector: 'app-profil',
  templateUrl: './profil.component.html',
  styleUrls: ['./profil.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, NgxMaskDirective],
})
export class ProfilComponent {
  kullanici: any = JSON.parse(localStorage.getItem('kullanici') || '{}');
  kullaniciResim: string = '';


  showNumaraModal = false;
  showSifreModal = false;
  showResimModal = false;


  yeniNumara = '';
  numaraMesaji = '';

  
  eskiSifre = '';
  yeniSifre = '';
  yeniSifreTekrar = '';
  parolaMesaji = '';


  selectedFile: File | null = null;
  secilenDosyaAdi: string = '';
  uploadMessage = '';
  uploadSuccess = false;

  mesaj: string = '';
  mesajTipi: 'success' | 'error' | '' = '';

  constructor(private http: HttpClient, private translate: TranslateService) {}

  ngOnInit() {
    this.kullanici = JSON.parse(localStorage.getItem('kullanici') || '{}');
    this.kullaniciResim =
      this.kullanici?.resimUrl || 'assets/default-profile.png';

    this.http
      .get<any>(
        `http://localhost:5165/api/kullanicilar/${this.kullanici.id}/resim`
      )
      .subscribe({
        next: (res) => {
          this.kullaniciResim = res.imageUrl || 'assets/default-profile.png';
        },
        error: () => {
          this.kullaniciResim = 'assets/default-profile.png';
        },
      });
  }

  openNumaraModal() {
    this.showNumaraModal = true;
    this.showSifreModal = false;
    this.showResimModal = false;
  }

  openSifreModal() {
    this.showSifreModal = true;
    this.showNumaraModal = false;
    this.showResimModal = false;
  }

  openResimModal() {
    this.showResimModal = true;
    this.showNumaraModal = false;
    this.showSifreModal = false;
  }
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.secilenDosyaAdi = this.selectedFile.name;
    }
  }

  uploadImage() {
    if (!this.selectedFile) return;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.http
      .post(
        `http://localhost:5165/api/kullanicilar/${this.kullanici.id}/upload-image?overwrite=false`,
        formData
      )
      .subscribe({
        next: (res: any) => {
          this.kullaniciResim = res.imageUrl;
          this.kullanici.resimUrl = res.imageUrl;
          localStorage.setItem('kullanici', JSON.stringify(this.kullanici));
          this.showResimModal = false;
          this.selectedFile = null;
          this.secilenDosyaAdi = '';

          this.mesajTipi = 'success';
          this.translate.get('RESIM_YUKLENDI').subscribe((msg) => {
            this.mesaj = msg;
            setTimeout(() => {
              this.mesaj = '';
              this.showResimModal = false;
            }, 3000);
          });
        },
        error: (err) => {
          if (err.status === 409) {
            this.translate.get('RESIM_ONAY').subscribe((uyari) => {
              if (confirm(uyari)) {
                this.http
                  .post(
                    `http://localhost:5165/api/kullanicilar/${this.kullanici.id}/upload-image?overwrite=true`,
                    formData
                  )
                  .subscribe({
                    next: (res2: any) => {
                      this.kullaniciResim = res2.imageUrl;
                      this.kullanici.resimUrl = res2.imageUrl;
                      localStorage.setItem(
                        'kullanici',
                        JSON.stringify(this.kullanici)
                      );
                      this.selectedFile = null;
                      this.secilenDosyaAdi = '';
                      this.mesajTipi = 'success';
                      this.translate.get('RESIM_YUKLENDI').subscribe((msg) => {
                        this.mesaj = msg;
                        setTimeout(() => {
                          this.showResimModal = false;
                          this.mesaj = '';
                        }, 3000);
                      });
                    },
                    error: () => {
                      this.mesajTipi = 'error';
                      this.translate
                        .get('RESIM_YUKLEME_HATA')
                        .subscribe((msg) => {
                          this.mesaj = msg;
                          setTimeout(() => {
                            this.mesaj = '';
                            this.showResimModal = false;
                          }, 3000);
                        });
                    },
                  });
              }
            });
          } else {
            this.mesajTipi = 'error';
            this.translate.get('RESIM_YUKLEME_HATA').subscribe((msg) => {
              this.mesaj = msg;
              setTimeout(() => {
                this.mesaj = '';
                this.showResimModal = false;
              }, 3000);
            });
          }
        },
      });
  }

  numaraKaydet() {
    if (!this.yeniNumara || this.yeniNumara.trim() === '') return;

    let temizTelefon = this.yeniNumara.replace(/\D/g, '');
    if (!temizTelefon.startsWith('0')) {
      temizTelefon = '0' + temizTelefon;
    }


    const formatliTelefon = `${temizTelefon.slice(0, 1)}(${temizTelefon.slice(
      1,
      4
    )}) ${temizTelefon.slice(4, 7)}-${temizTelefon.slice(7)}`;

    this.http
      .put(
        `http://localhost:5165/api/kullanicilar/${this.kullanici.id}/telefon`,
        { telefon: formatliTelefon }
      )
      .subscribe({
        next: () => {
          this.kullanici.telefon = formatliTelefon;
          localStorage.setItem('kullanici', JSON.stringify(this.kullanici));
          this.yeniNumara = '';
          this.numaraMesaji = this.translate.instant('NumaraBasarili');
          this.mesajTipi = 'success';

          setTimeout(() => {
            this.numaraMesaji = '';
            this.mesajTipi = '';
            this.showNumaraModal = false;
          }, 2500);
        },
        error: () => {
          this.numaraMesaji =
            this.translate.instant('NumaraGuncelleHata') ||
            'Numara güncellenirken hata oluştu.';
          this.mesajTipi = 'error';
        },
      });
  }

  parolaFormGecerli(): boolean {
    return (
      this.eskiSifre.trim().length > 0 &&
      this.yeniSifre.trim().length > 0 &&
      this.yeniSifre === this.yeniSifreTekrar
    );
  }

  parolaDegistir() {
    if (!this.parolaFormGecerli()) return;

    const data = {
      eskiSifre: this.eskiSifre,
      yeniSifre: this.yeniSifre,
      yeniSifreTekrar: this.yeniSifreTekrar,
    };

    this.http
      .post(
        `http://localhost:5165/api/kullanicilar/${this.kullanici.id}/parola-degistir`,
        data
      )
      .subscribe({
        next: () => {
          this.parolaMesaji = 'ParolaBasarili';
          this.eskiSifre = '';
          this.yeniSifre = '';
          this.yeniSifreTekrar = '';
          setTimeout(() => {
            this.showSifreModal = false;
            this.parolaMesaji = '';
          }, 2500);
        },
        error: (err) => {
          this.parolaMesaji =
            err.error?.message || this.translate.instant('ParolaHatali');
        },
      });
  }
}
