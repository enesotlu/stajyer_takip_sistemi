using System.ComponentModel.DataAnnotations;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Core.Entities;

public class Mentor
{
    public int Id { get; set; }

    public string KullaniciId { get; set; } = string.Empty;
    public ApplicationUser Kullanici { get; set; } = null!;

    [Required(ErrorMessage = "Unvan zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Unvan")]
    public string Unvan { get; set; } = string.Empty;

    [Display(Name = "Departman")]
    public int DepartmanId { get; set; }
    public Departman Departman { get; set; } = null!;

    public ICollection<Stajyer> Stajyerler { get; set; } = new List<Stajyer>();
}
