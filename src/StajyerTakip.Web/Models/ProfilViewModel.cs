using StajyerTakip.Core.Entities;

namespace StajyerTakip.Web.Models;

public class ProfilViewModel
{
    public string AdSoyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }

    // Role göre yalnızca biri dolu olur.
    public Stajyer? StajyerProfili { get; set; }
    public Mentor? MentorProfili { get; set; }

    // Stajyer.Mentor.Kullanici iç içe ilişkisi repository tarafından
    // yüklenmediği için mentörün adı ayrıca doldurulur.
    public string? MentorAdSoyad { get; set; }
}
