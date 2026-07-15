using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Core.Entities;
using StajyerTakip.Core.Identity;
using StajyerTakip.Web.Models;

namespace StajyerTakip.Web.Controllers;

// Mentörün kendi stajyerlerinden talepte bulunduğu ekran
// ("CV gönder", "şu tarihte mülakata gel" vb.).
[Authorize(Roles = Roller.Mentor)]
public class TalepController : Controller
{
    private readonly ITalepService _talepService;
    private readonly IStajyerService _stajyerService;
    private readonly IMentorService _mentorService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _ortam;

    public TalepController(
        ITalepService talepService,
        IStajyerService stajyerService,
        IMentorService mentorService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment ortam)
    {
        _talepService = talepService;
        _stajyerService = stajyerService;
        _mentorService = mentorService;
        _userManager = userManager;
        _ortam = ortam;
    }

    public async Task<IActionResult> Index()
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return NotFound("Mentör profiliniz bulunamadı.");
        }

        var talepler = await _talepService.GetByMentorAsync(mentor.Id);
        return View(talepler);
    }

    public async Task<IActionResult> Create()
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return NotFound();
        }

        await PopulateStajyerListesiAsync(mentor.Id);
        return View(new TalepCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TalepCreateViewModel model)
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateStajyerListesiAsync(mentor.Id, model.StajyerId);
            return View(model);
        }

        try
        {
            await _talepService.CreateAsync(mentor.Id, model.StajyerId, model.Baslik, model.Aciklama, model.DosyaIstensin);
            TempData["BilgiMesaji"] = "Talep oluşturuldu; stajyerin ekranına bildirim olarak düştü.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateStajyerListesiAsync(mentor.Id, model.StajyerId);
            return View(model);
        }
    }

    // Stajyerin yüklediği cevap dosyasını indirir (yalnızca talebin sahibi mentör).
    public async Task<IActionResult> DosyaIndir(int id)
    {
        var mentor = await GirisYapanMentorAsync();
        if (mentor is null)
        {
            return NotFound();
        }

        var talep = await _talepService.GetByIdAsync(id);
        if (talep is null || string.IsNullOrEmpty(talep.CevapDosyaAdi))
        {
            return NotFound();
        }

        var stajyer = await _stajyerService.GetByIdAsync(talep.StajyerId);
        if (stajyer is null || stajyer.MentorId != mentor.Id)
        {
            return Forbid();
        }

        var dosyaYolu = Path.Combine(_ortam.ContentRootPath, "Uploads", "Talepler", talep.CevapDosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
        {
            return NotFound();
        }

        return PhysicalFile(dosyaYolu, "application/octet-stream",
            talep.CevapDosyaOrijinalAdi ?? talep.CevapDosyaAdi);
    }

    private async Task<Mentor?> GirisYapanMentorAsync()
    {
        var kullaniciId = _userManager.GetUserId(User);
        return kullaniciId is null ? null : await _mentorService.GetByKullaniciIdAsync(kullaniciId);
    }

    private async Task PopulateStajyerListesiAsync(int mentorId, int? seciliId = null)
    {
        var stajyerler = (await _stajyerService.GetAllAsync())
            .Where(s => s.MentorId == mentorId)
            .Select(s => new { s.Id, Ad = $"{s.Kullanici.AdSoyad} ({s.Okul})" });

        ViewBag.StajyerId = new SelectList(stajyerler, "Id", "Ad", seciliId);
    }
}
