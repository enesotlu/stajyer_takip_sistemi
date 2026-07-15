using Microsoft.AspNetCore.Identity;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class RaporService : IRaporService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public RaporService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<RaporOzeti> GetOzetAsync()
    {
        var bugun = DateTime.Today;

        var stajyerler = await _unitOfWork.Stajyerler.GetAllAsync(s => s.Departman);
        var mentorler = await _unitOfWork.Mentorler.GetAllAsync();
        var departmanlar = await _unitOfWork.Departmanlar.GetAllAsync();
        var gorevler = await _unitOfWork.Gorevler.GetAllAsync();
        var devamlar = await _unitOfWork.DevamKayitlari.GetAllAsync();

        var gorevDagilimi = new Dictionary<string, int>
        {
            ["Başlamadı"] = gorevler.Count(g => g.Durum == GorevDurumu.Baslamadi),
            ["Devam Ediyor"] = gorevler.Count(g => g.Durum == GorevDurumu.DevamEdiyor),
            ["Tamamlandı"] = gorevler.Count(g => g.Durum == GorevDurumu.Tamamlandi)
        };

        var devamDagilimi = new Dictionary<string, int>
        {
            ["Bekliyor"] = devamlar.Count(d => d.OnayDurumu == OnayDurumu.Bekliyor),
            ["Onaylandı"] = devamlar.Count(d => d.OnayDurumu == OnayDurumu.Onaylandi),
            ["Reddedildi"] = devamlar.Count(d => d.OnayDurumu == OnayDurumu.Reddedildi)
        };

        var departmanDagilimi = stajyerler
            .GroupBy(s => s.Departman.Ad)
            .ToDictionary(g => g.Key, g => g.Count());

        // Onay bekleyen başvurular (rol talebine göre).
        var kullanicilar = _userManager.Users.ToList();
        var bekleyenMentor = kullanicilar.Count(k =>
            k.TalepEdilenRol == Roller.Mentor && k.OnayDurumu == OnayDurumlari.Bekliyor);
        var bekleyenStajyer = kullanicilar.Count(k =>
            k.TalepEdilenRol == Roller.Stajyer && k.OnayDurumu == OnayDurumlari.Bekliyor);

        return new RaporOzeti(
            ToplamStajyer: stajyerler.Count,
            AktifStajyer: stajyerler.Count(s => s.BaslangicTarihi.Date <= bugun && bugun <= s.BitisTarihi.Date),
            ToplamMentor: mentorler.Count,
            ToplamDepartman: departmanlar.Count,
            BekleyenMentorBasvurusu: bekleyenMentor,
            BekleyenStajyerBasvurusu: bekleyenStajyer,
            ToplamGorev: gorevler.Count,
            GorevDurumDagilimi: gorevDagilimi,
            DevamDurumDagilimi: devamDagilimi,
            DepartmanStajyerDagilimi: departmanDagilimi);
    }
}
