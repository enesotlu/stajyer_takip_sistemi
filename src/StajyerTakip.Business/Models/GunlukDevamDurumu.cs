using StajyerTakip.Core.Entities;

namespace StajyerTakip.Business.Models;

// Aylik devam takviminde bir gunu temsil eder. Kayit null ise stajyer o is
// gunu icin devam girmemistir ("Yok").
public record GunlukDevamDurumu(DateTime Tarih, Devam? Kayit);
