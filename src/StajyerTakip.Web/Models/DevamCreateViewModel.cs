using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class DevamCreateViewModel
{
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Giriş saati zorunludur.")]
    [Display(Name = "Giriş Saati")]
    public string GirisSaati { get; set; } = string.Empty;

    [Required(ErrorMessage = "Çıkış saati zorunludur.")]
    [Display(Name = "Çıkış Saati")]
    public string CikisSaati { get; set; } = string.Empty;
}
