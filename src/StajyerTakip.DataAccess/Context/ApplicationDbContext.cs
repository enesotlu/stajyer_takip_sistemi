using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.DataAccess.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Departman> Departmanlar => Set<Departman>();
    public DbSet<Mentor> Mentorler => Set<Mentor>();
    public DbSet<Stajyer> Stajyerler => Set<Stajyer>();
    public DbSet<Gorev> Gorevler => Set<Gorev>();
    public DbSet<Devam> DevamKayitlari => Set<Devam>();
    public DbSet<Degerlendirme> Degerlendirmeler => Set<Degerlendirme>();
    public DbSet<Duyuru> Duyurular => Set<Duyuru>();
    public DbSet<Talep> Talepler => Set<Talep>();
    public DbSet<Izin> Izinler => Set<Izin>();
    public DbSet<Toplanti> Toplantilar => Set<Toplanti>();
    public DbSet<ToplantiKatilimi> ToplantiKatilimlari => Set<ToplantiKatilimi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bir Stajyer silindiğinde Mentor/Departman'ın etkilenmemesi için
        // varsayılan "cascade delete" davranışını bilinçli olarak kapatıyoruz.
        modelBuilder.Entity<Stajyer>()
            .HasOne(s => s.Mentor)
            .WithMany(m => m.Stajyerler)
            .HasForeignKey(s => s.MentorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Stajyer>()
            .HasOne(s => s.Departman)
            .WithMany(d => d.Stajyerler)
            .HasForeignKey(s => s.DepartmanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Mentor>()
            .HasOne(m => m.Departman)
            .WithMany(d => d.Mentorler)
            .HasForeignKey(m => m.DepartmanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Degerlendirme>()
            .HasOne(d => d.Mentor)
            .WithMany()
            .HasForeignKey(d => d.MentorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Mentor>()
            .HasOne(m => m.Kullanici)
            .WithMany()
            .HasForeignKey(m => m.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Stajyer>()
            .HasOne(s => s.Kullanici)
            .WithMany()
            .HasForeignKey(s => s.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gorev>().Property(g => g.Durum).HasConversion<string>();
        modelBuilder.Entity<Devam>().Property(d => d.OnayDurumu).HasConversion<string>();
        modelBuilder.Entity<Talep>().Property(t => t.Durum).HasConversion<string>();
        modelBuilder.Entity<Izin>().Property(i => i.OnayDurumu).HasConversion<string>();
        modelBuilder.Entity<ToplantiKatilimi>().Property(k => k.Durum).HasConversion<string>();

        // ToplantiKatilimi'nin hem Toplanti hem Stajyer'e FK'i var - ikisini de
        // Restrict yapmazsak SQL Server "coklu cascade path" hatasi verir
        // (Stajyer'deki Mentor/Departman FK'larinda da ayni sebeple Restrict var).
        modelBuilder.Entity<ToplantiKatilimi>()
            .HasOne(k => k.Toplanti)
            .WithMany(t => t.Katilimlar)
            .HasForeignKey(k => k.ToplantiId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ToplantiKatilimi>()
            .HasOne(k => k.Stajyer)
            .WithMany()
            .HasForeignKey(k => k.StajyerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
