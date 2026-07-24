using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Core.Entities;

// Stajyerin mentöründen aldığı izin: belirli bir tarih/saat aralığında işe
// gelemeyeceğini bildirir. Mentör onaylar veya reddeder (OnayDurumu, Devam ile aynı desen).
public class Izin
{
    public int Id { get; set; }

    public int StajyerId { get; set; }
    public Stajyer Stajyer { get; set; } = null!;

    [Required(ErrorMessage = "Başlangıç tarihi/saati zorunludur.")]
    [Display(Name = "Başlangıç")]
    public DateTime BaslangicTarihi { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi/saati zorunludur.")]
    [Display(Name = "Bitiş")]
    public DateTime BitisTarihi { get; set; }

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string Aciklama { get; set; } = string.Empty;

    public DateTime OlusturmaTarihi { get; set; }

    public OnayDurumu OnayDurumu { get; set; } = OnayDurumu.Bekliyor;

    // Bildirim rozeti için: mentör "İzin Talepleri" listesini açtığında true olur, rozet sıfırlanır.
    public bool MentorGordu { get; set; }

    // Mentörün reddederken bıraktığı kısa gerekçe (Talep.MentorNotu ile aynı desen).
    [StringLength(500)]
    [Display(Name = "Mentör Notu")]
    public string? MentorNotu { get; set; }
}
