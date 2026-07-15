using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface ITalepService
{
    // Mentörün kendi stajyerlerine açtığı taleplerin tümü.
    Task<List<Talep>> GetByMentorAsync(int mentorId);

    // Stajyerin kendisine gelen talepleri.
    Task<List<Talep>> GetByStajyerAsync(int stajyerId);

    Task<Talep?> GetByIdAsync(int id);

    // Mentör yalnızca KENDİ stajyerine talep açabilir.
    Task CreateAsync(int mentorId, int stajyerId, string baslik, string? aciklama, bool dosyaIstensin);

    // Stajyer kendi talebini cevaplar; dosya istenmişse dosya zorunludur.
    Task CevaplaAsync(
        int talepId, int stajyerId, string? cevapMetni, string? dosyaAdi, string? orijinalDosyaAdi);

    // Bildirim rozeti için: stajyerin bekleyen talep sayısı.
    Task<int> BekleyenSayisiAsync(int stajyerId);
}
