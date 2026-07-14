using Microsoft.AspNetCore.Identity;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Models;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class MentorService : IMentorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public MentorService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public Task<List<Mentor>> GetAllAsync() => _unitOfWork.Mentorler.GetAllAsync(m => m.Departman, m => m.Kullanici);

    public Task<Mentor?> GetByIdAsync(int id) => _unitOfWork.Mentorler.GetByIdAsync(id);

    public async Task<Mentor?> GetByKullaniciIdAsync(string kullaniciId)
    {
        var eslesenler = await _unitOfWork.Mentorler.FindAsync(m => m.KullaniciId == kullaniciId);
        return eslesenler.SingleOrDefault();
    }

    public async Task CreateAsync(YeniMentorIstegi istek)
    {
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

        await RolVeProfilOlusturAsync(kullanici.Id, istek.Unvan, istek.DepartmanId);
    }

    public async Task AtaAsync(string kullaniciId, string unvan, int departmanId)
    {
        var kullanici = await _userManager.FindByIdAsync(kullaniciId);
        if (kullanici is null)
        {
            throw new InvalidOperationException("Kullanıcı bulunamadı.");
        }

        var mevcutRoller = await _userManager.GetRolesAsync(kullanici);
        if (mevcutRoller.Count > 0)
        {
            throw new InvalidOperationException("Bu kullanıcının zaten bir rolü var.");
        }

        await RolVeProfilOlusturAsync(kullaniciId, unvan, departmanId);
    }

    private async Task RolVeProfilOlusturAsync(string kullaniciId, string unvan, int departmanId)
    {
        var kullanici = await _userManager.FindByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        await _userManager.AddToRoleAsync(kullanici, Roller.Mentor);

        var mentor = new Mentor
        {
            KullaniciId = kullaniciId,
            Unvan = unvan,
            DepartmanId = departmanId
        };

        await _unitOfWork.Mentorler.AddAsync(mentor);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(Mentor mentor)
    {
        var mevcut = await _unitOfWork.Mentorler.GetByIdAsync(mentor.Id);
        if (mevcut is null)
        {
            throw new InvalidOperationException("Mentör bulunamadı.");
        }

        mevcut.Unvan = mentor.Unvan;
        mevcut.DepartmanId = mentor.DepartmanId;

        _unitOfWork.Mentorler.Update(mevcut);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var mentor = await _unitOfWork.Mentorler.GetByIdAsync(id);
        if (mentor is null)
        {
            return;
        }

        var baglıStajyerVarMi = (await _unitOfWork.Stajyerler.FindAsync(s => s.MentorId == id)).Any();
        if (baglıStajyerVarMi)
        {
            throw new InvalidOperationException(
                "Bu mentöre bağlı stajyer kayıtları var. Önce onları başka bir mentöre taşıyın veya silin.");
        }

        _unitOfWork.Mentorler.Remove(mentor);
        await _unitOfWork.SaveChangesAsync();
    }
}
