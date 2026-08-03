using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;

namespace StajyerTakip.Web.Controllers;

// Stajyer listesi ve düzenleme: Mentör/Yönetici görür.
// Mentör yalnızca kendi stajyerlerini görür ve düzenler.
// Artık admin/mentör stajyer oluşturamaz — stajyerler kayıt sistemi üzerinden başvurur.
[Authorize(Roles = Roller.Yonetici + "," + Roller.Mentor)]
public class StajyerController : Controller
{
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly IDepartmanService _departmanService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StajyerController(
        IStajyerService stajyerService,
        IMentorService mentorService,
        IDepartmanService departmanService,
        UserManager<ApplicationUser> userManager)
    {
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _departmanService = departmanService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? ad, DateTime? tarih)
    {
        var stajyerler = await FiltrelenmisListeAsync(ad, tarih);

        ViewBag.AramaAd = ad;
        ViewBag.AramaTarih = tarih?.ToString("yyyy-MM-dd");

        return View(stajyerler);
    }

    // Index'teki ile aynı filtreler (isim, tarih, mentöre görünürlük) uygulanmış
    // listeyi Excel'e aktarır - ekranda ne görünüyorsa dosyaya o iner.
    public async Task<IActionResult> ExcelIndir(string? ad, DateTime? tarih)
    {
        var stajyerler = await FiltrelenmisListeAsync(ad, tarih);

        using var workbook = new XLWorkbook();
        var sayfa = workbook.Worksheets.Add("Stajyerler");

        string[] basliklar = { "Ad Soyad", "Okul", "Bölüm", "Başlangıç", "Bitiş", "Durum", "Mentör", "Departman" };
        for (var i = 0; i < basliklar.Length; i++)
        {
            sayfa.Cell(1, i + 1).Value = basliklar[i];
        }
        sayfa.Row(1).Style.Font.Bold = true;

        var satir = 2;
        foreach (var stajyer in stajyerler)
        {
            bool aktif = stajyer.BaslangicTarihi.Date <= DateTime.Today && DateTime.Today <= stajyer.BitisTarihi.Date;

            sayfa.Cell(satir, 1).Value = stajyer.Kullanici.AdSoyad;
            sayfa.Cell(satir, 2).Value = stajyer.Okul;
            sayfa.Cell(satir, 3).Value = stajyer.Bolum;
            sayfa.Cell(satir, 4).Value = stajyer.BaslangicTarihi;
            sayfa.Cell(satir, 4).Style.DateFormat.Format = "dd.MM.yyyy";
            sayfa.Cell(satir, 5).Value = stajyer.BitisTarihi;
            sayfa.Cell(satir, 5).Style.DateFormat.Format = "dd.MM.yyyy";
            sayfa.Cell(satir, 6).Value = aktif ? "Aktif" : "Pasif";
            sayfa.Cell(satir, 7).Value = stajyer.Mentor?.Kullanici?.AdSoyad ?? stajyer.Mentor?.Unvan ?? "—";
            sayfa.Cell(satir, 8).Value = stajyer.Departman.Ad;
            satir++;
        }

        sayfa.Columns().AdjustToContents();

        using var akis = new MemoryStream();
        workbook.SaveAs(akis);

        var dosyaAdi = $"stajyerler_{DateTime.Today:yyyy-MM-dd}.xlsx";
        return File(
            akis.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            dosyaAdi);
    }

    private async Task<List<Stajyer>> FiltrelenmisListeAsync(string? ad, DateTime? tarih)
    {
        var stajyerler = await _stajyerService.GetAllAsync();

        // Mentör yalnızca kendi stajyerlerini görür.
        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is not null)
        {
            stajyerler = stajyerler.Where(s => s.MentorId == girenMentor.Id).ToList();
        }

        if (!string.IsNullOrWhiteSpace(ad))
        {
            stajyerler = stajyerler
                .Where(s => s.Kullanici.AdSoyad.Contains(ad, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Secilen tarihte stajda olan (baslangic-bitis araligina giren) stajyerleri bulur.
        if (tarih.HasValue)
        {
            stajyerler = stajyerler
                .Where(s => s.BaslangicTarihi.Date <= tarih.Value.Date && tarih.Value.Date <= s.BitisTarihi.Date)
                .ToList();
        }

        return stajyerler;
    }

    public async Task<IActionResult> Edit(int id)
    {
        var stajyer = await _stajyerService.GetByIdAsync(id);
        if (stajyer is null)
        {
            return NotFound();
        }

        if (!await BuStajyerBanaMiAitAsync(stajyer))
        {
            return Forbid();
        }

        await PopulateDropdownlarAsync(stajyer.MentorId, stajyer.DepartmanId);
        return View(stajyer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Stajyer stajyer)
    {
        if (id != stajyer.Id)
        {
            return NotFound();
        }

        var mevcut = await _stajyerService.GetByIdAsync(id);
        if (mevcut is null)
        {
            return NotFound();
        }

        if (!await BuStajyerBanaMiAitAsync(mevcut))
        {
            return Forbid();
        }

        // Mentör, düzenlerken stajyeri başka bir mentöre devredemez.
        var girenMentor = await GirisYapanMentorAsync();
        if (girenMentor is not null)
        {
            stajyer.MentorId = girenMentor.Id;
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownlarAsync(stajyer.MentorId, stajyer.DepartmanId);
            return View(stajyer);
        }

        try
        {
            await _stajyerService.UpdateAsync(stajyer);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownlarAsync(stajyer.MentorId, stajyer.DepartmanId);
            return View(stajyer);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var stajyer = await _stajyerService.GetByIdAsync(id);
        if (stajyer is null)
        {
            return NotFound();
        }

        if (!await BuStajyerBanaMiAitAsync(stajyer))
        {
            return Forbid();
        }

        return View(stajyer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var stajyer = await _stajyerService.GetByIdAsync(id);
        if (stajyer is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!await BuStajyerBanaMiAitAsync(stajyer))
        {
            return Forbid();
        }

        await _stajyerService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<Mentor?> GirisYapanMentorAsync()
    {
        if (User.IsInRole(Roller.Yonetici))
        {
            return null;
        }

        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _mentorService.GetByKullaniciIdAsync(kullaniciId);
    }

    // Yönetici için her stajyer "kendisine ait" sayılır; Mentör içinse yalnızca kendi stajyerleri.
    private async Task<bool> BuStajyerBanaMiAitAsync(Stajyer stajyer)
    {
        var girenMentor = await GirisYapanMentorAsync();
        return girenMentor is null || stajyer.MentorId == girenMentor.Id;
    }

    private async Task PopulateDropdownlarAsync(int? seciliMentorId = null, int? seciliDepartmanId = null)
    {
        var mentorler = await _mentorService.GetAllAsync();
        var departmanlar = await _departmanService.GetAllAsync();

        var mentorSecenekleri = mentorler.Select(m => new { m.Id, Ad = $"{m.Kullanici.AdSoyad} ({m.Unvan})" });
        ViewBag.MentorId = new SelectList(mentorSecenekleri, "Id", "Ad", seciliMentorId);
        ViewBag.DepartmanId = new SelectList(departmanlar, "Id", "Ad", seciliDepartmanId);
    }
}
