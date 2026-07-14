namespace StajyerTakip.Core.Entities;

public class Stajyer
{
    public int Id { get; set; }

    public string KullaniciId { get; set; } = string.Empty;

    public string Okul { get; set; } = string.Empty;
    public string Bolum { get; set; } = string.Empty;
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }

    public int MentorId { get; set; }
    public Mentor Mentor { get; set; } = null!;

    public int DepartmanId { get; set; }
    public Departman Departman { get; set; } = null!;

    public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
    public ICollection<Devam> DevamKayitlari { get; set; } = new List<Devam>();
    public ICollection<Degerlendirme> Degerlendirmeler { get; set; } = new List<Degerlendirme>();
}
