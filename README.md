# 🎓 Stajyer Takip Sistemi

Kurumların kendi bünyelerinde yürüttükleri staj programlarını uçtan uca (kayıt/onay → görevlendirme → devam takibi → izin → toplantı) dijital ortamda yönetebileceği, **rol tabanlı** bir ASP.NET Core web uygulaması.

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
- **Devam/Puantaj** — konum doğrulamalı otomatik giriş kaydı, mentör onayı, aylık özet
- **Talep Sistemi** — mentörün stajyerden belge/CV istemesi, dosya yükleyerek cevaplama
- **İzin Sistemi** — stajyerin izin talep etmesi, mentörün onaylaması/reddetmesi
- **Toplantı Sistemi** — mentörün topluca toplantı daveti göndermesi, stajyerlerin kabul/ret vermesi
- **E-posta Bildirimleri** — kayıt doğrulama, şifre sıfırlama, izin/görev/talep/toplantı olaylarında otomatik mail
- **Raporlama** — yönetici için özet sayılar ve grafiklerle genel durum paneli

## ✨ Özellikler

### 👥 Kullanıcı / Rol Yönetimi
- ✅ Kayıt olurken rol (Mentör/Stajyer) + departman talebi
- ✅ Yeni kayıtlar e-postaya gönderilen **6 haneli kodla doğrulanmadan** giriş yapamaz
- ✅ **Şifremi Unuttum** — e-postaya gönderilen linkle (15 dk geçerli) şifre sıfırlama
- ✅ Mentör başvurusu → Yönetici onayı, Stajyer başvurusu → ilgili departmandaki Mentör onayı
- ✅ Tek yöneticili model: yetki devri (bir sonraki yöneticiye), devreden otomatik oturumdan atılır
- ✅ Hesap pasifleştirme/aktifleştirme (silme değil — denetim izi korunur)
- ✅ Staj bitiş tarihi geçen stajyerin girişi otomatik engellenir
- ✅ Stajyer listesinde **isimle arama** ve **tarihe göre filtreleme** (seçilen tarihte stajı devam edenler)

### 📝 Görev Yönetimi
- ✅ Mentör kendi stajyerine görev atar (başlık, açıklama, son tarih)
- ✅ Durum akışı: Başlamadı → Devam Ediyor → Tamamlandı
- ✅ Mentör, yetersiz bulduğu görevi geri gönderebilir

### 🕒 Devam / Puantaj
- ✅ Stajyer manuel saat girmez — **girişte konum doğrulanır** (belirlenen referans noktaya belirli bir yarıçap içindeyse) ve devam kaydı otomatik açılır; konum dışındaysa ya da izin verilmediyse giriş reddedilir
- ✅ Mentör onaylar/reddeder; otomatik açılan kaydın saatini düzenleyebilir ya da tamamen kaldırabilir (unutulan/hatalı gün "Yok" olarak görünür)
- ✅ Onay listesinde kayıt konumu haritada görüntülenebilir (mentörün elle girdiği kayıtlarda konum yok, ayrıca belirtilir)
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

### 📧 E-posta Bildirimleri
- ✅ Kayıt doğrulama kodu, şifre sıfırlama linki
- ✅ İzin talebi açılınca mentöre, onaylanınca/reddedilince stajyere
- ✅ Görev atanınca stajyere, teslim edilince mentöre
- ✅ Talep açılınca stajyere, cevaplanınca mentöre
- ✅ Toplantı daveti açılınca davetli stajyerlere, kabul/red edilince mentöre
- ✅ SMTP hatası (yanlış ayar, geçici kesinti vb.) ana işlemi **bloklamaz** — sessizce loglanır, uygulama akışı bozulmaz

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

2. **Uygulamayı çalıştır:**
   ```bash
   dotnet run --project src/StajyerTakip.Web
   ```
   Bekleyen migration'lar açılışta **otomatik uygulanır** (`Program.cs`, `Database.MigrateAsync()`) — ayrıca `dotnet ef database update` çalıştırmana gerek yok.

3. Tarayıcıda açılan adrese git ve `/Account/Register` üzerinden bir hesap oluştur.

> 💡 **Not:** İlk çalıştırmada bir yönetici hesabı otomatik seed edilir (bkz. `Data/IdentitySeeder.cs`) — giriş bilgilerini oradan öğrenip **prod ortamına almadan önce şifresini değiştir.**

### 🐳 Docker ile Çalıştırma (Alternatif)

SQL Server'ı kendi makinene kurmadan, hem uygulamayı hem veritabanını container'larda çalıştırabilirsin — **.NET SDK'nın host makinede kurulu olmasına bile gerek yok**, tek bağımlılık Docker Desktop.

**Gereksinimler:** Docker Desktop (Windows'ta WSL2 backend'i etkin olmalı)

