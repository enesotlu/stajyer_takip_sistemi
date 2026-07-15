using StajyerTakip.Business.Models;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Business.Interfaces;

public interface IKullaniciYonetimService
{
    // Tüm onay bekleyenler (henüz rolü olmayan kullanıcılar).
    Task<List<ApplicationUser>> GetBekleyenlerAsync();

    // Mentör başvurusunda bulunmuş, onay bekleyen kullanıcılar (Admin için).
    Task<List<ApplicationUser>> GetMentorBekleyenlerAsync();

    // Belirtilen departmana stajyer başvurusu yapmış, onay bekleyen kullanıcılar (Mentör için).
    Task<List<ApplicationUser>> GetStajyerBekleyenlerByDepartmanAsync(int departmanId);

    // Tüm kullanıcılar, rolleri ve aktif/pasif durumlarıyla.
    Task<List<KullaniciOzeti>> GetTumKullanicilarAsync();

    // Devir teslim: Yönetici yetkisini hedef kullanıcıya devreder.
    Task YoneticiDevretAsync(string hedefKullaniciId, string devredenKullaniciId);

    // Hesabı kilitler (silmez - işlem geçmişi denetim için korunur).
    Task PasiflestirAsync(string kullaniciId, string islemiYapanKullaniciId);

    Task AktiflestirAsync(string kullaniciId);

    // Başvuruyu reddeder: hesabı kilitler ve onayDurumu = "Reddedildi" yapar.
    Task ReddetAsync(string kullaniciId);
}

