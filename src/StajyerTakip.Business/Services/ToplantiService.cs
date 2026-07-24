using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class ToplantiService : IToplantiService
{
    private readonly IUnitOfWork _unitOfWork;

    public ToplantiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<Toplanti>> GetByMentorAsync(int mentorId) =>
        _unitOfWork.Toplantilar.FindAsync(t => t.MentorId == mentorId);

    public Task<Toplanti?> GetByIdAsync(int id) => _unitOfWork.Toplantilar.GetByIdAsync(id);

    public Task<List<ToplantiKatilimi>> GetKatilimlarAsync(int toplantiId) =>
        _unitOfWork.ToplantiKatilimlari.FindAsync(k => k.ToplantiId == toplantiId, k => k.Stajyer.Kullanici);

    public Task<List<ToplantiKatilimi>> GetByStajyerAsync(int stajyerId) =>
        _unitOfWork.ToplantiKatilimlari.FindAsync(k => k.StajyerId == stajyerId, k => k.Toplanti);

    public Task<ToplantiKatilimi?> GetKatilimByIdAsync(int katilimId) =>
        _unitOfWork.ToplantiKatilimlari.GetByIdAsync(katilimId);

    public async Task CreateAsync(int mentorId, string baslik, string? aciklama, DateTime tarih)
    {
        var stajyerler = await _unitOfWork.Stajyerler.FindAsync(s => s.MentorId == mentorId);
        if (stajyerler.Count == 0)
        {
            throw new InvalidOperationException("Sorumlu olduğunuz bir stajyer yok, toplantı daveti gönderilemez.");
        }

        var toplanti = new Toplanti
        {
            MentorId = mentorId,
            Baslik = baslik,
            Aciklama = aciklama,
            Tarih = tarih,
            OlusturmaTarihi = DateTime.Now
        };

        await _unitOfWork.Toplantilar.AddAsync(toplanti);
        await _unitOfWork.SaveChangesAsync(); // Katilimlar icin Toplanti.Id gerekiyor, once kaydediyoruz.

        foreach (var stajyer in stajyerler)
        {
            await _unitOfWork.ToplantiKatilimlari.AddAsync(new ToplantiKatilimi
            {
                ToplantiId = toplanti.Id,
                StajyerId = stajyer.Id,
                Durum = OnayDurumu.Bekliyor
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task KabulEtAsync(int katilimId)
    {
        var katilim = await _unitOfWork.ToplantiKatilimlari.GetByIdAsync(katilimId);
        if (katilim is null)
        {
            throw new InvalidOperationException("Katılım kaydı bulunamadı.");
        }

        katilim.Durum = OnayDurumu.Onaylandi;
        katilim.CevapTarihi = DateTime.Now;
        _unitOfWork.ToplantiKatilimlari.Update(katilim);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReddetAsync(int katilimId, string sebep)
    {
        if (string.IsNullOrWhiteSpace(sebep))
        {
            throw new InvalidOperationException("Reddetme sebebi zorunludur.");
        }

        var katilim = await _unitOfWork.ToplantiKatilimlari.GetByIdAsync(katilimId);
        if (katilim is null)
        {
            throw new InvalidOperationException("Katılım kaydı bulunamadı.");
        }

        katilim.Durum = OnayDurumu.Reddedildi;
        katilim.RetSebebi = sebep;
        katilim.CevapTarihi = DateTime.Now;
        _unitOfWork.ToplantiKatilimlari.Update(katilim);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> BekleyenSayisiAsync(int stajyerId)
    {
        var bekleyenler = await _unitOfWork.ToplantiKatilimlari.FindAsync(
            k => k.StajyerId == stajyerId && k.Durum == OnayDurumu.Bekliyor && !k.StajyerGordu);
        return bekleyenler.Count;
    }

    public async Task StajyerGorduIsaretleAsync(int stajyerId)
    {
        var gorulmemisler = await _unitOfWork.ToplantiKatilimlari.FindAsync(
            k => k.StajyerId == stajyerId && k.Durum == OnayDurumu.Bekliyor && !k.StajyerGordu);

        if (gorulmemisler.Count == 0)
        {
            return;
        }

        foreach (var katilim in gorulmemisler)
        {
            katilim.StajyerGordu = true;
            _unitOfWork.ToplantiKatilimlari.Update(katilim);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
