namespace StajyerTakip.Core.Entities;

public class Mentor
{
    public int Id { get; set; }

    // 3. Hafta'da ASP.NET Core Identity eklenince AspNetUsers tablosundaki
    // kullanıcıya bağlanacak. Şimdilik sade bir metin alanı.
    public string KullaniciId { get; set; } = string.Empty;

    public string Unvan { get; set; } = string.Empty;

    public int DepartmanId { get; set; }
    public Departman Departman { get; set; } = null!;

    public ICollection<Stajyer> Stajyerler { get; set; } = new List<Stajyer>();
}
