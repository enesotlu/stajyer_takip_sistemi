using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class StajyerCreateViewModel
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

    [Required(ErrorMessage = "Okul zorunludur.")]
    [StringLength(150)]
    [Display(Name = "Okul")]
    public string Okul { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bölüm zorunludur.")]
    [StringLength(150)]
    [Display(Name = "Bölüm")]
    public string Bolum { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Başlangıç Tarihi")]
    public DateTime BaslangicTarihi { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Bitiş Tarihi")]
    public DateTime BitisTarihi { get; set; }

    [Display(Name = "Mentör")]
    public int MentorId { get; set; }

    [Display(Name = "Departman")]
    public int DepartmanId { get; set; }
}
