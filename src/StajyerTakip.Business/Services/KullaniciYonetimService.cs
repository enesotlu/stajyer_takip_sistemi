using Microsoft.AspNetCore.Identity;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Models;
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

    public async Task<List<KullaniciOzeti>> GetTumKullanicilarAsync()
    {
        var tumKullanicilar = _userManager.Users.ToList();
        var sonuc = new List<KullaniciOzeti>();

        foreach (var kullanici in tumKullanicilar)
        {
            var roller = await _userManager.GetRolesAsync(kullanici);
            var pasif = await _userManager.IsLockedOutAsync(kullanici);
            sonuc.Add(new KullaniciOzeti(kullanici, roller, pasif));
        }

        return sonuc
            .OrderByDescending(k => k.Roller.Contains(Roller.Yonetici))
            .ThenBy(k => k.Kullanici.AdSoyad)
            .ToList();
    }

    public async Task YoneticiYapAsync(string kullaniciId)
    {
        var kullanici = await _userManager.FindByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        if (await _userManager.IsLockedOutAsync(kullanici))
        {
            throw new InvalidOperationException("Pasif bir hesap Yönetici yapılamaz. Önce hesabı aktifleştirin.");
        }

        var roller = await _userManager.GetRolesAsync(kullanici);
        if (roller.Contains(Roller.Yonetici))
        {
            throw new InvalidOperationException("Bu kullanıcı zaten Yönetici.");
        }

        await _userManager.AddToRoleAsync(kullanici, Roller.Yonetici);
    }

    public async Task PasiflestirAsync(string kullaniciId, string islemiYapanKullaniciId)
    {
        if (kullaniciId == islemiYapanKullaniciId)
        {
            throw new InvalidOperationException("Kendi hesabını pasifleştiremezsin - sistem yöneticisiz kalabilir.");
        }

        var kullanici = await _userManager.FindByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        var roller = await _userManager.GetRolesAsync(kullanici);
        if (roller.Contains(Roller.Yonetici))
        {
            var yoneticiler = await _userManager.GetUsersInRoleAsync(Roller.Yonetici);
            var aktifYoneticiSayisi = 0;
            foreach (var yonetici in yoneticiler)
            {
                if (!await _userManager.IsLockedOutAsync(yonetici))
                {
                    aktifYoneticiSayisi++;
                }
            }

            if (aktifYoneticiSayisi <= 1)
            {
                throw new InvalidOperationException("Sistemdeki son aktif Yönetici pasifleştirilemez.");
            }
        }

        // Kilitleme mekanizmasıyla girişi kapatıyoruz; hesabı silmiyoruz ki
        // kullanıcının geçmiş işlemlerinin kaydı denetim için korunmuş kalsın.
        await _userManager.SetLockoutEnabledAsync(kullanici, true);
        await _userManager.SetLockoutEndDateAsync(kullanici, DateTimeOffset.MaxValue);

        // Güvenlik damgasını değiştirmek, açık oturumlarının da düşmesini sağlar.
        await _userManager.UpdateSecurityStampAsync(kullanici);
    }

    public async Task AktiflestirAsync(string kullaniciId)
    {
        var kullanici = await _userManager.FindByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        await _userManager.SetLockoutEndDateAsync(kullanici, null);
    }
}
