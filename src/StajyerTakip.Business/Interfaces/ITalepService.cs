using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface ITalepService
{
    // Mentörün kendi stajyerlerine açtığı taleplerin tümü.
    Task<List<Talep>> GetByMentorAsync(int mentorId);

    // Stajyerin kendisine gelen talepleri.
    Task<List<Talep>> GetByStajyerAsync(int stajyerId);

    Task<Talep?> GetByIdAsync(int id);

    // Mentör yalnızca KENDİ stajyerine talep açabilir. sonTarih, stajyerin
    // cevaplamak zorunda olduğu son gündür. ekDosyaAdi/ekDosyaOrijinalAdi,
    // mentörün talebe eklediği belgedir (örn. doldurulacak form, şablon).
    Task CreateAsync(
        int mentorId, int stajyerId, string baslik, string? aciklama, bool dosyaIstensin, DateTime sonTarih,
        string? ekDosyaAdi = null, string? ekDosyaOrijinalAdi = null);

    // Mentör, henüz cevaplanmamış kendi talebinin bilgilerini düzenler.
    Task UpdateAsync(
        int talepId, string baslik, string? aciklama, bool dosyaIstensin, DateTime sonTarih);

    Task DeleteAsync(int talepId);

    // Stajyer kendi talebini cevaplar; dosya istenmişse dosya zorunludur.
    Task CevaplaAsync(
        int talepId, int stajyerId, string? cevapMetni, string? dosyaAdi, string? orijinalDosyaAdi);

    // Mentör, stajyerin cevabını yetersiz bulup geri gönderir; talep tekrar
    // Bekliyor'a döner, eski cevap/dosya temizlenir, yeni bir son tarih zorunludur.
    // Dönüş değeri, controller'ın diskten silmesi için eski cevap dosyasının adıdır.
    Task<string?> MentorGeriGonderAsync(int talepId, string? mentorNotu, DateTime yeniSonTarih);

    // Bildirim rozeti için: stajyerin bekleyen talep sayısı.
    Task<int> BekleyenSayisiAsync(int stajyerId);

    // Stajyer "Taleplerim" listesini açtığında çağrılır: rozet sıfırlanır.
    Task StajyerGorduIsaretleAsync(int stajyerId);

    // Bildirim rozeti için: mentörün henüz incelemediği (stajyer tarafından
    // cevaplanmış, Tamamlandı durumundaki) talep sayısı.
    Task<int> MentorBekleyenSayisiAsync(int mentorId);

    // Mentör "Talepler" listesini açtığında çağrılır: o an gösterilen
    // cevapları "görüldü" işaretler, rozet sıfırlanır.
    Task MentorGorduIsaretleAsync(int mentorId);
}
