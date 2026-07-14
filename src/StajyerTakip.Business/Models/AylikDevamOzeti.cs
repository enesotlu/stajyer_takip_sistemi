namespace StajyerTakip.Business.Models;

public record AylikDevamOzeti(
    int Yil,
    int Ay,
    int ToplamGun,
    int OnaylananGun,
    int BekleyenGun,
    int ReddedilenGun);
