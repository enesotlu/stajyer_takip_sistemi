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

    // Stajyerin giriş yaptığı anda taranıp doğrulanan konum (Külliye içinde miydi?).
    // Mentörün elle girdiği kayıtlarda (MentorKayitGirAsync) null kalır.
    public double? Enlem { get; set; }
    public double? Boylam { get; set; }

    // Bildirim rozeti için: mentör "Devam Onayı" listesini açtığında true olur, rozet sıfırlanır.
    public bool MentorGordu { get; set; }
}
