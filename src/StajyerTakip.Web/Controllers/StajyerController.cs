using Microsoft.AspNetCore.Authorization;
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

    public StajyerController(
        IStajyerService stajyerService,
        IMentorService mentorService,
        IDepartmanService departmanService)
    {
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _departmanService = departmanService;
    }

    public async Task<IActionResult> Index()
    {
        var stajyerler = await _stajyerService.GetAllAsync();
        return View(stajyerler);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownlarAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StajyerCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownlarAsync(model.MentorId, model.DepartmanId);
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
            return View(model);
        }
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

        ViewBag.MentorId = new SelectList(mentorler, "Id", "Unvan", seciliMentorId);
        ViewBag.DepartmanId = new SelectList(departmanlar, "Id", "Ad", seciliDepartmanId);
    }
}
