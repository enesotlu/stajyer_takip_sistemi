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

    // Mentör listesinde stajyerin adı gösterilir; nested include (Stajyer.Kullanici)
    // EF Core'da property-zinciri olarak doğrudan çalışır (bkz. StajyerService'teki
    // s => s.Mentor!.Kullanici deseni).
    public Task<List<Gorev>> GetAllAsync() => _unitOfWork.Gorevler.GetAllAsync(g => g.Stajyer.Kullanici);

    public Task<List<Gorev>> GetByStajyerIdAsync(int stajyerId) =>
        _unitOfWork.Gorevler.FindAsync(g => g.StajyerId == stajyerId);

    public Task<Gorev?> GetByIdAsync(int id) => _unitOfWork.Gorevler.GetByIdAsync(id);

    public async Task CreateAsync(Gorev gorev)
    {
        if (gorev.TeslimTarihi.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Teslim tarihi geçmiş bir gün olamaz.");
        }

        gorev.Durum = GorevDurumu.Baslamadi;
        await _unitOfWork.Gorevler.AddAsync(gorev);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(int gorevId, string baslik, string? aciklama, DateTime teslimTarihi)
    {
        var gorev = await _unitOfWork.Gorevler.GetByIdAsync(gorevId)
            ?? throw new InvalidOperationException("Görev bulunamadı.");

        if (teslimTarihi.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Teslim tarihi geçmiş bir gün olamaz.");
        }

        gorev.Baslik = baslik;
        gorev.Aciklama = aciklama;
        gorev.TeslimTarihi = teslimTarihi.Date;

        _unitOfWork.Gorevler.Update(gorev);
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

        if (yeniDurum == GorevDurumu.Tamamlandi)
        {
            throw new InvalidOperationException("Görevi tamamlamak için \"Teslim Et\" butonunu kullan.");
        }

        if (gorev.TeslimTarihi.Date < DateTime.Today)
        {
            throw new InvalidOperationException(
                "Bu görevin son teslim tarihi geçti, artık işlem yapamazsın. Mentörünle iletişime geç.");
        }

        gorev.Durum = yeniDurum;
        _unitOfWork.Gorevler.Update(gorev);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task StajyerTeslimEtAsync(
        int gorevId, string kullaniciId, string? teslimNotu, string? dosyaAdi, string? orijinalDosyaAdi)
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

        if (gorev.Durum == GorevDurumu.Tamamlandi)
        {
            throw new InvalidOperationException("Bu görev zaten teslim edilmiş.");
        }

        if (gorev.TeslimTarihi.Date < DateTime.Today)
        {
            throw new InvalidOperationException(
                "Bu görevin son teslim tarihi geçti, artık teslim edemezsin. Mentörünle iletişime geç.");
        }

        gorev.Durum = GorevDurumu.Tamamlandi;
        gorev.TeslimNotu = teslimNotu;
        gorev.TeslimDosyaAdi = dosyaAdi;
        gorev.TeslimDosyaOrijinalAdi = orijinalDosyaAdi;
        gorev.TeslimEdilmeTarihi = DateTime.UtcNow;

        _unitOfWork.Gorevler.Update(gorev);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string?> MentorGeriGonderAsync(int gorevId, string? mentorNotu)
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

        var eskiDosyaAdi = gorev.TeslimDosyaAdi;

        gorev.Durum = GorevDurumu.DevamEdiyor;
        gorev.MentorNotu = mentorNotu;
        gorev.TeslimDosyaAdi = null;
        gorev.TeslimDosyaOrijinalAdi = null;
        gorev.TeslimNotu = null;
        gorev.TeslimEdilmeTarihi = null;

        _unitOfWork.Gorevler.Update(gorev);
        await _unitOfWork.SaveChangesAsync();

        return eskiDosyaAdi;
    }

    public async Task<int> BekleyenSayisiAsync(int stajyerId)
    {
        var yeniGorevler = await _unitOfWork.Gorevler.FindAsync(
            g => g.StajyerId == stajyerId && g.Durum == GorevDurumu.Baslamadi);
        return yeniGorevler.Count;
    }

    public async Task<int> MentorBekleyenSayisiAsync(int mentorId)
    {
        var teslimEdilenler = await _unitOfWork.Gorevler.FindAsync(
            g => g.Stajyer.MentorId == mentorId && g.Durum == GorevDurumu.Tamamlandi);
        return teslimEdilenler.Count;
    }
}
