# Stajyer Takip Sistemi

Staj süreçlerinin uçtan uca (kayıt → görevlendirme → devam takibi → değerlendirme) takip edildiği bir ASP.NET Core web uygulaması.

## Roller

- **Yönetici** — başvuruları onaylar, kullanıcı/departman yönetimi yapar, rapor panelini görür.
- **Mentör** — kendi stajyerlerine görev atar, devam kayıtlarını onaylar, belge/talep açar.
- **Stajyer** — görevlerini ve devam durumunu görür, mentörünün taleplerine yanıt verir.

Kayıt olan bir kullanıcı, talep ettiği rol + departman ile başvurur; ilgili yönetici/mentör onayıyla hesabı aktif olur.

## Teknoloji Yığını

- ASP.NET Core 8 (MVC / Razor Views)
- Entity Framework Core (Repository + Unit of Work deseni)
- ASP.NET Core Identity (rol tabanlı yetkilendirme)
- SQL Server
- Bootstrap 5, Chart.js (yerel olarak paketlenmiş, CDN'siz)

## Katmanlar

```
src/
  StajyerTakip.Core         Entity'ler, Identity modelleri
  StajyerTakip.DataAccess   DbContext, migrations, repository/unit of work
  StajyerTakip.Business     Servisler (iş kuralları)
  StajyerTakip.Web          MVC controller/view'lar, statik dosyalar
```

## Kurulum

Gereksinimler: .NET 8 SDK, SQL Server (Express yeterli).

1. `src/StajyerTakip.Web/appsettings.json` içindeki `ConnectionStrings:DefaultConnection` değerini kendi SQL Server instance'ına göre düzenle (varsayılan Windows kimlik doğrulaması kullanır, şifre gerekmez).
2. Veritabanını oluştur:

   ```bash
   dotnet tool install --global dotnet-ef   # ilk kurulumda gerekli
   dotnet ef database update --project src/StajyerTakip.DataAccess --startup-project src/StajyerTakip.Web
   ```

3. Uygulamayı çalıştır:

   ```bash
   dotnet run --project src/StajyerTakip.Web
   ```

4. Tarayıcıda açılan adrese git ve `/Account/Register` üzerinden bir hesap oluştur.

## Durum

Aktif geliştirme aşamasında.
