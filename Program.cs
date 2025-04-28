using ETicaret.Data;
using ETicaret.Models; // ApplicationUser modelini kullanmak için
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) Veritabaný baðlantýsýný ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Identity ayarlarý ve þifre politikalarý
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Þifre ayarlarý
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Giriþ yaparken email onayý zorunlu mu?
    options.SignIn.RequireConfirmedAccount = false;

    // Hesap kilitleme ayarlarý
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 dakika kilitli kalýr
    options.Lockout.MaxFailedAccessAttempts = 5; // 5 yanlýþ giriþten sonra kilitlenir
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders(); // Þifre resetleme, email onayý gibi token iþlemleri için

// 3) Identity Cookie ayarlarý (Giriþ-Çýkýþ Yollarý)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // 1 saat boyunca oturum aktif
    options.SlidingExpiration = true; // Süre boyunca kullanýcý aktifse süre uzar
});

// 4) MVC Controller + Views
builder.Services.AddControllersWithViews();

// 5) Session ayarlarý
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dk oturum
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 6) Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();           // Session kullanýlacaksa önce bunu yaz
app.UseAuthentication();    // Kimlik doðrulama
app.UseAuthorization();     // Yetkilendirme

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
