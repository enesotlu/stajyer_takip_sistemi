using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Helpers;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

// Mentörün kendi stajyerlerinden talepte bulunduğu ekran
// ("CV gönder", "şu tarihte mülakata gel" vb.).
[Authorize(Roles = Roller.Mentor)]
public class TalepController : Controller
{
    private readonly ITalepService _talepService;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _ortam;

    public TalepController(
        ITalepService talepService,
        IStajyerService stajyerService,
        IMentorService mentorService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment ortam)
    {
        _talepService = talepService;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _userManager = userManager;
        _ortam = ortam;
    }

    public async Task<IActionResult> Index()
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return NotFound("Mentör profiliniz bulunamadı.");
        }

        var talepler = await _talepService.GetByMentorAsync(mentor.Id);
        await _talepService.MentorGorduIsaretleAsync(mentor.Id);
        return View(talepler);
    }

    public async Task<IActionResult> Create()
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return NotFound();
        }

        await PopulateStajyerListesiAsync(mentor.Id);
        return View(new TalepCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TalepCreateViewModel model)
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateStajyerListesiAsync(mentor.Id, model.StajyerId);
            return View(model);
        }

        try
        {
            await _talepService.CreateAsync(
                mentor.Id, model.StajyerId, model.Baslik, model.Aciklama, model.DosyaIstensin, model.SonTarih!.Value);
            TempData["BilgiMesaji"] = "Talep oluşturuldu; stajyerin ekranına bildirim olarak düştü.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateStajyerListesiAsync(mentor.Id, model.StajyerId);
            return View(model);
        }
    }

    // Stajyerin yüklediği cevap dosyasını indirir (yalnızca talebin sahibi mentör).
    public async Task<IActionResult> DosyaIndir(int id)
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return NotFound();
        }

        var talep = await _talepService.GetByIdAsync(id);
        if (talep is null || string.IsNullOrEmpty(talep.CevapDosyaAdi))
        {
            return NotFound();
        }

        var stajyer = await _stajyerService.GetByIdAsync(talep.StajyerId);
        if (stajyer is null || stajyer.MentorId != mentor.Id)
        {
            return Forbid();
        }

        var dosyaYolu = Path.Combine(_ortam.ContentRootPath, "Uploads", "Talepler", talep.CevapDosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
        {
            return NotFound();
        }

        return PhysicalFile(dosyaYolu, "application/octet-stream",
            talep.CevapDosyaOrijinalAdi ?? talep.CevapDosyaAdi);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var talep = await _talepService.GetByIdAsync(id);
        if (talep is null)
        {
            return NotFound();
        }

        if (!await BuTalepBanaMiAitAsync(talep))
        {
            return Forbid();
        }

        var mentor = await GirisYapanMentorAsync();
        await PopulateStajyerListesiAsync(mentor!.Id);
        ViewBag.StajyerAdi = (await _stajyerService.GetByIdWithDetailsAsync(talep.StajyerId))?.Kullanici.AdSoyad ?? "—";

        return View(talep);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string baslik, string? aciklama, bool dosyaIstensin, DateTime sonTarih)
    {
        var talep = await _talepService.GetByIdAsync(id);
        if (talep is null)
        {
            return NotFound();
        }

        if (!await BuTalepBanaMiAitAsync(talep))
        {
            return Forbid();
        }

        try
        {
            await _talepService.UpdateAsync(id, baslik, aciklama, dosyaIstensin, sonTarih);
            TempData["BilgiMesaji"] = "Talep güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            talep.Baslik = baslik;
            talep.Aciklama = aciklama;
            talep.DosyaIstensin = dosyaIstensin;
            talep.SonTarih = sonTarih;

            ViewBag.StajyerAdi = (await _stajyerService.GetByIdWithDetailsAsync(talep.StajyerId))?.Kullanici.AdSoyad ?? "—";
            return View(talep);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var talep = await _talepService.GetByIdAsync(id);
        if (talep is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await BuTalepBanaMiAitAsync(talep))
        {
            return Forbid();
        }

        await _talepService.DeleteAsync(id);
        TempData["BilgiMesaji"] = "Talep silindi.";
        return RedirectToAction(nameof(Index));
    }

    // Mentör, stajyerin cevabini yetersiz bulup geri gönderir; yeni son tarih zorunludur.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeriGonder(int id, string? mentorNotu, DateTime yeniSonTarih)
    {
        var talep = await _talepService.GetByIdAsync(id);
        if (talep is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await BuTalepBanaMiAitAsync(talep))
        {
            return Forbid();
        }

        try
        {
            var eskiDosyaAdi = await _talepService.MentorGeriGonderAsync(id, mentorNotu, yeniSonTarih);
            DosyaYukleyici.SilVarsa(_ortam.ContentRootPath, "Talepler", eskiDosyaAdi);
            TempData["BilgiMesaji"] = "Talep geri gönderildi, stajyer yeniden cevaplayabilir.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> BuTalepBanaMiAitAsync(Talep talep)
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return false;
        }

        var stajyer = await _stajyerService.GetByIdAsync(talep.StajyerId);
        return stajyer is not null && stajyer.MentorId == mentor.Id;
    }

    private async Task<Mentor?> GirisYapanMentorAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _mentorService.GetByKullaniciIdAsync(kullaniciId);
    }

    private async Task PopulateStajyerListesiAsync(int mentorId, int? seciliId = null)
    {
        var stajyerler = (await _stajyerService.GetAllAsync())
            .Where(s => s.MentorId == mentorId)
            .Select(s => new { s.Id, Ad = $"{s.Kullanici.AdSoyad} ({s.Okul})" });

        ViewBag.StajyerId = new SelectList(stajyerler, "Id", "Ad", seciliId);
    }
}
