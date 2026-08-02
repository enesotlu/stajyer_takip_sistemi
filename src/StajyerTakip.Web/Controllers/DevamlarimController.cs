using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Stajyer)]
public class DevamlarimController : Controller
{
    private readonly IDevamService _devamService;
    private readonly IStajyerService _stajyerService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DevamlarimController(
        IDevamService devamService, IStajyerService stajyerService, UserManager<ApplicationUser> userManager)
    {
        _devamService = devamService;
        _stajyerService = stajyerService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var stajyer = await BenimStajyerimAsync();
        ViewBag.ProfilYok = stajyer is null;
        if (stajyer is null)
        {
            return View(new List<Business.Models.GunlukDevamDurumu>());
        }

        var takvim = await _devamService.GetTumDonemTakvimAsync(stajyer.Id);
        var ozet = await _devamService.GetTumDonemOzetiAsync(stajyer.Id);
        ViewBag.DonemOzeti = ozet;
        ViewBag.Stajyer = stajyer;

        return View(takvim.OrderByDescending(g => g.Tarih).ToList());
    }

    private async Task<Stajyer?> BenimStajyerimAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _stajyerService.GetByKullaniciIdAsync(kullaniciId);
    }
}
