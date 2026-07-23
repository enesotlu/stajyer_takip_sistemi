using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Core.Entities;

// Mentörün açtığı toplantı daveti. Oluşturulduğunda mentörün TÜM stajyerlerine
// otomatik olarak birer ToplantiKatilimi kaydı açılır (bkz. ToplantiService.CreateAsync).
public class Toplanti
{
    public int Id { get; set; }

    public int MentorId { get; set; }
    public Mentor Mentor { get; set; } = null!;

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Required(ErrorMessage = "Tarih/saat zorunludur.")]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public ICollection<ToplantiKatilimi> Katilimlar { get; set; } = new List<ToplantiKatilimi>();
}
