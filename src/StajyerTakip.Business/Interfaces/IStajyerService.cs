using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IStajyerService
{
    Task<List<Stajyer>> GetAllAsync();
    Task<Stajyer?> GetByIdAsync(int id);
    Task<Stajyer?> GetByKullaniciIdAsync(string kullaniciId);
    Task CreateAsync(YeniStajyerIstegi istek);

    // Zaten kayıt olmuş (hesabı var ama henüz rolü olmayan) bir kullanıcıyı
    // Yönetici'nin veya Mentör'ün onayıyla Stajyer yapar.
    Task AtaAsync(
        string kullaniciId, string okul, string bolum, DateTime baslangicTarihi, DateTime bitisTarihi,
        int mentorId, int departmanId);
    Task UpdateAsync(Stajyer stajyer);
    Task DeleteAsync(int id);
}
