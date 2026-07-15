using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IMentorService
{
    Task<List<Mentor>> GetAllAsync();
    Task<Mentor?> GetByIdAsync(int id);
    Task<Mentor?> GetByKullaniciIdAsync(string kullaniciId);

    // Zaten kayıt olmuş (hesabı var ama henüz rolü olmayan) bir kullanıcıyı
    // Yönetici'nin onayıyla Mentör yapar.
    Task AtaAsync(string kullaniciId, string unvan, int departmanId);
    Task UpdateAsync(Mentor mentor);
    Task DeleteAsync(int id);

    // Aynı departmandaki mentörleri getirir (stajyer başvuru yönlendirme için).
    Task<List<Mentor>> GetByDepartmanAsync(int departmanId);
}

