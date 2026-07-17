namespace StajyerTakip.Web.Models;

/// <summary>Liste sayfalarinin ustundeki standart baslik blogu icin model.</summary>
public record SayfaBaslikModel(
    string Baslik,
    string Aciklama,
    string? AksiyonMetni = null,
    string? AksiyonController = null,
    string? AksiyonAction = null,
    string AksiyonIkon = "arti");
