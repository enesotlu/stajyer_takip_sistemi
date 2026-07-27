using Microsoft.Extensions.Logging;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class ToplantiService : IToplantiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ToplantiService> _logger;

    public ToplantiService(IUnitOfWork unitOfWork, IEmailSender emailSender, ILogger<ToplantiService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    // E-posta gönderimi toplantı daveti/cevap akışını bloklamamalı: SMTP
    // ayarlanmamışsa ya da geçici bir hata olursa sessizce loglanır, işlem
    // başarıyla devam eder.
    private async Task GuvenliGonderAsync(string? aliciEmail, string konu, string govdeHtml)
    {
        if (string.IsNullOrWhiteSpace(aliciEmail))
        {
            return;
        }

        try
        {
            await _emailSender.GonderAsync(aliciEmail, konu, govdeHtml);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Toplantı bildirim e-postası gönderilemedi: {Alici}", aliciEmail);
        }
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
        var stajyerler = await _unitOfWork.Stajyerler.FindAsync(s => s.MentorId == mentorId, s => s.Kullanici);
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

        foreach (var stajyer in stajyerler)
        {
            await GuvenliGonderAsync(
                stajyer.Kullanici?.Email,
                "Yeni Toplantı Daveti",
                $"<p><strong>{baslik}</strong> başlıklı yeni bir toplantı daveti aldın.</p>" +
                $"<p>Tarih: {tarih:dd.MM.yyyy HH:mm}</p>" +
                (string.IsNullOrWhiteSpace(aciklama) ? "" : $"<p>{aciklama}</p>"));
        }
    }

    public async Task KabulEtAsync(int katilimId)
    {
        var katilim = (await _unitOfWork.ToplantiKatilimlari.FindAsync(
            k => k.Id == katilimId, k => k.Stajyer.Kullanici, k => k.Toplanti.Mentor.Kullanici)).SingleOrDefault();
        if (katilim is null)
        {
            throw new InvalidOperationException("Katılım kaydı bulunamadı.");
        }

        katilim.Durum = OnayDurumu.Onaylandi;
        katilim.CevapTarihi = DateTime.Now;
        _unitOfWork.ToplantiKatilimlari.Update(katilim);
        await _unitOfWork.SaveChangesAsync();

        await GuvenliGonderAsync(
            katilim.Toplanti.Mentor?.Kullanici?.Email,
            "Toplantı Daveti Kabul Edildi",
            $"<p><strong>{katilim.Stajyer.Kullanici?.AdSoyad}</strong>, " +
            $"<strong>{katilim.Toplanti.Baslik}</strong> toplantısını kabul etti.</p>");
    }

    public async Task ReddetAsync(int katilimId, string sebep)
    {
        if (string.IsNullOrWhiteSpace(sebep))
        {
            throw new InvalidOperationException("Reddetme sebebi zorunludur.");
        }

        var katilim = (await _unitOfWork.ToplantiKatilimlari.FindAsync(
            k => k.Id == katilimId, k => k.Stajyer.Kullanici, k => k.Toplanti.Mentor.Kullanici)).SingleOrDefault();
        if (katilim is null)
        {
            throw new InvalidOperationException("Katılım kaydı bulunamadı.");
        }

        katilim.Durum = OnayDurumu.Reddedildi;
        katilim.RetSebebi = sebep;
        katilim.CevapTarihi = DateTime.Now;
        _unitOfWork.ToplantiKatilimlari.Update(katilim);
        await _unitOfWork.SaveChangesAsync();

        await GuvenliGonderAsync(
            katilim.Toplanti.Mentor?.Kullanici?.Email,
            "Toplantı Daveti Reddedildi",
            $"<p><strong>{katilim.Stajyer.Kullanici?.AdSoyad}</strong>, " +
            $"<strong>{katilim.Toplanti.Baslik}</strong> toplantısını reddetti.</p>" +
            $"<p><strong>Sebep:</strong> {sebep}</p>");
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
