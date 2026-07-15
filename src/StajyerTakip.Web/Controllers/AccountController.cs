using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmanService _departmanService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IDepartmanService departmanService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _departmanService = departmanService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sonuc = await _signInManager.PasswordSignInAsync(
            model.Email, model.Sifre, model.BeniHatirla, lockoutOnFailure: true);

        if (sonuc.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (sonuc.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız kilitli veya pasifleştirilmiş durumda. Yöneticinizle iletişime geçin.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        await PopulateDepartmanListesiAsync();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmanListesiAsync(model.TalepEdilenDepartmanId);
            return View(model);
        }

        // Sadece geçerli rol talepleri kabul edilir.
        if (model.TalepEdilenRol != Roller.Mentor && model.TalepEdilenRol != Roller.Stajyer)
        {
            ModelState.AddModelError(nameof(model.TalepEdilenRol), "Geçersiz rol seçimi.");
            await PopulateDepartmanListesiAsync(model.TalepEdilenDepartmanId);
            return View(model);
        }

        var kullanici = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            AdSoyad = model.AdSoyad,
            EmailConfirmed = true,
            KayitTarihi = DateTime.UtcNow,
            TalepEdilenRol = model.TalepEdilenRol,
            TalepEdilenDepartmanId = model.TalepEdilenDepartmanId,
            OnayDurumu = OnayDurumlari.Bekliyor
        };

        var sonuc = await _userManager.CreateAsync(kullanici, model.Sifre);

        if (sonuc.Succeeded)
        {
            // Bilinçli olarak rol atamıyoruz: hesap "onay bekliyor" durumunda kalır.
            // Mentör başvurusu → Yönetici onaylar.
            // Stajyer başvurusu → Aynı departmandaki Mentör(ler) onaylar.
            await _signInManager.SignInAsync(kullanici, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var hata in sonuc.Errors)
        {
            ModelState.AddModelError(string.Empty, hata.Description);
        }

        await PopulateDepartmanListesiAsync(model.TalepEdilenDepartmanId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task PopulateDepartmanListesiAsync(int? seciliId = null)
    {
        var departmanlar = await _departmanService.GetAllAsync();
        ViewBag.DepartmanListesi = new SelectList(departmanlar, "Id", "Ad", seciliId);
    }
}
