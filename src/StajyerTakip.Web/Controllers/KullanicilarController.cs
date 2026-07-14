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
    private readonly SignInManager<ApplicationUser> _signInManager;

    public KullanicilarController(
        IKullaniciYonetimService kullaniciYonetimService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _kullaniciYonetimService = kullaniciYonetimService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index()
    {
        var kullanicilar = await _kullaniciYonetimService.GetTumKullanicilarAsync();
        ViewBag.BenimId = _userManager.GetUserId(User);
        return View(kullanicilar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YoneticiDevret(string id)
    {
        try
        {
            var benimId = _userManager.GetUserId(User) ?? string.Empty;
            await _kullaniciYonetimService.YoneticiDevretAsync(id, benimId);

            // Devir tamamlandı: bu hesabın rolü alındı ve kilitlendi,
            // açık oturumu da hemen sonlandırıyoruz.
            await _signInManager.SignOutAsync();
            TempData["BasariMesaji"] = "Yönetici yetkisi devredildi ve hesabın kapatıldı. Yeni Yönetici artık giriş yapabilir.";
            return RedirectToAction("Login", "Account");
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
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
