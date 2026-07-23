# Stajyer Takip Sistemi

Staj süreçlerinin uçtan uca — kayıt/onay, görevlendirme, devam takibi, izin, toplantı ve raporlama dahil — takip edildiği bir ASP.NET Core web uygulaması. Üç rol (Yönetici, Mentör, Stajyer), tek giriş ekranı ve rol bazlı yetkilendirme üzerine kurulu.

## Roller ve Kayıt/Onay Akışı

- **Yönetici** — mentör başvurularını onaylar, kullanıcı yetkilerini yönetir (yetki devri, pasifleştirme), departmanları yönetir, mentör-stajyer atamalarını yapar, genel rapor panelini görür.
- **Mentör** — kendi departmanına yapılan stajyer başvurularını onaylar, sorumlu olduğu stajyerlere görev atar, devam/puantaj kayıtlarını onaylar, belge talebi açar, toplantı daveti gönderir, izin taleplerini değerlendirir.
- **Stajyer** — görevlerini ve devam durumunu görür/günceller, mentörünün taleplerine belge yükleyerek yanıt verir, izin talep eder, toplantı davetlerini kabul/reddeder.

Bir kullanıcı kayıt olurken **rol talep eder** (Mentör veya Stajyer) ve bir **departman seçer**; hesap "onay bekliyor" durumunda kalır. Mentör başvurusunu Yönetici, Stajyer başvurusunu ilgili departmandaki bir Mentör onaylar — onaydan sonra hesap gerçek role ve profile kavuşur.

## Özellikler

- **Kullanıcı/rol yönetimi:** rol bazlı başvuru+onay akışı, yönetici yetki devri (tek admin modeli), hesap pasifleştirme/aktifleştirme (silme değil, denetim izi korunur).
- **Görev yönetimi:** mentör stajyerine görev atar, stajyer durumu günceller (Başlamadı → Devam Ediyor → Tamamlandı), mentör gerekirse geri gönderir.
- **Devam / Puantaj:** stajyer günlük giriş-çıkış saati girer, mentör onaylar/reddeder; aylık takvim görünümü ve özet (onaylanan/bekleyen/reddedilen/eksik gün sayısı).
- **Talep sistemi:** mentör stajyerinden belge/CV ister (son tarihli), stajyer dosya yükleyerek cevaplar, mentör yetersiz bulursa geri gönderebilir.
- **İzin sistemi:** stajyer belirli bir tarih/saat aralığı için izin talep eder, mentör onaylar veya gerekçeyle reddeder; yönetici raporlarda bekleyen izin sayısını görür.
- **Toplantı sistemi:** mentör toplantı daveti oluşturduğunda, sorumlu olduğu **tüm stajyerlere otomatik olarak** birer davet gider; her stajyer kabul eder veya sebep yazarak reddeder, mentör kimin ne cevap verdiğini tek ekrandan görür.
- **Otomatik erişim kontrolü:** stajın bitiş tarihi geçen bir stajyer artık sisteme giriş yapamaz.
- **Bildirim rozetleri:** bekleyen başvuru/talep/izin/toplantı sayıları sidebar'da kırmızı rozetle anlık görünür.
- **Raporlar (Yönetici Dashboard):** toplam/aktif stajyer, mentör, departman, bekleyen başvuru/izin sayıları; mentöre göre stajyer dağılımı, talep durumu ve departmana göre stajyer dağılımı grafikleri (Chart.js).

## Teknoloji Yığını

- ASP.NET Core 8 (MVC / Razor Views)
- Entity Framework Core (Repository + Unit of Work deseni, code-first migrations)
- ASP.NET Core Identity (rol tabanlı yetkilendirme, claim tabanlı görünen ad)
- SQL Server
- Bootstrap 5, Chart.js, jQuery Validation (hepsi yerel paketli, CDN'siz)

## Katmanlar

```
src/
  StajyerTakip.Core         Entity'ler, Identity modelleri, ortak enum'lar
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

İlk çalıştırmada bir yönetici hesabı otomatik olarak seed edilir (bkz. `Data/IdentitySeeder.cs`) — giriş bilgilerini oradan öğrenip **prod ortamına almadan önce şifresini değiştir.**

## Durum

Temel akışların tamamı (kayıt/onay, görev, devam, talep, izin, toplantı, raporlama) tamamlandı; geliştirme sürecinin sonuna yaklaşılıyor.
