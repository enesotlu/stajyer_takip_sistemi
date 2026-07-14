using Microsoft.AspNetCore.Identity;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Business.Services;

public class KullaniciYonetimService : IKullaniciYonetimService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public KullaniciYonetimService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<ApplicationUser>> GetBekleyenlerAsync()
    {
        var tumKullanicilar = _userManager.Users.ToList();
        var bekleyenler = new List<ApplicationUser>();

        foreach (var kullanici in tumKullanicilar)
        {
            var roller = await _userManager.GetRolesAsync(kullanici);
            if (roller.Count == 0)
            {
                bekleyenler.Add(kullanici);
            }
        }

        return bekleyenler.OrderByDescending(k => k.KayitTarihi).ToList();
    }
}
