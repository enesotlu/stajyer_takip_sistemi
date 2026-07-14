using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Services;
using StajyerTakip.Core.Identity;
using StajyerTakip.Core.Interfaces;
using StajyerTakip.DataAccess;
using StajyerTakip.DataAccess.Context;
using StajyerTakip.Web.Data;

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
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDepartmanService, DepartmanService>();
builder.Services.AddScoped<IMentorService, MentorService>();
builder.Services.AddScoped<IStajyerService, StajyerService>();
builder.Services.AddScoped<IGorevService, GorevService>();
builder.Services.AddScoped<IDevamService, DevamService>();

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
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
