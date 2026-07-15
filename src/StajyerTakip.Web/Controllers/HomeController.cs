using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Giriş yapmamış kullanıcıyı login ekranına gönderiyoruz.
        if (User.Identity is null || !User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");
        }

        // Onay bekleyen kullanıcılar için talep edilen rol bilgisini view'a aktar.
        var kullanici = await _userManager.GetUserAsync(User);
        if (kullanici is not null)
        {
            ViewBag.TalepEdilenRol = kullanici.TalepEdilenRol;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
