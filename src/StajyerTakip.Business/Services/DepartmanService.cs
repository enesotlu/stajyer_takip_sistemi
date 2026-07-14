using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class DepartmanService : IDepartmanService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmanService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<Departman>> GetAllAsync() => _unitOfWork.Departmanlar.GetAllAsync();

    public Task<Departman?> GetByIdAsync(int id) => _unitOfWork.Departmanlar.GetByIdAsync(id);

    public async Task CreateAsync(Departman departman)
    {
        await EnsureAdBenzersizAsync(departman.Ad, excludeId: null);

        await _unitOfWork.Departmanlar.AddAsync(departman);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(Departman departman)
    {
        await EnsureAdBenzersizAsync(departman.Ad, excludeId: departman.Id);

        _unitOfWork.Departmanlar.Update(departman);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var departman = await _unitOfWork.Departmanlar.GetByIdAsync(id);
        if (departman is null)
        {
            return;
        }

        var baglıMentorVarMi = (await _unitOfWork.Mentorler.FindAsync(m => m.DepartmanId == id)).Any();
        var baglıStajyerVarMi = (await _unitOfWork.Stajyerler.FindAsync(s => s.DepartmanId == id)).Any();

        if (baglıMentorVarMi || baglıStajyerVarMi)
        {
            throw new InvalidOperationException(
                "Bu departmana bağlı mentör veya stajyer kayıtları var. Önce onları başka bir departmana taşıyın veya silin.");
        }

        _unitOfWork.Departmanlar.Remove(departman);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureAdBenzersizAsync(string ad, int? excludeId)
    {
        var aynıAdaSahipOlanlar = await _unitOfWork.Departmanlar.FindAsync(
            d => d.Ad.ToLower() == ad.ToLower() && d.Id != (excludeId ?? 0));

        if (aynıAdaSahipOlanlar.Count > 0)
        {
            throw new InvalidOperationException($"\"{ad}\" adında bir departman zaten mevcut.");
        }
    }
}
