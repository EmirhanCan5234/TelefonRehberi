using System;
using System.Collections.Generic;

namespace TelefonRehberi.Models;

public partial class Kisiler
{
    public int Id { get; set; }

    public string Ad { get; set; } = null!;

    public string Soyad { get; set; } = null!;

    public string Telefon { get; set; } = null!;

    public string? Email { get; set; }

    public int KullaniciId { get; set; }

    public virtual User Kullanici { get; set; } = null!;
}
