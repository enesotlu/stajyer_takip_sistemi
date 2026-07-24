using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Helpers;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Stajyer)]
public class GorevlerimController : Controller
{
    private const string DosyaAltKlasoru = "Gorevler";

    private readonly IGorevService _gorevService;
    private readonly IStajyerService _stajyerService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _ortam;

    public GorevlerimController(
        IGorevService gorevService,
        IStajyerService stajyerService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment ortam)
    {
        _gorevService = gorevService;
        _stajyerService = stajyerService;
        _userManager = userManager;
        _ortam = ortam;
    }

    public async Task<IActionResult> Index()
    {
        var kullaniciId = _userManager.GetUserId(User);
        var stajyer = kullaniciId is null ? null : await _stajyerService.GetByKullaniciIdAsync(kullaniciId);
        ViewBag.ProfilYok = stajyer is null;
        if (stajyer is null)
        {
            return View(new List<Gorev>());
        }

        var gorevler = await _gorevService.GetByStajyerIdAsync(stajyer.Id);
        await _gorevService.StajyerGorduIsaretleAsync(stajyer.Id);
        return View(gorevler);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumGuncelle(int id, GorevDurumu yeniDurum)
    {
        var kullaniciId = _userManager.GetUserId(User);
        if (kullaniciId is not null)
        {
            try
            {
                await _gorevService.StajyerDurumGuncelleAsync(id, kullaniciId, yeniDurum);
            }
            catch (InvalidOperationException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TeslimEt(int id, string? teslimNotu, IFormFile? dosya)
    {
        var kullaniciId = _userManager.GetUserId(User);
        if (kullaniciId is null)
        {
            return RedirectToAction(nameof(Index));
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

            await _gorevService.StajyerTeslimEtAsync(id, kullaniciId, teslimNotu, kayitliDosyaAdi, orijinalDosyaAdi);
            TempData["BilgiMesaji"] = "Görev teslim edildi, mentörüne iletildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
            DosyaYukleyici.SilVarsa(_ortam.ContentRootPath, DosyaAltKlasoru, kayitliDosyaAdi);
        }

        return RedirectToAction(nameof(Index));
    }

    // Stajyer kendi teslim ettiği dosyayı tekrar indirebilir.
    public async Task<IActionResult> DosyaIndir(int id)
    {
        var kullaniciId = _userManager.GetUserId(User);
        var stajyer = kullaniciId is null ? null : await _stajyerService.GetByKullaniciIdAsync(kullaniciId);
        if (stajyer is null)
        {
            return NotFound();
        }

        var gorev = await _gorevService.GetByIdAsync(id);
        if (gorev is null || gorev.StajyerId != stajyer.Id || string.IsNullOrEmpty(gorev.TeslimDosyaAdi))
        {
            return NotFound();
        }

        var dosyaYolu = DosyaYukleyici.TamYol(_ortam.ContentRootPath, DosyaAltKlasoru, gorev.TeslimDosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
        {
            return NotFound();
        }

        return PhysicalFile(dosyaYolu, "application/octet-stream",
            gorev.TeslimDosyaOrijinalAdi ?? gorev.TeslimDosyaAdi);
    }
}
