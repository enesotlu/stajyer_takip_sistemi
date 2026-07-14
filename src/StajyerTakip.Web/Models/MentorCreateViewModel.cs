using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class MentorCreateViewModel
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(150)]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Geçici Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unvan zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Unvan")]
    public string Unvan { get; set; } = string.Empty;

    [Display(Name = "Departman")]
    public int DepartmanId { get; set; }
}
