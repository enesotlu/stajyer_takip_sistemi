using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

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
            return View(new List<Devam>());
        }

        var kayitlar = await _devamService.GetByStajyerIdAsync(stajyer.Id);
        var ozet = await _devamService.GetAylikOzetAsync(stajyer.Id, DateTime.Today.Year, DateTime.Today.Month);
        ViewBag.AylikOzet = ozet;

        return View(kayitlar.OrderByDescending(d => d.Tarih).ToList());
    }

    public async Task<IActionResult> Create()
    {
        var stajyer = await BenimStajyerimAsync();
        if (stajyer is null)
        {
            TempData["HataMesaji"] = "Devam kaydı girebilmen için önce bir Stajyer profilin olması gerekiyor. Yöneticinle iletişime geç.";
            return RedirectToAction(nameof(Index));
        }

        return View(new DevamCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DevamCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullaniciId = _userManager.GetUserId(User);
        if (kullaniciId is null ||
            !TimeSpan.TryParse(model.GirisSaati, out var giris) ||
            !TimeSpan.TryParse(model.CikisSaati, out var cikis))
        {
            ModelState.AddModelError(string.Empty, "Saat bilgileri okunamadı.");
            return View(model);
        }

        try
        {
            await _devamService.CreateAsync(kullaniciId, model.Tarih, giris, cikis);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private async Task<Stajyer?> BenimStajyerimAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _stajyerService.GetByKullaniciIdAsync(kullaniciId);
    }
}
