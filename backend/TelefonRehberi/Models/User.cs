using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TelefonRehberi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string KullaniciAdi { get; set; }

        [Required]
        public string Sifre { get; set; }

        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Tema { get; set; }
        public string? Telefon { get; set; }
        public byte[]? Resim { get; set; }

        public ICollection<Kisiler>? Kisilers { get; set; }
    }
}
