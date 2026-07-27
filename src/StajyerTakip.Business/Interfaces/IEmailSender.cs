namespace StajyerTakip.Business.Interfaces;

public interface IEmailSender
{
    Task GonderAsync(string aliciEmail, string konu, string govdeHtml);
}
