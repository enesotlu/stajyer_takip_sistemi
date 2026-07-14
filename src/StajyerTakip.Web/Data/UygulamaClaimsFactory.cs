using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Data;

// Giriş sırasında oluşturulan kimlik çerezine AdSoyad bilgisini claim olarak
// ekler; böylece her sayfada "hoş geldin {isim}" göstermek için veritabanına
// gitmek gerekmez.
public class UygulamaClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public UygulamaClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("AdSoyad", user.AdSoyad));
        return identity;
    }
}
