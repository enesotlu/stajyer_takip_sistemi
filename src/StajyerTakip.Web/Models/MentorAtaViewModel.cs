using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class MentorAtaViewModel
{
    public string KullaniciId { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unvan zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Unvan")]
    public string Unvan { get; set; } = string.Empty;

    [Display(Name = "Departman")]
    public int DepartmanId { get; set; }
}
