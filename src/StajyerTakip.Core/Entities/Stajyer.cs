using System.ComponentModel.DataAnnotations;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Core.Entities;

public class Stajyer
{
    public int Id { get; set; }

    public string KullaniciId { get; set; } = string.Empty;
    public ApplicationUser Kullanici { get; set; } = null!;

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
    public Mentor Mentor { get; set; } = null!;

    // Basvurunun onaylanip Stajyer profilinin gercekten olusturuldugu an.
    // Devam takviminin baslangici icin kullanilir - BaslangicTarihi nominal/idari
    // bir tarih olabilir ama profil bu tarihten once hic var olmamis olabilir.
    // Bu alan eklenmeden once olusturulan eski kayitlarda null'dir; DevamService
    // bu durumda BaslangicTarihi'ne geri doner.
    public DateTime? OlusturmaTarihi { get; set; }

    [Display(Name = "Departman")]
    public int DepartmanId { get; set; }
    public Departman Departman { get; set; } = null!;

    public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
    public ICollection<Devam> DevamKayitlari { get; set; } = new List<Devam>();
}
