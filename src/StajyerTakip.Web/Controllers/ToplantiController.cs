using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

// Mentör toplantı daveti açar; açıldığı anda kendi TÜM stajyerlerine otomatik
// olarak birer katılım daveti gider (bkz. ToplantiService.CreateAsync).
[Authorize(Roles = Roller.Mentor)]
public class ToplantiController : Controller
{
    private readonly IToplantiService _toplantiService;
    private readonly IMentorService _mentorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ToplantiController(
        IToplantiService toplantiService, IMentorService mentorService, UserManager<ApplicationUser> userManager)
    {
        _toplantiService = toplantiService;
        _mentorService = mentorService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var mentor = await BenimMentorProfilimAsync();
        if (mentor is null)
        {
            return View(new List<Toplanti>());
        }

        var toplantilar = await _toplantiService.GetByMentorAsync(mentor.Id);

        // Her toplantının altında kimin kabul/reddettiğini gösterebilmek için
        // toplantı Id'sine göre katılım listelerini ayrıca topluyoruz.
        var katilimlarByToplanti = new Dictionary<int, List<ToplantiKatilimi>>();
        foreach (var toplanti in toplantilar)
        {
            katilimlarByToplanti[toplanti.Id] = await _toplantiService.GetKatilimlarAsync(toplanti.Id);
        }
        ViewBag.KatilimlarByToplanti = katilimlarByToplanti;

        return View(toplantilar.OrderByDescending(t => t.Tarih).ToList());
    }

    public IActionResult Create() => View(new ToplantiCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ToplantiCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var mentor = await BenimMentorProfilimAsync();
        if (mentor is null)
        {
            ModelState.AddModelError(string.Empty, "Mentör profiliniz bulunamadı.");
            return View(model);
        }

        try
        {
            await _toplantiService.CreateAsync(mentor.Id, model.Baslik, model.Aciklama, model.Tarih);
            TempData["BilgiMesaji"] = "Toplantı daveti stajyerlerinize gönderildi.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private async Task<Mentor?> BenimMentorProfilimAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _mentorService.GetByKullaniciIdAsync(kullaniciId);
    }
}
