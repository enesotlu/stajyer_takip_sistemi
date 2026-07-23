using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Core.Entities;

// Bir toplantı davetinin, tek bir stajyer için kabul/ret kaydı.
// Durum, Devam/Izin ile aynı OnayDurumu enum'ını kullanır: Bekliyor = henüz
// cevaplanmadı, Onaylandi = kabul etti, Reddedildi = reddetti (RetSebebi zorunlu).
public class ToplantiKatilimi
{
    public int Id { get; set; }

    public int ToplantiId { get; set; }
    public Toplanti Toplanti { get; set; } = null!;

    public int StajyerId { get; set; }
    public Stajyer Stajyer { get; set; } = null!;

    public OnayDurumu Durum { get; set; } = OnayDurumu.Bekliyor;

    [StringLength(500)]
    [Display(Name = "Ret Sebebi")]
    public string? RetSebebi { get; set; }

    public DateTime? CevapTarihi { get; set; }
}
