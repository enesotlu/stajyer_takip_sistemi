using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StajyerTakip.Core.Identity;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(150)]
    public string AdSoyad { get; set; } = string.Empty;

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    // Kayıt sırasında kullanıcının talep ettiği rol ("Mentor" veya "Stajyer").
    // Yönetici/Mentör onaylayana kadar bu değer bilgi amaçlı saklanır.
    [StringLength(50)]
    public string? TalepEdilenRol { get; set; }

    // Kayıt sırasında seçilen departman Id'si.
    public int? TalepEdilenDepartmanId { get; set; }

    // "Bekliyor" | "Onaylandi" | "Reddedildi"
    [StringLength(20)]
    public string OnayDurumu { get; set; } = OnayDurumlari.Bekliyor;

    // Bildirim rozeti için: onaylayacak kişi (Yönetici/Mentör) başvuru
    // listesini bir kez açtığında true olur, rozet sıfırlanır.
    public bool BasvuruGorulduMu { get; set; }

    // Kayıt sırasında e-postanın gerçek olduğunu doğrulamak için gönderilen
    // 6 haneli kod ve son geçerlilik zamanı. Doğrulandıktan sonra ikisi de temizlenir.
    // Bu alanlar eklenmeden önce oluşturulan hesaplar zaten EmailConfirmed=true
    // olduğu için bu akıştan etkilenmez (bkz. AccountController.Register).
    [StringLength(6)]
    public string? EmailDogrulamaKodu { get; set; }
    public DateTime? EmailDogrulamaKoduSonGecerlilik { get; set; }
}
