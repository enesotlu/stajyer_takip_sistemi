using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class DevamCreateViewModel
{
    // Tarih artik kullanicidan alinmiyor; kayit her zaman bugun icin girilir
    // (bkz. IDevamService.CreateAsync).
    [Required(ErrorMessage = "Giriş saati zorunludur.")]
    [Display(Name = "Giriş Saati")]
    public string GirisSaati { get; set; } = string.Empty;

    [Required(ErrorMessage = "Çıkış saati zorunludur.")]
    [Display(Name = "Çıkış Saati")]
    public string CikisSaati { get; set; } = string.Empty;
}
