using System;
using System.Collections.Generic;

namespace TelefonRehberi.Models;

public partial class Log
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public string? Islem { get; set; }

    public DateTime? Tarih { get; set; }

    public string? Detay { get; set; }
}
