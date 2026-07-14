using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Display(Name = "Beni hatırla")]
    public bool BeniHatirla { get; set; }

    public string? ReturnUrl { get; set; }
}
