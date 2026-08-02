using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Options;
using StajyerTakip.Business.Services;
using StajyerTakip.Core.Identity;
using StajyerTakip.Core.Interfaces;
using StajyerTakip.DataAccess;
using StajyerTakip.DataAccess.Context;
using StajyerTakip.Web.Data;

// Sunucunun isletim sistemi kulturu ortama gore degisebilir (Windows'ta
// Turkce, Linux/Docker container'inda genelde Ingilizce) - gun/ay adlari
// gibi kultur bazli metinlerin her ortamda ayni (Turkce) gorunmesi icin
// varsayilan kulturu sabitliyoruz.
var turkceKultur = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = turkceKultur;
CultureInfo.DefaultThreadCurrentUICulture = turkceKultur;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Entity'lerdeki "Departman Departman { get; set; } = null!;" gibi navigation
// property'ler nullable olmayan referans tipleri olduğu için, MVC bunları
// varsayılan olarak zorunlu (required) sayar. Formda sadece DepartmanId
// gönderdiğimizden bu, görünmeyen bir doğrulama hatasına yol açar. Kapatıyoruz.
builder.Services.AddControllersWithViews()
    .AddMvcOptions(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Rapordaki "parola politikası" gereksinimi burada somutlaşıyor.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;

        // Yeni kayıt olan hesaplar e-postasını 6 haneli kodla doğrulamadan
        // giriş yapamaz (bkz. AccountController.Register/EmailDogrula). Bu
        // alan eklenmeden önceki hesaplar EmailConfirmed=true ile oluşturulduğu
        // için (IdentitySeeder, eski Register akışı) bu kısıttan etkilenmez.
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<UygulamaClaimsFactory>()
    .AddDefaultTokenProviders();

// Şifre sıfırlama linkindeki token'ın geçerlilik süresi (varsayılan 1 gündü).
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(15);
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.Configure<EmailAyarlari>(builder.Configuration.GetSection("EmailAyarlari"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDepartmanService, DepartmanService>();
builder.Services.AddScoped<IMentorService, MentorService>();
builder.Services.AddScoped<IStajyerService, StajyerService>();
builder.Services.AddScoped<IGorevService, GorevService>();
builder.Services.AddScoped<IDevamService, DevamService>();
builder.Services.AddScoped<IKullaniciYonetimService, KullaniciYonetimService>();
builder.Services.AddScoped<ITalepService, TalepService>();
builder.Services.AddScoped<IIzinService, IzinService>();
builder.Services.AddScoped<IToplantiService, ToplantiService>();
builder.Services.AddScoped<IRaporService, RaporService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    // Uygulama açılışta bekleyen migration'ları kendi kendine uygular - Docker
    // gibi ortamlarda host makinede .NET SDK/dotnet-ef kurulu olmadan da
    // veritabanı şeması otomatik hazır hale gelir.
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
