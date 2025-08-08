namespace TelefonRehberi.Models.DTO
{
    public class KisiEkleDTO
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public int KullaniciId { get; set; } // Oturum açmış kullanıcıdan alınacak
    }
}
