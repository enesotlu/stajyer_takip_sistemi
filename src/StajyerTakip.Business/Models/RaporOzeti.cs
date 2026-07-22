namespace StajyerTakip.Business.Models;

// Yönetici gösterge paneli için özet sayılar ve grafik verileri
// (proje raporu 6.6: toplam stajyer, devam oranı, tamamlanan görev oranı).
public record RaporOzeti(
    int ToplamStajyer,
    int AktifStajyer,
    int ToplamMentor,
    int ToplamDepartman,
    int BekleyenMentorBasvurusu,
    int BekleyenStajyerBasvurusu,
    int BekleyenIzinTalebi,
    Dictionary<string, int> MentorStajyerDagilimi,
    Dictionary<string, int> TalepDurumDagilimi,
    Dictionary<string, int> DepartmanStajyerDagilimi);
