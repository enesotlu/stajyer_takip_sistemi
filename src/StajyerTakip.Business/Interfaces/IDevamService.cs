using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IDevamService
{
    // Mesai saati bu saatte kapanmis sayilir; stajyer bugunku kaydini bu saatten
    // sonra giremez - unutursa mentoru MentorKayitGirAsync ile ertesi gun girer.
    static readonly TimeSpan GunSonuKayitSiniri = new(18, 0, 0);

    Task<List<Devam>> GetAllAsync();
    Task<Devam?> GetByIdAsync(int id);
    Task<List<Devam>> GetByStajyerIdAsync(int stajyerId);
    // Stajyer yalnizca BUGUN icin kayit girebilir; tarih parametresi yoktur.
    Task CreateAsync(string kullaniciId, TimeSpan girisSaati, TimeSpan cikisSaati);
    Task OnaylaAsync(int id);
    Task ReddetAsync(int id);
    Task<AylikDevamOzeti> GetAylikOzetAsync(int stajyerId, int yil, int ay);

    // Stajyerin baslangic-bitis araligindaki her is gunu icin, o gune ait
    // kayit varsa onu, yoksa null (=Yok) dondurur - eksik gunleri gorunur kilar.
    Task<List<GunlukDevamDurumu>> GetAylikTakvimAsync(int stajyerId, int yil, int ay);

    // Mentorun, stajyerin girmeyi unuttugu bir gun icin onun adina girdigi
    // kayit; mentor kendisi girdigi icin dogrudan onayli olusturulur.
    Task MentorKayitGirAsync(int stajyerId, DateTime tarih, TimeSpan girisSaati, TimeSpan cikisSaati);
}
