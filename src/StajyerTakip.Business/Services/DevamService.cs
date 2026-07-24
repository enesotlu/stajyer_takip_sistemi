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

    public Task<List<Devam>> GetAllAsync() => _unitOfWork.DevamKayitlari.GetAllAsync(d => d.Stajyer.Kullanici);

    public Task<Devam?> GetByIdAsync(int id) => _unitOfWork.DevamKayitlari.GetByIdAsync(id);

    public Task<List<Devam>> GetByStajyerIdAsync(int stajyerId) =>
        _unitOfWork.DevamKayitlari.FindAsync(d => d.StajyerId == stajyerId);

    public async Task<DevamOtomatikSonucu> OtomatikOlusturAsync(string kullaniciId, double? enlem, double? boylam)
    {
        // Konum hic gelmemis (izin verilmedi/tarayici desteklemiyor) - giris reddedilir.
        if (enlem is null || boylam is null)
        {
            return new DevamOtomatikSonucu(false, "Giriş yapabilmen için konumunu açman gerekiyor.");
        }

        // Konum Kulliye yaricapi disindaysa giris reddedilir - bu, kayit olusturmanin
        // otesinde artik bir GIRIS SARTI (bkz. AccountController.Login).
        var mesafeMetre = MesafeMetre(enlem.Value, boylam.Value, IDevamService.KulliyeEnlem, IDevamService.KulliyeBoylam);
        if (mesafeMetre > IDevamService.KulliyeYaricapMetre)
        {
            return new DevamOtomatikSonucu(false,
                $"Külliye dışındasın (yaklaşık {mesafeMetre / 1000:F1} km uzakta). Giriş yapabilmek için Külliye'de olmalısın.");
        }

        var stajyerEslesenleri = await _unitOfWork.Stajyerler.FindAsync(s => s.KullaniciId == kullaniciId);
        var stajyer = stajyerEslesenleri.SingleOrDefault();
        if (stajyer is null)
        {
            return new DevamOtomatikSonucu(true, "Stajyer profili bulunamadı.");
        }

        // Bundan sonraki durumlar giris'i engellemez, sadece o gun icin
        // devam kaydi acilmamasina sebep olur (hafta sonu, zaten kayit var, mesai bitmis).
        var bugun = DateTime.Today;
        if (bugun.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return new DevamOtomatikSonucu(true, "Hafta sonu, devam kaydı açılmadı.");
        }

        var ayniGunKaydiVarMi = (await _unitOfWork.DevamKayitlari.FindAsync(
            d => d.StajyerId == stajyer.Id && d.Tarih.Date == bugun)).Any();
        if (ayniGunKaydiVarMi)
        {
            return new DevamOtomatikSonucu(true, "Bugün için zaten bir devam kaydın var.");
        }

        // Giris saati, gercek anda giris yapilan saattir - sabit 09:00 degil.
        var girisSaati = DateTime.Now.TimeOfDay;
        if (girisSaati >= IDevamService.MesaiBitis)
        {
            return new DevamOtomatikSonucu(true, "Mesai bitti, bugün için devam kaydı açılmadı.");
        }

        var devam = new Devam
        {
            StajyerId = stajyer.Id,
            Tarih = bugun,
            GirisSaati = girisSaati,
            CikisSaati = IDevamService.MesaiBitis,
            OnayDurumu = OnayDurumu.Bekliyor,
            Enlem = enlem,
            Boylam = boylam
        };

        await _unitOfWork.DevamKayitlari.AddAsync(devam);
        await _unitOfWork.SaveChangesAsync();

        return new DevamOtomatikSonucu(true, $"Devam kaydın oluşturuldu (giriş: {girisSaati:hh\\:mm}).");
    }

    // Haversine formulu: iki enlem/boylam noktasi arasindaki mesafeyi metre cinsinden dondurur.
    private static double MesafeMetre(double enlem1, double boylam1, double enlem2, double boylam2)
    {
        const double dunyaYaricapiMetre = 6371000;
        var enlemFarkiRadyan = DegreeToRadian(enlem2 - enlem1);
        var boylamFarkiRadyan = DegreeToRadian(boylam2 - boylam1);

        var a = Math.Sin(enlemFarkiRadyan / 2) * Math.Sin(enlemFarkiRadyan / 2)
            + Math.Cos(DegreeToRadian(enlem1)) * Math.Cos(DegreeToRadian(enlem2))
            * Math.Sin(boylamFarkiRadyan / 2) * Math.Sin(boylamFarkiRadyan / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return dunyaYaricapiMetre * c;
    }

    private static double DegreeToRadian(double derece) => derece * Math.PI / 180;

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

    public async Task UpdateSaatleriAsync(int id, TimeSpan girisSaati, TimeSpan cikisSaati)
    {
        var devam = await _unitOfWork.DevamKayitlari.GetByIdAsync(id);
        if (devam is null)
        {
            throw new InvalidOperationException("Devam kaydı bulunamadı.");
        }

        if (cikisSaati <= girisSaati)
        {
            throw new InvalidOperationException("Çıkış saati, giriş saatinden sonra olmalıdır.");
        }

        devam.GirisSaati = girisSaati;
        devam.CikisSaati = cikisSaati;
        _unitOfWork.DevamKayitlari.Update(devam);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var devam = await _unitOfWork.DevamKayitlari.GetByIdAsync(id);
        if (devam is null)
        {
            return;
        }

        _unitOfWork.DevamKayitlari.Remove(devam);
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

        var bekleyenler = await _unitOfWork.DevamKayitlari.FindAsync(d => d.OnayDurumu == OnayDurumu.Bekliyor && !d.MentorGordu);
        return bekleyenler.Count(d => stajyerIdleri.Contains(d.StajyerId));
    }

    public async Task MentorGorduIsaretleAsync(int mentorId)
    {
        var stajyerler = await _unitOfWork.Stajyerler.FindAsync(s => s.MentorId == mentorId);
        var stajyerIdleri = stajyerler.Select(s => s.Id).ToHashSet();

        var gorulmemisler = await _unitOfWork.DevamKayitlari.FindAsync(
            d => d.OnayDurumu == OnayDurumu.Bekliyor && !d.MentorGordu);
        var kendiKayitlari = gorulmemisler.Where(d => stajyerIdleri.Contains(d.StajyerId)).ToList();

        if (kendiKayitlari.Count == 0)
        {
            return;
        }

        foreach (var devam in kendiKayitlari)
        {
            devam.MentorGordu = true;
            _unitOfWork.DevamKayitlari.Update(devam);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
