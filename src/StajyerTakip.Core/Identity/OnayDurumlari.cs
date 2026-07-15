namespace StajyerTakip.Core.Identity;

// ApplicationUser.OnayDurumu için sabitler - serbest metin yazım
// hatalarını (örn. "bekliyor" / "Bekliyor") derleme zamanında önler.
public static class OnayDurumlari
{
    public const string Bekliyor = "Bekliyor";
    public const string Onaylandi = "Onaylandi";
    public const string Reddedildi = "Reddedildi";
}
