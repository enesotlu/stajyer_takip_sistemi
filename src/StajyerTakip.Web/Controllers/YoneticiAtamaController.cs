using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Yönetici, onaylanmış stajyerlerin sorumlu mentörünü değiştirebilir.
[Authorize(Roles = Roller.Yonetici)]
public class YoneticiAtamaController : Controller
{
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;

    public YoneticiAtamaController(IStajyerService stajyerService, IMentorService mentorService)
    {
        _stajyerService = stajyerService;
        _mentorService = mentorService;
    }

    // Tüm stajyerleri ve mevcut mentör atamalarını gösterir.
    public async Task<IActionResult> Index()
    {
        var stajyerler = await _stajyerService.GetAllAsync();
        return View(stajyerler);
    }

    // Stajyerin mentörünü değiştirme formu.
    public async Task<IActionResult> MentorDegistir(int id)
    {
        var stajyer = await _stajyerService.GetByIdWithDetailsAsync(id);
        if (stajyer is null) return NotFound();

        await PopulateMentorListesiAsync(stajyer.MentorId);
        return View(stajyer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MentorDegistir(int id, int mentorId)
    {
        try
        {
            await _stajyerService.MentorAtaAsync(id, mentorId);
            TempData["BilgiMesaji"] = "Mentör ataması güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateMentorListesiAsync(int? seciliId = null)
    {
        var mentorler = await _mentorService.GetAllAsync();
        var secenekler = mentorler.Select(m => new { m.Id, Ad = $"{m.Kullanici.AdSoyad} ({m.Departman.Ad})" });
        ViewBag.MentorId = new SelectList(secenekler, "Id", "Ad", seciliId);
    }
}
