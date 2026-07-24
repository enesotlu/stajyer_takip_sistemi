using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Stajyer)]
public class ToplantilarimController : Controller
{
    private readonly IToplantiService _toplantiService;
    private readonly IStajyerService _stajyerService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ToplantilarimController(
        IToplantiService toplantiService, IStajyerService stajyerService, UserManager<ApplicationUser> userManager)
    {
        _toplantiService = toplantiService;
        _stajyerService = stajyerService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var stajyer = await BenimStajyerimAsync();
        ViewBag.ProfilYok = stajyer is null;
        if (stajyer is null)
        {
            return View(new List<ToplantiKatilimi>());
        }

        var katilimlar = await _toplantiService.GetByStajyerAsync(stajyer.Id);
        await _toplantiService.StajyerGorduIsaretleAsync(stajyer.Id);
        return View(katilimlar.OrderByDescending(k => k.Toplanti.Tarih).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KabulEt(int id)
    {
        if (!await BuKatilimBanaMiAitAsync(id))
        {
            return Forbid();
        }

        await _toplantiService.KabulEtAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reddet(int id, string sebep)
    {
        if (!await BuKatilimBanaMiAitAsync(id))
        {
            return Forbid();
        }

        try
        {
            await _toplantiService.ReddetAsync(id, sebep);
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Stajyer?> BenimStajyerimAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _stajyerService.GetByKullaniciIdAsync(kullaniciId);
    }

    private async Task<bool> BuKatilimBanaMiAitAsync(int katilimId)
    {
        var stajyer = await BenimStajyerimAsync();
        if (stajyer is null)
        {
            return false;
        }

        var katilim = await _toplantiService.GetKatilimByIdAsync(katilimId);
        return katilim is not null && katilim.StajyerId == stajyer.Id;
    }
}
