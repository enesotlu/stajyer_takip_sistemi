using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

// Mentör, kendi departmanından gelen stajyer başvurularını onaylar veya reddeder.
// Onaylarken okul, bölüm, başlangıç/bitiş tarihi bilgilerini girer.
[Authorize(Roles = Roller.Mentor)]
public class StajyerBasvuruController : Controller
{
    private readonly IKullaniciYonetimService _kullaniciYonetimService;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StajyerBasvuruController(
        IKullaniciYonetimService kullaniciYonetimService,
        IStajyerService stajyerService,
        IMentorService mentorService,
        UserManager<ApplicationUser> userManager)
    {
        _kullaniciYonetimService = kullaniciYonetimService;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _userManager = userManager;
    }

    // Kendi departmanından gelen bekleyen stajyer başvurularını listeler.
    public async Task<IActionResult> Index()
    {
        var mentor = await GirisYapanMentorAlAsync();
        if (mentor is null)
        {
            return NotFound("Mentör profiliniz bulunamadı.");
        }

        var bekleyenler = await _kullaniciYonetimService.GetStajyerBekleyenlerByDepartmanAsync(mentor.DepartmanId);
        return View(bekleyenler);
    }

    // Onaylama formu: okul, bölüm, başlangıç/bitiş tarihi.
    public async Task<IActionResult> Onayla(string id)
    {
        var mentor = await GirisYapanMentorAlAsync();
        if (mentor is null) return NotFound();

        var bekleyenler = await _kullaniciYonetimService.GetStajyerBekleyenlerByDepartmanAsync(mentor.DepartmanId);
        var kullanici = bekleyenler.SingleOrDefault(k => k.Id == id);
        if (kullanici is null) return NotFound();

        return View(new StajyerAtaViewModel
        {
            KullaniciId = kullanici.Id,
            AdSoyad = kullanici.AdSoyad,
            Email = kullanici.Email ?? string.Empty,
            MentorId = mentor.Id,
            DepartmanId = mentor.DepartmanId,
            BaslangicTarihi = DateTime.Today,
            BitisTarihi = DateTime.Today.AddMonths(3)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(StajyerAtaViewModel model)
    {
        var mentor = await GirisYapanMentorAlAsync();
        if (mentor is null) return NotFound();

        // Onaylayan mentör kendisi olur; form'dan gelen MentorId değeri dikkate alınmaz.
        model.MentorId = mentor.Id;
        model.DepartmanId = mentor.DepartmanId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _stajyerService.AtaAsync(
                model.KullaniciId, model.Okul, model.Bolum,
                model.BaslangicTarihi, model.BitisTarihi,
                model.MentorId, model.DepartmanId);

            TempData["BilgiMesaji"] = $"{model.AdSoyad} başarıyla Stajyer olarak onaylandı.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // Başvuruyu reddet.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reddet(string id)
    {
        var mentor = await GirisYapanMentorAlAsync();
        if (mentor is null) return NotFound();

        // Sadece kendi departmanındaki başvuruları reddedebilir.
        var bekleyenler = await _kullaniciYonetimService.GetStajyerBekleyenlerByDepartmanAsync(mentor.DepartmanId);
        if (!bekleyenler.Any(k => k.Id == id))
        {
            return Forbid();
        }

        try
        {
            await _kullaniciYonetimService.ReddetAsync(id);
            TempData["BilgiMesaji"] = "Başvuru reddedildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Core.Entities.Mentor?> GirisYapanMentorAlAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _mentorService.GetByKullaniciIdAsync(kullaniciId);
    }
}
