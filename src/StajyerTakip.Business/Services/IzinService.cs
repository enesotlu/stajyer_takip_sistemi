using Microsoft.Extensions.Logging;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class IzinService : IIzinService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<IzinService> _logger;

    public IzinService(IUnitOfWork unitOfWork, IEmailSender emailSender, ILogger<IzinService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    // E-posta gönderimi izin onay/red akışını bloklamamalı: SMTP ayarlanmamışsa
    // ya da geçici bir hata olursa sessizce loglanır, işlem başarıyla devam eder.
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
            _logger.LogWarning(ex, "İzin bildirim e-postası gönderilemedi: {Alici}", aliciEmail);
        }
    }

    public Task<List<Izin>> GetAllAsync() => _unitOfWork.Izinler.GetAllAsync(i => i.Stajyer.Kullanici);

    public Task<Izin?> GetByIdAsync(int id) => _unitOfWork.Izinler.GetByIdAsync(id);

    public Task<List<Izin>> GetByStajyerIdAsync(int stajyerId) =>
        _unitOfWork.Izinler.FindAsync(i => i.StajyerId == stajyerId);

    public async Task CreateAsync(string kullaniciId, DateTime baslangic, DateTime bitis, string aciklama)
    {
        var stajyerEslesenleri = await _unitOfWork.Stajyerler.FindAsync(
            s => s.KullaniciId == kullaniciId, s => s.Mentor.Kullanici, s => s.Kullanici);
        var stajyer = stajyerEslesenleri.SingleOrDefault();
        if (stajyer is null)
        {
            throw new InvalidOperationException("Bu kullanıcıya bağlı bir stajyer profili bulunamadı.");
        }

        if (bitis <= baslangic)
        {
            throw new InvalidOperationException("Bitiş, başlangıçtan sonra olmalıdır.");
        }

        if (baslangic < DateTime.Now)
        {
            throw new InvalidOperationException("Geçmiş bir tarih/saat için izin talebi oluşturulamaz.");
        }

        var izin = new Izin
        {
            StajyerId = stajyer.Id,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            Aciklama = aciklama,
            OlusturmaTarihi = DateTime.Now,
            OnayDurumu = OnayDurumu.Bekliyor
        };

        await _unitOfWork.Izinler.AddAsync(izin);
        await _unitOfWork.SaveChangesAsync();

        await GuvenliGonderAsync(
            stajyer.Mentor?.Kullanici?.Email,
            "Yeni İzin Talebi",
            $"<p><strong>{stajyer.Kullanici?.AdSoyad}</strong> yeni bir izin talebi oluşturdu.</p>" +
            $"<p>{baslangic:dd.MM.yyyy HH:mm} - {bitis:dd.MM.yyyy HH:mm}</p>" +
            $"<p>{aciklama}</p>");
    }

    public async Task OnaylaAsync(int id)
    {
        var izin = (await _unitOfWork.Izinler.FindAsync(i => i.Id == id, i => i.Stajyer.Kullanici)).SingleOrDefault();
        if (izin is null)
        {
            throw new InvalidOperationException("İzin talebi bulunamadı.");
        }

        izin.OnayDurumu = OnayDurumu.Onaylandi;
        _unitOfWork.Izinler.Update(izin);
        await _unitOfWork.SaveChangesAsync();

        await GuvenliGonderAsync(
            izin.Stajyer.Kullanici?.Email,
            "İzin Talebiniz Onaylandı",
            $"<p>{izin.BaslangicTarihi:dd.MM.yyyy HH:mm} - {izin.BitisTarihi:dd.MM.yyyy HH:mm} tarihli izin talebiniz onaylandı.</p>");
    }

    public async Task ReddetAsync(int id, string? mentorNotu)
    {
        var izin = (await _unitOfWork.Izinler.FindAsync(i => i.Id == id, i => i.Stajyer.Kullanici)).SingleOrDefault();
        if (izin is null)
        {
            throw new InvalidOperationException("İzin talebi bulunamadı.");
        }

        izin.OnayDurumu = OnayDurumu.Reddedildi;
        izin.MentorNotu = mentorNotu;
        _unitOfWork.Izinler.Update(izin);
        await _unitOfWork.SaveChangesAsync();

        await GuvenliGonderAsync(
            izin.Stajyer.Kullanici?.Email,
            "İzin Talebiniz Reddedildi",
            $"<p>{izin.BaslangicTarihi:dd.MM.yyyy HH:mm} - {izin.BitisTarihi:dd.MM.yyyy HH:mm} tarihli izin talebiniz reddedildi.</p>" +
            (string.IsNullOrWhiteSpace(mentorNotu) ? "" : $"<p><strong>Gerekçe:</strong> {mentorNotu}</p>"));
    }

    public async Task<int> BekleyenSayisiAsync(int mentorId)
    {
        var stajyerler = await _unitOfWork.Stajyerler.FindAsync(s => s.MentorId == mentorId);
        var stajyerIdleri = stajyerler.Select(s => s.Id).ToHashSet();

        var bekleyenIzinler = await _unitOfWork.Izinler.FindAsync(i => i.OnayDurumu == OnayDurumu.Bekliyor && !i.MentorGordu);
        return bekleyenIzinler.Count(i => stajyerIdleri.Contains(i.StajyerId));
    }

    public async Task MentorGorduIsaretleAsync(int mentorId)
    {
        var stajyerler = await _unitOfWork.Stajyerler.FindAsync(s => s.MentorId == mentorId);
        var stajyerIdleri = stajyerler.Select(s => s.Id).ToHashSet();

        var gorulmemisler = await _unitOfWork.Izinler.FindAsync(i => i.OnayDurumu == OnayDurumu.Bekliyor && !i.MentorGordu);
        var kendiIzinleri = gorulmemisler.Where(i => stajyerIdleri.Contains(i.StajyerId)).ToList();

        if (kendiIzinleri.Count == 0)
        {
            return;
        }

        foreach (var izin in kendiIzinleri)
        {
            izin.MentorGordu = true;
            _unitOfWork.Izinler.Update(izin);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
