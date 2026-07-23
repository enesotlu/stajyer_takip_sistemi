using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;
using StajyerTakip.DataAccess.Context;
using StajyerTakip.DataAccess.Repositories;

namespace StajyerTakip.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private IRepository<Departman>? _departmanlar;
    private IRepository<Mentor>? _mentorler;
    private IRepository<Stajyer>? _stajyerler;
    private IRepository<Gorev>? _gorevler;
    private IRepository<Devam>? _devamKayitlari;
    private IRepository<Degerlendirme>? _degerlendirmeler;
    private IRepository<Duyuru>? _duyurular;
    private IRepository<Talep>? _talepler;
    private IRepository<Izin>? _izinler;
    private IRepository<Toplanti>? _toplantilar;
    private IRepository<ToplantiKatilimi>? _toplantiKatilimlari;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Departman> Departmanlar => _departmanlar ??= new Repository<Departman>(_context);
    public IRepository<Mentor> Mentorler => _mentorler ??= new Repository<Mentor>(_context);
    public IRepository<Stajyer> Stajyerler => _stajyerler ??= new Repository<Stajyer>(_context);
    public IRepository<Gorev> Gorevler => _gorevler ??= new Repository<Gorev>(_context);
    public IRepository<Devam> DevamKayitlari => _devamKayitlari ??= new Repository<Devam>(_context);
    public IRepository<Degerlendirme> Degerlendirmeler => _degerlendirmeler ??= new Repository<Degerlendirme>(_context);
    public IRepository<Duyuru> Duyurular => _duyurular ??= new Repository<Duyuru>(_context);
    public IRepository<Talep> Talepler => _talepler ??= new Repository<Talep>(_context);
    public IRepository<Izin> Izinler => _izinler ??= new Repository<Izin>(_context);
    public IRepository<Toplanti> Toplantilar => _toplantilar ??= new Repository<Toplanti>(_context);
    public IRepository<ToplantiKatilimi> ToplantiKatilimlari =>
        _toplantiKatilimlari ??= new Repository<ToplantiKatilimi>(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
