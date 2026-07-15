using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IStajyerService
{
    Task<List<Stajyer>> GetAllAsync();
    Task<Stajyer?> GetByIdAsync(int id);
    Task<Stajyer?> GetByIdWithDetailsAsync(int id);
    Task<Stajyer?> GetByKullaniciIdAsync(string kullaniciId);

    // Zaten kayıt olmuş (hesabı var ama henüz rolü olmayan) bir kullanıcıyı
    // Mentör'ün onayıyla Stajyer yapar.
    Task AtaAsync(
        string kullaniciId, string okul, string bolum, DateTime baslangicTarihi, DateTime bitisTarihi,
        int mentorId, int departmanId);

    Task UpdateAsync(Stajyer stajyer);
    Task DeleteAsync(int id);

    // Admin: stajyerin sorumlu mentörünü değiştirir.
    Task MentorAtaAsync(int stajyerId, int yeniMentorId);
}

