using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Stajyer)]
public class GorevlerimController : Controller
{
    private readonly IGorevService _gorevService;
    private readonly IStajyerService _stajyerService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GorevlerimController(
        IGorevService gorevService, IStajyerService stajyerService, UserManager<ApplicationUser> userManager)
    {
        _gorevService = gorevService;
        _stajyerService = stajyerService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var kullaniciId = _userManager.GetUserId(User);
        var stajyer = kullaniciId is null ? null : await _stajyerService.GetByKullaniciIdAsync(kullaniciId);
        if (stajyer is null)
        {
            return View(new List<Gorev>());
        }

        var gorevler = await _gorevService.GetByStajyerIdAsync(stajyer.Id);
        return View(gorevler);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumGuncelle(int id, GorevDurumu yeniDurum)
    {
        var kullaniciId = _userManager.GetUserId(User);
        if (kullaniciId is not null)
        {
            try
            {
                await _gorevService.StajyerDurumGuncelleAsync(id, kullaniciId, yeniDurum);
            }
            catch (InvalidOperationException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
