using StajyerTakip.Business.Models;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Business.Interfaces;

public interface IKullaniciYonetimService
{
    // Kayıt olmuş ama henüz Yönetici/Mentör/Stajyer rollerinden hiçbirine
    // sahip olmayan kullanıcılar - onay bekleyenler.
    Task<List<ApplicationUser>> GetBekleyenlerAsync();

    // Tüm kullanıcılar, rolleri ve aktif/pasif durumlarıyla.
    Task<List<KullaniciOzeti>> GetTumKullanicilarAsync();

    // Devir teslim: Yönetici yetkisini hedef kullanıcıya devreder.
    // Devreden yöneticinin rolü alınır ve hesabı otomatik kapatılır -
    // sistemde her an tek aktif Yönetici bulunur. Stajyerler devralamaz.
    Task YoneticiDevretAsync(string hedefKullaniciId, string devredenKullaniciId);

    // Hesabı kilitler (silmez - işlem geçmişi denetim için korunur).
    Task PasiflestirAsync(string kullaniciId, string islemiYapanKullaniciId);

    Task AktiflestirAsync(string kullaniciId);
}
