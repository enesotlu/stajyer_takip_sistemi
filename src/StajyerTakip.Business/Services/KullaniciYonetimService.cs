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

    // Tüm onay bekleyenler (henüz rolü olmayan kullanıcılar).
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

    // Mentör başvurusunda bulunmuş, onay bekleyen kullanıcılar (Admin için).
    public async Task<List<ApplicationUser>> GetMentorBekleyenlerAsync()
    {
        var tumKullanicilar = _userManager.Users.ToList();
        var bekleyenler = new List<ApplicationUser>();

        foreach (var kullanici in tumKullanicilar)
        {
            if (kullanici.TalepEdilenRol == Roller.Mentor && kullanici.OnayDurumu == "Bekliyor")
            {
                var roller = await _userManager.GetRolesAsync(kullanici);
                if (roller.Count == 0)
                {
                    bekleyenler.Add(kullanici);
                }
            }
        }

        return bekleyenler.OrderByDescending(k => k.KayitTarihi).ToList();
    }

    // Belirtilen departmana stajyer başvurusu yapmış, onay bekleyen kullanıcılar (Mentör için).
    public async Task<List<ApplicationUser>> GetStajyerBekleyenlerByDepartmanAsync(int departmanId)
    {
        var tumKullanicilar = _userManager.Users.ToList();
        var bekleyenler = new List<ApplicationUser>();

        foreach (var kullanici in tumKullanicilar)
        {
            if (kullanici.TalepEdilenRol == Roller.Stajyer
                && kullanici.TalepEdilenDepartmanId == departmanId
                && kullanici.OnayDurumu == "Bekliyor")
            {
                var roller = await _userManager.GetRolesAsync(kullanici);
                if (roller.Count == 0)
                {
                    bekleyenler.Add(kullanici);
                }
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

    public async Task YoneticiDevretAsync(string hedefKullaniciId, string devredenKullaniciId)
    {
        if (hedefKullaniciId == devredenKullaniciId)
        {
            throw new InvalidOperationException("Yetkiyi kendine devredemezsin.");
        }

        var hedef = await _userManager.FindByIdAsync(hedefKullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        if (await _userManager.IsLockedOutAsync(hedef))
        {
            throw new InvalidOperationException("Pasif bir hesaba yetki devredilemez. Önce hesabı aktifleştirin.");
        }

        var hedefRolleri = await _userManager.GetRolesAsync(hedef);
        if (hedefRolleri.Contains(Roller.Stajyer))
        {
            throw new InvalidOperationException("Stajyerler Yönetici yapılamaz.");
        }

        if (hedefRolleri.Contains(Roller.Yonetici))
        {
            throw new InvalidOperationException("Bu kullanıcı zaten Yönetici.");
        }

        await _userManager.AddToRoleAsync(hedef, Roller.Yonetici);

        // Devir teslim: devreden yöneticinin rolü alınır ve hesabı kapatılır.
        var devreden = await _userManager.FindByIdAsync(devredenKullaniciId);
        if (devreden is not null)
        {
            await _userManager.RemoveFromRoleAsync(devreden, Roller.Yonetici);
            await KilitleAsync(devreden);
        }
    }

    public async Task PasiflestirAsync(string kullaniciId, string islemiYapanKullaniciId)
    {
        if (kullaniciId == islemiYapanKullaniciId)
        {
            throw new InvalidOperationException("Kendi hesabını pasifleştiremezsin.");
        }

        var kullanici = await _userManager.FindByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        var roller = await _userManager.GetRolesAsync(kullanici);
        if (roller.Contains(Roller.Yonetici))
        {
            throw new InvalidOperationException(
                "Yönetici hesabı pasifleştirilemez. Yönetici hesabı yalnızca yetki devriyle kapanır.");
        }

        await KilitleAsync(kullanici);
    }

    // Başvuruyu reddeder: hesabı kilitler ve onayDurumu = "Reddedildi" yapar.
    public async Task ReddetAsync(string kullaniciId)
    {
        var kullanici = await _userManager.FindByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        kullanici.OnayDurumu = "Reddedildi";
        await _userManager.UpdateAsync(kullanici);
        await KilitleAsync(kullanici);
    }

    public async Task AktiflestirAsync(string kullaniciId)
    {
        var kullanici = await _userManager.FindByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        await _userManager.SetLockoutEndDateAsync(kullanici, null);
    }

    private async Task KilitleAsync(ApplicationUser kullanici)
    {
        // Kilitleme mekanizmasıyla girişi kapatıyoruz; hesabı silmiyoruz ki
        // kullanıcının geçmiş işlemlerinin kaydı denetim için korunmuş kalsın.
        await _userManager.SetLockoutEnabledAsync(kullanici, true);
        await _userManager.SetLockoutEndDateAsync(kullanici, DateTimeOffset.MaxValue);

        // Güvenlik damgasını değiştirmek, açık oturumlarının da düşmesini sağlar.
        await _userManager.UpdateSecurityStampAsync(kullanici);
    }
}
