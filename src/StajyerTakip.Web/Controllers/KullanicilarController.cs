using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Yönetici'nin tüm hesapları görüp yetki devri (Yönetici Yap) ve
// pasifleştirme/aktifleştirme yapabildiği ekran.
[Authorize(Roles = Roller.Yonetici)]
public class KullanicilarController : Controller
{
    private readonly IKullaniciYonetimService _kullaniciYonetimService;
    private readonly UserManager<ApplicationUser> _userManager;

    public KullanicilarController(
        IKullaniciYonetimService kullaniciYonetimService,
        UserManager<ApplicationUser> userManager)
    {
        _kullaniciYonetimService = kullaniciYonetimService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var kullanicilar = await _kullaniciYonetimService.GetTumKullanicilarAsync();
        ViewBag.BenimId = _userManager.GetUserId(User);
        return View(kullanicilar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YoneticiYap(string id)
    {
        try
        {
            await _kullaniciYonetimService.YoneticiYapAsync(id);
            TempData["BasariMesaji"] = "Kullanıcı Yönetici yapıldı. Yeni yetkileri bir sonraki girişinde etkinleşir.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pasiflestir(string id)
    {
        try
        {
            var benimId = _userManager.GetUserId(User) ?? string.Empty;
            await _kullaniciYonetimService.PasiflestirAsync(id, benimId);
            TempData["BasariMesaji"] = "Hesap pasifleştirildi. Kullanıcı artık giriş yapamaz.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aktiflestir(string id)
    {
        try
        {
            await _kullaniciYonetimService.AktiflestirAsync(id);
            TempData["BasariMesaji"] = "Hesap yeniden aktifleştirildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
