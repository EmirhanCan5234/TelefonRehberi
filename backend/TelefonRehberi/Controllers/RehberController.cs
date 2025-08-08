using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using TelefonRehberi.Models;
using TelefonRehberi.Models.DTO;
using TelefonRehberi.Repositories;

namespace TelefonRehberi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RehberController : ControllerBase
    {
        private readonly IRehberRepository _rehberRepository;
        private readonly IGenericRepository<User> _userRepository;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IUnitOfWork _unitOfWork;

        public RehberController(IRehberRepository rehberRepository, IGenericRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _rehberRepository = rehberRepository;
            _userRepository = userRepository;
        }

        [HttpGet("kisiler")]
        public async Task<IActionResult> GetKisiler(int kullaniciId, int page = 1, int pageSize = 10)
        {
            if (kullaniciId <= 0)
                return BadRequest("Geçersiz Kullanıcı");

            var (kisiler, totalCount) = await _rehberRepository
                .GetPagedAsync(k => k.KullaniciId == kullaniciId, page, pageSize);

            return Ok(new
            {
                totalCount = totalCount,
                data = kisiler
            });
        }

        [HttpPost("ekle")]
        public async Task<IActionResult> KisiEkle([FromBody] KisiDto model)
        {
            try
            {       
                var yeniKisi = new Kisiler
                {
                    
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    Telefon = model.Telefon,
                    Email = model.Email,
                    KullaniciId = model.KullaniciId
                };

                await _unitOfWork.Kisiler.AddAsync(yeniKisi);
                await _unitOfWork.CompleteAsync();

                var successLog = new LogEventInfo(NLog.LogLevel.Info, "", $"Yeni kişi eklendi: {model.Ad} {model.Soyad}");
                successLog.Properties["KullaniciId"] = model.KullaniciId;
                successLog.Properties["Islem"] = "Kişi Eklendi";
                logger.Log(successLog);

                return Ok(new { mesaj = "KisiBasariylaEklendi" });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Kisi eklenirken hata oluştu.");
                return StatusCode(500, "Bir hata oluştu.");
            }
        }

        [HttpPut("guncelle/{id}")]
        public async Task<IActionResult> KisiGuncelle(int id, [FromBody] KisiDto model)
        {
            try
            {
                var kisi = await _unitOfWork.Kisiler.GetByIdAsync(id);
                if (kisi == null)
                    return NotFound("Kisi bulunamadı.");

                kisi.Ad = model.Ad;
                kisi.Soyad = model.Soyad;
                kisi.Telefon = model.Telefon;
                kisi.Email = model.Email;

                await _unitOfWork.CompleteAsync();

                var successLog = new LogEventInfo(NLog.LogLevel.Info, "", $"Kişi güncellendi: {kisi.Ad} {kisi.Soyad}");
                successLog.Properties["KullaniciId"] = model.KullaniciId;
                successLog.Properties["Islem"] = "Kişi Güncellendi";
                logger.Log(successLog);

                return Ok(new { mesaj = "KisiGuncellendi" });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Kisi güncellenirken hata oluştu.");
                return StatusCode(500, "Bir hata oluştu.");
            }
        }

        [HttpDelete("sil/{id}")]
        public async Task<IActionResult> KisiSil(int id)
        {
            try
            {
                var kisi = await _unitOfWork.Kisiler.GetByIdAsync(id);
                if (kisi == null)
                    return NotFound("Kisi bulunamadı.");

                _unitOfWork.Kisiler.Remove(kisi);
                await _unitOfWork.CompleteAsync();

                var successLog = new LogEventInfo(NLog.LogLevel.Info, "", $"Kişi silindi: {kisi.Ad} {kisi.Soyad}");
                successLog.Properties["KullaniciId"] = kisi.KullaniciId;
                successLog.Properties["Islem"] = "Kişi Silindi";
                logger.Log(successLog);

                return Ok(new { mesaj = "KisiSilindi" });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Kisi silinirken hata oluştu.");
                return StatusCode(500, "Bir hata oluştu.");
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportRehberToExcel()
        {
            var kullaniciAdi = User.Identity?.Name;
            if (string.IsNullOrEmpty(kullaniciAdi))
                return Unauthorized();

            var user = await _userRepository.FindAsync(u => u.KullaniciAdi == kullaniciAdi);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı");

            var rehberListesi = await _rehberRepository.GetRehberByKullaniciIdAsync(user.Id);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Rehber");

            worksheet.Cell(1, 1).Value = "Ad";
            worksheet.Cell(1, 2).Value = "Soyad";
            worksheet.Cell(1, 3).Value = "Telefon";
            worksheet.Cell(1, 4).Value = "Email";

            for (int i = 0; i < rehberListesi.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = rehberListesi[i].Ad;
                worksheet.Cell(i + 2, 2).Value = rehberListesi[i].Soyad;
                worksheet.Cell(i + 2, 3).Value = rehberListesi[i].Telefon;
                worksheet.Cell(i + 2, 4).Value = rehberListesi[i].Email;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            var fileName = $"Rehber_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }
    }
}

