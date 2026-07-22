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
        var mentorler = await _unitOfWork.Mentorler.GetAllAsync(m => m.Kullanici);
        var departmanlar = await _unitOfWork.Departmanlar.GetAllAsync();
        var talepler = await _unitOfWork.Talepler.GetAllAsync();
        var izinler = await _unitOfWork.Izinler.GetAllAsync();

        // Anahtarlar enum adlarıdır (Türkçesi değil): grafiğin renk/etiket haritası
        // (Rapor/Index.cshtml) bu adlarla eşleşiyor; çeviri orada, görünüm katmanında yapılır.
        var talepDagilimi = new Dictionary<string, int>
        {
            [nameof(TalepDurumu.Bekliyor)] = talepler.Count(t => t.Durum == TalepDurumu.Bekliyor),
            [nameof(TalepDurumu.Tamamlandi)] = talepler.Count(t => t.Durum == TalepDurumu.Tamamlandi)
        };

        // Mentör başına stajyer yükünü gösterir (yöneticinin dağılımı dengelemesine yardımcı olur).
        var stajyerSayisiByMentorId = stajyerler
            .GroupBy(s => s.MentorId)
            .ToDictionary(g => g.Key, g => g.Count());
        var mentorDagilimi = mentorler
            .ToDictionary(m => m.Kullanici.AdSoyad, m => stajyerSayisiByMentorId.GetValueOrDefault(m.Id, 0));

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
            BekleyenIzinTalebi: izinler.Count(i => i.OnayDurumu == OnayDurumu.Bekliyor),
            MentorStajyerDagilimi: mentorDagilimi,
            TalepDurumDagilimi: talepDagilimi,
            DepartmanStajyerDagilimi: departmanDagilimi);
    }
}
