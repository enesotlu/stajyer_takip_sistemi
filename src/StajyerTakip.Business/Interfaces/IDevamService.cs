using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IDevamService
{
    Task<List<Devam>> GetAllAsync();
    Task<List<Devam>> GetByStajyerIdAsync(int stajyerId);
    Task CreateAsync(string kullaniciId, DateTime tarih, TimeSpan girisSaati, TimeSpan cikisSaati);
    Task OnaylaAsync(int id);
    Task ReddetAsync(int id);
    Task<AylikDevamOzeti> GetAylikOzetAsync(int stajyerId, int yil, int ay);
}
