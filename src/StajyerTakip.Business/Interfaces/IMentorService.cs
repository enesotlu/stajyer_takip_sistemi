using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IMentorService
{
    Task<List<Mentor>> GetAllAsync();
    Task<Mentor?> GetByIdAsync(int id);
    Task CreateAsync(YeniMentorIstegi istek);
    Task UpdateAsync(Mentor mentor);
    Task DeleteAsync(int id);
}
