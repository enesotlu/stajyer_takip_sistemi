namespace StajyerTakip.Business.Models;

// OtomatikOlusturAsync'in donus degeri: hem basarili/basarisiz bilgisini
// (login'i engelleyip engellememek icin) hem de aciklama metnini tasir.
public record DevamOtomatikSonucu(bool Basarili, string Mesaj);
