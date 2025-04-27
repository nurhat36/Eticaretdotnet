using ETicaret.Data; // AppDbContext'i kullanmak i�in
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // Cookie authentication i�in gerekli
using Microsoft.AspNetCore.Identity;
using ETicaret.Models; // Identity kullan�m� i�in

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant�s�n� ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC servisini ekle
builder.Services.AddControllersWithViews();

// Session i�lemleri i�in gerekli servisleri ekle
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session 30 dakika aktif kal�r
    options.Cookie.HttpOnly = true; // �erez sadece HTTP �zerinden eri�ilebilir
    options.Cookie.IsEssential = true; // Kullan�c� onay� gerekmeden �al���r
});

// Authentication i�lemleri i�in gerekli servisleri ekle
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giri� yap�lmam��sa y�nlendirilecek path
        options.LogoutPath = "/Account/Logout"; // ��k�� pathi
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz giri� pathi
    });

// Identity servislerini ekle
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Hesap onay� gerektirme
    options.Password.RequireDigit = true; // �ifrede rakam gereklili�i
    options.Password.RequireLowercase = true; // �ifrede k���k harf gereklili�i
    options.Password.RequireNonAlphanumeric = false; // �ifrede �zel karakter gereklili�i
    options.Password.RequireUppercase = true; // �ifrede b�y�k harf gereklili�i
    options.Password.RequiredLength = 6; // �ifre minimum uzunlu�u
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// MVC servisini eklemeyi unutma
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Hata y�netimi ve HTTPS ayarlar�
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// S�ralama �NEML�:
// 1. Session
// 2. Authentication
// 3. Authorization
app.UseSession(); // Session y�netimi
app.UseAuthentication(); // Kimlik do�rulama
app.UseAuthorization();  // Yetkilendirme

// Varsay�lan route ayar�
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
