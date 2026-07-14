using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IDepartmanService
{
    Task<List<Departman>> GetAllAsync();
    Task<Departman?> GetByIdAsync(int id);
    Task CreateAsync(Departman departman);
    Task UpdateAsync(Departman departman);
    Task DeleteAsync(int id);
}
