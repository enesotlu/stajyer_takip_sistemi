using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

// Yönetici yalnızca Mentör başvurularını onaylar/reddeder.
// Stajyer başvuruları → StajyerBasvuruController (Mentör görür).
[Authorize(Roles = Roller.Yonetici)]
public class OnayBekleyenlerController : Controller
{
    private readonly IKullaniciYonetimService _kullaniciYonetimService;
    private readonly IMentorService _mentorService;
    private readonly IDepartmanService _departmanService;

    public OnayBekleyenlerController(
        IKullaniciYonetimService kullaniciYonetimService,
        IMentorService mentorService,
        IDepartmanService departmanService)
    {
        _kullaniciYonetimService = kullaniciYonetimService;
        _mentorService = mentorService;
        _departmanService = departmanService;
    }

    // Mentör başvurularını listeler.
    public async Task<IActionResult> Index()
    {
        var bekleyenler = await _kullaniciYonetimService.GetMentorBekleyenlerAsync();

        // Listede departman Id'si değil adı görünsün diye Id→Ad sözlüğü.
        var departmanlar = await _departmanService.GetAllAsync();
        ViewBag.DepartmanAdlari = departmanlar.ToDictionary(d => d.Id, d => d.Ad);

        return View(bekleyenler);
    }

    // Mentör onaylama: unvan + departman girme formu.
    public async Task<IActionResult> MentorOnayla(string id)
    {
        var bekleyenler = await _kullaniciYonetimService.GetMentorBekleyenlerAsync();
        var kullanici = bekleyenler.SingleOrDefault(k => k.Id == id);
        if (kullanici is null)
        {
            return NotFound();
        }

        await PopulateDepartmanListesiAsync(kullanici.TalepEdilenDepartmanId);

        return View(new MentorAtaViewModel
        {
            KullaniciId = kullanici.Id,
            AdSoyad = kullanici.AdSoyad,
            Email = kullanici.Email ?? string.Empty,
            DepartmanId = kullanici.TalepEdilenDepartmanId ?? 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MentorOnayla(MentorAtaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmanListesiAsync(model.DepartmanId);
            return View(model);
        }

        try
        {
            await _mentorService.AtaAsync(model.KullaniciId, model.Unvan, model.DepartmanId);
            TempData["BilgiMesaji"] = $"{model.AdSoyad} başarıyla Mentör olarak onaylandı.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDepartmanListesiAsync(model.DepartmanId);
            return View(model);
        }
    }

    // Başvuruyu reddet.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reddet(string id)
    {
        try
        {
            await _kullaniciYonetimService.ReddetAsync(id);
            TempData["BilgiMesaji"] = "Başvuru reddedildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDepartmanListesiAsync(int? seciliId = null)
    {
        var departmanlar = await _departmanService.GetAllAsync();
        ViewBag.DepartmanId = new SelectList(departmanlar, "Id", "Ad", seciliId);
    }
}
