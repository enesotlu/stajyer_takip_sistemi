using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Core.Entities;

public class Departman
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Departman adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    public ICollection<Mentor> Mentorler { get; set; } = new List<Mentor>();
    public ICollection<Stajyer> Stajyerler { get; set; } = new List<Stajyer>();
}
