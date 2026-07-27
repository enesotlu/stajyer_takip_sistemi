using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmanService _departmanService;
    private readonly IStajyerService _stajyerService;
    private readonly IDevamService _devamService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AccountController> _logger;

    // Kayıt doğrulama kodunun geçerlilik süresi.
    private static readonly TimeSpan DogrulamaKoduGecerlilikSuresi = TimeSpan.FromMinutes(15);

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IDepartmanService departmanService,
        IStajyerService stajyerService,
        IDevamService devamService,
        IEmailSender emailSender,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _departmanService = departmanService;
        _stajyerService = stajyerService;
        _devamService = devamService;
        _emailSender = emailSender;
        _logger = logger;
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
            // Stajı bitiş tarihini geçmiş bir stajyer artık giriş yapamaz.
            var girisYapanKullanici = await _userManager.FindByEmailAsync(model.Email);
            var stajyerProfili = girisYapanKullanici is null
                ? null
                : await _stajyerService.GetByKullaniciIdAsync(girisYapanKullanici.Id);

            if (stajyerProfili is not null && stajyerProfili.BitisTarihi.Date < DateTime.Today)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "Stajınız sona erdiği için sisteme giriş yapamazsınız.");
                return View(model);
            }

            // Stajyer icin konum sarti: Kulliye disindaysa ya da konum hic
            // gelmediyse giris reddedilir. Konum gecerliyse ayni cagri bugunun
            // devam kaydini da olusturur (bkz. IDevamService.OtomatikOlusturAsync).
            if (stajyerProfili is not null)
            {
                double? enlemDeger = double.TryParse(
                    model.Enlem, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var e) ? e : null;
                double? boylamDeger = double.TryParse(
                    model.Boylam, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var b) ? b : null;

                var konumSonucu = await _devamService.OtomatikOlusturAsync(girisYapanKullanici!.Id, enlemDeger, boylamDeger);
                if (!konumSonucu.Basarili)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, konumSonucu.Mesaj);
                    return View(model);
                }
            }

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
        else if (sonuc.IsNotAllowed)
        {
            // RequireConfirmedEmail=true nedeniyle e-postasını doğrulamamış kullanıcı
            // için PasswordSignInAsync başarısız döner (bkz. Program.cs Identity ayarları).
            TempData["HataMesaji"] = "E-posta adresini henüz doğrulamadın. Sana gönderdiğimiz kodu gir.";
            return RedirectToAction(nameof(EmailDogrula), new { email = model.Email });
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
            // E-postanın gerçek olduğunu doğrulamadan giriş yapamaz (bkz. EmailDogrula).
            EmailConfirmed = false,
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
            // Ama önce e-postasını doğrulamalı - oturum burada açılmıyor.
            await KoduOlusturVeGonderAsync(kullanici);
            return RedirectToAction(nameof(EmailDogrula), new { email = kullanici.Email });
        }

        foreach (var hata in sonuc.Errors)
        {
            ModelState.AddModelError(string.Empty, hata.Description);
        }

        await PopulateDepartmanListesiAsync(model.TalepEdilenDepartmanId);
        return View(model);
    }

    [HttpGet]
    public IActionResult EmailDogrula(string email)
    {
        return View(new EmailDogrulaViewModel { Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmailDogrula(EmailDogrulaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullanici = await _userManager.FindByEmailAsync(model.Email);
        if (kullanici is null || kullanici.EmailConfirmed)
        {
            // Zaten doğrulanmış ya da hesap yok - kod tahmin denemesine bilgi sızdırma.
            return RedirectToAction(nameof(Login));
        }

        var kodGecerli = kullanici.EmailDogrulamaKodu == model.Kod
            && kullanici.EmailDogrulamaKoduSonGecerlilik is not null
            && kullanici.EmailDogrulamaKoduSonGecerlilik.Value > DateTime.UtcNow;

        if (!kodGecerli)
        {
            ModelState.AddModelError(string.Empty, "Kod hatalı veya süresi dolmuş. Yeni kod isteyebilirsin.");
            return View(model);
        }

        kullanici.EmailConfirmed = true;
        kullanici.EmailDogrulamaKodu = null;
        kullanici.EmailDogrulamaKoduSonGecerlilik = null;
        await _userManager.UpdateAsync(kullanici);

        await _signInManager.SignInAsync(kullanici, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KoduTekrarGonder(string email)
    {
        var kullanici = await _userManager.FindByEmailAsync(email);
        if (kullanici is not null && !kullanici.EmailConfirmed)
        {
            await KoduOlusturVeGonderAsync(kullanici);
            TempData["BilgiMesaji"] = "Yeni kod gönderildi.";
        }

        return RedirectToAction(nameof(EmailDogrula), new { email });
    }

    private async Task KoduOlusturVeGonderAsync(ApplicationUser kullanici)
    {
        var kod = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        kullanici.EmailDogrulamaKodu = kod;
        kullanici.EmailDogrulamaKoduSonGecerlilik = DateTime.UtcNow.Add(DogrulamaKoduGecerlilikSuresi);
        await _userManager.UpdateAsync(kullanici);

        try
        {
            await _emailSender.GonderAsync(
                kullanici.Email!,
                "E-posta Doğrulama Kodun",
                $"<p>Stajyer Takip Sistemi'ne hoş geldin.</p>" +
                $"<p>E-postanı doğrulamak için kod: <strong style=\"font-size:20px\">{kod}</strong></p>" +
                $"<p>Bu kod {DogrulamaKoduGecerlilikSuresi.TotalMinutes:0} dakika geçerlidir.</p>");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Doğrulama kodu e-postası gönderilemedi: {Email}", kullanici.Email);
        }
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullanici = await _userManager.FindByEmailAsync(model.Email);
        if (kullanici is not null && kullanici.EmailConfirmed)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(kullanici);
            var link = Url.Action(
                nameof(ResetPassword), "Account",
                new { email = model.Email, token }, Request.Scheme);

            try
            {
                await _emailSender.GonderAsync(
                    model.Email,
                    "Şifre Sıfırlama",
                    $"<p>Şifreni sıfırlamak için <a href=\"{link}\">buraya tıkla</a>.</p>" +
                    "<p>Bu isteği sen yapmadıysan bu e-postayı yok sayabilirsin.</p>");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Şifre sıfırlama e-postası gönderilemedi: {Email}", model.Email);
            }
        }

        // Kullanıcının sistemde olup olmadığını sızdırmamak için her durumda aynı mesaj.
        TempData["BilgiMesaji"] = "Bu e-posta sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderildi.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullanici = await _userManager.FindByEmailAsync(model.Email);
        if (kullanici is null)
        {
            // Var olmayan kullanıcı için de başarılıymış gibi davran (numaralandırma saldırısını önlemek için).
            TempData["BilgiMesaji"] = "Şifren güncellendi, şimdi giriş yapabilirsin.";
            return RedirectToAction(nameof(Login));
        }

        var sonuc = await _userManager.ResetPasswordAsync(kullanici, model.Token, model.Sifre);
        if (sonuc.Succeeded)
        {
            TempData["BilgiMesaji"] = "Şifren güncellendi, şimdi giriş yapabilirsin.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var hata in sonuc.Errors)
        {
            ModelState.AddModelError(string.Empty, hata.Description);
        }

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
