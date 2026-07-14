using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;

namespace StajyerTakip.Web.Controllers;

public class DepartmanController : Controller
{
    private readonly IDepartmanService _departmanService;

    public DepartmanController(IDepartmanService departmanService)
    {
        _departmanService = departmanService;
    }

    public async Task<IActionResult> Index()
    {
        var departmanlar = await _departmanService.GetAllAsync();
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
