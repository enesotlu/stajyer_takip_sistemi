using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class DevamService : IDevamService
{
    private readonly IUnitOfWork _unitOfWork;

    public DevamService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<Devam>> GetAllAsync() => _unitOfWork.DevamKayitlari.GetAllAsync(d => d.Stajyer);

    public Task<Devam?> GetByIdAsync(int id) => _unitOfWork.DevamKayitlari.GetByIdAsync(id);

    public Task<List<Devam>> GetByStajyerIdAsync(int stajyerId) =>
        _unitOfWork.DevamKayitlari.FindAsync(d => d.StajyerId == stajyerId);

    public async Task CreateAsync(string kullaniciId, TimeSpan girisSaati, TimeSpan cikisSaati)
    {
        var stajyerEslesenleri = await _unitOfWork.Stajyerler.FindAsync(s => s.KullaniciId == kullaniciId);
        var stajyer = stajyerEslesenleri.SingleOrDefault();
        if (stajyer is null)
        {
            throw new InvalidOperationException("Bu kullanıcıya bağlı bir stajyer profili bulunamadı.");
        }

        if (cikisSaati <= girisSaati)
        {
            throw new InvalidOperationException("Çıkış saati, giriş saatinden sonra olmalıdır.");
        }

        // Mesai bittikten (18:00) sonra bugun icin kendi kaydini giremez -
        // o saatten sonra unutulan gun sayilir, mentoru ertesi gun girer.
        if (DateTime.Now.TimeOfDay > IDevamService.GunSonuKayitSiniri)
        {
            throw new InvalidOperationException(
                $"Bugün için kayıt girme süresi ({IDevamService.GunSonuKayitSiniri:hh\\:mm}) geçti. Mentörün senin adına girebilir.");
        }

        // Stajyer yalnizca BUGUN icin kayit girebilir; gecmis/gelecek gunler icin
        // (unutulan gunler dahil) yalnizca mentoru MentorKayitGirAsync ile girebilir.
        var bugun = DateTime.Today;

        var ayniGunKaydiVarMi = (await _unitOfWork.DevamKayitlari.FindAsync(
            d => d.StajyerId == stajyer.Id && d.Tarih.Date == bugun)).Any();
        if (ayniGunKaydiVarMi)
        {
            throw new InvalidOperationException("Bugün için zaten bir devam kaydın var.");
        }

        var devam = new Devam
        {
            StajyerId = stajyer.Id,
            Tarih = bugun,
            GirisSaati = girisSaati,
            CikisSaati = cikisSaati,
            OnayDurumu = OnayDurumu.Bekliyor
        };

        await _unitOfWork.DevamKayitlari.AddAsync(devam);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task OnaylaAsync(int id)
    {
        var devam = await _unitOfWork.DevamKayitlari.GetByIdAsync(id);
        if (devam is null)
        {
            throw new InvalidOperationException("Devam kaydı bulunamadı.");
        }

        devam.OnayDurumu = OnayDurumu.Onaylandi;
        _unitOfWork.DevamKayitlari.Update(devam);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReddetAsync(int id)
    {
        var devam = await _unitOfWork.DevamKayitlari.GetByIdAsync(id);
        if (devam is null)
        {
            throw new InvalidOperationException("Devam kaydı bulunamadı.");
        }

        devam.OnayDurumu = OnayDurumu.Reddedildi;
        _unitOfWork.DevamKayitlari.Update(devam);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AylikDevamOzeti> GetAylikOzetAsync(int stajyerId, int yil, int ay)
    {
        var takvim = await GetAylikTakvimAsync(stajyerId, yil, ay);

        return new AylikDevamOzeti(
            yil,
            ay,
            ToplamGun: takvim.Count,
            OnaylananGun: takvim.Count(g => g.Kayit?.OnayDurumu == OnayDurumu.Onaylandi),
            BekleyenGun: takvim.Count(g => g.Kayit?.OnayDurumu == OnayDurumu.Bekliyor),
            ReddedilenGun: takvim.Count(g => g.Kayit?.OnayDurumu == OnayDurumu.Reddedildi),
            EksikGun: takvim.Count(g => g.Kayit is null));
    }

    public async Task<List<GunlukDevamDurumu>> GetAylikTakvimAsync(int stajyerId, int yil, int ay)
    {
        var stajyer = await _unitOfWork.Stajyerler.GetByIdAsync(stajyerId);
        if (stajyer is null)
        {
            throw new InvalidOperationException("Stajyer bulunamadı.");
        }

        var ayBaslangic = new DateTime(yil, ay, 1);
        var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

        // Yoklama, staj sozlesmesindeki BaslangicTarihi ile basvurunun ONAYLANDIGI
        // (profilin gercekten olusturuldugu) OlusturmaTarihi'nden HANGISI DAHA GEC
        // ise ondan itibaren beklenir - aksi halde profil henuz onaylanmamisken bile
        // o gunler "Yok" gorunurdu. Bu alan eklenmeden once olusturulmus eski
        // kayitlarda null'dir; o durumda sadece BaslangicTarihi esas alinir.
        // Bugune kadar olan is gunleri "eksik" sayilir; gelecek gunler icin
        // henuz "Yok" demek erken olur.
        var altSinir = new[] { ayBaslangic, stajyer.BaslangicTarihi.Date, stajyer.OlusturmaTarihi?.Date ?? DateTime.MinValue }.Max();
        var ustSinir = new[] { ayBitis, stajyer.BitisTarihi.Date, DateTime.Today }.Min();

        if (altSinir > ustSinir)
        {
            return new List<GunlukDevamDurumu>();
        }

        var kayitlar = await _unitOfWork.DevamKayitlari.FindAsync(
            d => d.StajyerId == stajyerId && d.Tarih.Year == yil && d.Tarih.Month == ay);
        var kayitlarTariheGore = kayitlar.ToDictionary(d => d.Tarih.Date);

        var sonuc = new List<GunlukDevamDurumu>();
        for (var gun = altSinir; gun <= ustSinir; gun = gun.AddDays(1))
        {
            if (gun.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            kayitlarTariheGore.TryGetValue(gun, out var kayit);
            sonuc.Add(new GunlukDevamDurumu(gun, kayit));
        }

        return sonuc;
    }

    public async Task MentorKayitGirAsync(int stajyerId, DateTime tarih, TimeSpan girisSaati, TimeSpan cikisSaati)
    {
        var stajyer = await _unitOfWork.Stajyerler.GetByIdAsync(stajyerId);
        if (stajyer is null)
        {
            throw new InvalidOperationException("Stajyer bulunamadı.");
        }

        if (cikisSaati <= girisSaati)
        {
            throw new InvalidOperationException("Çıkış saati, giriş saatinden sonra olmalıdır.");
        }

        // Mentor da yalnizca GECMIS/BUGUNku unutulan gunleri girebilir; gelecek
        // bir tarihe kayit girilemez (stajyer henuz o gunu yasamadi).
        if (tarih.Date > DateTime.Today)
        {
            throw new InvalidOperationException("Gelecek bir tarih için devam kaydı girilemez.");
        }

        var ayniGunKaydiVarMi = (await _unitOfWork.DevamKayitlari.FindAsync(
            d => d.StajyerId == stajyerId && d.Tarih.Date == tarih.Date)).Any();
        if (ayniGunKaydiVarMi)
        {
            throw new InvalidOperationException("Bu tarih için zaten bir devam kaydı var.");
        }

        var devam = new Devam
        {
            StajyerId = stajyerId,
            Tarih = tarih.Date,
            GirisSaati = girisSaati,
            CikisSaati = cikisSaati,
            // Mentor bu kaydi kendisi (stajyerin unuttugu bir gun icin) girdigi
            // icin ek bir onay adimina gerek yok, dogrudan onayli baslar.
            OnayDurumu = OnayDurumu.Onaylandi
        };

        await _unitOfWork.DevamKayitlari.AddAsync(devam);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> BekleyenSayisiAsync(int mentorId)
    {
        var stajyerler = await _unitOfWork.Stajyerler.FindAsync(s => s.MentorId == mentorId);
        var stajyerIdleri = stajyerler.Select(s => s.Id).ToHashSet();

        var bekleyenler = await _unitOfWork.DevamKayitlari.FindAsync(d => d.OnayDurumu == OnayDurumu.Bekliyor);
        return bekleyenler.Count(d => stajyerIdleri.Contains(d.StajyerId));
    }
}
