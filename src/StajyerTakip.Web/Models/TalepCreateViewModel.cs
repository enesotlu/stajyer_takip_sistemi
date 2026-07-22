using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class TalepCreateViewModel
{
    [Display(Name = "Stajyer")]
    [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir stajyer seçin.")]
    public int StajyerId { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Required(ErrorMessage = "Son tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Son Tarih")]
    public DateTime? SonTarih { get; set; }

    [Display(Name = "Dosya istensin (CV, belge vb.)")]
    public bool DosyaIstensin { get; set; }
}
