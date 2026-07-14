using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IStajyerService
{
    Task<List<Stajyer>> GetAllAsync();
    Task<Stajyer?> GetByIdAsync(int id);
    Task<Stajyer?> GetByKullaniciIdAsync(string kullaniciId);
    Task CreateAsync(YeniStajyerIstegi istek);
    Task UpdateAsync(Stajyer stajyer);
    Task DeleteAsync(int id);
}
