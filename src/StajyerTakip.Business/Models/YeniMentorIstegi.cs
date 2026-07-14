namespace StajyerTakip.Business.Models;

// Mentör oluşturmak, hem bir kullanıcı hesabı (AspNetUsers) hem de bir
// Mentor profili gerektirdiği için bu isteği tek bir yerde topluyoruz.
public record YeniMentorIstegi(
    string AdSoyad,
    string Email,
    string Sifre,
    string Unvan,
    int DepartmanId);
