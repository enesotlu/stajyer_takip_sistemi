using Microsoft.AspNetCore.Identity;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Data;

// Uygulama her başladığında roller ile tek bir yönetici hesabının var
// olduğundan emin olur. Sadece geliştirme/demoya yönelik bir kolaylıktır.
public static class IdentitySeeder
{
    public const string VarsayilanYoneticiEmail = "admin@stajyertakip.local";
    public const string VarsayilanYoneticiSifre = "Admin123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in Roller.Hepsi)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var yonetici = await userManager.FindByEmailAsync(VarsayilanYoneticiEmail);
        if (yonetici is null)
        {
            yonetici = new ApplicationUser
            {
                UserName = VarsayilanYoneticiEmail,
                Email = VarsayilanYoneticiEmail,
                AdSoyad = "Sistem Yöneticisi",
                EmailConfirmed = true
            };

            var sonuc = await userManager.CreateAsync(yonetici, VarsayilanYoneticiSifre);
            if (sonuc.Succeeded)
            {
                await userManager.AddToRoleAsync(yonetici, Roller.Yonetici);
            }
        }
    }
}
