using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TelefonRehberi.Models;
using TelefonRehberi.Models.DTO;
using TelefonRehberi.Repositories;

namespace TelefonRehberi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KullanicilarController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private readonly IGenericRepository<User> _userRepo;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IGenericRepository<Log> _logRepo;
        public KullanicilarController(IStringLocalizer<SharedResource> localizer, IGenericRepository<User> userRepo, IConfiguration configuration, IGenericRepository<Log> logRepo)
        {
            _logRepo = logRepo;
            _localizer = localizer;
            _userRepo = userRepo;
            _configuration = configuration;
        }
        [HttpPost("giris")]
        public async Task<IActionResult> GirisYap([FromBody] KullaniciGirisModel model)
        {
            var user1 = await _userRepo.FindAsync(u => u.KullaniciAdi == model.KullaniciAdi );

            var user = await _userRepo.FindAsync(u => u.KullaniciAdi == model.KullaniciAdi && u.Sifre == model.Sifre);
           if(user1.KullaniciAdi==model.KullaniciAdi && model.Sifre!=user1.Sifre)
            {
                var Girishatalog = new LogEventInfo(NLog.LogLevel.Info, "", $"Hatalı Şifre Denemesi - Kullanıcı: {user1.KullaniciAdi}");
                Girishatalog.Properties["KullaniciId"] = user1.Id;
                Girishatalog.Properties["Islem"] = "Giriş Başarısız";
                logger.Log(Girishatalog);
            }
            if (user == null)
            {

                return Unauthorized(new { message = "Yanlisbilgi" });
            }
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.KullaniciAdi),
        new Claim("UserId", user.Id.ToString())
    };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var successLog = new LogEventInfo(NLog.LogLevel.Info, "", $"Giriş yapıldı - Kullanıcı: {user.KullaniciAdi}");
            successLog.Properties["KullaniciId"] = user.Id;
            successLog.Properties["Islem"] = "Giriş Başarılı";
            logger.Log(successLog);
            return Ok(new
            {
                token = tokenString,
                kullanici = new
                {
                    user.Id,
                    ad = user.Ad,
                    soyad = user.Soyad,
                    tema = user.Tema,
                    telefon = user.Telefon ?? "",
                    kullanici = user.Ad + " " + user.Soyad
                }
            });
        }


        [HttpPost("kayit")]
        public async Task<IActionResult> KayitOl([FromBody] User kullanici)
        {
            if (string.IsNullOrWhiteSpace(kullanici.KullaniciAdi) ||
                string.IsNullOrWhiteSpace(kullanici.Sifre) ||
                string.IsNullOrWhiteSpace(kullanici.Ad) ||
                string.IsNullOrWhiteSpace(kullanici.Soyad))
            {
                return BadRequest(new { message = "ZorunluAlanHatasi" });
            }

            var mevcut = await _userRepo.FindAsync(k => k.KullaniciAdi.ToLower() == kullanici.KullaniciAdi.ToLower());

            if (mevcut != null)
            {
                return BadRequest(new { message = "KullaniciAdiAlinmis" });
            }

            kullanici.Tema = "light";
            await _userRepo.AddAsync(kullanici);
            await _userRepo.SaveAsync();
            var successLog = new LogEventInfo(NLog.LogLevel.Info, "", $"Kayıt Yapan Kullanıcı: {kullanici.Ad} {kullanici.Soyad}");
            successLog.Properties["KullaniciId"] = 0;
            successLog.Properties["Islem"] = "Kayıt Başarılı";
            logger.Log(successLog);
            return Ok(new { message = "KayitBasarili" });
        }

        [HttpPost("cikis")]
        public async Task<IActionResult> CikisYap([FromBody] KullaniciCikisModel model)
        {
            var user = await _userRepo.GetByIdAsync(model.Id);
            if (user == null) return NotFound();

            var log = new LogEventInfo(NLog.LogLevel.Info, "", $"{user.KullaniciAdi} çıkış yaptı.");
            log.Properties["KullaniciId"] = user.Id;
            log.Properties["Islem"] = "Çıkış";
            logger.Log(log);

            return Ok(new { mesaj = "Çıkış logu kaydedildi." });
        }

        [HttpGet("sonaktiviteler/{kullaniciId}")]
        public async Task<IActionResult> sonaktiviteler(int kullaniciId)
        {
            try
            {
                var loglar = await _logRepo
                    .FindAllAsync(l =>
                        l.KullaniciId == kullaniciId &&
                        (l.Islem.ToLower().Trim() == "Kişi Eklendi" ||
        l.Islem.ToLower().Trim() == "Kişi Güncellendi" ||
        l.Islem.ToLower().Trim() == "Kişi Silindi"));

                var son3Log = loglar
                    .OrderByDescending(l => l.Tarih)
                    .Take(3)
                    .ToList();

                return Ok(son3Log);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Loglar alınamadı.");
                return StatusCode(500, "Loglar alınırken sunucu hatası.");
            }
        }
        [HttpGet("GirisHata/{kullaniciId}")]
        public async Task<IActionResult> Girishata(int kullaniciId)
        {
            try
            {
                var loglar = await _logRepo
                    .FindAllAsync(l =>
                        l.KullaniciId == kullaniciId &&
                        (l.Islem.ToLower().Trim() == "Giriş Başarısız"));
                var son3Log = loglar
                    .OrderByDescending(l => l.Tarih)
                    .Take(3)
                    .ToList();

                return Ok(son3Log);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Loglar alınamadı.");
                return StatusCode(500, "Loglar alınırken sunucu hatası.");
            }
        }


        [HttpPost("{id}/parola-degistir")]
        public async Task<IActionResult> ParolaDegistir(int id, [FromBody] ParolaDegistirModel model)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Sifre != model.EskiSifre)
                return BadRequest(new { message = "Eski şifre yanlış." });

            if (model.YeniSifre != model.YeniSifreTekrar)
                return BadRequest(new { message = "Yeni şifreler uyuşmuyor." });

            user.Sifre = model.YeniSifre;
            _userRepo.Update(user);
            await _userRepo.SaveAsync();

            return Ok(new { message = "Parola başarıyla değiştirildi." });
        }

        [HttpPut("{id}/telefon")]
        public async Task<IActionResult> TelefonGuncelle(int id, [FromBody] TelefonGuncelleDto model)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "KullaniciBulunamadi" });

            if (string.IsNullOrWhiteSpace(model.Telefon))
                return BadRequest(new { message = "TelefonBosOlamaz" });

            user.Telefon = model.Telefon;
            _userRepo.Update(user);
            await _userRepo.SaveAsync();

            return Ok(new { message = "TelefonGuncellendi" });
        }

        [HttpPut("{id}/tema")]
        public async Task<IActionResult> TemaGuncelle(int id, [FromBody] TemaGuncelleDto temaDto)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.Tema = temaDto.Tema;
            _userRepo.Update(user);
            await _userRepo.SaveAsync();

            return Ok(new { message = "Tema başarıyla güncellendi." });
        }

        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file, [FromQuery] bool overwrite = false)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Resim != null && user.Resim.Length > 0 && !overwrite)
                return Conflict(new { message = "Resim mevcut, değiştirmek için onay ver." });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            user.Resim = fileBytes;
            _userRepo.Update(user);
            await _userRepo.SaveAsync();

            var base64Image = Convert.ToBase64String(fileBytes);
            var imageUrl = $"data:{file.ContentType};base64,{base64Image}";

            return Ok(new { imageUrl });
        }

        [HttpGet("{id}/resim")]
        public async Task<IActionResult> GetUserImage(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null || user.Resim == null) return NotFound();

            var base64 = Convert.ToBase64String(user.Resim);
            var imgSrc = $"data:image/jpeg;base64,{base64}";
            return Ok(new { imageUrl = imgSrc });
        }
    }

    public class TemaGuncelleDto { public string Tema { get; set; } }
    public class TelefonGuncelleDto { public string Telefon { get; set; } }
    public class ParolaDegistirModel { public string EskiSifre { get; set; } public string YeniSifre { get; set; } public string YeniSifreTekrar { get; set; } }
    public class KullaniciGirisModel { public string KullaniciAdi { get; set; } public string Sifre { get; set; } }
    public class KullaniciCikisModel { public int Id { get; set; } }
}