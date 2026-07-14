using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IMentorService
{
    Task<List<Mentor>> GetAllAsync();
    Task<Mentor?> GetByIdAsync(int id);
    Task CreateAsync(Mentor mentor);
    Task UpdateAsync(Mentor mentor);
    Task DeleteAsync(int id);
}
