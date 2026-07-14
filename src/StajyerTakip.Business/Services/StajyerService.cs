using Microsoft.AspNetCore.Identity;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class StajyerService : IStajyerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public StajyerService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public Task<List<Stajyer>> GetAllAsync() =>
        _unitOfWork.Stajyerler.GetAllAsync(s => s.Mentor, s => s.Departman);

    public Task<Stajyer?> GetByIdAsync(int id) => _unitOfWork.Stajyerler.GetByIdAsync(id);

    public async Task CreateAsync(YeniStajyerIstegi istek)
    {
        if (istek.BaslangicTarihi >= istek.BitisTarihi)
        {
            throw new InvalidOperationException("Başlangıç tarihi, bitiş tarihinden önce olmalıdır.");
        }

        var mevcutKullanici = await _userManager.FindByEmailAsync(istek.Email);
        if (mevcutKullanici is not null)
        {
            throw new InvalidOperationException($"\"{istek.Email}\" e-postası zaten kullanımda.");
        }

        var kullanici = new ApplicationUser
        {
            UserName = istek.Email,
            Email = istek.Email,
            AdSoyad = istek.AdSoyad,
            EmailConfirmed = true
        };

        var kullaniciSonucu = await _userManager.CreateAsync(kullanici, istek.Sifre);
        if (!kullaniciSonucu.Succeeded)
        {
            var hatalar = string.Join(" ", kullaniciSonucu.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Kullanıcı hesabı oluşturulamadı: {hatalar}");
        }

        await _userManager.AddToRoleAsync(kullanici, Roller.Stajyer);

        var stajyer = new Stajyer
        {
            KullaniciId = kullanici.Id,
            Okul = istek.Okul,
            Bolum = istek.Bolum,
            BaslangicTarihi = istek.BaslangicTarihi,
            BitisTarihi = istek.BitisTarihi,
            MentorId = istek.MentorId,
            DepartmanId = istek.DepartmanId
        };

        await _unitOfWork.Stajyerler.AddAsync(stajyer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(Stajyer stajyer)
    {
        EnsureTarihlerGecerli(stajyer);

        var mevcut = await _unitOfWork.Stajyerler.GetByIdAsync(stajyer.Id);
        if (mevcut is null)
        {
            throw new InvalidOperationException("Stajyer bulunamadı.");
        }

        mevcut.Okul = stajyer.Okul;
        mevcut.Bolum = stajyer.Bolum;
        mevcut.BaslangicTarihi = stajyer.BaslangicTarihi;
        mevcut.BitisTarihi = stajyer.BitisTarihi;
        mevcut.MentorId = stajyer.MentorId;
        mevcut.DepartmanId = stajyer.DepartmanId;

        _unitOfWork.Stajyerler.Update(mevcut);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var stajyer = await _unitOfWork.Stajyerler.GetByIdAsync(id);
        if (stajyer is null)
        {
            return;
        }

        _unitOfWork.Stajyerler.Remove(stajyer);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void EnsureTarihlerGecerli(Stajyer stajyer)
    {
        if (stajyer.BaslangicTarihi >= stajyer.BitisTarihi)
        {
            throw new InvalidOperationException("Başlangıç tarihi, bitiş tarihinden önce olmalıdır.");
        }
    }
}
