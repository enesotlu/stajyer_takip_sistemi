using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Interfaces;

public interface IGorevService
{
    Task<List<Gorev>> GetAllAsync();
    Task<List<Gorev>> GetByStajyerIdAsync(int stajyerId);
    Task<Gorev?> GetByIdAsync(int id);
    // Teslim tarihi gecmis bir gun olamaz.
    Task CreateAsync(Gorev gorev);

    // Mentor kendi verdigi gorevin baslik/aciklama/teslim tarihini duzenler.
    Task UpdateAsync(int gorevId, string baslik, string? aciklama, DateTime teslimTarihi);

    Task DeleteAsync(int id);

    // Stajyer kendi görevinin durumunu günceller (Başlamadı/Devam Ediyor arası);
    // geriye gitmeye izin verilmez. Tamamlandı'ya geçiş yalnızca TeslimEtAsync iledir.
    Task StajyerDurumGuncelleAsync(int gorevId, string kullaniciId, GorevDurumu yeniDurum);

    // Stajyer görevi teslim eder: durum Tamamlandı olur, dosya/not opsiyoneldir.
    Task StajyerTeslimEtAsync(
        int gorevId, string kullaniciId, string? teslimNotu, string? dosyaAdi, string? orijinalDosyaAdi);

    // Mentör, "Tamamlandı" olarak işaretlenmiş bir görevi geri gönderir (yetersiz bulursa).
    // Eski teslim dosyası temizlenir; dönüş değeri controller'ın diskten silmesi için eski dosya adıdır.
    Task<string?> MentorGeriGonderAsync(int gorevId, string? mentorNotu);
}
