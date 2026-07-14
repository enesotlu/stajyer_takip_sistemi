using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

// Kayıt olmuş ama henüz Mentör/Stajyer rolü atanmamış kullanıcıları
// Yönetici'nin onaylayıp rol vermesi için.
[Authorize(Roles = Roller.Yonetici)]
public class OnayBekleyenlerController : Controller
{
    private readonly IKullaniciYonetimService _kullaniciYonetimService;
    private readonly IMentorService _mentorService;
    private readonly IStajyerService _stajyerService;
    private readonly IDepartmanService _departmanService;

    public OnayBekleyenlerController(
        IKullaniciYonetimService kullaniciYonetimService,
        IMentorService mentorService,
        IStajyerService stajyerService,
        IDepartmanService departmanService)
    {
        _kullaniciYonetimService = kullaniciYonetimService;
        _mentorService = mentorService;
        _stajyerService = stajyerService;
        _departmanService = departmanService;
    }

    public async Task<IActionResult> Index()
    {
        var bekleyenler = await _kullaniciYonetimService.GetBekleyenlerAsync();
        return View(bekleyenler);
    }

    public async Task<IActionResult> MentorYap(string id)
    {
        var kullanici = (await _kullaniciYonetimService.GetBekleyenlerAsync())
            .SingleOrDefault(k => k.Id == id);
        if (kullanici is null)
        {
            return NotFound();
        }

        await PopulateDepartmanListesiAsync();
        return View(new MentorAtaViewModel
        {
            KullaniciId = kullanici.Id,
            AdSoyad = kullanici.AdSoyad,
            Email = kullanici.Email ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MentorYap(MentorAtaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmanListesiAsync(model.DepartmanId);
            return View(model);
        }

        try
        {
            await _mentorService.AtaAsync(model.KullaniciId, model.Unvan, model.DepartmanId);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDepartmanListesiAsync(model.DepartmanId);
            return View(model);
        }
    }

    public async Task<IActionResult> StajyerYap(string id)
    {
        var kullanici = (await _kullaniciYonetimService.GetBekleyenlerAsync())
            .SingleOrDefault(k => k.Id == id);
        if (kullanici is null)
        {
            return NotFound();
        }

        await PopulateDropdownlarAsync();
        return View(new StajyerAtaViewModel
        {
            KullaniciId = kullanici.Id,
            AdSoyad = kullanici.AdSoyad,
            Email = kullanici.Email ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StajyerYap(StajyerAtaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownlarAsync(model.MentorId, model.DepartmanId);
            return View(model);
        }

        try
        {
            await _stajyerService.AtaAsync(
                model.KullaniciId, model.Okul, model.Bolum, model.BaslangicTarihi, model.BitisTarihi,
                model.MentorId, model.DepartmanId);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownlarAsync(model.MentorId, model.DepartmanId);
            return View(model);
        }
    }

    private async Task PopulateDepartmanListesiAsync(int? seciliId = null)
    {
        var departmanlar = await _departmanService.GetAllAsync();
        ViewBag.DepartmanId = new SelectList(departmanlar, "Id", "Ad", seciliId);
    }

    private async Task PopulateDropdownlarAsync(int? seciliMentorId = null, int? seciliDepartmanId = null)
    {
        var mentorler = await _mentorService.GetAllAsync();
        var departmanlar = await _departmanService.GetAllAsync();

        var mentorSecenekleri = mentorler.Select(m => new { m.Id, Ad = $"{m.Kullanici.AdSoyad} ({m.Unvan})" });
        ViewBag.MentorId = new SelectList(mentorSecenekleri, "Id", "Ad", seciliMentorId);
        ViewBag.DepartmanId = new SelectList(departmanlar, "Id", "Ad", seciliDepartmanId);
    }
}
