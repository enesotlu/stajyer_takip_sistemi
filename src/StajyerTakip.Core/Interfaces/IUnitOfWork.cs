using StajyerTakip.Core.Entities;

namespace StajyerTakip.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Departman> Departmanlar { get; }
    IRepository<Mentor> Mentorler { get; }
    IRepository<Stajyer> Stajyerler { get; }
    IRepository<Gorev> Gorevler { get; }
    IRepository<Devam> DevamKayitlari { get; }
    IRepository<Degerlendirme> Degerlendirmeler { get; }
    IRepository<Duyuru> Duyurular { get; }
    IRepository<Talep> Talepler { get; }
    IRepository<Izin> Izinler { get; }

    Task<int> SaveChangesAsync();
}
