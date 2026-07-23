# 🎓 Stajyer Takip Sistemi

Kurumların kendi bünyelerinde yürüttükleri staj programlarını uçtan uca (kayıt/onay → görevlendirme → devam takibi → izin → toplantı → değerlendirme) dijital ortamda yönetebileceği, **rol tabanlı** bir ASP.NET Core web uygulaması.

## 📖 Proje Tanıtımı

### 🎯 Projenin Amacı

Stajyer Takip Sistemi, bir kurumun aldığı stajyerlerin süreçlerini (başvuru, görevlendirme, devam/puantaj, izin, mentör-stajyer iletişimi) tek bir yerden yönetmesini sağlar. Sistem üç rol üzerine kurulu: **Yönetici** genel yönetimi ve onayları yapar, **Mentör** kendi stajyerlerinden sorumludur, **Stajyer** kendi süreçlerini takip eder.

### 📍 Kullanım Alanları

- Kurumların/şirketlerin kendi bünyelerinde yürüttükleri **stajyer eğitim programları**
- Birden fazla departmanı ve her departmanda birden fazla mentörü olan orta/büyük ölçekli staj programları
- Stajyer başına devam/puantaj takibi, görev takibi ve mentör onayı gerektiren süreçler

### ⚡ Ana Özellikler

- **Rol Bazlı Başvuru/Onay Akışı** — kayıt olan kullanıcı Mentör ya da Stajyer rolü talep eder, ilgili yetkili onaylar
- **Görev Yönetimi** — mentör stajyerine görev atar, durumunu takip eder
- **Devam/Puantaj** — günlük giriş-çıkış kaydı, mentör onayı, aylık özet
- **Talep Sistemi** — mentörün stajyerden belge/CV istemesi, dosya yükleyerek cevaplama
- **İzin Sistemi** — stajyerin izin talep etmesi, mentörün onaylaması/reddetmesi
- **Toplantı Sistemi** — mentörün topluca toplantı daveti göndermesi, stajyerlerin kabul/ret vermesi
- **Raporlama** — yönetici için özet sayılar ve grafiklerle genel durum paneli

## ✨ Özellikler

### 👥 Kullanıcı / Rol Yönetimi
- ✅ Kayıt olurken rol (Mentör/Stajyer) + departman talebi
- ✅ Mentör başvurusu → Yönetici onayı, Stajyer başvurusu → ilgili departmandaki Mentör onayı
- ✅ Tek yöneticili model: yetki devri (bir sonraki yöneticiye), devreden otomatik oturumdan atılır
- ✅ Hesap pasifleştirme/aktifleştirme (silme değil — denetim izi korunur)
- ✅ Staj bitiş tarihi geçen stajyerin girişi otomatik engellenir

### 📝 Görev Yönetimi
- ✅ Mentör kendi stajyerine görev atar (başlık, açıklama, son tarih)
- ✅ Durum akışı: Başlamadı → Devam Ediyor → Tamamlandı
- ✅ Mentör, yetersiz bulduğu görevi geri gönderebilir

### 🕒 Devam / Puantaj
- ✅ Stajyer günlük giriş/çıkış saati girer (yalnızca bugün için, mesai saatine kadar)
- ✅ Mentör onaylar/reddeder; unutulan günü mentör kendisi (onaylı olarak) girebilir
- ✅ Aylık takvim görünümü + özet (onaylanan/bekleyen/reddedilen/eksik gün sayısı)

### 📁 Talep Sistemi
- ✅ Mentör stajyerinden belge/CV ister, son tarih belirler
- ✅ Stajyer metin + dosya (PDF/Word/PNG/JPG, 10 MB sınır) ile cevaplar
- ✅ Mentör cevabı yetersiz bulursa yeni son tarihle geri gönderebilir

