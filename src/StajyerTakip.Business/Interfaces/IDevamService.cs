using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IDevamService
{
    // Cumhurbaşkanlığı Külliyesi'nin referans koordinatları - otomatik devam
    // kaydı yalnızca stajyer bu noktaya belirli bir yarıçap (KulliyeYaricapMetre)
    // içindeyken oluşturulur.
    const double KulliyeEnlem = 39.929838128448814;
    const double KulliyeBoylam = 32.798423354933185;
    const double KulliyeYaricapMetre = 1000;

    // Mesai bitis saati: otomatik kayitta cikis saati olarak kullanilir, ayrica
    // bu saatten sonraki bir girisi "gelinmemis" sayip kayit acilmasini engeller.
    static readonly TimeSpan MesaiBitis = new(18, 0, 0);

    Task<List<Devam>> GetAllAsync();
    Task<Devam?> GetByIdAsync(int id);
    Task<List<Devam>> GetByStajyerIdAsync(int stajyerId);

    // Stajyer giriş yaparken tarayıcıdan alınan konumla çağrılır. Giriş
    // yapmasını hiçbir zaman engellemez - sadece konum Külliye içindeyse
    // bugünün devam kaydını oluşturur; konum yoksa/dışarıdaysa kayıt
    // sessizce açılmaz (o gün takvimde "Yok" görünür).
    Task OtomatikOlusturAsync(string kullaniciId, double? enlem, double? boylam);

    Task OnaylaAsync(int id);
    Task ReddetAsync(int id);

    // Mentör, bir devam kaydının saatlerini düzenler (örn. stajyer izin alıp erken çıktıysa).
    Task UpdateSaatleriAsync(int id, TimeSpan girisSaati, TimeSpan cikisSaati);

    // Mentör, hatalı/geçersiz bir kaydı tamamen kaldırır - o gün takvimde "Yok" görünür.
    Task DeleteAsync(int id);

    Task<AylikDevamOzeti> GetAylikOzetAsync(int stajyerId, int yil, int ay);

    // Stajyerin baslangic-bitis araligindaki her is gunu icin, o gune ait
    // kayit varsa onu, yoksa null (=Yok) dondurur - eksik gunleri gorunur kilar.
    Task<List<GunlukDevamDurumu>> GetAylikTakvimAsync(int stajyerId, int yil, int ay);

    // Mentorun "Devam Takvimi" ekraninda kullanilir: tek bir ay yerine,
    // stajin basladigi gunden (ya da bugune kadar) bitis tarihine/bugune
    // kadar TUM donemi gosterir - gecmis kayitlar ve eksik gunler dahil.
    Task<List<GunlukDevamDurumu>> GetTumDonemTakvimAsync(int stajyerId);
    Task<DonemDevamOzeti> GetTumDonemOzetiAsync(int stajyerId);

    // Mentorun, stajyerin girmeyi unuttugu bir gun icin onun adina girdigi
    // kayit; mentor kendisi girdigi icin dogrudan onayli olusturulur.
    Task MentorKayitGirAsync(int stajyerId, DateTime tarih, TimeSpan girisSaati, TimeSpan cikisSaati);

    // Bildirim rozeti: mentörün kendi stajyerlerinden bekleyen devam kaydı sayısı.
    Task<int> BekleyenSayisiAsync(int mentorId);

    // Mentör "Devam Onayı" listesini açtığında çağrılır: rozet sıfırlanır.
    Task MentorGorduIsaretleAsync(int mentorId);
}
