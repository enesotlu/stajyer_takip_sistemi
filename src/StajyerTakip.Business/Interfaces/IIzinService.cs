using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IIzinService
{
    Task<List<Izin>> GetAllAsync();
    Task<Izin?> GetByIdAsync(int id);
    Task<List<Izin>> GetByStajyerIdAsync(int stajyerId);

    Task CreateAsync(string kullaniciId, DateTime baslangic, DateTime bitis, string aciklama);
    Task OnaylaAsync(int id);
    Task ReddetAsync(int id, string? mentorNotu);

    // Bildirim rozeti için: mentörün kendi stajyerlerinden bekleyen izin sayısı.
    Task<int> BekleyenSayisiAsync(int mentorId);

    // Mentör "İzin Talepleri" listesini açtığında çağrılır: rozet sıfırlanır.
    Task MentorGorduIsaretleAsync(int mentorId);
}
