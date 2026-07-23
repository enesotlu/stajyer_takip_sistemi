using StajyerTakip.Core.Entities;

namespace StajyerTakip.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Departman> Departmanlar { get; }
    IRepository<Mentor> Mentorler { get; }
    IRepository<Stajyer> Stajyerler { get; }
    IRepository<Gorev> Gorevler { get; }
    IRepository<Devam> DevamKayitlari { get; }
    IRepository<Duyuru> Duyurular { get; }
    IRepository<Talep> Talepler { get; }
    IRepository<Izin> Izinler { get; }
    IRepository<Toplanti> Toplantilar { get; }
    IRepository<ToplantiKatilimi> ToplantiKatilimlari { get; }

    Task<int> SaveChangesAsync();
}
