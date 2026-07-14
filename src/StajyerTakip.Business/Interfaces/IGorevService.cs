using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IGorevService
{
    Task<List<Gorev>> GetAllAsync();
    Task<List<Gorev>> GetByStajyerIdAsync(int stajyerId);
    Task<Gorev?> GetByIdAsync(int id);
    Task CreateAsync(Gorev gorev);
    Task DeleteAsync(int id);

    // Stajyer kendi görevinin durumunu günceller; geriye gitmeye izin verilmez.
    Task StajyerDurumGuncelleAsync(int gorevId, string kullaniciId, GorevDurumu yeniDurum);

    // Mentör, "Tamamlandı" olarak işaretlenmiş bir görevi geri gönderir (yetersiz bulursa).
    Task MentorGeriGonderAsync(int gorevId);
}
