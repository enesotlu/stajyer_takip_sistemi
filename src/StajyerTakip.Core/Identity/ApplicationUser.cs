using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StajyerTakip.Core.Identity;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(150)]
    public string AdSoyad { get; set; } = string.Empty;
}
