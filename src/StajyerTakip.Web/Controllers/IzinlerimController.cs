using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Stajyer)]
public class IzinlerimController : Controller
{
    private readonly IIzinService _izinService;
    private readonly IStajyerService _stajyerService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IzinlerimController(
        IIzinService izinService, IStajyerService stajyerService, UserManager<ApplicationUser> userManager)
    {
        _izinService = izinService;
        _stajyerService = stajyerService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var stajyer = await BenimStajyerimAsync();
        ViewBag.ProfilYok = stajyer is null;
        if (stajyer is null)
        {
            return View(new List<Izin>());
        }

        var izinler = await _izinService.GetByStajyerIdAsync(stajyer.Id);
        return View(izinler.OrderByDescending(i => i.BaslangicTarihi).ToList());
    }

    public async Task<IActionResult> Create()
    {
        var stajyer = await BenimStajyerimAsync();
        if (stajyer is null)
        {
            TempData["HataMesaji"] = "İzin talebi oluşturabilmen için önce bir Stajyer profilin olması gerekiyor. Yöneticinle iletişime geç.";
            return RedirectToAction(nameof(Index));
        }

        return View(new IzinCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IzinCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullaniciId = _userManager.GetUserId(User);
        if (kullaniciId is null)
        {
            ModelState.AddModelError(string.Empty, "Oturum bilgisi okunamadı.");
            return View(model);
        }

        try
        {
            await _izinService.CreateAsync(kullaniciId, model.BaslangicTarihi, model.BitisTarihi, model.Aciklama);
            TempData["BilgiMesaji"] = "İzin talebin mentörüne gönderildi.";
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
