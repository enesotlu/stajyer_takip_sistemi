using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Mentör kendi stajyerlerinin izin taleplerini onaylar/reddeder.
[Authorize(Roles = Roller.Mentor)]
public class IzinController : Controller
{
    private readonly IIzinService _izinService;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IzinController(
        IIzinService izinService,
        IStajyerService stajyerService,
        IMentorService mentorService,
        UserManager<ApplicationUser> userManager)
    {
        _izinService = izinService;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var izinler = await _izinService.GetAllAsync();

        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is not null)
        {
            izinler = izinler.Where(i => i.Stajyer.MentorId == girenMentor.Id).ToList();
            await _izinService.MentorGorduIsaretleAsync(girenMentor.Id);
        }

        var siraliIzinler = izinler
            .OrderBy(i => i.OnayDurumu == OnayDurumu.Bekliyor ? 0 : 1)
            .ThenByDescending(i => i.BaslangicTarihi)
            .ToList();

        return View(siraliIzinler);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(int id)
    {
        if (!await BuIzinBanaMiAitAsync(id))
        {
            return Forbid();
        }

        await _izinService.OnaylaAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reddet(int id, string? mentorNotu)
    {
        if (!await BuIzinBanaMiAitAsync(id))
        {
            return Forbid();
        }

        await _izinService.ReddetAsync(id, mentorNotu);
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

    private async Task<bool> BuIzinBanaMiAitAsync(int izinId)
    {
        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is null)
        {
            return true;
        }

        var izin = await _izinService.GetByIdAsync(izinId);
        if (izin is null)
        {
            return false;
        }

        var stajyer = await _stajyerService.GetByIdAsync(izin.StajyerId);
        return stajyer is not null && stajyer.MentorId == girenMentor.Id;
    }
}
