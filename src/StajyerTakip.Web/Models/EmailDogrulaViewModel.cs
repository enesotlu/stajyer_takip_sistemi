using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class EmailDogrulaViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod 6 haneli olmalıdır.")]
    [Display(Name = "Doğrulama Kodu")]
    public string Kod { get; set; } = string.Empty;
}
