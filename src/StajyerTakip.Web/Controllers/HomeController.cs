using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Giriş yapmamış kullanıcıyı ara sayfayla oyalamadan doğrudan
        // login ekranına gönderiyoruz.
        if (User.Identity is null || !User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");
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
