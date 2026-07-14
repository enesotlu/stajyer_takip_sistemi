using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Core.Entities;

public class Mentor
{
    public int Id { get; set; }

    // 3. Hafta'da ASP.NET Core Identity eklenince AspNetUsers tablosundaki
    // kullanıcıya bağlanacak. Şimdilik sade bir metin alanı, formda gösterilmiyor.
    public string KullaniciId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unvan zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Unvan")]
    public string Unvan { get; set; } = string.Empty;

    [Display(Name = "Departman")]
    public int DepartmanId { get; set; }
    public Departman Departman { get; set; } = null!;

    public ICollection<Stajyer> Stajyerler { get; set; } = new List<Stajyer>();
}
