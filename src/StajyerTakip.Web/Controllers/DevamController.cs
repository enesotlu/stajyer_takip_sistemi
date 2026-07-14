using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

[Authorize(Roles = Roller.Yonetici + "," + Roller.Mentor)]
public class DevamController : Controller
{
    private readonly IDevamService _devamService;

    public DevamController(IDevamService devamService)
    {
        _devamService = devamService;
    }

    public async Task<IActionResult> Index()
    {
        var kayitlar = await _devamService.GetAllAsync();
        var siraliKayitlar = kayitlar
            .OrderBy(d => d.OnayDurumu == Core.Entities.OnayDurumu.Bekliyor ? 0 : 1)
            .ThenByDescending(d => d.Tarih)
            .ToList();

        return View(siraliKayitlar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(int id)
    {
        await _devamService.OnaylaAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reddet(int id)
    {
        await _devamService.ReddetAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
