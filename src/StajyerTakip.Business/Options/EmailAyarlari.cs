namespace StajyerTakip.Business.Options;

// appsettings.json'daki "EmailAyarlari" bölümüne karşılık gelir. Gerçek
// kullanıcı adı/şifre appsettings.json'a YAZILMAZ - dotnet user-secrets
// veya ortam değişkeni ile sağlanır (bkz. README kurulum adımları).
public class EmailAyarlari
{
    public string SmtpSunucu { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string GonderenEmail { get; set; } = string.Empty;
    public string GonderenAd { get; set; } = "Stajyer Takip Sistemi";
    public bool SslKullan { get; set; } = true;
}
