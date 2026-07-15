using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class TalepService : ITalepService
{
    private readonly IUnitOfWork _unitOfWork;

    public TalepService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Talep>> GetByMentorAsync(int mentorId)
    {
        var talepler = await _unitOfWork.Talepler.FindAsync(
            t => t.Stajyer.MentorId == mentorId, t => t.Stajyer.Kullanici!);
        return talepler.OrderByDescending(t => t.OlusturmaTarihi).ToList();
    }

    public async Task<List<Talep>> GetByStajyerAsync(int stajyerId)
    {
        var talepler = await _unitOfWork.Talepler.FindAsync(t => t.StajyerId == stajyerId);
        return talepler
            .OrderBy(t => t.Durum == TalepDurumu.Bekliyor ? 0 : 1)
            .ThenByDescending(t => t.OlusturmaTarihi)
            .ToList();
    }

    public Task<Talep?> GetByIdAsync(int id) => _unitOfWork.Talepler.GetByIdAsync(id);

    public async Task CreateAsync(int mentorId, int stajyerId, string baslik, string? aciklama, bool dosyaIstensin)
    {
        var stajyer = await _unitOfWork.Stajyerler.GetByIdAsync(stajyerId)
            ?? throw new InvalidOperationException("Stajyer bulunamadı.");

        if (stajyer.MentorId != mentorId)
        {
            throw new InvalidOperationException("Yalnızca kendi stajyerine talep açabilirsin.");
        }

        var talep = new Talep
        {
            StajyerId = stajyerId,
            Baslik = baslik,
            Aciklama = aciklama,
            DosyaIstensin = dosyaIstensin,
            OlusturmaTarihi = DateTime.UtcNow,
            Durum = TalepDurumu.Bekliyor
        };

        await _unitOfWork.Talepler.AddAsync(talep);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CevaplaAsync(
        int talepId, int stajyerId, string? cevapMetni, string? dosyaAdi, string? orijinalDosyaAdi)
    {
        var talep = await _unitOfWork.Talepler.GetByIdAsync(talepId)
            ?? throw new InvalidOperationException("Talep bulunamadı.");

        if (talep.StajyerId != stajyerId)
        {
            throw new InvalidOperationException("Bu talep sana ait değil.");
        }

        if (talep.Durum == TalepDurumu.Tamamlandi)
        {
            throw new InvalidOperationException("Bu talep zaten cevaplanmış.");
        }

        if (talep.DosyaIstensin && string.IsNullOrEmpty(dosyaAdi))
        {
            throw new InvalidOperationException("Bu talep için dosya yüklemen gerekiyor.");
        }

        if (!talep.DosyaIstensin && string.IsNullOrWhiteSpace(cevapMetni) && string.IsNullOrEmpty(dosyaAdi))
        {
            throw new InvalidOperationException("Cevap metni yaz veya bir dosya yükle.");
        }

        talep.CevapMetni = cevapMetni;
        talep.CevapDosyaAdi = dosyaAdi;
        talep.CevapDosyaOrijinalAdi = orijinalDosyaAdi;
        talep.CevapTarihi = DateTime.UtcNow;
        talep.Durum = TalepDurumu.Tamamlandi;

        _unitOfWork.Talepler.Update(talep);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> BekleyenSayisiAsync(int stajyerId)
    {
        var bekleyenler = await _unitOfWork.Talepler.FindAsync(
            t => t.StajyerId == stajyerId && t.Durum == TalepDurumu.Bekliyor);
        return bekleyenler.Count;
    }
}
