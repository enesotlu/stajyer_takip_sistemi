using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Stajyerin kendisine gelen talepleri (bildirimleri) görüp cevapladığı
// ekran. Dosya istenen taleplerde CV/belge yüklenir.
[Authorize(Roles = Roller.Stajyer)]
public class TaleplerimController : Controller
{
    // Rapordaki girdi doğrulama maddesi gereği yalnızca beklenen belge
    // türlerine izin veriyoruz; boyut da 10 MB ile sınırlı.
    private static readonly string[] IzinliUzantilar = { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
    private const long MaksimumDosyaBoyutu = 10 * 1024 * 1024;

    private readonly ITalepService _talepService;
    private readonly IStajyerService _stajyerService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _ortam;

    public TaleplerimController(
        ITalepService talepService,
        IStajyerService stajyerService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment ortam)
    {
        _talepService = talepService;
        _stajyerService = stajyerService;
        _userManager = userManager;
        _ortam = ortam;
    }

    public async Task<IActionResult> Index()
    {
        var stajyer = await BenimStajyerimAsync();
        ViewBag.ProfilYok = stajyer is null;
        if (stajyer is null)
        {
            return View(new List<Talep>());
        }

        var talepler = await _talepService.GetByStajyerAsync(stajyer.Id);
        return View(talepler);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cevapla(int id, string? cevapMetni, IFormFile? dosya)
    {
        var stajyer = await BenimStajyerimAsync();
        if (stajyer is null)
        {
            return NotFound();
        }

        string? kayitliDosyaAdi = null;
        string? orijinalDosyaAdi = null;

        try
        {
            if (dosya is not null && dosya.Length > 0)
            {
                var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
                if (!IzinliUzantilar.Contains(uzanti))
                {
                    throw new InvalidOperationException(
                        $"Bu dosya türüne izin verilmiyor. İzinli türler: {string.Join(", ", IzinliUzantilar)}");
                }

                if (dosya.Length > MaksimumDosyaBoyutu)
                {
                    throw new InvalidOperationException("Dosya boyutu 10 MB'ı aşamaz.");
                }

                // Dosya, kullanıcının verdiği adla değil güvenli bir GUID adla
                // ve wwwroot DIŞINDA saklanır - indirme yalnızca yetki kontrolü
                // yapan controller action'ları üzerinden olur.
                var klasor = Path.Combine(_ortam.ContentRootPath, "Uploads", "Talepler");
                Directory.CreateDirectory(klasor);

                kayitliDosyaAdi = $"{Guid.NewGuid():N}{uzanti}";
                orijinalDosyaAdi = Path.GetFileName(dosya.FileName);

                var tamYol = Path.Combine(klasor, kayitliDosyaAdi);
                await using var akis = System.IO.File.Create(tamYol);
                await dosya.CopyToAsync(akis);
            }

            await _talepService.CevaplaAsync(id, stajyer.Id, cevapMetni, kayitliDosyaAdi, orijinalDosyaAdi);
            TempData["BilgiMesaji"] = "Cevabın mentörüne iletildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;

            // Servis reddettiyse diske yazılmış dosyayı geride bırakma.
            if (kayitliDosyaAdi is not null)
            {
                var tamYol = Path.Combine(_ortam.ContentRootPath, "Uploads", "Talepler", kayitliDosyaAdi);
                if (System.IO.File.Exists(tamYol))
                {
                    System.IO.File.Delete(tamYol);
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // Stajyer kendi yüklediği dosyayı tekrar indirebilir.
    public async Task<IActionResult> DosyaIndir(int id)
    {
        var stajyer = await BenimStajyerimAsync();
        if (stajyer is null)
        {
            return NotFound();
        }

        var talep = await _talepService.GetByIdAsync(id);
        if (talep is null || talep.StajyerId != stajyer.Id || string.IsNullOrEmpty(talep.CevapDosyaAdi))
        {
            return NotFound();
        }

        var dosyaYolu = Path.Combine(_ortam.ContentRootPath, "Uploads", "Talepler", talep.CevapDosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
        {
            return NotFound();
        }

        return PhysicalFile(dosyaYolu, "application/octet-stream",
            talep.CevapDosyaOrijinalAdi ?? talep.CevapDosyaAdi);
    }

    private async Task<Stajyer?> BenimStajyerimAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _stajyerService.GetByKullaniciIdAsync(kullaniciId);
    }
}
