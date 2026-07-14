using Microsoft.EntityFrameworkCore;
using StajyerTakip.Business.Interfaces;
using StajyerTakip.Business.Services;
using StajyerTakip.Core.Interfaces;
using StajyerTakip.DataAccess;
using StajyerTakip.DataAccess.Context;

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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDepartmanService, DepartmanService>();
builder.Services.AddScoped<IMentorService, MentorService>();
builder.Services.AddScoped<IStajyerService, StajyerService>();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
