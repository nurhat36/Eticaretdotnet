using ETicaret.Data; // AppDbContext'i kullanmak için
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // Cookie authentication için gerekli
using Microsoft.AspNetCore.Identity;
using ETicaret.Models; // Identity kullanýmý için

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsýný ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC servisini ekle
builder.Services.AddControllersWithViews();

// Session iþlemleri için gerekli servisleri ekle
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session 30 dakika aktif kalýr
    options.Cookie.HttpOnly = true; // Çerez sadece HTTP üzerinden eriþilebilir
    options.Cookie.IsEssential = true; // Kullanýcý onayý gerekmeden çalýþýr
});

// Authentication iþlemleri için gerekli servisleri ekle
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriþ yapýlmamýþsa yönlendirilecek path
        options.LogoutPath = "/Account/Logout"; // Çýkýþ pathi
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz giriþ pathi
    });

// Identity servislerini ekle
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Hesap onayý gerektirme
    options.Password.RequireDigit = true; // Þifrede rakam gerekliliði
    options.Password.RequireLowercase = true; // Þifrede küçük harf gerekliliði
    options.Password.RequireNonAlphanumeric = false; // Þifrede özel karakter gerekliliði
    options.Password.RequireUppercase = true; // Þifrede büyük harf gerekliliði
    options.Password.RequiredLength = 6; // Þifre minimum uzunluðu
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// MVC servisini eklemeyi unutma
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Hata yönetimi ve HTTPS ayarlarý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Sýralama ÖNEMLÝ:
// 1. Session
// 2. Authentication
// 3. Authorization
app.UseSession(); // Session yönetimi
app.UseAuthentication(); // Kimlik doðrulama
app.UseAuthorization();  // Yetkilendirme

// Varsayýlan route ayarý
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
