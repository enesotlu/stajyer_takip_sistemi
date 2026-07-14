namespace StajyerTakip.Core.Entities;

public class Departman
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }

    public ICollection<Mentor> Mentorler { get; set; } = new List<Mentor>();
    public ICollection<Stajyer> Stajyerler { get; set; } = new List<Stajyer>();
}
