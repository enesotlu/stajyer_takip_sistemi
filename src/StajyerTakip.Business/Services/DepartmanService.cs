using System.Text;
using System.Text.RegularExpressions;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Interfaces;

namespace StajyerTakip.Business.Services;

public class DepartmanService : IDepartmanService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmanService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<Departman>> GetAllAsync() => _unitOfWork.Departmanlar.GetAllAsync();

    public Task<Departman?> GetByIdAsync(int id) => _unitOfWork.Departmanlar.GetByIdAsync(id);

    public async Task CreateAsync(Departman departman)
    {
        departman.Ad = AdiNormallestir(departman.Ad);
        await EnsureAdBenzersizAsync(departman.Ad, excludeId: null);

        await _unitOfWork.Departmanlar.AddAsync(departman);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(Departman departman)
    {
        departman.Ad = AdiNormallestir(departman.Ad);
        await EnsureAdBenzersizAsync(departman.Ad, excludeId: departman.Id);

        _unitOfWork.Departmanlar.Update(departman);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var departman = await _unitOfWork.Departmanlar.GetByIdAsync(id);
        if (departman is null)
        {
            return;
        }

        var baglıMentorVarMi = (await _unitOfWork.Mentorler.FindAsync(m => m.DepartmanId == id)).Any();
        var baglıStajyerVarMi = (await _unitOfWork.Stajyerler.FindAsync(s => s.DepartmanId == id)).Any();

        if (baglıMentorVarMi || baglıStajyerVarMi)
        {
            throw new InvalidOperationException(
                "Bu departmana bağlı mentör veya stajyer kayıtları var. Önce onları başka bir departmana taşıyın veya silin.");
        }

        _unitOfWork.Departmanlar.Remove(departman);
        await _unitOfWork.SaveChangesAsync();
    }

    // Departman adları her zaman büyük harf ve Türkçe karaktersiz saklanır:
    // "Yazılım Geliştirme" → "YAZILIM GELISTIRME". Amaç, aynı departmanın
    // "Yazılım"/"YAZILIM"/"yazilim" gibi farklı yazımlarla çoğalmasını önlemek.
    public static string AdiNormallestir(string ad)
    {
        const string turkce = "çÇğĞıİöÖşŞüÜ";
        const string karsilik = "CCGGIIOOSSUU";

        var sb = new StringBuilder(ad.Trim().Length);
        foreach (var karakter in ad.Trim())
        {
            var indeks = turkce.IndexOf(karakter);
            sb.Append(indeks >= 0 ? karsilik[indeks] : char.ToUpperInvariant(karakter));
        }

        // Birden fazla ardışık boşluğu teke indir.
        return Regex.Replace(sb.ToString(), @"\s+", " ");
    }

    private async Task EnsureAdBenzersizAsync(string ad, int? excludeId)
    {
        // Eski (normalize edilmemiş) kayıtlara karşı da güvenli olması için
        // karşılaştırma bellekte, iki taraf da normalize edilerek yapılır.
        var digerleri = await _unitOfWork.Departmanlar.FindAsync(d => d.Id != (excludeId ?? 0));
        var ayniOlanVar = digerleri.Any(d => AdiNormallestir(d.Ad) == ad);

        if (ayniOlanVar)
        {
            throw new InvalidOperationException($"\"{ad}\" adında bir departman zaten mevcut.");
        }
    }
}
