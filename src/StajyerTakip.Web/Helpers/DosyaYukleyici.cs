namespace StajyerTakip.Web.Helpers;

/// <summary>Kullanıcı dosyası yükleme/silmede tek doğrulama kaynağı; Talep ve Görev
/// teslimlerinde aynı kurallar (izinli tür, boyut, güvenli adlandırma) uygulanır.</summary>
public static class DosyaYukleyici
{
    private static readonly string[] IzinliUzantilar = { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
    private const long MaksimumDosyaBoyutu = 10 * 1024 * 1024;

    public static string IzinliUzantilarMetni => string.Join(", ", IzinliUzantilar);

    // Dosyayı wwwroot DIŞINDaki bir klasöre, GUID adla kaydeder; indirme yalnızca
    // yetki kontrolü yapan controller action'ları üzerinden yapılmalıdır.
    public static async Task<(string DosyaAdi, string OrijinalAdi)> KaydetAsync(
        IFormFile dosya, string contentRootPath, string altKlasor)
    {
        var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
        if (!IzinliUzantilar.Contains(uzanti))
        {
            throw new InvalidOperationException(
                $"Bu dosya türüne izin verilmiyor. İzinli türler: {IzinliUzantilarMetni}");
        }

        if (dosya.Length > MaksimumDosyaBoyutu)
        {
            throw new InvalidOperationException("Dosya boyutu 10 MB'ı aşamaz.");
        }

        var klasor = Path.Combine(contentRootPath, "Uploads", altKlasor);
        Directory.CreateDirectory(klasor);

        var dosyaAdi = $"{Guid.NewGuid():N}{uzanti}";
        var orijinalAdi = Path.GetFileName(dosya.FileName);

        var tamYol = Path.Combine(klasor, dosyaAdi);
        await using var akis = File.Create(tamYol);
        await dosya.CopyToAsync(akis);

        return (dosyaAdi, orijinalAdi);
    }

    public static void SilVarsa(string contentRootPath, string altKlasor, string? dosyaAdi)
    {
        if (string.IsNullOrEmpty(dosyaAdi))
        {
            return;
        }

        var tamYol = Path.Combine(contentRootPath, "Uploads", altKlasor, dosyaAdi);
        if (File.Exists(tamYol))
        {
            File.Delete(tamYol);
        }
    }

    public static string TamYol(string contentRootPath, string altKlasor, string dosyaAdi) =>
        Path.Combine(contentRootPath, "Uploads", altKlasor, dosyaAdi);
}
