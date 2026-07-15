using Microsoft.AspNetCore.Identity;
using StajyerTakip.Business.Interfaces;
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
        _unitOfWork.Stajyerler.GetAllAsync(s => s.Mentor!.Kullanici, s => s.Departman, s => s.Kullanici);

    public Task<Stajyer?> GetByIdAsync(int id) => _unitOfWork.Stajyerler.GetByIdAsync(id);

    public async Task<Stajyer?> GetByIdWithDetailsAsync(int id)
    {
        var result = await _unitOfWork.Stajyerler.FindAsync(s => s.Id == id, s => s.Kullanici, s => s.Departman);
        return result.FirstOrDefault();
    }

    public async Task<Stajyer?> GetByKullaniciIdAsync(string kullaniciId)
    {
        var eslesenler = await _unitOfWork.Stajyerler.FindAsync(s => s.KullaniciId == kullaniciId);
        return eslesenler.SingleOrDefault();
    }

    // Zaten kayıt olmuş bir kullanıcıyı Mentör'ün onayıyla Stajyer yapar.
    public async Task AtaAsync(
        string kullaniciId, string okul, string bolum, DateTime baslangicTarihi, DateTime bitisTarihi,
        int mentorId, int departmanId)
    {
        if (baslangicTarihi >= bitisTarihi)
        {
            throw new InvalidOperationException("Başlangıç tarihi, bitiş tarihinden önce olmalıdır.");
        }

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

        await RolVeProfilOlusturAsync(kullaniciId, okul, bolum, baslangicTarihi, bitisTarihi, mentorId, departmanId);
    }

    private async Task RolVeProfilOlusturAsync(
        string kullaniciId, string okul, string bolum, DateTime baslangicTarihi, DateTime bitisTarihi,
        int mentorId, int departmanId)
    {
        var kullanici = await _userManager.FindByIdAsync(kullaniciId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        await _userManager.AddToRoleAsync(kullanici, Roller.Stajyer);

        kullanici.OnayDurumu = OnayDurumlari.Onaylandi;
        await _userManager.UpdateAsync(kullanici);

        var stajyer = new Stajyer
        {
            KullaniciId = kullaniciId,
            Okul = okul,
            Bolum = bolum,
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi,
            MentorId = mentorId,
            DepartmanId = departmanId
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

    // Admin: stajyerin sorumlu mentörünü değiştirir.
    public async Task MentorAtaAsync(int stajyerId, int yeniMentorId)
    {
        var stajyer = await _unitOfWork.Stajyerler.GetByIdAsync(stajyerId)
            ?? throw new InvalidOperationException("Stajyer bulunamadı.");

        // Geçersiz/boş seçim veritabanına inmeden burada yakalanır;
        // aksi halde foreign key ihlali 500 hatası olarak patlar.
        var yeniMentor = await _unitOfWork.Mentorler.GetByIdAsync(yeniMentorId)
            ?? throw new InvalidOperationException("Lütfen geçerli bir mentör seçin.");

        stajyer.MentorId = yeniMentor.Id;
        _unitOfWork.Stajyerler.Update(stajyer);
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
