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

    public async Task CreateAsync(string kullaniciId, DateTime tarih, TimeSpan girisSaati, TimeSpan cikisSaati)
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

        var ayniGunKaydiVarMi = (await _unitOfWork.DevamKayitlari.FindAsync(
            d => d.StajyerId == stajyer.Id && d.Tarih.Date == tarih.Date)).Any();
        if (ayniGunKaydiVarMi)
        {
            throw new InvalidOperationException("Bu tarih için zaten bir devam kaydın var.");
        }

        var devam = new Devam
        {
            StajyerId = stajyer.Id,
            Tarih = tarih.Date,
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
        var kayitlar = await _unitOfWork.DevamKayitlari.FindAsync(
            d => d.StajyerId == stajyerId && d.Tarih.Year == yil && d.Tarih.Month == ay);

        return new AylikDevamOzeti(
            yil,
            ay,
            ToplamGun: kayitlar.Count,
            OnaylananGun: kayitlar.Count(d => d.OnayDurumu == OnayDurumu.Onaylandi),
            BekleyenGun: kayitlar.Count(d => d.OnayDurumu == OnayDurumu.Bekliyor),
            ReddedilenGun: kayitlar.Count(d => d.OnayDurumu == OnayDurumu.Reddedildi));
    }
}
