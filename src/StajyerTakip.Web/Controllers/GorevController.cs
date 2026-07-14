using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Görev atama ve takibi Mentör/Yönetici tarafında; stajyerin kendi
// görevlerini görüp durum güncellemesi GorevlerimController'da.
[Authorize(Roles = Roller.Yonetici + "," + Roller.Mentor)]
public class GorevController : Controller
{
    private readonly IGorevService _gorevService;
    private readonly IStajyerService _stajyerService;

    public GorevController(IGorevService gorevService, IStajyerService stajyerService)
    {
        _gorevService = gorevService;
        _stajyerService = stajyerService;
    }

    public async Task<IActionResult> Index()
    {
        var gorevler = await _gorevService.GetAllAsync();
        return View(gorevler);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateStajyerListesiAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Gorev gorev)
    {
        if (!ModelState.IsValid)
        {
            await PopulateStajyerListesiAsync(gorev.StajyerId);
            return View(gorev);
        }

        await _gorevService.CreateAsync(gorev);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeriGonder(int id)
    {
        try
        {
            await _gorevService.MentorGeriGonderAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _gorevService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateStajyerListesiAsync(int? seciliId = null)
    {
        var stajyerler = await _stajyerService.GetAllAsync();
        var secenekler = stajyerler.Select(s => new { s.Id, Ad = $"{s.Kullanici.AdSoyad} ({s.Okul})" });
        ViewBag.StajyerId = new SelectList(secenekler, "Id", "Ad", seciliId);
    }
}