1. `.env.example` dosyasını `.env` olarak kopyala; `MSSQL_SA_PASSWORD` ve (e-posta göndermek istiyorsan) `SMTP_*` değerlerini kendi bilgilerinle değiştir.
2. Container'ları build edip başlat:
   ```bash
   docker compose up --build
   ```
3. Tarayıcıda `http://localhost:8080` adresine git.

Veritabanı şeması ve yönetici hesabı **otomatik oluşur** — ayrıca migration komutu çalıştırman gerekmez (yukarıdaki "Uygulamayı çalıştır" notuyla aynı otomatik migration mekanizması).

`docker-compose.yml`, `web` (çok aşamalı `Dockerfile` ile derlenen ASP.NET Core uygulaması) ve `db` (SQL Server 2022 Linux) servislerini tanımlar. `db` servisindeki `healthcheck`, SQL Server gerçekten bağlantı kabul edecek hâle gelene kadar `web`'in başlamasını bekletir. Veritabanı dosyaları ve antiforgery/cookie şifreleme anahtarları kalıcı Docker volume'lerinde tutulur — `docker compose down` ile container'ları kaldırsan bile veri kaybolmaz (`down -v` volume'leri de siler).

> ⚠️ **Not:** Docker'daki veritabanı, senin yerel (Windows) SQL Server'ından tamamen ayrı ve bağımsızdır. `dotnet run` ile oluşturduğun hesaplar Docker'da görünmez, aynı şekilde tersi de geçerlidir — ikisi birbirinden habersiz iki farklı ortam.

### ☁️ Canlı Deploy (AWS)

Proje, `docker-compose.prod.yml` ile bir AWS EC2 sunucusuna deploy edilecek şekilde de yapılandırılmış:

- **EC2** (`t2.micro`/`t3.micro`, ücretsiz kota) — sadece `web` ve `caddy` container'larını çalıştırır.
- **Amazon RDS** (SQL Server Express, ücretsiz kota) — veritabanı EC2'nin üzerinde değil, ayrı, yönetilen bir serviste. Sebebi: SQL Server'ın kendisi en az 2GB RAM istiyor, `t2.micro`/`t3.micro`'nun 1GB'ı buna yetmiyor (swap bu kontrolü atlatmıyor).
- **Caddy** — reverse proxy; Let's Encrypt sertifikasını **otomatik** alıp yeniliyor, `web` servisi dışarıya doğrudan port açmıyor. Bu **zorunlu**: stajyer girişindeki konum doğrulaması (`navigator.geolocation`) tarayıcılarda yalnızca HTTPS (ya da `localhost`) üzerinde çalışıyor, düz HTTP'de tarayıcı isteği tamamen engelliyor.

**Kurulum özeti** (sunucuda, kod çekildikten sonra):
```bash
cp .env.example .env
nano .env   # RDS_ENDPOINT, RDS_KULLANICI_ADI, RDS_SIFRE, SMTP_* değerlerini gir
docker compose -f docker-compose.prod.yml up --build -d
```
`Caddyfile`'daki domain adını kendi alan adınla değiştirmen gerekir.

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
- **Entity Framework Core** — Repository + Unit of Work deseni, code-first migrations, açılışta otomatik migration
- **ASP.NET Core Identity** — rol tabanlı yetkilendirme, e-posta doğrulamalı hesaplar, claim tabanlı görünen ad
- **SMTP (System.Net.Mail)** — kayıt doğrulama, şifre sıfırlama, izin/görev/talep/toplantı bildirimleri

### Frontend
- **Razor Views** — sunucu taraflı render, ayrı bir frontend framework'ü yok
- **Bootstrap 5** — grid/form baseline'ı
- Özel bir tasarım katmanı (`site.css`) — kart/tablo/rozet bileşenleri
- **Chart.js** — rapor grafikleri
- **jQuery + jQuery Validation** — istemci taraflı form doğrulama

Hepsi `wwwroot/lib/` altında **yerel olarak paketli** — CDN bağımlılığı yok.

### Database
- **SQL Server** — yerelde Express, Docker'da SQL Server 2022 Linux container'ı, canlıda Amazon RDS (SQL Server Express)

