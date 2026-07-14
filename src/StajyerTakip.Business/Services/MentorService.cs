using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class MentorService : IMentorService
{
    private readonly IUnitOfWork _unitOfWork;

    public MentorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<Mentor>> GetAllAsync() => _unitOfWork.Mentorler.GetAllAsync(m => m.Departman);

    public Task<Mentor?> GetByIdAsync(int id) => _unitOfWork.Mentorler.GetByIdAsync(id);

    public async Task CreateAsync(Mentor mentor)
    {
        // 3. Hafta'da Identity gelene kadar geçici bir kimlik ataması.
        mentor.KullaniciId = Guid.NewGuid().ToString();

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
