using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class IzinService : IIzinService
{
    private readonly IUnitOfWork _unitOfWork;

    public IzinService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<Izin>> GetAllAsync() => _unitOfWork.Izinler.GetAllAsync(i => i.Stajyer.Kullanici);

    public Task<Izin?> GetByIdAsync(int id) => _unitOfWork.Izinler.GetByIdAsync(id);

    public Task<List<Izin>> GetByStajyerIdAsync(int stajyerId) =>
        _unitOfWork.Izinler.FindAsync(i => i.StajyerId == stajyerId);

    public async Task CreateAsync(string kullaniciId, DateTime baslangic, DateTime bitis, string aciklama)
    {
        var stajyerEslesenleri = await _unitOfWork.Stajyerler.FindAsync(s => s.KullaniciId == kullaniciId);
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
    }

    public async Task OnaylaAsync(int id)
    {
        var izin = await _unitOfWork.Izinler.GetByIdAsync(id);
        if (izin is null)
        {
            throw new InvalidOperationException("İzin talebi bulunamadı.");
        }

        izin.OnayDurumu = OnayDurumu.Onaylandi;
        _unitOfWork.Izinler.Update(izin);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReddetAsync(int id, string? mentorNotu)
    {
        var izin = await _unitOfWork.Izinler.GetByIdAsync(id);
        if (izin is null)
        {
            throw new InvalidOperationException("İzin talebi bulunamadı.");
        }

        izin.OnayDurumu = OnayDurumu.Reddedildi;
        izin.MentorNotu = mentorNotu;
        _unitOfWork.Izinler.Update(izin);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> BekleyenSayisiAsync(int mentorId)
    {
        var stajyerler = await _unitOfWork.Stajyerler.FindAsync(s => s.MentorId == mentorId);
        var stajyerIdleri = stajyerler.Select(s => s.Id).ToHashSet();

        var bekleyenIzinler = await _unitOfWork.Izinler.FindAsync(i => i.OnayDurumu == OnayDurumu.Bekliyor);
        return bekleyenIzinler.Count(i => stajyerIdleri.Contains(i.StajyerId));
    }
}