### DevOps
- **Docker / docker-compose** — çok aşamalı `Dockerfile`; yerel geliştirme için `docker-compose.yml` (`web`+`db`), deploy için `docker-compose.prod.yml` (`web`+`caddy`, veritabanı RDS'te)
- **Caddy** — canlıda otomatik Let's Encrypt HTTPS sağlayan reverse proxy
- **AWS (EC2 + RDS)** — deploy edilen ortam

## 📊 Veritabanı Şeması (özet)

| Tablo | Açıklama |
|---|---|
| `AspNetUsers` | Identity kullanıcıları — ad soyad, talep edilen rol/departman, onay durumu, e-posta doğrulama kodu |
| `Departmanlar` | Departman listesi |
| `Mentorler` | Mentör profili — unvan, departman, bağlı kullanıcı |
| `Stajyerler` | Stajyer profili — okul, bölüm, başlangıç/bitiş tarihi, mentör, departman |
| `Gorevler` | Görev — başlık, açıklama, son tarih, durum |
| `DevamKayitlari` | Konum doğrulamalı giriş/çıkış kaydı, onay durumu, enlem/boylam |
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
- Yerel geliştirmede veritabanı bağlantısı Windows Authentication ile — bağlantı dizesinde şifre yok. Docker ortamında SQL kimlik doğrulaması kullanılır; şifre `.env` dosyasında tutulur, `.gitignore` ile repoya dahil edilmez (bkz. `.env.example`)

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

### Docker Desktop açılmıyor ("unable to start")
Windows'ta genelde WSL2 kurulu/etkin değildir. Yönetici olarak PowerShell'de `wsl --install` çalıştır, bilgisayarı yeniden başlat, tekrar dene.

### `docker compose up` sonrası `web` container'ı veritabanına bağlanamıyor
- **"Login failed" / bağlantı hatası hemen başlangıçta:** `db` servisi henüz tam açılmamışken `web` bağlanmayı deniyor olabilir. `docker-compose.yml`'deki `healthcheck` + `depends_on: db: condition: service_healthy` bunu önler; yine de görürsen `db`'nin loglarını (`docker compose logs db`) kontrol et.

### Docker'da e-posta gönderilmiyor / hesap bulunamıyor
Docker'daki veritabanı yerelden tamamen ayrı (yukarıdaki uyarıya bak). Ayrıca `.env`'deki `SMTP_*` değerlerinin dolu olduğundan emin ol — boşsa e-posta gönderimi sessizce başarısız olur (`docker compose logs web` içinde "gönderilemedi" uyarısı arayabilirsin).

### Docker container'ı yeniden başlayınca "antiforgery token could not be decrypted" hatası
`web` servisinin `docker-compose.yml`'deki DataProtection anahtar volume'ü (`stajyer-takip-dataprotection-keys`) kaldırılmış/bozulmuş olabilir. Sayfayı yenile (F5) — tarayıcın yeni bir token alır, sorun geçer.

### `db` container'ı "requires a machine with at least 2000 megabytes of memory" hatasıyla çöküyor
SQL Server'ın sabit bir minimum fiziksel RAM kontrolü var (**swap bunu atlatmaz**, sadece fiziksel RAM'e bakar). `t2.micro`/`t3.micro` gibi 1GB'lık sunucularda bu her zaman başarısız olur — çözüm, veritabanını Amazon RDS gibi ayrı, yönetilen bir servise taşımak (bkz. "Canlı Deploy" bölümü) ya da en az 2GB RAM'li bir instance kullanmak.

### EC2'de `docker compose` çalışırken "No space left on device"
Varsayılan EC2 kök disk boyutu (genelde 8GB) Docker imajları için yetersiz kalabilir. AWS Console'dan volume'ü büyüt (Free tier 30GB'a kadar izin verir), sonra sunucuda: `sudo growpart /dev/nvme0n1 1 && sudo resize2fs /dev/nvme0n1p1`.

### Domain bağladım ama site açılmıyor / bazı cihazlarda açılıyor bazılarında açılmıyor
DNS değişiklikleri anında yayılmaz (dakikalar-saatler sürebilir), farklı ağlar/cihazlar farklı zamanlarda güncel kaydı görür. `ipconfig /flushdns` (Windows) ile yerel önbelleği temizlemek çoğu zaman yardımcı olur. Ayrıca alan adını GitHub Student Pack gibi bir kaynaktan aldıysan, varsayılan olarak eklenmiş GitHub Pages A/CNAME kayıtları olabilir — DNS ayarlarında sadece kendi sunucunun IP'sine giden tek bir kayıt kalana kadar fazlalıkları sil.

### Stajyer girişinde konum isteği hiç çıkmıyor / "konumu açamıyorum"
Tarayıcılar, `navigator.geolocation` API'sini yalnızca **güvenli bağlamda** (HTTPS ya da `localhost`) çalıştırır. Düz HTTP ile canlıya alınmış bir domain'de bu istek sessizce engellenir. Çözüm: HTTPS kur (bkz. "Canlı Deploy" bölümündeki Caddy kurulumu).

## 🎯 Gelecek Planları

- [ ] Duyuru ekranları (entity hazır, arayüz henüz yok)
- [ ] Reddedilen başvurular için yeniden başvuru akışı
- [ ] PDF/Excel export (rapor ve staj belgesi)
- [ ] Unit test kapsamı

---

**Durum:** Temel akışların tamamı (kayıt/onay, görev, devam, talep, izin, toplantı, raporlama, e-posta bildirimleri) tamamlandı; proje Docker ile paketlenip AWS'e (EC2 + RDS + Caddy/HTTPS) canlı olarak deploy edildi.
