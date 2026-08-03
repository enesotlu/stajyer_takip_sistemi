using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Core.Entities;

// Mentörün kendi stajyerinden yaptığı istek: "CV gönder", "şu tarihte
// mülakata gel" gibi. Stajyer tarafında bildirim olarak görünür;
// gerekirse dosya (CV, belge) yüklenerek cevaplanır.
public class Talep
{
    public int Id { get; set; }

    public int StajyerId { get; set; }
    public Stajyer Stajyer { get; set; } = null!;

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    // İşaretliyse stajyer cevap verirken dosya yüklemek zorundadır.
    [Display(Name = "Dosya istensin (CV, belge vb.)")]
    public bool DosyaIstensin { get; set; }

    // Mentörün talebi açarken eklediği belge (örn. doldurulacak form, şablon) -
    // stajyerin CevapDosyaAdi'sinden ayrı, aynı güvenli GUID-adlandırma deseniyle saklanır.
    public string? EkDosyaAdi { get; set; }
    public string? EkDosyaOrijinalAdi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    // Stajyerin cevaplamak zorunda oldugu son gun. Yeni taleplerde zorunludur
    // (bkz. TalepCreateViewModel); bu alan eklenmeden once olusturulmus eski
    // taleplerde null'dir ve suresi hic dolmaz.
    [DataType(DataType.Date)]
    [Display(Name = "Son Tarih")]
    public DateTime? SonTarih { get; set; }

    public TalepDurumu Durum { get; set; } = TalepDurumu.Bekliyor;

    [StringLength(1000)]
    [Display(Name = "Cevap")]
    public string? CevapMetni { get; set; }

    // Diskteki güvenli (GUID'li) dosya adı - indirme bu adla yapılır.
    public string? CevapDosyaAdi { get; set; }

    // Kullanıcının yüklediği orijinal dosya adı - ekranda bu gösterilir.
    public string? CevapDosyaOrijinalAdi { get; set; }

    public DateTime? CevapTarihi { get; set; }

    // Mentörün, cevabı yetersiz bulup geri gönderirken bıraktığı geri bildirim
    // (Gorev.MentorNotu ile ayni desen).
    [StringLength(1000)]
    [Display(Name = "Mentör Notu")]
    public string? MentorNotu { get; set; }

    // Bildirim rozeti için: mentör "Talepler" listesini açtığında, o an
    // Tamamlandı görünen cevaplar true'ya çekilir (bkz. TalepService.MentorGorduIsaretleAsync).
    public bool MentorGordu { get; set; }

    // Bildirim rozeti için: stajyer "Taleplerim" listesini açtığında true olur, rozet sıfırlanır.
    public bool StajyerGordu { get; set; }
}
