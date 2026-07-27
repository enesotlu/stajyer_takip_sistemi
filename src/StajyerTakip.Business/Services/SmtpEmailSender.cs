using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Options;

namespace StajyerTakip.Business.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailAyarlari _ayarlar;

    public SmtpEmailSender(IOptions<EmailAyarlari> ayarlar)
    {
        _ayarlar = ayarlar.Value;
    }

    public async Task GonderAsync(string aliciEmail, string konu, string govdeHtml)
    {
        using var mesaj = new MailMessage
        {
            From = new MailAddress(_ayarlar.GonderenEmail, _ayarlar.GonderenAd),
            Subject = konu,
            Body = govdeHtml,
            IsBodyHtml = true
        };
        mesaj.To.Add(aliciEmail);

        using var client = new SmtpClient(_ayarlar.SmtpSunucu, _ayarlar.SmtpPort)
        {
            Credentials = new NetworkCredential(_ayarlar.KullaniciAdi, _ayarlar.Sifre),
            EnableSsl = _ayarlar.SslKullan
        };

        await client.SendMailAsync(mesaj);
    }
}
