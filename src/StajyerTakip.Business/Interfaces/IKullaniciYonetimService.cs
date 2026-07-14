using StajyerTakip.Core.Identity;

namespace StajyerTakip.Business.Interfaces;

public interface IKullaniciYonetimService
{
    // Kayıt olmuş ama henüz Yönetici/Mentör/Stajyer rollerinden hiçbirine
    // sahip olmayan kullanıcılar - onay bekleyenler.
    Task<List<ApplicationUser>> GetBekleyenlerAsync();
}
