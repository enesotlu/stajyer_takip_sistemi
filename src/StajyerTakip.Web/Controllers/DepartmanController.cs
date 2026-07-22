using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Yonetici)]
public class DepartmanController : Controller
{
    private readonly IDepartmanService _departmanService;
    private readonly IMentorService _mentorService;

    public DepartmanController(IDepartmanService departmanService, IMentorService mentorService)
    {
        _departmanService = departmanService;
        _mentorService = mentorService;
    }

    public async Task<IActionResult> Index()
    {
        var departmanlar = await _departmanService.GetAllAsync();

        var mentorler = await _mentorService.GetAllAsync();
        ViewBag.MentorlerByDepartman = mentorler
            .GroupBy(m => m.DepartmanId)
            .ToDictionary(g => g.Key, g => g.Select(m => m.Kullanici.AdSoyad).OrderBy(ad => ad).ToList());

        return View(departmanlar);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Departman departman)
    {
        if (!ModelState.IsValid)
        {
            return View(departman);
        }

        try
        {
            await _departmanService.CreateAsync(departman);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(departman);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var departman = await _departmanService.GetByIdAsync(id);
        if (departman is null)
        {
            return NotFound();
        }

        return View(departman);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Departman departman)
    {
        if (id != departman.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(departman);
        }

        try
        {
            await _departmanService.UpdateAsync(departman);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(departman);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var departman = await _departmanService.GetByIdAsync(id);
        if (departman is null)
        {
            return NotFound();
        }

        return View(departman);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _departmanService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
