namespace StajyerTakip.Core.Identity;

/// <summary>Rol adlarinin ve rozet renklerinin tek kaynagi; sidebar, Profilim ve
/// Kullanicilar ekranlari ayni eslesmeyi kullanir ki roller birbirinden ayirt edilebilsin.</summary>
public static class RolGorunumleri
{
    public static string Ad(string rol) => rol switch
    {
        Roller.Yonetici => "Yönetici",
        Roller.Mentor => "Mentör",
        Roller.Stajyer => "Stajyer",
        _ => "Onay Bekliyor"
    };

    public static string RozetSinifi(string rol) => rol switch
    {
        Roller.Yonetici => "stk-rozet-mor",
        Roller.Mentor => "stk-rozet-mavi",
        Roller.Stajyer => "stk-rozet-yesil",
        _ => "stk-rozet-amber"
    };
}
