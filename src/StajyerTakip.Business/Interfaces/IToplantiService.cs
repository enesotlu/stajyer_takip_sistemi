using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IToplantiService
{
    Task<List<Toplanti>> GetByMentorAsync(int mentorId);
    Task<Toplanti?> GetByIdAsync(int id);

    // Bir toplantının tüm katılım kayıtları (mentörün "kim kabul etti / kim reddetti" görünümü).
    Task<List<ToplantiKatilimi>> GetKatilimlarAsync(int toplantiId);

    // Bir stajyerin kendisine gelen tüm toplantı davetleri.
    Task<List<ToplantiKatilimi>> GetByStajyerAsync(int stajyerId);

    Task<ToplantiKatilimi?> GetKatilimByIdAsync(int katilimId);

    // Mentör toplantı açar: kendi TÜM stajyerlerine otomatik olarak birer davet (Bekliyor) oluşturulur.
    Task CreateAsync(int mentorId, string baslik, string? aciklama, DateTime tarih);

    Task KabulEtAsync(int katilimId);

    // Reddederken sebep zorunludur.
    Task ReddetAsync(int katilimId, string sebep);

    // Bildirim rozeti: stajyerin cevaplamadığı toplantı daveti sayısı.
    Task<int> BekleyenSayisiAsync(int stajyerId);

    // Stajyer "Toplantılarım" listesini açtığında çağrılır: rozet sıfırlanır.
    Task StajyerGorduIsaretleAsync(int stajyerId);
}
