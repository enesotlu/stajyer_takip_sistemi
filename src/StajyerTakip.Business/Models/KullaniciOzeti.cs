using StajyerTakip.Core.Identity;

namespace StajyerTakip.Business.Models;

public record KullaniciOzeti(
    ApplicationUser Kullanici,
    IList<string> Roller,
    bool Pasif);
