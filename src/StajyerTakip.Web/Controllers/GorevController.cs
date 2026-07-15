using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Görev atama ve takibi Mentör/Yönetici tarafında; stajyerin kendi
// görevlerini görüp durum güncellemesi GorevlerimController'da.
// Rapor gereği Mentör yalnızca KENDİ stajyerlerinin görevlerini yönetir.
[Authorize(Roles = Roller.Mentor)]
public class GorevController : Controller
{
    private readonly IGorevService _gorevService;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GorevController(
        IGorevService gorevService,
        IStajyerService stajyerService,
        IMentorService mentorService,
        UserManager<ApplicationUser> userManager)
    {
        _gorevService = gorevService;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var gorevler = await _gorevService.GetAllAsync();

        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is not null)
        {
            gorevler = gorevler.Where(g => g.Stajyer.MentorId == girenMentor.Id).ToList();
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
    public async Task<IActionResult> Create(Gorev gorev)
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

        await _gorevService.CreateAsync(gorev);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeriGonder(int id)
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
            await _gorevService.MentorGeriGonderAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
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
