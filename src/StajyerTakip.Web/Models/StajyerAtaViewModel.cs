using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Web.Models;

public class StajyerAtaViewModel
{
    public string KullaniciId { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

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
    public DateTime BaslangicTarihi { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Bitiş Tarihi")]
    public DateTime BitisTarihi { get; set; } = DateTime.Today.AddMonths(2);

    [Display(Name = "Mentör")]
    public int MentorId { get; set; }

    [Display(Name = "Departman")]
    public int DepartmanId { get; set; }
}
