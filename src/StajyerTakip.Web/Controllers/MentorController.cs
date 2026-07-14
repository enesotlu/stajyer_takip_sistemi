using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Yonetici)]
public class MentorController : Controller
{
    private readonly IMentorService _mentorService;
    private readonly IDepartmanService _departmanService;

    public MentorController(IMentorService mentorService, IDepartmanService departmanService)
    {
        _mentorService = mentorService;
        _departmanService = departmanService;
    }

    public async Task<IActionResult> Index()
    {
        var mentorler = await _mentorService.GetAllAsync();
        return View(mentorler);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDepartmanListesiAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Mentor mentor)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmanListesiAsync(mentor.DepartmanId);
            return View(mentor);
        }

        await _mentorService.CreateAsync(mentor);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var mentor = await _mentorService.GetByIdAsync(id);
        if (mentor is null)
        {
            return NotFound();
        }

        await PopulateDepartmanListesiAsync(mentor.DepartmanId);
        return View(mentor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Mentor mentor)
    {
        if (id != mentor.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDepartmanListesiAsync(mentor.DepartmanId);
            return View(mentor);
        }

        await _mentorService.UpdateAsync(mentor);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var mentor = await _mentorService.GetByIdAsync(id);
        if (mentor is null)
        {
            return NotFound();
        }

        return View(mentor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _mentorService.DeleteAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            TempData["HataMesaji"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDepartmanListesiAsync(int? seciliId = null)
    {
        var departmanlar = await _departmanService.GetAllAsync();
        ViewBag.DepartmanId = new SelectList(departmanlar, "Id", "Ad", seciliId);
    }
}
