using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

[Authorize]
public class ProfilController : Controller
{
    // Profil fotoğrafları herkese görünür avatar olduğu için (Talep/Görev
    // belgelerinin aksine) wwwroot İÇİNDE, statik dosya olarak saklanır.
    private const string FotografAltKlasoru = "profil-fotograflari";
    private static readonly string[] IzinliUzantilar = { ".png", ".jpg", ".jpeg" };
    private const long MaksimumFotografBoyutu = 3 * 1024 * 1024;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly IWebHostEnvironment _ortam;

    public ProfilController(
        UserManager<ApplicationUser> userManager,
        IStajyerService stajyerService,
        IMentorService mentorService,
        IWebHostEnvironment ortam)
    {
        _userManager = userManager;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _ortam = ortam;
    }

    public async Task<IActionResult> Index()
    {
        var kullanici = await _userManager.GetUserAsync(User);
        if (kullanici is null)
        {
            return NotFound();
        }

        var roller = await _userManager.GetRolesAsync(kullanici);
        var model = new ProfilViewModel
        {
            AdSoyad = kullanici.AdSoyad,
            Email = kullanici.Email ?? string.Empty,
            KayitTarihi = kullanici.KayitTarihi,
            Rol = roller.FirstOrDefault() ?? string.Empty,
            ProfilFotografUrl = kullanici.ProfilFotografUrl
        };

        if (roller.Contains(Roller.Stajyer))
        {
            // Liste Mentor/Departman/Kullanici ilişkileriyle birlikte yüklenir;
            // profil sayfasında bu isimleri göstermek için gerekli.
            model.StajyerProfili = (await _stajyerService.GetAllAsync())
                .SingleOrDefault(s => s.KullaniciId == kullanici.Id);

            if (model.StajyerProfili is not null)
            {
                var mentor = (await _mentorService.GetAllAsync())
                    .SingleOrDefault(m => m.Id == model.StajyerProfili.MentorId);
                model.MentorAdSoyad = mentor?.Kullanici.AdSoyad;
            }
        }
        else if (roller.Contains(Roller.Mentor))
        {
            model.MentorProfili = (await _mentorService.GetAllAsync())
                .SingleOrDefault(m => m.KullaniciId == kullanici.Id);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FotografYukle(IFormFile fotograf)
    {
        var kullanici = await _userManager.GetUserAsync(User);
        if (kullanici is null)
        {
            return NotFound();
        }

        if (fotograf is null || fotograf.Length == 0)
        {
            TempData["HataMesaji"] = "Bir fotoğraf seçmelisin.";
            return RedirectToAction(nameof(Index));
        }

        var uzanti = Path.GetExtension(fotograf.FileName).ToLowerInvariant();
        if (!IzinliUzantilar.Contains(uzanti))
        {
            TempData["HataMesaji"] = "Bu dosya türüne izin verilmiyor. İzinli türler: PNG, JPG.";
            return RedirectToAction(nameof(Index));
        }

        if (fotograf.Length > MaksimumFotografBoyutu)
        {
            TempData["HataMesaji"] = "Fotoğraf boyutu 3 MB'ı aşamaz.";
            return RedirectToAction(nameof(Index));
        }

        var klasor = Path.Combine(_ortam.WebRootPath, "uploads", FotografAltKlasoru);
        Directory.CreateDirectory(klasor);

        var eskiDosyaAdi = kullanici.ProfilFotografAdi;
        var yeniDosyaAdi = $"{Guid.NewGuid():N}{uzanti}";

        await using (var akis = System.IO.File.Create(Path.Combine(klasor, yeniDosyaAdi)))
        {
            await fotograf.CopyToAsync(akis);
        }

        kullanici.ProfilFotografAdi = yeniDosyaAdi;
        await _userManager.UpdateAsync(kullanici);

        FotografSilVarsa(klasor, eskiDosyaAdi);

        TempData["BilgiMesaji"] = "Profil fotoğrafın güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FotografKaldir()
    {
        var kullanici = await _userManager.GetUserAsync(User);
        if (kullanici is null)
        {
            return NotFound();
        }

        var klasor = Path.Combine(_ortam.WebRootPath, "uploads", FotografAltKlasoru);
        FotografSilVarsa(klasor, kullanici.ProfilFotografAdi);

        kullanici.ProfilFotografAdi = null;
        await _userManager.UpdateAsync(kullanici);

        TempData["BilgiMesaji"] = "Profil fotoğrafın kaldırıldı.";
        return RedirectToAction(nameof(Index));
    }

    private static void FotografSilVarsa(string klasor, string? dosyaAdi)
    {
        if (string.IsNullOrEmpty(dosyaAdi))
        {
            return;
        }

        var tamYol = Path.Combine(klasor, dosyaAdi);
        if (System.IO.File.Exists(tamYol))
        {
            System.IO.File.Delete(tamYol);
        }
    }
}
