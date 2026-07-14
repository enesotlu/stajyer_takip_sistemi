namespace StajyerTakip.Core.Entities;

public class Duyuru
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Icerik { get; set; } = string.Empty;
    public DateTime YayinTarihi { get; set; }
}
