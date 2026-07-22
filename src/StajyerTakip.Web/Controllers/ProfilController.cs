using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

[Authorize]
public class ProfilController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;

    public ProfilController(
        UserManager<ApplicationUser> userManager,
        IStajyerService stajyerService,
        IMentorService mentorService)
    {
        _userManager = userManager;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
    }

    public async Task<IActionResult> Index()
    {
        var kullanici = await _userManager.GetUserAsync(User);
        if (kullanici is null)
        {
            return NotFound();
        }

        var roller = await _userManager.GetRolesAsync(kullanici);
        var model = new ProfilViewModel
        {
            AdSoyad = kullanici.AdSoyad,
            Email = kullanici.Email ?? string.Empty,
            KayitTarihi = kullanici.KayitTarihi,
            Rol = roller.FirstOrDefault() ?? string.Empty
        };

        if (roller.Contains(Roller.Stajyer))
        {
            // Liste Mentor/Departman/Kullanici ilişkileriyle birlikte yüklenir;
            // profil sayfasında bu isimleri göstermek için gerekli.
            model.StajyerProfili = (await _stajyerService.GetAllAsync())
                .SingleOrDefault(s => s.KullaniciId == kullanici.Id);

            if (model.StajyerProfili is not null)
            {
                var mentor = (await _mentorService.GetAllAsync())
                    .SingleOrDefault(m => m.Id == model.StajyerProfili.MentorId);
                model.MentorAdSoyad = mentor?.Kullanici.AdSoyad;
            }
        }
        else if (roller.Contains(Roller.Mentor))
        {
            model.MentorProfili = (await _mentorService.GetAllAsync())
                .SingleOrDefault(m => m.KullaniciId == kullanici.Id);
        }

        return View(model);
    }
}