### 🌴 İzin Sistemi
- ✅ Stajyer, tarih/saat aralığı + açıklama ile izin talep eder
- ✅ Mentör onaylar veya (isteğe bağlı gerekçeyle) reddeder
- ✅ Yönetici, bekleyen izin sayısını rapor panelinde görür

### 📅 Toplantı Sistemi
- ✅ Mentör toplantı daveti oluşturduğunda, sorumlu olduğu **tüm stajyerlere otomatik davet gider**
- ✅ Her stajyer ayrı ayrı kabul eder veya **sebep yazarak** reddeder
- ✅ Mentör, tek ekrandan kimin kabul kimin reddettiğini görür

### 📊 Raporlama (Yönetici Paneli)
- ✅ Toplam/aktif stajyer, mentör, departman sayıları
- ✅ Bekleyen mentör/stajyer başvurusu ve bekleyen izin sayıları
- ✅ Mentöre göre stajyer dağılımı, talep durumu ve departmana göre stajyer dağılımı grafikleri (Chart.js)

### 🔔 Bildirim Rozetleri
- ✅ Bekleyen başvuru, talep, izin ve toplantı yanıtı sayıları sidebar'da kırmızı rozetle anlık görünür

## 🚀 Kurulum

### Gereksinimler
- **.NET 8 SDK**
- **SQL Server** (Express yeterli)
- **İşletim Sistemi:** Windows, Linux, macOS (ASP.NET Core çapraz platform)

### Adımlar

1. **Bağlantı dizesini ayarla** — `src/StajyerTakip.Web/appsettings.json` içindeki `ConnectionStrings:DefaultConnection` değerini kendi SQL Server instance'ına göre düzenle (varsayılan Windows kimlik doğrulaması kullanır, şifre gerekmez).

2. **Veritabanını oluştur:**
   ```bash
   dotnet tool install --global dotnet-ef   # ilk kurulumda gerekli
   dotnet ef database update --project src/StajyerTakip.DataAccess --startup-project src/StajyerTakip.Web
   ```

3. **Uygulamayı çalıştır:**
   ```bash
   dotnet run --project src/StajyerTakip.Web
   ```

4. Tarayıcıda açılan adrese git ve `/Account/Register` üzerinden bir hesap oluştur.

> 💡 **Not:** İlk çalıştırmada bir yönetici hesabı otomatik seed edilir (bkz. `Data/IdentitySeeder.cs`) — giriş bilgilerini oradan öğrenip **prod ortamına almadan önce şifresini değiştir.**

## 📸 Ekranlar

- **Giriş/Kayıt** — split-screen tasarım, rol+departman seçimli kayıt formu
- **Ana Sayfa** — role göre değişen kısayol kartları, onay bekleyen kullanıcılar için durum bildirimi
- **Raporlar** — istatistik kartları + 3 grafik (yönetici)
- **Stajyerlerim / Görevler / Devam Onayı / İzin Talepleri / Toplantılar** — mentöre özel liste ve onay ekranları
- **Görevlerim / Devam Kayıtlarım / İzinlerim / Toplantılarım / Taleplerim** — stajyere özel liste ve işlem ekranları

## 🏗️ Proje Yapısı

```
src/
├── StajyerTakip.Core         # Entity'ler, Identity modelleri, ortak enum'lar
├── StajyerTakip.DataAccess   # DbContext, migrations, repository/unit of work
├── StajyerTakip.Business     # Servisler (iş kuralları)
└── StajyerTakip.Web          # MVC controller/view'lar, statik dosyalar (wwwroot)
```

Katmanlar arası bağımlılık tek yönlü: `Web → Business → DataAccess → Core`.

## 🛠️ Teknolojiler

### Backend
- **ASP.NET Core 8** — MVC / Razor Views
- **Entity Framework Core** — Repository + Unit of Work deseni, code-first migrations
- **ASP.NET Core Identity** — rol tabanlı yetkilendirme, claim tabanlı görünen ad

