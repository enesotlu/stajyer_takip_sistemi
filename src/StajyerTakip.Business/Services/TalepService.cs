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

    public async Task CreateAsync(
        int mentorId, int stajyerId, string baslik, string? aciklama, bool dosyaIstensin, DateTime sonTarih)
    {
        var stajyer = await _unitOfWork.Stajyerler.GetByIdAsync(stajyerId)
            ?? throw new InvalidOperationException("Stajyer bulunamadı.");

        if (stajyer.MentorId != mentorId)
        {
            throw new InvalidOperationException("Yalnızca kendi stajyerine talep açabilirsin.");
        }

        if (sonTarih.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Son tarih geçmiş bir gün olamaz.");
        }

        var talep = new Talep
        {
            StajyerId = stajyerId,
            Baslik = baslik,
            Aciklama = aciklama,
            DosyaIstensin = dosyaIstensin,
            OlusturmaTarihi = DateTime.UtcNow,
            SonTarih = sonTarih.Date,
            Durum = TalepDurumu.Bekliyor
        };

        await _unitOfWork.Talepler.AddAsync(talep);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        int talepId, string baslik, string? aciklama, bool dosyaIstensin, DateTime sonTarih)
    {
        var talep = await _unitOfWork.Talepler.GetByIdAsync(talepId)
            ?? throw new InvalidOperationException("Talep bulunamadı.");

        if (talep.Durum != TalepDurumu.Bekliyor)
        {
            throw new InvalidOperationException("Yalnızca henüz cevaplanmamış talepler düzenlenebilir.");
        }

        if (sonTarih.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Son tarih geçmiş bir gün olamaz.");
        }

        talep.Baslik = baslik;
        talep.Aciklama = aciklama;
        talep.DosyaIstensin = dosyaIstensin;
        talep.SonTarih = sonTarih.Date;

        _unitOfWork.Talepler.Update(talep);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int talepId)
    {
        var talep = await _unitOfWork.Talepler.GetByIdAsync(talepId);
        if (talep is null)
        {
            return;
        }

        _unitOfWork.Talepler.Remove(talep);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string?> MentorGeriGonderAsync(int talepId, string? mentorNotu, DateTime yeniSonTarih)
    {
        var talep = await _unitOfWork.Talepler.GetByIdAsync(talepId)
            ?? throw new InvalidOperationException("Talep bulunamadı.");

        if (talep.Durum != TalepDurumu.Tamamlandi)
        {
            throw new InvalidOperationException("Yalnızca cevaplanmış talepler geri gönderilebilir.");
        }

        if (yeniSonTarih.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Yeni son tarih geçmiş bir gün olamaz.");
        }

        var eskiDosyaAdi = talep.CevapDosyaAdi;

        talep.Durum = TalepDurumu.Bekliyor;
        talep.MentorNotu = mentorNotu;
        talep.SonTarih = yeniSonTarih.Date;
        talep.CevapMetni = null;
        talep.CevapDosyaAdi = null;
        talep.CevapDosyaOrijinalAdi = null;
        talep.CevapTarihi = null;

        _unitOfWork.Talepler.Update(talep);
        await _unitOfWork.SaveChangesAsync();

        return eskiDosyaAdi;
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

        if (talep.SonTarih.HasValue && DateTime.Today > talep.SonTarih.Value.Date)
        {
            throw new InvalidOperationException(
                "Bu talebin son cevaplama tarihi geçti, artık cevap veremezsin. Mentörünle iletişime geç.");
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
