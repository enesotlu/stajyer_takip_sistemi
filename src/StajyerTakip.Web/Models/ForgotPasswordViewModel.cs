using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;
}
