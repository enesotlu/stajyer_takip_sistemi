namespace StajyerTakip.Business.Models;

public record YeniStajyerIstegi(
    string AdSoyad,
    string Email,
    string Sifre,
    string Okul,
    string Bolum,
    DateTime BaslangicTarihi,
    DateTime BitisTarihi,
    int MentorId,
    int DepartmanId);
