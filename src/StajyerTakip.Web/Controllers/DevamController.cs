using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Rapor gereği Mentör yalnızca KENDİ stajyerlerinin devam kayıtlarını
// görür ve onaylar; Yönetici tümünü görür.
[Authorize(Roles = Roller.Yonetici + "," + Roller.Mentor)]
public class DevamController : Controller
{
    private readonly IDevamService _devamService;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DevamController(
        IDevamService devamService,
        IStajyerService stajyerService,
        IMentorService mentorService,
        UserManager<ApplicationUser> userManager)
    {
        _devamService = devamService;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var kayitlar = await _devamService.GetAllAsync();

        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is not null)
        {
            kayitlar = kayitlar.Where(d => d.Stajyer.MentorId == girenMentor.Id).ToList();
        }

        var siraliKayitlar = kayitlar
            .OrderBy(d => d.OnayDurumu == OnayDurumu.Bekliyor ? 0 : 1)
            .ThenByDescending(d => d.Tarih)
            .ToList();

        return View(siraliKayitlar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(int id)
    {
        if (!await BuKayitBanaMiAitAsync(id))
        {
            return Forbid();
        }

        await _devamService.OnaylaAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reddet(int id)
    {
        if (!await BuKayitBanaMiAitAsync(id))
        {
            return Forbid();
        }

        await _devamService.ReddetAsync(id);
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

    private async Task<bool> BuKayitBanaMiAitAsync(int devamId)
    {
        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is null)
        {
            return true;
        }

        var devam = await _devamService.GetByIdAsync(devamId);
        if (devam is null)
        {
            return false;
        }

        var stajyer = await _stajyerService.GetByIdAsync(devam.StajyerId);
        return stajyer is not null && stajyer.MentorId == girenMentor.Id;
    }
}
