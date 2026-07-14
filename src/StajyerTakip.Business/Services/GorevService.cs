using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class GorevService : IGorevService
{
    private readonly IUnitOfWork _unitOfWork;

    public GorevService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<Gorev>> GetAllAsync() => _unitOfWork.Gorevler.GetAllAsync(g => g.Stajyer);

    public Task<List<Gorev>> GetByStajyerIdAsync(int stajyerId) =>
        _unitOfWork.Gorevler.FindAsync(g => g.StajyerId == stajyerId);

    public Task<Gorev?> GetByIdAsync(int id) => _unitOfWork.Gorevler.GetByIdAsync(id);

    public async Task CreateAsync(Gorev gorev)
    {
        gorev.Durum = GorevDurumu.Baslamadi;
        await _unitOfWork.Gorevler.AddAsync(gorev);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var gorev = await _unitOfWork.Gorevler.GetByIdAsync(id);
        if (gorev is null)
        {
            return;
        }

        _unitOfWork.Gorevler.Remove(gorev);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task StajyerDurumGuncelleAsync(int gorevId, string kullaniciId, GorevDurumu yeniDurum)
    {
        var gorev = await _unitOfWork.Gorevler.GetByIdAsync(gorevId);
        if (gorev is null)
        {
            throw new InvalidOperationException("Görev bulunamadı.");
        }

        var stajyer = await _unitOfWork.Stajyerler.GetByIdAsync(gorev.StajyerId);
        if (stajyer is null || stajyer.KullaniciId != kullaniciId)
        {
            throw new InvalidOperationException("Bu görev sana ait değil.");
        }

        if (yeniDurum < gorev.Durum)
        {
            throw new InvalidOperationException("Görev durumu geriye alınamaz. Mentörünle iletişime geç.");
        }

        gorev.Durum = yeniDurum;
        _unitOfWork.Gorevler.Update(gorev);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MentorGeriGonderAsync(int gorevId)
    {
        var gorev = await _unitOfWork.Gorevler.GetByIdAsync(gorevId);
        if (gorev is null)
        {
            throw new InvalidOperationException("Görev bulunamadı.");
        }

        if (gorev.Durum != GorevDurumu.Tamamlandi)
        {
            throw new InvalidOperationException("Yalnızca tamamlanmış görevler geri gönderilebilir.");
        }

        gorev.Durum = GorevDurumu.DevamEdiyor;
        _unitOfWork.Gorevler.Update(gorev);
        await _unitOfWork.SaveChangesAsync();
    }
}
