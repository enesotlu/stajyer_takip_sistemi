using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IStajyerService
{
    Task<List<Stajyer>> GetAllAsync();
    Task<Stajyer?> GetByIdAsync(int id);
    Task CreateAsync(Stajyer stajyer);
    Task UpdateAsync(Stajyer stajyer);
    Task DeleteAsync(int id);
}
