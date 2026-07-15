using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Yönetici gösterge paneli: özet sayılar ve Chart.js grafikleri.
[Authorize(Roles = Roller.Yonetici)]
public class RaporController : Controller
{
    private readonly IRaporService _raporService;

    public RaporController(IRaporService raporService)
    {
        _raporService = raporService;
    }

    public async Task<IActionResult> Index()
    {
        var ozet = await _raporService.GetOzetAsync();
        return View(ozet);
    }
}
