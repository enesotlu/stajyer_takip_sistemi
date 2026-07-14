namespace StajyerTakip.Core.Entities;

public class Devam
{
    public int Id { get; set; }

    public int StajyerId { get; set; }
    public Stajyer Stajyer { get; set; } = null!;

    public DateTime Tarih { get; set; }
    public TimeSpan? GirisSaati { get; set; }
    public TimeSpan? CikisSaati { get; set; }
    public OnayDurumu OnayDurumu { get; set; } = OnayDurumu.Bekliyor;
}
