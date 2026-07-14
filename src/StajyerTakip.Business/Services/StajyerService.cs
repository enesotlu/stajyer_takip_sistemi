using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class StajyerService : IStajyerService
{
    private readonly IUnitOfWork _unitOfWork;

    public StajyerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<Stajyer>> GetAllAsync() =>
        _unitOfWork.Stajyerler.GetAllAsync(s => s.Mentor, s => s.Departman);

    public Task<Stajyer?> GetByIdAsync(int id) => _unitOfWork.Stajyerler.GetByIdAsync(id);

    public async Task CreateAsync(Stajyer stajyer)
    {
        EnsureTarihlerGecerli(stajyer);

        stajyer.KullaniciId = Guid.NewGuid().ToString();

        await _unitOfWork.Stajyerler.AddAsync(stajyer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(Stajyer stajyer)
    {
        EnsureTarihlerGecerli(stajyer);

        var mevcut = await _unitOfWork.Stajyerler.GetByIdAsync(stajyer.Id);
        if (mevcut is null)
        {
            throw new InvalidOperationException("Stajyer bulunamadı.");
        }

        mevcut.Okul = stajyer.Okul;
        mevcut.Bolum = stajyer.Bolum;
        mevcut.BaslangicTarihi = stajyer.BaslangicTarihi;
        mevcut.BitisTarihi = stajyer.BitisTarihi;
        mevcut.MentorId = stajyer.MentorId;
        mevcut.DepartmanId = stajyer.DepartmanId;

        _unitOfWork.Stajyerler.Update(mevcut);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var stajyer = await _unitOfWork.Stajyerler.GetByIdAsync(id);
        if (stajyer is null)
        {
            return;
        }

        _unitOfWork.Stajyerler.Remove(stajyer);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void EnsureTarihlerGecerli(Stajyer stajyer)
    {
        if (stajyer.BaslangicTarihi >= stajyer.BitisTarihi)
        {
            throw new InvalidOperationException("Başlangıç tarihi, bitiş tarihinden önce olmalıdır.");
        }
    }
}
