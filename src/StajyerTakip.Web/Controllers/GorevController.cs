using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Helpers;

namespace StajyerTakip.Web.Controllers;

// Görev atama ve takibi Mentör/Yönetici tarafında; stajyerin kendi
// görevlerini görüp durum güncellemesi GorevlerimController'da.
// Rapor gereği Mentör yalnızca KENDİ stajyerlerinin görevlerini yönetir.
[Authorize(Roles = Roller.Mentor)]
public class GorevController : Controller
{
    private const string DosyaAltKlasoru = "Gorevler";

    private readonly IGorevService _gorevService;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _ortam;

    public GorevController(
        IGorevService gorevService,
        IStajyerService stajyerService,
        IMentorService mentorService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment ortam)
    {
        _gorevService = gorevService;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _userManager = userManager;
        _ortam = ortam;
    }

    public async Task<IActionResult> Index()
    {
        var gorevler = await _gorevService.GetAllAsync();

        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is not null)
        {
            gorevler = gorevler.Where(g => g.Stajyer.MentorId == girenMentor.Id).ToList();
            await _gorevService.MentorGorduIsaretleAsync(girenMentor.Id);
        }

        return View(gorevler);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateStajyerListesiAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Gorev gorev, IFormFile? ekDosya)
    {
        if (!await BuStajyerBanaMiAitAsync(gorev.StajyerId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await PopulateStajyerListesiAsync(gorev.StajyerId);
            return View(gorev);
        }

        string? kayitliDosyaAdi = null;
        string? orijinalDosyaAdi = null;

        try
        {
            if (ekDosya is not null && ekDosya.Length > 0)
            {
                (kayitliDosyaAdi, orijinalDosyaAdi) =
                    await DosyaYukleyici.KaydetAsync(ekDosya, _ortam.ContentRootPath, DosyaAltKlasoru);
            }

            gorev.EkDosyaAdi = kayitliDosyaAdi;
            gorev.EkDosyaOrijinalAdi = orijinalDosyaAdi;

            await _gorevService.CreateAsync(gorev);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            DosyaYukleyici.SilVarsa(_ortam.ContentRootPath, DosyaAltKlasoru, kayitliDosyaAdi);
            ModelState.AddModelError(nameof(Gorev.TeslimTarihi), ex.Message);
            await PopulateStajyerListesiAsync(gorev.StajyerId);
            return View(gorev);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var gorev = await _gorevService.GetByIdAsync(id);
        if (gorev is null)
        {
            return NotFound();
        }

        if (!await BuStajyerBanaMiAitAsync(gorev.StajyerId))
        {
            return Forbid();
        }

        // GetByIdAsync navigasyon yuklemez; stajyer adini basliktabgostermek icin ayrica cekilir.
        var stajyer = await _stajyerService.GetByIdWithDetailsAsync(gorev.StajyerId);
        ViewBag.StajyerAdi = stajyer?.Kullanici.AdSoyad ?? "—";

        return View(gorev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string baslik, string? aciklama, DateTime teslimTarihi)
    {
        var gorev = await _gorevService.GetByIdAsync(id);
        if (gorev is null)
        {
            return NotFound();
        }

        if (!await BuStajyerBanaMiAitAsync(gorev.StajyerId))
        {
            return Forbid();
        }

        try
        {
            await _gorevService.UpdateAsync(id, baslik, aciklama, teslimTarihi);
            TempData["BilgiMesaji"] = "Görev güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            gorev.Baslik = baslik;
            gorev.Aciklama = aciklama;
            gorev.TeslimTarihi = teslimTarihi;

            var stajyer = await _stajyerService.GetByIdWithDetailsAsync(gorev.StajyerId);
            ViewBag.StajyerAdi = stajyer?.Kullanici.AdSoyad ?? "—";
            return View(gorev);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeriGonder(int id, string? mentorNotu)
    {
        var gorev = await _gorevService.GetByIdAsync(id);
        if (gorev is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await BuStajyerBanaMiAitAsync(gorev.StajyerId))
        {
            return Forbid();
        }

        try
        {
            var eskiDosyaAdi = await _gorevService.MentorGeriGonderAsync(id, mentorNotu);
            DosyaYukleyici.SilVarsa(_ortam.ContentRootPath, DosyaAltKlasoru, eskiDosyaAdi);
            TempData["BilgiMesaji"] = "Görev geri gönderildi, stajyer yeniden teslim edebilir.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Mentör, stajyerin teslim ettiği dosyayı indirir.
    public async Task<IActionResult> DosyaIndir(int id)
    {
        var gorev = await _gorevService.GetByIdAsync(id);
        if (gorev is null || string.IsNullOrEmpty(gorev.TeslimDosyaAdi))
        {
            return NotFound();
        }

        if (!await BuStajyerBanaMiAitAsync(gorev.StajyerId))
        {
            return Forbid();
        }

        var dosyaYolu = DosyaYukleyici.TamYol(_ortam.ContentRootPath, DosyaAltKlasoru, gorev.TeslimDosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
        {
            return NotFound();
        }

        return PhysicalFile(dosyaYolu, "application/octet-stream",
            gorev.TeslimDosyaOrijinalAdi ?? gorev.TeslimDosyaAdi);
    }

    // Mentör kendi eklediği belgeyi tekrar indirebilir.
    public async Task<IActionResult> EkDosyaIndir(int id)
    {
        var gorev = await _gorevService.GetByIdAsync(id);
        if (gorev is null || string.IsNullOrEmpty(gorev.EkDosyaAdi))
        {
            return NotFound();
        }

        if (!await BuStajyerBanaMiAitAsync(gorev.StajyerId))
        {
            return Forbid();
        }

        var dosyaYolu = DosyaYukleyici.TamYol(_ortam.ContentRootPath, DosyaAltKlasoru, gorev.EkDosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
        {
            return NotFound();
        }

        return PhysicalFile(dosyaYolu, "application/octet-stream",
            gorev.EkDosyaOrijinalAdi ?? gorev.EkDosyaAdi);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var gorev = await _gorevService.GetByIdAsync(id);
        if (gorev is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await BuStajyerBanaMiAitAsync(gorev.StajyerId))
        {
            return Forbid();
        }

        await _gorevService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<Mentor?> GirisYapanMentorAsync()
    {
        if (User.IsInRole(Roller.Yonetici))
        {
            return null;
        }

        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _mentorService.GetByKullaniciIdAsync(kullaniciId);
    }

    private async Task<bool> BuStajyerBanaMiAitAsync(int stajyerId)
    {
        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is null)
        {
            return true;
        }

        var stajyer = await _stajyerService.GetByIdAsync(stajyerId);
        return stajyer is not null && stajyer.MentorId == girenMentor.Id;
    }

    private async Task PopulateStajyerListesiAsync(int? seciliId = null)
    {
        var stajyerler = await _stajyerService.GetAllAsync();

        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is not null)
        {
            stajyerler = stajyerler.Where(s => s.MentorId == girenMentor.Id).ToList();
        }

        var secenekler = stajyerler.Select(s => new { s.Id, Ad = $"{s.Kullanici.AdSoyad} ({s.Okul})" });
        ViewBag.StajyerId = new SelectList(secenekler, "Id", "Ad", seciliId);
    }
}