### Frontend
- **Razor Views** — sunucu taraflı render, ayrı bir frontend framework'ü yok
- **Bootstrap 5** — grid/form baseline'ı
- Özel bir tasarım katmanı (`site.css`) — kart/tablo/rozet bileşenleri
- **Chart.js** — rapor grafikleri
- **jQuery + jQuery Validation** — istemci taraflı form doğrulama

Hepsi `wwwroot/lib/` altında **yerel olarak paketli** — CDN bağımlılığı yok.

### Database
- **SQL Server** (Express dahil)

## 📊 Veritabanı Şeması (özet)

| Tablo | Açıklama |
|---|---|
| `AspNetUsers` | Identity kullanıcıları — ad soyad, talep edilen rol/departman, onay durumu |
| `Departmanlar` | Departman listesi |
| `Mentorler` | Mentör profili — unvan, departman, bağlı kullanıcı |
| `Stajyerler` | Stajyer profili — okul, bölüm, başlangıç/bitiş tarihi, mentör, departman |
| `Gorevler` | Görev — başlık, açıklama, son tarih, durum |
| `DevamKayitlari` | Günlük giriş/çıkış kaydı, onay durumu |
| `Talepler` | Mentörden stajyere belge talebi, cevap + dosya bilgisi |
| `Izinler` | Stajyer izin talebi, onay durumu, mentör notu |
| `Toplantilar` / `ToplantiKatilimlari` | Toplantı daveti ve her stajyer için ayrı katılım durumu |

## 🔒 Güvenlik

- Parolalar **ASP.NET Core Identity** ile hash'lenerek saklanır (düz metin şifre hiçbir yerde tutulmaz)
- Tüm form gönderimlerinde **anti-forgery token** doğrulaması
- **Rol tabanlı yetkilendirme** — her controller/action ilgili role kısıtlı (`[Authorize(Roles = ...)]`)
- **Sahiplik kontrolü** — bir mentör yalnızca kendi stajyerlerinin kayıtlarını görebilir/onaylayabilir
- EF Core (parametreli sorgular) ile **SQL Injection** koruması, Razor'un otomatik encode'u ile **XSS** koruması
- Dosya yüklemelerinde uzantı beyaz listesi + boyut sınırı (Talep sistemi)
- Veritabanı bağlantısı Windows Authentication ile — bağlantı dizesinde şifre yok

## 🚨 Sorun Giderme

### Uygulama derlenmiyor / dosya kilitli hatası
Uygulama zaten çalışıyorsa (`dotnet run`), aynı anda tekrar `dotnet build` çalıştırmak dosya kilidi hatası verir. Önce çalışan sunucuyu `Ctrl+C` ile durdur.

### Migration hatası
```bash
dotnet ef migrations add <IsimVer> --project src/StajyerTakip.DataAccess --startup-project src/StajyerTakip.Web
dotnet ef database update --project src/StajyerTakip.DataAccess --startup-project src/StajyerTakip.Web
```
Migration eklerken de uygulamanın çalışmıyor olması gerekir (yukarıdaki dosya kilidi sebebiyle).

### Veritabanına bağlanamıyor
`appsettings.json`'daki `Server=...` değerinin kendi SQL Server instance adınla eşleştiğinden emin ol (SSMS veya `sqlcmd -L` ile instance adını görebilirsin).

## 🎯 Gelecek Planları

- [ ] Duyuru ekranları (entity hazır, arayüz henüz yok)
- [ ] Reddedilen başvurular için yeniden başvuru akışı
- [ ] Docker (uygulama + SQL Server için Dockerfile/docker-compose)
- [ ] PDF/Excel export (rapor ve staj belgesi)
- [ ] Unit test kapsamı

---

**Durum:** Temel akışların tamamı (kayıt/onay, görev, devam, talep, izin, toplantı, raporlama) tamamlandı; geliştirme sürecinin sonuna yaklaşılıyor.
