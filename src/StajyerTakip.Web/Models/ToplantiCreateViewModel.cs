using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class ToplantiCreateViewModel
{
    [Required(ErrorMessage = "Başlık zorunludur.")]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Required(ErrorMessage = "Tarih/saat zorunludur.")]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; }
}
