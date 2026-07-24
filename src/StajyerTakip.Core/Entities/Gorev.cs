using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Core.Entities;

public class Gorev
{
    public int Id { get; set; }

    [Display(Name = "Stajyer")]
    public int StajyerId { get; set; }
    public Stajyer Stajyer { get; set; } = null!;

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Teslim Tarihi")]
    public DateTime TeslimTarihi { get; set; }

    [Display(Name = "Durum")]
    public GorevDurumu Durum { get; set; } = GorevDurumu.Baslamadi;

    // Stajyerin "Teslim Et" ile yüklediği dosya (opsiyonel) - Talep'teki
    // aynı güvenli GUID-adlandırma deseniyle wwwroot dışında saklanır.
    public string? TeslimDosyaAdi { get; set; }
    public string? TeslimDosyaOrijinalAdi { get; set; }

    [StringLength(1000)]
    [Display(Name = "Teslim Notu")]
    public string? TeslimNotu { get; set; }

    public DateTime? TeslimEdilmeTarihi { get; set; }

    // Mentörün "Geri Gönder" ile bıraktığı geri bildirim; bir sonraki
    // teslimde stajyer için görünür kalır.
    [StringLength(1000)]
    [Display(Name = "Mentör Notu")]
    public string? MentorNotu { get; set; }

    // Bildirim rozeti için: mentör "Ödevler" listesini açtığında, o an
    // Tamamlandı görünen teslimler true'ya çekilir (bkz. GorevService.MentorGorduIsaretleAsync).
    public bool MentorGordu { get; set; }

    // Bildirim rozeti için: stajyer "Ödevlerim" listesini açtığında true olur, rozet sıfırlanır.
    public bool StajyerGordu { get; set; }
}
