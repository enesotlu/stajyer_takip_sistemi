using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Helpers;

namespace StajyerTakip.Web.Controllers;

// Stajyerin kendisine gelen talepleri (bildirimleri) görüp cevapladığı
// ekran. Dosya istenen taleplerde CV/belge yüklenir.
[Authorize(Roles = Roller.Stajyer)]
public class TaleplerimController : Controller
{
    private const string DosyaAltKlasoru = "Talepler";

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
        await _talepService.StajyerGorduIsaretleAsync(stajyer.Id);
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
                (kayitliDosyaAdi, orijinalDosyaAdi) =
                    await DosyaYukleyici.KaydetAsync(dosya, _ortam.ContentRootPath, DosyaAltKlasoru);
            }

            await _talepService.CevaplaAsync(id, stajyer.Id, cevapMetni, kayitliDosyaAdi, orijinalDosyaAdi);
            TempData["BilgiMesaji"] = "Cevabın mentörüne iletildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;

            // Servis reddettiyse diske yazılmış dosyayı geride bırakma.
            DosyaYukleyici.SilVarsa(_ortam.ContentRootPath, DosyaAltKlasoru, kayitliDosyaAdi);
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

        var dosyaYolu = DosyaYukleyici.TamYol(_ortam.ContentRootPath, DosyaAltKlasoru, talep.CevapDosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
        {
            return NotFound();
        }

        return PhysicalFile(dosyaYolu, "application/octet-stream",
            talep.CevapDosyaOrijinalAdi ?? talep.CevapDosyaAdi);
    }

    // Stajyer, mentörün talebe eklediği belgeyi indirir.
    public async Task<IActionResult> EkDosyaIndir(int id)
    {
        var stajyer = await BenimStajyerimAsync();
        if (stajyer is null)
        {
            return NotFound();
        }

        var talep = await _talepService.GetByIdAsync(id);
        if (talep is null || talep.StajyerId != stajyer.Id || string.IsNullOrEmpty(talep.EkDosyaAdi))
        {
            return NotFound();
        }

        var dosyaYolu = DosyaYukleyici.TamYol(_ortam.ContentRootPath, DosyaAltKlasoru, talep.EkDosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
        {
            return NotFound();
        }

        return PhysicalFile(dosyaYolu, "application/octet-stream",
            talep.EkDosyaOrijinalAdi ?? talep.EkDosyaAdi);
    }

    private async Task<Stajyer?> BenimStajyerimAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _stajyerService.GetByKullaniciIdAsync(kullaniciId);
    }
}
