namespace StajyerTakip.Core.Entities;

public class Degerlendirme
{
    public int Id { get; set; }

    public int StajyerId { get; set; }
    public Stajyer Stajyer { get; set; } = null!;

    public int MentorId { get; set; }
    public Mentor Mentor { get; set; } = null!;

    public int Puan { get; set; }
    public string? Yorum { get; set; }
    public DateTime Tarih { get; set; }
}
