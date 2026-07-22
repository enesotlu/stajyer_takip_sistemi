using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class IzinCreateViewModel
{
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
}
