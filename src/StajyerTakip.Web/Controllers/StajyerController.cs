using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Yonetici + "," + Roller.Mentor)]
public class StajyerController : Controller
{
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly IDepartmanService _departmanService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StajyerController(
        IStajyerService stajyerService,
        IMentorService mentorService,
        IDepartmanService departmanService,
        UserManager<ApplicationUser> userManager)
    {
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _departmanService = departmanService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var stajyerler = await _stajyerService.GetAllAsync();
        return View(stajyerler);
    }

    public async Task<IActionResult> Create()
    {
        var girenMentor = await GirisYapanMentorAsync();
        await PopulateDropdownlarAsync(girenMentor?.Id);
        ViewBag.MentorSabit = girenMentor is not null;

        var model = new StajyerCreateViewModel();
        if (girenMentor is not null)
        {
            model.MentorId = girenMentor.Id;
            model.DepartmanId = girenMentor.DepartmanId;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StajyerCreateViewModel model)
    {
        // Bir Mentör stajyer eklerken, Mentör alanı ne gönderilirse gönderilsin
        // her zaman kendisi olur - başka bir mentöre stajyer atayamaz.
        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is not null)
        {
            model.MentorId = girenMentor.Id;
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownlarAsync(model.MentorId, model.DepartmanId);
            ViewBag.MentorSabit = girenMentor is not null;
            return View(model);
        }

        try
        {
            await _stajyerService.CreateAsync(new YeniStajyerIstegi(
                model.AdSoyad, model.Email, model.Sifre, model.Okul, model.Bolum,
                model.BaslangicTarihi, model.BitisTarihi, model.MentorId, model.DepartmanId));
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownlarAsync(model.MentorId, model.DepartmanId);
            ViewBag.MentorSabit = girenMentor is not null;
            return View(model);
        }
    }

    private async Task<Mentor?> GirisYapanMentorAsync()
    {
        if (User.IsInRole(Roller.Yonetici))
        {
            return null;
        }

        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _mentorService.GetByKullaniciIdAsync(kullaniciId);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var stajyer = await _stajyerService.GetByIdAsync(id);
        if (stajyer is null)
        {
            return NotFound();
        }

        await PopulateDropdownlarAsync(stajyer.MentorId, stajyer.DepartmanId);
        return View(stajyer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Stajyer stajyer)
    {
        if (id != stajyer.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownlarAsync(stajyer.MentorId, stajyer.DepartmanId);
            return View(stajyer);
        }

        try
        {
            await _stajyerService.UpdateAsync(stajyer);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownlarAsync(stajyer.MentorId, stajyer.DepartmanId);
            return View(stajyer);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var stajyer = await _stajyerService.GetByIdAsync(id);
        if (stajyer is null)
        {
            return NotFound();
        }

        return View(stajyer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _stajyerService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownlarAsync(int? seciliMentorId = null, int? seciliDepartmanId = null)
    {
        var mentorler = await _mentorService.GetAllAsync();
        var departmanlar = await _departmanService.GetAllAsync();

        var mentorSecenekleri = mentorler.Select(m => new { m.Id, Ad = $"{m.Kullanici.AdSoyad} ({m.Unvan})" });
        ViewBag.MentorId = new SelectList(mentorSecenekleri, "Id", "Ad", seciliMentorId);
        ViewBag.DepartmanId = new SelectList(departmanlar, "Id", "Ad", seciliDepartmanId);
    }
}
