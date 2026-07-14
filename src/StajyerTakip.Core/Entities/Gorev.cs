namespace StajyerTakip.Core.Entities;

public class Gorev
{
    public int Id { get; set; }

    public int StajyerId { get; set; }
    public Stajyer Stajyer { get; set; } = null!;

    public string Baslik { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public DateTime TeslimTarihi { get; set; }
    public GorevDurumu Durum { get; set; } = GorevDurumu.Baslamadi;
}
